"""Loopback-only workbench. Static paths and card portraits are explicit allowlists."""
import argparse
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import mimetypes
import os
from pathlib import Path
import re
import secrets
import signal
import sys
import threading
from urllib.parse import unquote, urlsplit

from workbench_model import ValidationError, RevisionConflict, CorruptWorkspace, compose_graph
from workbench_store import WorkspaceStore
from workbench_assets import MAX_BYTES
from relation_rules import load_profiles, suggest_relations
from workbench_model import require, preview_relations

STATIC=set(('index.html graph.css graph.js graph-data.js relation-style.js card-references.js card-aliases.js '
            'workbench-api.js draft-editor.js draft-editor.css workbench.js README.md '
            'vendor/cytoscape-3.33.1.min.js vendor/lucide-0.468.0.min.js').split())


class HttpError(Exception):
    def __init__(self,status,message): self.status=status;self.message=message


def create_server(root:Path, workspace:Path, port=8765):
    root=Path(root).resolve();baseline=json.loads((root/'graph.json').read_text())
    profiles=load_profiles(root/'mechanic-profiles.json',baseline)
    store=WorkspaceStore(workspace,baseline,profiles)
    token=secrets.token_urlsafe(32)
    assets=(root.parents[3]/baseline['meta']['source_worktree']/baseline['meta']['mod_directory']/'assets').resolve()
    portraits={n['id']:(root/n['portrait']).resolve() for n in baseline['nodes'] if n.get('portrait')}

    def http_graph(graph):
        for node in graph['nodes']:
            if node['id'] in portraits: node['portrait']='/api/portraits/'+node['id']
            profile=profiles['cards'].get(node['id'])
            if profile:
                for key in ('keywords','upgraded_keywords','upgraded_cost'):node[key]=profile[key]
                node['mechanics']=profile['facts']
        return graph

    class Handler(BaseHTTPRequestHandler):
        server_version='CardWorkbench/1'
        def log_message(self,fmt,*args): pass
        def reply(self,status,body,mime='application/json; charset=utf-8',download=False):
            if not isinstance(body,bytes): body=json.dumps(body,ensure_ascii=False,allow_nan=False).encode()
            self.send_response(status);self.send_header('Content-Type',mime);self.send_header('Content-Length',str(len(body)))
            self.send_header('Cache-Control','no-store');self.send_header('X-Content-Type-Options','nosniff')
            self.send_header('Referrer-Policy','no-referrer');self.send_header('Cross-Origin-Resource-Policy','same-origin')
            self.send_header('Content-Security-Policy',"default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' blob:; connect-src 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'none'")
            if download: self.send_header('Content-Disposition','attachment; filename="sakiko-workspace.json"')
            self.end_headers();self.wfile.write(body)
        def origin_check(self,writing=False):
            host=f'127.0.0.1:{self.server.server_address[1]}';origin='http://'+host
            if self.headers.get('Host')!=host or self.headers.get('Origin',origin)!=origin:
                raise HttpError(403,'仅允许本机同源访问')
            if writing and (self.headers.get('Origin')!=origin or not secrets.compare_digest(self.headers.get('X-Workbench-Token',''),token)):
                raise HttpError(403,'页面会话已失效，请重新连接')
        def safe_file(self,path,allowed_root):
            resolved=path.resolve()
            if not resolved.is_relative_to(allowed_root) or not resolved.is_file(): raise HttpError(404,'文件不存在')
            return resolved.read_bytes()
        def do_GET(self): self.dispatch(False)
        def do_POST(self): self.dispatch(True)
        def dispatch(self,writing):
            try:
                self.origin_check(writing)
                path=unquote(urlsplit(self.path).path)
                if writing: return self.post(path)
                if path=='/api/bootstrap':
                    return self.reply(200,{'token':token,'graph':http_graph(compose_graph(baseline,store.read(),store.profiles)),
                        'workspace':store.read(),'profiles_revision':store.profiles.get('revision','none'),
                        'capabilities':{'editing':True,'analysis':True}})
                if path=='/api/graph': return self.reply(200,http_graph(compose_graph(baseline,store.read(),store.profiles)))
                if path=='/api/export': return self.reply(200,store.read(),download=True)
                if path.startswith('/api/portraits/'):
                    card_id=path.removeprefix('/api/portraits/');image=portraits.get(card_id)
                    if image is None: raise HttpError(404,'卡图不存在')
                    return self.reply(200,self.safe_file(image,assets),mimetypes.guess_type(image)[0] or 'image/png')
                if path.startswith('/api/images/'):
                    identifier=path.removeprefix('/api/images/');meta=store.read()['images'].get(identifier)
                    if not meta or not re.fullmatch(r'[0-9a-f-]{36}',identifier): raise HttpError(404,'图片不存在')
                    extension=meta.get('extension')
                    if extension not in ('png','jpg','webp'): raise HttpError(404,'图片不存在')
                    image=store.root/'uploads'/f'{identifier}.{extension}'
                    return self.reply(200,self.safe_file(image,store.root),meta['mime'])
                name=path.lstrip('/') if path!='/' else 'index.html'
                if name not in STATIC: raise HttpError(404,'文件不存在')
                return self.reply(200,self.safe_file(root/name,root),mimetypes.guess_type(name)[0] or 'text/plain; charset=utf-8')
            except HttpError as error: self.error(error.status,error.message)
            except CorruptWorkspace as error: self.error(503,str(error),error.code,error.fields)
            except RevisionConflict as error: self.error(409,str(error),error.code,error.fields)
            except ValidationError as error: self.error(415 if error.code=='image_type' else 422,str(error),error.code,error.fields)
            except (OSError,TimeoutError): self.error(503,'本机读取或保存失败，未覆盖工作区','io_error')
            except (ValueError,TypeError,KeyError): self.error(400,'请求格式无效','bad_request')
        def error(self,status,message,code='request_error',fields=None):
            self.close_connection=True
            try: self.reply(status,{'error':{'code':code,'message':message,'fields':fields or []}})
            except (BrokenPipeError,ConnectionResetError): pass
        def post(self,path):
            if self.headers.get('Transfer-Encoding'): raise HttpError(400,'不支持分块请求')
            length=self.headers.get('Content-Length')
            if length is None: raise HttpError(411,'需要Content-Length')
            if not length.isdigit(): raise HttpError(400,'Content-Length无效')
            limit=MAX_BYTES if path=='/api/uploads' else 2*1024*1024
            if int(length)>limit: raise HttpError(413,'请求超过大小限制')
            self.connection.settimeout(10)
            raw=self.rfile.read(int(length))
            if len(raw)!=int(length): raise HttpError(400,'请求未传输完整')
            if path=='/api/uploads':
                revision=self.headers.get('X-Workspace-Revision','')
                if not revision.isdigit(): raise HttpError(400,'缺少工作区版本')
                return self.reply(200,store.add_image(raw,int(revision)))
            try: body=json.loads(raw,parse_constant=lambda _: (_ for _ in ()).throw(ValueError()))
            except (ValueError,UnicodeDecodeError): raise HttpError(400,'JSON格式无效')
            if not isinstance(body,dict): raise HttpError(400,'请求必须为对象')
            if path=='/api/commands': return self.reply(200,store.apply(body.get('command'),body.get('expected_revision')))
            if path in ('/api/analyze','/api/preview'):
                state=store.read();identifier=body.get('draft_id')
                require(isinstance(identifier,str) and identifier in state['entries'],'找不到草稿')
                analysis=suggest_relations(identifier,body.get('card'),baseline,state,profiles)
                if path=='/api/analyze': return self.reply(200,analysis)
                preview={**body,'relations':preview_relations(analysis,state['entries'][identifier],profiles)}
                return self.reply(200,{'graph':http_graph(compose_graph(baseline,state,profiles,preview)), 'analysis':analysis})
            raise HttpError(404,'接口不存在')

    try:
        try: server=ThreadingHTTPServer(('127.0.0.1',port),Handler)
        except OSError:
            if port!=8765: raise
            server=ThreadingHTTPServer(('127.0.0.1',0),Handler)
    except Exception: store.close();raise
    server.store=store
    return server


def main():
    parser=argparse.ArgumentParser();parser.add_argument('--workspace',type=Path,default=Path(__file__).parent/'workspace')
    parser.add_argument('--port',type=int,default=8765);args=parser.parse_args()
    server=create_server(Path(__file__).parent,args.workspace,args.port)
    def stop(*_): threading.Thread(target=server.shutdown,daemon=True).start()
    signal.signal(signal.SIGTERM,stop);signal.signal(signal.SIGINT,stop)
    print(json.dumps({'url':f'http://127.0.0.1:{server.server_address[1]}/','pid':os.getpid()}),flush=True)
    try: server.serve_forever()
    finally: server.server_close();server.store.close()


if __name__=='__main__': main()
