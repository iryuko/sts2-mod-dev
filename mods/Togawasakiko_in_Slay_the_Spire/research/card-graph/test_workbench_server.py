import http.client
import json
from pathlib import Path
import tempfile
import threading
import unittest

from server import create_server
from test_workbench_model import card

ROOT=Path(__file__).parent


class ServerTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.start()
    def start(self):
        self.server=create_server(ROOT,Path(self.temp.name),0)
        self.thread=threading.Thread(target=self.server.serve_forever,daemon=True);self.thread.start()
        self.port=self.server.server_address[1];self.origin=f'http://127.0.0.1:{self.port}'
        self.boot=self.request('GET','/api/bootstrap')[1]
    def stop(self):
        self.server.shutdown();self.server.server_close();self.thread.join();self.server.store.close()
    def tearDown(self): self.stop();self.temp.cleanup()
    def request(self,method,path,data=None,headers=None):
        h={'Origin':self.origin}
        if method=='POST': h['X-Workbench-Token']=self.boot['token']
        if isinstance(data,dict): data=json.dumps(data).encode();h['Content-Type']='application/json'
        h.update(headers or {})
        conn=http.client.HTTPConnection('127.0.0.1',self.port,timeout=5)
        try:
            conn.request(method,path,data,headers=h);r=conn.getresponse();body=r.read()
            return r.status,json.loads(body) if 'application/json' in r.getheader('Content-Type','') else body,dict(r.getheaders())
        finally: conn.close()

    def test_same_origin_token_and_static_boundaries(self):
        command={'expected_revision':0,'command':{'action':'create','card':card()}}
        for headers in [{'X-Workbench-Token':''},{'Origin':'null'},{'Origin':'https://example.com'}, {'Host':'evil.test'}]:
            self.assertEqual(self.request('POST','/api/commands',command,headers)[0],403)
        for path in ['/../AGENTS.md','/%2e%2e/AGENTS.md','/workspace/workspace.json','/server.py','/api/images/nope']:
            self.assertEqual(self.request('GET',path)[0],404)
        self.assertEqual(self.request('GET','/api/bootstrap',headers={'Host':'evil.test'})[0],403)
        status,data,headers=self.request('POST','/api/commands',command)
        self.assertEqual(status,200);self.assertEqual(data['workspace']['revision'],1)
        self.assertNotIn('Access-Control-Allow-Origin',headers)
        self.assertNotIn('token',self.request('GET','/api/export')[1])
        self.assertEqual(self.request('POST','/api/commands',command)[0],409)

    def test_image_original_bytes_restart_and_upload_revision(self):
        graph=json.loads((ROOT/'graph.json').read_text())
        data=(ROOT/next(n['portrait'] for n in graph['nodes'] if n.get('portrait'))).read_bytes()
        status,result,_=self.request('POST','/api/uploads',data,{'X-Workspace-Revision':'0','Content-Type':'image/png'})
        self.assertEqual(status,200)
        path='/api/images/'+result['image_id']
        self.assertEqual(self.request('GET',path)[1],data)
        self.stop();self.start()
        self.assertEqual(self.request('GET',path)[1],data)
        self.assertEqual(self.request('POST','/api/commands',{'expected_revision':0,'command':{'action':'create','card':card()}})[0],409)
        for body in [b'<svg/>',b'not an image']:
            status,_,_=self.request('POST','/api/uploads',body,{'X-Workspace-Revision':'1','Content-Type':'image/png'})
            self.assertIn(status,(413,415,422))
        self.assertEqual(self.request('POST','/api/uploads',b'',{'Content-Length':str(10*1024*1024+1)})[0],413)
        self.assertEqual(self.server.store.read()['revision'],1)

    def test_bad_json_invalid_domain_values_and_disk_corruption(self):
        self.assertEqual(self.request('POST','/api/commands',b'{')[0],400)
        self.assertEqual(self.request('POST','/api/commands',{'expected_revision':0,'command':{'action':'create','card':card(cost=True)}})[0],422)
        self.request('POST','/api/commands',{'expected_revision':0,'command':{'action':'create','card':card()}})
        path=Path(self.temp.name)/'workspace.json';path.write_text('corrupt')
        result=self.request('POST','/api/commands',{'expected_revision':1,'command':{'action':'create','card':card()}})
        self.assertEqual(result[0],503);self.assertEqual(path.read_text(),'corrupt')
        self.assertNotIn(str(self.temp.name),json.dumps(result[1]))

    def test_analysis_and_preview_do_not_write_workspace(self):
        created=self.request('POST','/api/commands',{'expected_revision':0,'command':{'action':'create','card':card(song=True)}})[1]
        identifier=created['result']['id'];path=Path(self.temp.name)/'workspace.json';before=path.read_bytes()
        for route in ['/api/analyze','/api/preview']:
            status,result,_=self.request('POST',route,{'draft_id':identifier,'card':card(song=True)})
            self.assertEqual(status,200)
            self.assertTrue((result.get('analysis') or result)['suggestions'])
            self.assertEqual(path.read_bytes(),before)
        self.assertEqual(self.request('POST','/api/analyze',{'draft_id':'unknown','card':card()})[0],422)


if __name__=='__main__': unittest.main()
