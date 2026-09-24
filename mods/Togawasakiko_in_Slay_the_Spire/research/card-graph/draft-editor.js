(() => {
  'use strict';
  const empty=()=>({name:'',cost:null,upgraded_cost:null,type:null,rarity:null,source_pool:null,song:false,
    keywords:[],upgraded_keywords:[],base:'',upgrade:'',upgrade_mode:'unchanged',notes:'',image_id:null,portrait_source_id:null,mechanics:[]});
  const resources={pressure:'压力',block:'格挡',hp_loss:'失去生命',heal:'治疗',strength:'力量',dexterity:'敏捷',inferiority:'自卑',despair_echo:'绝望回响',damage_received:'正伤害事件',pressure_token:'压力衍生牌',card_discarded:'弃牌事件',card_exhausted:'消耗事件',token_played:'衍生牌出牌',song_played:'歌曲出牌',attack_played:'攻击出牌',card_type_order:'类型交错',song_pool_access:'歌曲池',exhaust_pile_access:'消耗堆',discard_pile_access:'弃牌堆',draw_pile_access:'抽牌堆'};
  const timings={on_play:'出牌结算',after_play:'出牌之后',after_draw:'抽牌后',enemy_hit:'怪物命中时',own_turn_start:'自己回合开始',own_turn_end:'自己回合结束',enemy_turn_start:'敌方回合开始',continuous:'持续'};
  const el=(tag,cls,text)=>{const node=document.createElement(tag);if(cls)node.className=cls;if(text!==undefined)node.textContent=text;return node;};
  function download(value,name){const url=URL.createObjectURL(new Blob([JSON.stringify(value,null,2)],{type:'application/json'}));const a=el('a');a.href=url;a.download=name;a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);}
  function mount(root,{api,onPreview,onOpenCard}){
    let entry=null,working=empty(),seq=0,saved=0,timer,flushing=null,uploadTask=null,upgraded=false;
    const fields={},mechanics=el('div','mechanic-list'),preview=el('aside','draft-preview');preview.id='draft-preview';
    const form=el('form','draft-form');form.onsubmit=e=>e.preventDefault();
    const status=el('span','save-status','已保存');status.id='save-status';status.setAttribute('role','status');
    const toolbar=el('div','draft-toolbar');const title=el('h2','','未选择草案');toolbar.append(title,status);
    const save=el('button','command-button','保存草稿');save.type='button';save.onclick=()=>run(flush);
    const analyze=el('button','command-button primary','试放并分析');analyze.type='button';analyze.onclick=()=>run(async()=>{await flush();await onPreview(entry.id,structuredClone(working));});
    toolbar.append(save,analyze);root.append(toolbar);
    const layout=el('div','draft-layout');layout.append(form,preview);root.append(layout);
    const fieldsArea=el('div','draft-fields');form.append(fieldsArea);
    function field(label,key,type='text',options=null){
      const wrapper=el('label',type==='textarea'?'field wide':'field',label),input=el(options?'select':type==='textarea'?'textarea':'input');
      input.setAttribute('aria-label',label);input.name=key;
      if(options){Object.entries(options).forEach(([value,name])=>{const option=el('option','',name);option.value=value;input.append(option);});}
      else if(type!=='textarea')input.type=type;
      if(type==='textarea'){input.rows=key==='notes'?3:5;input.maxLength=key==='notes'?12000:8000;}
      if(key==='name')input.maxLength=200;
      if(key.includes('cost')){input.inputMode='text';input.placeholder=key==='cost'?'0–99 / X':'不变';}
      wrapper.append(input);fieldsArea.append(wrapper);fields[key]=input;
      input.addEventListener('input',change);
      if(type==='textarea')bindReferences(input);
      return input;
    }
    field('卡牌名称','name');field('基础费用','cost');field('升级费用','upgraded_cost');
    field('卡牌类型','type','text',{'':'未定',Attack:'攻击',Skill:'技能',Power:'能力',Curse:'诅咒'});
    field('品质','rarity','text',{'':'未定',Basic:'基础',Common:'普通',Uncommon:'非凡',Rare:'稀有',Ancient:'远古',Token:'衍生',Event:'事件'});
    field('卡牌来源','source_pool','text',{'':'未定',main:'主卡池',token:'压力衍生',event:'事件',relic:'遗物',special:'其他特殊来源'});
    field('Song','song','checkbox');
    field('升级效果模式','upgrade_mode','text',{unchanged:'效果不变',delta:'升级差异',full:'完整升级效果'});
    field('基础关键词','keywords');field('升级关键词','upgraded_keywords');
    for(const key of ['keywords','upgraded_keywords']){
      const choices=el('div','keyword-choices');
      for(const name of ['Exhaust','Ethereal','Retain','Innate','Unplayable']){
        const label=el('label','check-label'),input=el('input');input.type='checkbox';input.dataset.keyword=name;input.dataset.key=key;input.setAttribute('aria-label',`${key==='keywords'?'基础':'升级'} ${name}`);
        input.onchange=()=>{const values=new Set(splitKeywords(fields[key].value));input.checked?values.add(name):values.delete(name);fields[key].value=[...values].join(', ');change();};
        label.append(input,document.createTextNode(name));choices.append(label);
      }
      fields[key].parentElement.append(choices);
    }
    const keywordRow=el('div','keyword-fields');keywordRow.append(fields.keywords.parentElement,fields.upgraded_keywords.parentElement);fieldsArea.append(keywordRow);
    field('基础效果','base','textarea');field('升级效果','upgrade','textarea');field('设计备注','notes','textarea');
    const imageLabel=el('label','field wide','卡图'),upload=el('input');upload.type='file';upload.accept='image/png,image/jpeg,image/webp';upload.setAttribute('aria-label','上传卡图');
    const remove=el('button','command-button','移除卡图');remove.type='button';remove.onclick=()=>{working.image_id=null;working.portrait_source_id=null;mark();};
    upload.onchange=()=>run(async()=>{
      if(uploadTask)await uploadTask;
      const file=upload.files[0];if(!file)return;if(file.size>10*1024*1024)throw Error('图片不能超过10MiB');
      uploadTask=(async()=>{await flush();const result=await api.upload(file);working.image_id=result.image_id;working.portrait_source_id=null;mark();await flush();upload.value='';})();
      try{await uploadTask;}finally{uploadTask=null;}
    });
    imageLabel.append(upload,remove);fieldsArea.append(imageLabel);
    const mechanicsHead=el('div','section-heading');mechanicsHead.append(el('h3','','显式机制'));
    const add=el('button','command-button','添加机制');add.type='button';add.onclick=()=>{working.mechanics.push({action:'produce',resource:'pressure',scope:'enemy',timing:'on_play',variant:'both',amount:'',limit:null,condition:'无额外条件',evidence:[]});renderMechanics();mark();};
    mechanicsHead.append(add);form.append(mechanicsHead,mechanics);
    const message=el('p','editor-error');message.setAttribute('role','alert');root.append(message);
    function run(fn){return Promise.resolve().then(fn).catch(error=>{message.textContent=error.message;status.textContent='未保存';});}
    function splitKeywords(value){return [...new Set(value.split(/[,，\n]/).map(x=>x.trim()).filter(Boolean))];}
    function cost(value){if(!value.trim())return null;if(value.trim().toUpperCase()==='X')return 'X';if(!/^\d{1,2}$/.test(value.trim()))throw Error('费用应为0至99或X');return Number(value);}
    function readFields(){
      const next={...working};
      for(const [key,input] of Object.entries(fields)){
        next[key]=key.includes('cost')?cost(input.value):key==='song'?input.checked:key.includes('keywords')?splitKeywords(input.value):['type','rarity','source_pool'].includes(key)?input.value||null:input.value;
      }
      working=next;
      root.querySelectorAll('[data-keyword]').forEach(input=>{input.checked=working[input.dataset.key].includes(input.dataset.keyword);});
    }
    function change(){seq++;clearTimeout(timer);status.textContent='未保存';try{readFields();message.textContent='';renderPreview();timer=setTimeout(()=>run(flush),700);}catch(error){message.textContent=error.message;}}
    function mark(){seq++;status.textContent='未保存';renderPreview();clearTimeout(timer);timer=setTimeout(()=>run(flush),700);}
    async function flush(){
      clearTimeout(timer);if(flushing){await flushing;if(seq!==saved)return flush();return;}
      if(!entry||seq===saved)return;
      readFields();
      flushing=(async()=>{while(seq!==saved){readFields();const version=seq;status.textContent='保存中';await api.command({action:'update',id:entry.id,card:structuredClone(working)});saved=version;}
        status.textContent='已保存';message.textContent='';})();
      try{await flushing;}finally{flushing=null;}
    }
    function renderMechanics(){
      mechanics.replaceChildren();
      working.mechanics.forEach((fact,index)=>{
        const row=el('fieldset','mechanic-row');row.append(el('legend','',`机制 ${index+1}`));
        const menus={action:{produce:'产生',consume:'消耗',read:'读取',trigger:'触发',generate:'生成',replay:'重放',sequence:'序列'},resource:resources,
          scope:{self:'自身',enemy:'单一敌人',all_enemies:'全体敌人',ally:'队友'},timing:timings,variant:{both:'基础和升级',base:'仅基础',upgraded:'仅升级'}};
        const labels={action:'动作',resource:'资源',scope:'对象',timing:'时点',variant:'版本',amount:'数量',limit:'次数上限',condition:'成立条件'};
        for(const key of ['action','resource','scope','timing','variant','amount','limit','condition']){
          const label=el('label','field',labels[key]),input=el(menus[key]?'select':'input');input.setAttribute('aria-label',`机制${index+1} ${labels[key]}`);
          if(menus[key])Object.entries(menus[key]).forEach(([value,name])=>{const opt=el('option','',name);opt.value=value;input.append(opt);});
          else if(key==='limit'){input.type='number';input.min='0';input.max='999';}
          input.value=fact[key]??'';input.oninput=()=>{fact[key]=key==='limit'?(input.value===''?null:Number(input.value)):input.value;mark();};
          label.append(input);row.append(label);
        }
        const del=el('button','icon-command');del.type='button';del.title='移除机制';del.setAttribute('aria-label',`移除机制 ${index+1}`);const icon=el('i');icon.dataset.lucide='trash-2';del.append(icon);
        del.onclick=()=>{working.mechanics.splice(index,1);renderMechanics();mark();};row.append(del);mechanics.append(row);
      });
      window.lucide?.createIcons();
    }
    function renderPreview(){
      preview.replaceChildren();const tabs=el('div','segmented');
      for(const [name,value] of [['基础',false],['升级',true]]){const b=el('button','',name);b.type='button';b.setAttribute('aria-pressed',value===upgraded);b.onclick=()=>{upgraded=value;renderPreview();};tabs.append(b);}
      preview.append(tabs);
      const face=el('article','card-face'),header=el('header');header.append(el('h3','',working.name||'未命名草案'),el('span','preview-cost',String(upgraded?(working.upgraded_cost??working.cost??'?'):(working.cost??'?'))));
      const portrait=working.image_id?`/api/images/${working.image_id}`:working.portrait_source_id?`/api/portraits/${encodeURIComponent(working.portrait_source_id)}`:null;
      face.append(header);
      if(portrait){const img=el('img');img.src=portrait;img.alt=working.name||'草案卡图';face.append(img);}else face.append(el('div','empty-art','未设置卡图'));
      face.append(el('p','card-face-meta',`${working.type||'类型未定'} · ${working.rarity||'品质未定'}${working.song?' · Song':''}`));
      const p=el('p','card-face-effect');let effect=working.base;
      if(upgraded&&working.upgrade_mode==='full')effect=working.upgrade;
      else if(upgraded&&working.upgrade_mode==='delta')effect+=`\n升级改动：${working.upgrade}`;
      const index=new Map(window.cardGraphView.data.nodes.map(n=>[n.id,n]));
      Object.values(api.workspace?.entries||{}).forEach(e=>index.set(e.id,{...e.working,id:e.id,name:e.working.name||'未命名草案'}));
      CardReferences.render(p,CardReferences.tokenize(effect,index,CardAliases),onOpenCard);face.append(p);
      const words=el('div','preview-keywords');(upgraded?working.upgraded_keywords:working.keywords).forEach(word=>words.append(el('span','',word)));face.append(words);preview.append(face);
    }
    function bindReferences(input){
      const list=el('div','reference-picker');list.hidden=true;input.parentElement?.append(list);
      input.addEventListener('input',()=>{
        if(!list.parentElement)input.parentElement.append(list);
        const start=input.selectionStart,match=/@([^@\n]{0,50})$/.exec(input.value.slice(0,start));
        list.replaceChildren();list.hidden=!match;if(!match)return;
        const candidates=new Map(window.cardGraphView.data.nodes.map(n=>[n.id,n]));
        Object.values(api.workspace?.entries||{}).forEach(e=>candidates.set(e.id,{...e.working,id:e.id,name:e.working.name||'未命名草案'}));
        const cards=[...candidates.values()].filter(n=>`${n.name} ${n.id}`.toLowerCase().includes(match[1].toLowerCase())).slice(0,8);
        for(const card of cards){const button=el('button','',card.name);button.type='button';button.onclick=()=>{input.setRangeText(`[[card:${card.id}]]`,start-match[0].length,start,'end');list.hidden=true;change();input.focus();};list.append(button);}
      });
      input.addEventListener('keydown',e=>{if(e.key==='Escape')list.hidden=true;});
    }
    const beforeUnload=e=>{if(seq!==saved){e.preventDefault();e.returnValue='';}};window.addEventListener('beforeunload',beforeUnload);
    return {async open(next){if(uploadTask)await uploadTask;await flush();next=api.workspace.entries[next.id];entry=structuredClone(next);working=structuredClone(next.working);seq=0;saved=0;status.textContent='已保存';message.textContent='';title.textContent=next.archived?'已归档草案':'设计草案';
        for(const [key,input] of Object.entries(fields)){if(key==='song')input.checked=working.song;else input.value=key.includes('keywords')?working[key].join(', '):working[key]??'';}
        save.disabled=next.archived;analyze.disabled=next.archived;renderMechanics();form.querySelectorAll('input,select,textarea,button').forEach(input=>input.disabled=next.archived);readFields();renderPreview();},
      flush,get id(){return entry?.id;},get card(){readFields();return structuredClone(working);},get dirty(){return seq!==saved;},
      download(){download({id:entry?.id,card:working,form:Object.fromEntries(Object.entries(fields).map(([k,input])=>[k,input.type==='checkbox'?input.checked:input.value]))},'sakiko-local-draft.json');},
      discard(){clearTimeout(timer);saved=seq;},dispose(){clearTimeout(timer);window.removeEventListener('beforeunload',beforeUnload);root.replaceChildren();}};
  }
  window.DraftEditor={mount,empty,download};
})();
