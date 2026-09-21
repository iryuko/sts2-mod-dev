(() => {
  'use strict';
  const $=id=>document.getElementById(id),api=window.WorkbenchApi,view=window.cardGraphView;
  const el=(tag,cls,text)=>{const n=document.createElement(tag);if(cls)n.className=cls;if(text!==undefined)n.textContent=text;return n;};
  const graphRoot=document.querySelector('main.workspace');let ready=false;
  const editor=DraftEditor.mount($('draft-editor'),{api,onPreview:async(id,card)=>{
    const result=await api.preview(id,card);view.replaceGraph(result.graph);view.focusCard(id);show('graph');
  },onOpenCard:id=>view.focusCard(id)});
  window.sakikoWorkbench={api,editor};
  function show(mode){graphRoot.hidden=mode!=='graph';$('draft-workspace').hidden=mode!=='drafts';$('graph-tab').setAttribute('aria-pressed',mode==='graph');$('draft-tab').setAttribute('aria-pressed',mode==='drafts');if(mode==='graph')view.cy.resize();}
  function failed(error){$('connection-error').textContent=error.message;$('recovery-tools').hidden=false;}
  const run=fn=>Promise.resolve().then(fn).catch(failed);
  function renderList(){
    const list=$('draft-list');list.replaceChildren();
    Object.values(api.workspace?.entries||{}).filter(e=>$('show-archived').checked||!e.archived).forEach(entry=>{
      const button=el('button','',entry.working.name||'未命名草案');button.type='button';button.setAttribute('aria-label',entry.working.name||'未命名草案');button.setAttribute('aria-current',entry.id===editor.id);
      button.append(el('small','',entry.archived?'已归档':entry.published?'已纳入提案':'草稿'));
      button.onclick=()=>run(async()=>{await editor.open(entry);$('draft-editor').hidden=false;renderList();});list.append(button);
    });
    $('draft-count').textContent=`${Object.keys(api.workspace?.entries||{}).length} 张`;
  }
  async function create(card=DraftEditor.empty(),origin_id=null){await editor.flush();const result=await api.command({action:'create',card,origin_id});await editor.open(result.workspace.entries[result.result.id]);$('draft-editor').hidden=false;show('drafts');renderList();}
  $('new-draft').onclick=()=>run(()=>create());
  $('clone-card').onclick=()=>run(async()=>{
    const node=view.data.nodes.find(n=>n.id===view.state.selected);if(!node)return;
    const card={...DraftEditor.empty(),name:node.name,cost:node.cost,type:node.type,rarity:node.rarity,source_pool:['main','token','event','relic'].includes(node.pool)?node.pool:'special',song:node.song,base:node.base,upgrade:node.upgrade,upgrade_mode:'delta',notes:(node.notes||[]).join('\n'),portrait_source_id:node.status==='implemented'?node.id:null};
    await create(card,node.id);
  });
  $('graph-tab').onclick=()=>run(async()=>{await editor.flush();show('graph');});$('draft-tab').onclick=()=>show('drafts');$('show-archived').onchange=renderList;
  $('download-local').onclick=()=>editor.download();
  $('reconnect').onclick=()=>run(async()=>{const result=await api.connect();ready=true;enable();await editor.flush();view.replaceGraph(result.graph);$('connection-error').textContent='';$('recovery-tools').hidden=true;});
  $('reload-workspace').onclick=()=>run(async()=>{
    if(!confirm('丢弃本页未保存内容并重新加载？可先下载本页草稿。'))return;
    editor.discard();const result=await api.connect({reload:true});view.replaceGraph(result.graph);$('draft-editor').hidden=true;$('connection-error').textContent='';$('recovery-tools').hidden=true;renderList();
  });
  function enable(){for(const id of ['new-draft','clone-card','draft-tab'])$(id).disabled=!ready;}
  api.subscribe(({status,blocked})=>{$('connection-status').textContent=status;if(blocked)$('recovery-tools').hidden=false;renderList();});
  view.setReferenceResolver(id=>{const entry=api.workspace?.entries[id];if(!entry)return null;const card=entry.working;return {...card,portrait:card.image_id?`/api/images/${card.image_id}`:card.portrait_source_id?`/api/portraits/${card.portrait_source_id}`:null};});
  window.addEventListener('card-reference-open',event=>run(async()=>{const entry=api.workspace?.entries[event.detail.id];if(entry){await editor.open(entry);$('draft-editor').hidden=false;show('drafts');}}));
  enable();
  if(location.protocol==='file:'){$('connection-status').textContent='只读文件模式';return;}
  run(async()=>{const result=await api.connect();view.replaceGraph(result.graph);ready=true;enable();renderList();});
})();
