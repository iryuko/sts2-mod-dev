/* global cytoscape, lucide */
(() => {
  "use strict";
  let graph = window.CARD_GRAPH;
  if(location.protocol!=='file:'&&graph){graph=structuredClone(graph);graph.nodes.forEach(n=>{if(n.portrait)n.portrait='/api/portraits/'+encodeURIComponent(n.id);});}
  const $ = id => document.getElementById(id);
  if (!graph || typeof cytoscape !== "function") {
    $("error").hidden = false;
    $("error").textContent = "图谱数据或本地绘图库未能加载。";
    return;
  }
  const nodes = new Map(graph.nodes.map(node => [node.id, node]));
  const kindNames = {supply:"资源供给",payoff:"效果收益",sequence:"牌序编排",replay:"重复打出",generation:"生成候选",tradeoff:"资源取舍",conflict:"流程冲突"};
  const typeNames = {Attack:"攻击",Skill:"技能",Power:"能力",Curse:"诅咒"};
  const rarityNames = {Basic:"基础",Common:"普通",Uncommon:"非凡",Rare:"稀有",Ancient:"Ancient",Token:"衍生",Event:"事件"};
  const poolNames = {main:"主池",token:"压力衍生",event:"事件",relic:"遗物授予",proposal:"未批准提案"};
  const colors = {Attack:"#b94c54",Skill:"#267f77",Power:"#a58324",Curse:"#756378"};
  const state = {selected:"Completeness",mode:"focus",kind:"all",direction:"both",showProposals:true,search:"",type:"all",pool:"all",edge:null};
  let visibleEdges = [], layout;
  const el = (tag, cls, text) => { const item=document.createElement(tag); if(cls)item.className=cls; if(text!==undefined)item.textContent=text; return item; };
  const icons = () => { if(window.lucide)lucide.createIcons({attrs:{width:17,height:17,"aria-hidden":"true"}}); };
  const negative = edge => ["tradeoff","conflict"].includes(edge.kind);
  const label = node => node.name.length>22 ? node.name.slice(0,20)+"…" : node.name;
  const cy = cytoscape({container:$("graph"),elements:[],minZoom:0.15,maxZoom:3,boxSelectionEnabled:false,
    style:[
      {selector:"node",style:{"label":"data(label)","background-color":"data(color)","width":28,"height":28,"font-size":12,"font-family":"PingFang SC, sans-serif","color":"#303a37","text-valign":"bottom","text-margin-y":6,"text-wrap":"wrap","text-max-width":100,"text-background-color":"#fbfcfb","text-background-opacity":0.88,"text-background-padding":2,"border-width":0}},
      {selector:"node.selected",style:{"width":38,"height":38,"border-width":3,"border-color":"#263f37","font-weight":600}},
      {selector:"node.proposal",style:{"background-opacity":0.2,"border-width":2,"border-style":"dashed","border-color":"#737672"}},
      {selector:"node.overview",style:{"label":""}},
      {selector:"node.overview.selected, node.overview.hovered",style:{"label":"data(label)"}},
      {selector:"edge",style:{"width":1.6,"line-color":"data(color)","target-arrow-color":"data(color)","target-arrow-shape":"data(arrowShape)","line-style":"data(lineStyle)","line-dash-pattern":e=>e.data('dashPattern'),"arrow-scale":1,"curve-style":"bezier","opacity":0.68}},
      {selector:"edge.proposal",style:{"opacity":0.55}},
      {selector:"edge.suggested",style:{"opacity":0.3}},
      {selector:"edge.highlight",style:{"width":3,"opacity":1,"z-index":10}},
      {selector:".dim",style:{"opacity":0.15}}
    ]});
  const history=[];
  let resolveExtra=()=>null;
  function capture(){return {state:{...state},nodePositions:Object.fromEntries(cy.nodes().map(n=>[n.id(),{...n.position()}])),zoom:cy.zoom(),pan:{...cy.pan()}};}
  function restore(snapshot){
    Object.assign(state,snapshot.state);
    if(!nodes.has(state.selected)){state.selected='Completeness';state.edge=null;$('error').hidden=false;$('error').textContent='原卡牌已不可用，已返回完美无缺。';}
    const edge=state.edge;syncControls();renderList();renderGraph();
    cy.nodes().forEach(n=>{if(snapshot.nodePositions[n.id()])n.position(snapshot.nodePositions[n.id()]);});
    cy.zoom(snapshot.zoom);cy.pan(snapshot.pan);
    const relation=graph.edges.find(e=>e.id===edge);if(relation)selectEdge(relation);
  }
  function syncControls(){
    $('search').value=state.search;$('type-filter').value=state.type;$('pool-filter').value=state.pool;
    $('proposal-filter').checked=state.showProposals;$('relation-filter').value=state.kind;$('direction-filter').value=state.direction;
  }
  function navigate(id){
    if(!nodes.has(id)){window.dispatchEvent(new CustomEvent('card-reference-open',{detail:{id}}));return;}
    history.push(capture());state.showProposals=true;state.kind='all';state.direction='both';syncControls();selectNode(id);
  }
  function historyButton(box){if(history.length){const back=el('button','back-button','返回上个视图');back.type='button';back.onclick=()=>restore(history.pop());box.prepend(back);}}
  window.cardGraphView = {cy,state,get data(){return graph;},capture,restore,focusCard:navigate,
    selectEdge:id=>{const edge=graph.edges.find(e=>e.id===id);if(edge)selectEdge(edge);},
    setReferenceResolver:resolver=>{resolveExtra=resolver;},
    replaceGraph(next,{preserveView=true}={}){const previous=capture();graph=next;nodes.clear();graph.nodes.forEach(n=>nodes.set(n.id,n));
      $('total-count').textContent=`${graph.counts.implemented} 已实现 · ${graph.counts.proposals} 提案`;
      if(preserveView)restore(previous);else{if(!nodes.has(state.selected))state.selected='Completeness';renderList();renderGraph();}
    }};

  function baseNodes() { return graph.nodes.filter(n => state.showProposals || n.status!=="proposal"); }
  function filteredEdges() {
    return graph.edges.filter(e => (state.showProposals || e.status!=="proposal") && (state.kind==="all" || e.kind===state.kind));
  }
  function incident(edges) {
    return edges.filter(e => state.direction==="in" ? e.target===state.selected : state.direction==="out" ? e.source===state.selected : e.source===state.selected || e.target===state.selected);
  }
  function renderGraph() {
    if(layout)layout.stop();
    state.edge=null;
    visibleEdges = state.mode==="focus" ? incident(filteredEdges()) : filteredEdges();
    const ids = state.mode==="focus" ? new Set([state.selected,...visibleEdges.flatMap(e=>[e.source,e.target])]) : new Set(baseNodes().map(n=>n.id));
    const shown = baseNodes().filter(n=>ids.has(n.id));
    cy.batch(() => {
      cy.elements().remove();
      cy.add(shown.map(n=>({group:"nodes",data:{id:n.id,label:label(n),color:colors[n.type]||"#737672"},classes:[n.id===state.selected?"selected":"",n.status==="proposal"?"proposal":"",state.mode==="global"?"overview":""].join(" ")})));
      cy.add(visibleEdges.filter(e=>ids.has(e.source)&&ids.has(e.target)).map(e=>({group:"edges",data:{id:e.id,source:e.source,target:e.target,kind:e.kind,...RelationStyle.styles(e.kind)},classes:[negative(e)?"negative":"",e.status==="proposal"?"proposal":"",e.suggested?'suggested':''].join(" ")})));
    });
    layout=cy.layout(state.mode==="focus"
      ? {name:"concentric",animate:false,fit:true,padding:32,nodeDimensionsIncludeLabels:true,avoidOverlap:true,minNodeSpacing:18,concentric:node=>node.id()===state.selected?2:1,levelWidth:()=>1}
      : {name:"cose",animate:false,randomize:true,fit:true,padding:40,nodeDimensionsIncludeLabels:true,nodeRepulsion:()=>140000,idealEdgeLength:()=>130,componentSpacing:80,numIter:650});
    layout.run();
    $("graph-title").textContent = state.mode==="focus" ? nodes.get(state.selected).name : "全卡池关系";
    $("visible-count").textContent = `${shown.length} 节点 · ${visibleEdges.length} 条件边`;
    $("focus-mode").setAttribute("aria-pressed",state.mode==="focus");
    $("global-mode").setAttribute("aria-pressed",state.mode==="global");
    $("direction-filter").disabled=state.mode==="global";
    document.querySelectorAll('#relation-legend button').forEach(b=>b.setAttribute('aria-pressed',b.dataset.kind===state.kind));
    renderDetail();
  }
  function renderList() {
    const result=baseNodes().filter(n=>(state.type==="all"||n.type===state.type)&&(state.pool==="all"||n.pool===state.pool)&&`${n.name} ${n.id}`.toLowerCase().includes(state.search.toLowerCase()));
    const list=$("card-list"); list.replaceChildren();
    result.sort((a,b)=>(a.status==="proposal")-(b.status==="proposal") || a.name.localeCompare(b.name,"zh-CN"));
    result.forEach(n=>{
      const button=el("button",`card-item ${n.id===state.selected?"selected":""}`); button.type="button"; button.setAttribute("aria-label",n.name); button.setAttribute("aria-current",n.id===state.selected?"true":"false");
      const dot=el("span",`dot ${n.type} ${n.status==="proposal"?"proposal":""}`);
      const title=el("span"); title.append(el("span","card-name",n.name),el("span","card-meta",`${typeNames[n.type]} · ${poolNames[n.pool]}${n.song?" · Song":""}`));
      button.append(dot,title,el("span","card-cost",`${n.cost}费`)); button.onclick=()=>selectNode(n.id); list.append(button);
    });
    if(!result.length)list.append(el("p","empty","没有匹配的卡牌"));
    $("list-count").textContent=`${result.length} 张`;
  }
  function section(title,text) { const box=el("section","effect-section"),p=el('p');CardReferences.render(p,CardReferences.tokenize(text||'',nodes,CardAliases),navigate);box.append(el("h3","",title),p);return box; }
  function evidenceLinks(container,references) {
    [...new Set(references)].forEach(reference=>{
      const entry=graph.evidence[reference];
      if(!entry||entry.proposal)return;
      const a=el("a","source-link",`${entry.path.split("/").pop()}:${entry.line}`);a.href=entry.url;a.target="_blank";a.rel="noreferrer";container.append(a);
    });
  }
  function renderDetail() {
    const node=nodes.get(state.selected), box=$("detail-content");box.replaceChildren();
    const heading=el("div","detail-heading");
    if(node.portrait){const img=el("img","portrait");img.src=node.portrait;img.alt=`${node.name}卡图`;heading.append(img);}
    const title=el("div");title.append(el("h2","",node.name),el("p","detail-meta",`${node.cost}费 · ${typeNames[node.type]} · ${rarityNames[node.rarity]}${node.song?" · Song":""}`));
    if(node.status==="proposal")title.append(el("span","status-note","未批准 / 未实现 / 数值待测"));
    heading.append(title);box.append(heading,section("基础效果",node.base),section("升级改动",node.upgrade));
    if(node.notes?.length){const notes=el("ul","notes");node.notes.forEach(note=>notes.append(el("li","",note)));box.append(notes);}
    if(node.source)evidenceLinks(box,[`${node.source}#${node.id}`]);
    const edges=incident(filteredEdges()).sort((a,b)=>Number(negative(b))-Number(negative(a))||a.mechanism.localeCompare(b.mechanism));
    const heading2=el("div","relations-heading");heading2.append(el("h3","","条件关系"),el("span","",`${edges.length} 条`));box.append(heading2);
    edges.forEach(edge=>{
      const outgoing=edge.source===node.id,other=nodes.get(outgoing?edge.target:edge.source),button=el("button",`relation-row ${negative(edge)?"negative":""}`);button.type="button";
      const row=el("span","relation-title");row.append(RelationStyle.sample(edge.kind),el("span","sign",outgoing?"→":"←"),el("span","",other.name));
      button.append(row,el("span","relation-type",`${kindNames[edge.kind]} · ${edge.mechanism}${edge.status==="proposal"?" · 提案":""}`));button.onclick=()=>selectEdge(edge);box.append(button);
    });
    if(!edges.length)box.append(el("p","empty","该筛选下无已记录的直接关系。"));
    historyButton(box);icons();
  }
  function selectNode(id) {
    state.selected=id;renderList();renderGraph();
  }
  function selectEdge(edge) {
    state.edge=edge.id;
    cy.elements().removeClass("highlight dim");
    const selected=cy.getElementById(edge.id);selected.addClass("highlight");
    cy.elements().not(selected.union(selected.connectedNodes())).addClass('dim');
    const box=$("detail-content");box.replaceChildren();
    const back=el("button","back-button","返回卡牌");back.type="button";back.onclick=()=>{state.edge=null;cy.elements().removeClass("highlight dim");renderDetail();};box.append(back);
    const heading=el("h2","edge-heading");
    [edge.source,edge.target].forEach((id,i)=>{if(i)heading.append(el("span",""," → "));const button=el("button","card-reference",nodes.get(id).name);button.type="button";button.dataset.cardId=id;button.onclick=()=>navigate(id);heading.append(button);});
    box.append(heading,el("p",`badge ${negative(edge)?"negative":""}`,`${kindNames[edge.kind]} · ${edge.mechanism}`));
    if(edge.status==="proposal")box.append(el("p","status-note","涉及未实现提案，不代表当前游戏效果"));
    box.append(section("成立条件",edge.condition),section("关系解释",edge.reason));
    const evidence=el("section","effect-section");evidence.append(el("h3","","源码依据 · 0.2.3"));evidenceLinks(evidence,edge.evidence);box.append(evidence);
    historyButton(box);
  }
  cy.on("tap","node",event=>selectNode(event.target.id()));
  cy.on("mouseover","node",event=>event.target.addClass("hovered"));
  cy.on("mouseout","node",event=>event.target.removeClass("hovered"));
  cy.on("tap","edge",event=>{const edge=graph.edges.find(e=>e.id===event.target.id());if(edge)selectEdge(edge);});
  $("search").oninput=event=>{state.search=event.target.value;renderList();};
  $("type-filter").onchange=event=>{state.type=event.target.value;renderList();};
  $("pool-filter").onchange=event=>{state.pool=event.target.value;renderList();};
  $("proposal-filter").onchange=event=>{state.showProposals=event.target.checked;if(!state.showProposals&&nodes.get(state.selected).status==="proposal")state.selected="Completeness";renderList();renderGraph();};
  $("relation-filter").onchange=event=>{state.kind=event.target.value;renderGraph();};
  $("direction-filter").onchange=event=>{state.direction=event.target.value;renderGraph();};
  $("focus-mode").onclick=()=>{state.mode="focus";renderGraph();};
  $("global-mode").onclick=()=>{state.mode="global";renderGraph();};
  $("zoom-in").onclick=()=>cy.zoom({level:Math.min(3,cy.zoom()*1.3),renderedPosition:{x:cy.width()/2,y:cy.height()/2}});
  $("zoom-out").onclick=()=>cy.zoom({level:Math.max(0.15,cy.zoom()/1.3),renderedPosition:{x:cy.width()/2,y:cy.height()/2}});
  $("fit").onclick=()=>cy.fit(undefined,40);
  $("export").onclick=()=>{
    const ids=new Set(cy.nodes().map(n=>n.id()));
    const payload={meta:graph.meta,nodes:graph.nodes.filter(n=>ids.has(n.id)),edges:visibleEdges,evidence:graph.evidence};
    const url=URL.createObjectURL(new Blob([JSON.stringify(payload,null,2)],{type:"application/json"}));
    const link=document.createElement("a");link.href=url;link.download="sakiko-card-subgraph.json";link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
  };
  new ResizeObserver(()=>{cy.resize();}).observe($("graph"));
  $("version").textContent=`v${graph.meta.release}`;
  RelationStyle.legend().forEach(s=>{
    const button=el('button','legend-button');button.type='button';button.dataset.kind=s.kind;
    button.append(RelationStyle.sample(s.kind),document.createTextNode(s.label));
    button.onclick=()=>{state.kind=state.kind===s.kind?'all':s.kind; $('relation-filter').value=state.kind;renderGraph();};
    $('relation-legend').append(button);
  });
  $("total-count").textContent=`${graph.counts.implemented} 已实现 · ${graph.counts.proposals} 提案`;
  renderList();renderGraph();icons();
  CardReferences.bindPreview(document.body,id=>nodes.get(id)||resolveExtra(id),navigate);
})();
