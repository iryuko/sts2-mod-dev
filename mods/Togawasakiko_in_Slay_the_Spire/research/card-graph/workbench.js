(() => {
  'use strict';
  const $=id=>document.getElementById(id),api=window.WorkbenchApi,view=window.cardGraphView;
  const el=(tag,cls,text)=>{const n=document.createElement(tag);if(cls)n.className=cls;if(text!==undefined)n.textContent=text;return n;};
  const graphRoot=document.querySelector('main.workspace');let ready=false,analysis=null,reviewTab='suggested',previewing=false;
  const editor=DraftEditor.mount($('draft-editor'),{api,onPreview:analyze,onOpenCard:id=>{view.focusCard(id);show('graph');}});
  window.sakikoWorkbench={api,editor};
  function show(mode){graphRoot.hidden=mode!=='graph';$('draft-workspace').hidden=mode!=='drafts';$('graph-tab').setAttribute('aria-pressed',mode==='graph');$('draft-tab').setAttribute('aria-pressed',mode==='drafts');if(mode==='graph')view.cy.resize();}
  function failed(error){$('connection-error').textContent=error.message;$('recovery-tools').hidden=false;}
  const run=fn=>Promise.resolve().then(fn).catch(failed);
  function renderList(){
    const list=$('draft-list');list.replaceChildren();
    Object.values(api.workspace?.entries||{}).filter(e=>$('show-archived').checked||!e.archived).forEach(entry=>{
      const button=el('button','',entry.working.name||'未命名草案');button.type='button';button.setAttribute('aria-label',entry.working.name||'未命名草案');button.setAttribute('aria-current',entry.id===editor.id);
      button.append(el('small','',`${entry.archived?'已归档':entry.published?'已纳入提案':'草稿'} · ${entry.id.slice(-6)}`));
      button.onclick=()=>run(async()=>{await editor.open(entry);analysis=null;$('review-panel').hidden=true;$('draft-editor').hidden=false;renderList();renderActions();});list.append(button);
    });
    $('draft-count').textContent=`${Object.keys(api.workspace?.entries||{}).length} 张`;
  }
  async function create(card=DraftEditor.empty(),origin_id=null){await editor.flush();const result=await api.command({action:'create',card,origin_id});await editor.open(result.workspace.entries[result.result.id]);analysis=null;$('review-panel').hidden=true;$('draft-editor').hidden=false;show('drafts');renderList();renderActions();}
  $('new-draft').onclick=()=>run(()=>create());
  $('clone-card').onclick=()=>run(async()=>{
    await editor.flush();
    const identifier=!$('draft-workspace').hidden&&editor.id?editor.id:view.state.selected;
    const entry=api.workspace.entries[identifier];
    if(entry){await create(structuredClone(!$('draft-workspace').hidden?entry.working:entry.published?.card||entry.working),identifier);return;}
    const node=view.data.nodes.find(n=>n.id===identifier);if(!node)return;
    const card={...DraftEditor.empty(),name:node.name,cost:node.cost,upgraded_cost:node.upgraded_cost??null,type:node.type,rarity:node.rarity,source_pool:['main','token','event','relic'].includes(node.pool)?node.pool:'special',song:node.song,base:node.base,upgrade:node.upgrade,upgrade_mode:'delta',notes:(node.notes||[]).join('\n'),portrait_source_id:node.status==='implemented'?node.id:null,
      keywords:node.keywords||[],upgraded_keywords:node.upgraded_keywords||[],mechanics:node.mechanics||[]};
    await create(card,node.id);
  });
  $('graph-tab').onclick=()=>run(async()=>{await editor.flush();if(!previewing)view.replaceGraph(await api.graph());show('graph');});$('draft-tab').onclick=()=>show('drafts');$('show-archived').onchange=renderList;
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
  const actions=el('div','draft-actions');$('draft-editor').append(actions);
  function commandButton(label,action){const b=el('button','command-button',label);b.type='button';b.onclick=()=>run(action);return b;}
  function renderActions(){
    actions.replaceChildren();const entry=api.workspace?.entries[editor.id];if(!entry)return;
    if(entry.archived){actions.append(commandButton('恢复草案',async()=>{await api.command({action:'restore',id:entry.id});await editor.open(api.workspace.entries[entry.id]);renderActions();}));return;}
    actions.append(commandButton('归档草案',async()=>{
      if(!confirm('归档此草案并撤回已纳入快照？引用它的提案关系将需要复核。'))return;
      await editor.flush();await api.command({action:'archive',id:entry.id});await editor.open(api.workspace.entries[entry.id]);
      view.replaceGraph(await api.graph());$('review-panel').hidden=true;previewing=false;renderActions();
    }));
  }
  const panel=el('section','review-panel');panel.id='review-panel';panel.hidden=true;document.body.append(panel);
  const reviewHead=el('div','review-heading'),reviewTitle=el('h2'),reviewTabs=el('div','segmented'),reviewList=el('div','review-list'),limits=el('ul','analysis-limits');
  const publish=commandButton('纳入全局提案',publishDialog),withdraw=commandButton('撤回提案',async()=>{
    if(!confirm('撤回已纳入快照？草稿和审核记录仍保留。'))return;
    await editor.flush();await api.command({action:'withdraw',id:editor.id});view.replaceGraph(await api.graph());previewing=false;renderReviews();
  });
  const exitPreview=commandButton('返回全局快照',async()=>{view.replaceGraph(await api.graph());previewing=false;});
  reviewHead.append(reviewTitle,publish,withdraw,exitPreview);panel.append(reviewHead,reviewTabs,limits,reviewList);
  const manual=el('details','manual-relation');manual.append(el('summary','','手动添加关系'));
  const manualForm=el('form','manual-fields');manualForm.onsubmit=e=>e.preventDefault();manual.append(manualForm);panel.append(manual);
  const manualFields={};
  for(const [key,label,options] of [['other','关联卡牌',{}],['direction','方向',{out:'草案 → 关联卡',in:'关联卡 → 草案'}],['kind','关系类型',Object.fromEntries(RelationStyle.legend().map(s=>[s.kind,s.label]))],['mechanism','机制名称'],['condition','成立条件'],['reason','关系解释']]){
    const wrapper=el('label','field',label),input=el(options?'select':'textarea');input.setAttribute('aria-label',`手动关系 ${label}`);
    if(options)Object.entries(options).forEach(([value,name])=>{const opt=el('option','',name);opt.value=value;input.append(opt);});else{input.rows=2;input.maxLength=4000;}
    wrapper.append(input);manualForm.append(wrapper);manualFields[key]=input;
  }
  const selfLabel=el('label','check-label'),selfCheck=el('input');selfCheck.type='checkbox';selfLabel.append(selfCheck,document.createTextNode('已核对自环的不同实例及触发限制'));manualForm.append(selfLabel);
  manualForm.append(commandButton('保存人工关系',async()=>{
    await editor.flush();const latest=await api.analyze(editor.id,editor.card),other=manualFields.other.value;
    const relation={source:manualFields.direction.value==='out'?editor.id:other,target:manualFields.direction.value==='out'?other:editor.id,
      kind:manualFields.kind.value,mechanism:manualFields.mechanism.value,condition:manualFields.condition.value,reason:manualFields.reason.value,evidence:[],self_reviewed:selfCheck.checked};
    await review(relation,'accepted',latest);
  }));
  async function analyze(id,card){
    const result=await api.preview(id,card);analysis=result.analysis;previewing=true;
    view.replaceGraph(result.graph);view.focusCard(id);show('graph');reviewTab='suggested';renderReviews();
    panel.scrollIntoView({block:'start',behavior:'instant'});
  }
  function referenceIndex(){
    const map=new Map(view.data.nodes.map(n=>[n.id,n]));
    Object.values(api.workspace.entries).forEach(e=>map.set(e.id,{...e.working,id:e.id,name:e.working.name||'未命名草案'}));return map;
  }
  function current(review){return review.profile_revision===analysis.profile_revision&&[review.source,review.target].every(id=>analysis.endpoint_hashes[id]?analysis.endpoint_hashes[id]===review.endpoint_hashes[id]:review.decision==='rejected'&&review.endpoint_hashes[id]===null);}
  async function review(relation,decision,latest=analysis){
    await editor.flush();
    const allowed=['id','source','target','kind','mechanism','condition','reason','evidence','rule_id','self_reviewed'];
    const clean=Object.fromEntries(allowed.filter(k=>Object.hasOwn(relation,k)).map(k=>[k,relation[k]]));
    await api.command({action:'review',id:editor.id,relation:clean,decision,
      expected_endpoint_hashes:Object.fromEntries([clean.source,clean.target].map(id=>[id,latest.endpoint_hashes[id]]))});
    if(previewing){const result=await api.preview(editor.id,editor.card);analysis=result.analysis;view.replaceGraph(result.graph);}
    else analysis=await api.analyze(editor.id,editor.card);
    renderReviews();
  }
  function renderReviews(){
    const entry=api.workspace?.entries[editor.id];if(!entry||!analysis)return;
    panel.hidden=false;reviewTitle.textContent=`${entry.working.name||'未命名草案'} · 关系审核`;
    withdraw.disabled=!entry.published;publish.disabled=entry.archived;
    const reviews=Object.values(entry.reviews),groups={
      suggested:analysis.suggestions.filter(r=>!entry.reviews[r.id]),accepted:reviews.filter(r=>r.decision==='accepted'&&current(r)),
      stale:reviews.filter(r=>!current(r)),rejected:reviews.filter(r=>r.decision==='rejected'&&current(r))};
    reviewTabs.replaceChildren();
    for(const [id,name] of [['suggested','建议'],['accepted','已确认'],['stale','待复核'],['rejected','已拒绝']]){
      const button=el('button','',`${name} ${groups[id].length}`);button.type='button';button.setAttribute('aria-pressed',reviewTab===id);button.onclick=()=>{reviewTab=id;renderReviews();};reviewTabs.append(button);
    }
    limits.replaceChildren();analysis.limitations.forEach(text=>limits.append(el('li','',text)));
    const index=referenceIndex();reviewList.replaceChildren();
    for(const relation of groups[reviewTab]){
      const row=el('details','review-row'),summary=el('summary'),names=el('span');
      CardReferences.render(names,CardReferences.tokenize(`[[card:${relation.source}]] → [[card:${relation.target}]]`,index,{}),id=>{view.focusCard(id);show('graph');});
      summary.append(RelationStyle.sample(relation.kind),names,el('span','review-kind',RelationStyle.legend().find(s=>s.kind===relation.kind).label));row.append(summary);
      const conditionLabel=el('label','field','成立条件'),condition=el('textarea');condition.value=relation.condition;condition.rows=3;condition.maxLength=4000;conditionLabel.append(condition);
      const reasonLabel=el('label','field','关系解释'),reason=el('textarea');reason.value=relation.reason;reason.rows=2;reason.maxLength=4000;reasonLabel.append(reason);
      row.append(conditionLabel,reasonLabel);
      const explain=el('p','review-explanation');CardReferences.render(explain,CardReferences.tokenize(relation.reason,index,CardAliases),id=>{view.focusCard(id);show('graph');});row.append(explain);
      for(const ref of relation.evidence||[]){const source=view.data.evidence[ref];if(source?.url){const a=el('a','source-link',ref);a.href=source.url;a.target='_blank';a.rel='noreferrer';row.append(a);}}
      const commands=el('div','review-commands');
      commands.append(commandButton('确认关系',()=>review({...relation,condition:condition.value,reason:reason.value},'accepted')),
        commandButton('拒绝关系',()=>review({...relation,condition:condition.value,reason:reason.value},'rejected')));row.append(commands);reviewList.append(row);
    }
    if(!groups[reviewTab].length)reviewList.append(el('p','empty','此分组暂无关系'));
    const options=manualFields.other,selected=options.value;options.replaceChildren();
    for(const node of view.data.nodes){if(node.id.startsWith('draft:')&&node.id!==editor.id&&!api.workspace.entries[node.id]?.published)continue;
      const option=el('option','',`${node.name} · ${node.id}`);option.value=node.id;options.append(option);}
    if([...options.options].some(o=>o.value===selected))options.value=selected;
  }
  async function publishDialog(){
    await editor.flush();analysis=await api.analyze(editor.id,editor.card);renderReviews();
    const entry=api.workspace.entries[editor.id],old=entry.published?.card||{},changed=Object.keys(entry.working).filter(key=>JSON.stringify(old[key])!==JSON.stringify(entry.working[key]));
    const dialog=el('dialog','publish-dialog'),head=el('h2','','纳入全局提案'),list=el('div','publish-diff');
    const labels={name:'卡牌名称',cost:'费用',upgraded_cost:'升级费用',type:'类型',rarity:'品质',source_pool:'来源',song:'Song',keywords:'基础词条',upgraded_keywords:'升级词条',base:'基础效果',upgrade:'升级效果',upgrade_mode:'升级模式',notes:'设计备注',image_id:'卡图',portrait_source_id:'原卡卡图',mechanics:'显式机制'};
    for(const key of changed){const section=el('section');section.append(el('h3','',labels[key]),el('p','diff-before',`原：${JSON.stringify(old[key]??null)}`),el('p','',`新：${JSON.stringify(entry.working[key])}`));list.append(section);}
    const accepted=Object.values(entry.reviews).filter(r=>r.decision==='accepted');
    list.append(el('p','',`已确认关系 ${accepted.length} 条${accepted.length?'':' · 无已确认关系'}`));
    const stale=accepted.filter(r=>!current(r));if(stale.length)list.append(el('p','editor-error',`${stale.length} 条关系待复核，确认或拒绝后才能纳入。`));
    const confirmButton=commandButton('确认纳入',async()=>{
      await api.command({action:'publish',id:editor.id});view.replaceGraph(await api.graph());previewing=false;dialog.close();dialog.remove();renderReviews();renderList();
    });confirmButton.disabled=stale.length>0;
    const cancel=commandButton('取消',()=>{dialog.close();dialog.remove();});const footer=el('footer');footer.append(cancel,confirmButton);
    dialog.append(head,list,footer);dialog.addEventListener('cancel',()=>dialog.remove());document.body.append(dialog);dialog.showModal();
  }
  enable();
  if(location.protocol==='file:'){$('connection-status').textContent='只读文件模式';return;}
  run(async()=>{const result=await api.connect();view.replaceGraph(result.graph);ready=true;enable();renderList();});
})();
