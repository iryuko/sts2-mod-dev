(function(root, factory) {
  const api=factory();
  if(typeof module==='object'&&module.exports)module.exports=api;
  else root.CardReferences=api;
})(globalThis,()=>{
  'use strict';
  function tokenize(text,index,aliases={}) {
    const result=[], names=Object.keys(aliases).sort((a,b)=>b.length-a.length);
    let plain='';
    const flush=()=>{if(plain){result.push({type:'text',text:plain});plain='';}};
    for(let i=0;i<text.length;) {
      const explicit=/^\[\[card:([^\]\r\n]+)\]\]/.exec(text.slice(i));
      const alias=explicit?null:names.find(name=>text.startsWith(name,i)&&index.has(aliases[name]));
      if(explicit||alias) {
        flush();const id=explicit?explicit[1]:aliases[alias],card=index.get(id);
        result.push(card?{type:'card',id,text:card.name}:{type:'missing',id,text:`[未找到卡牌: ${id}]`});
        i+=explicit?explicit[0].length:alias.length;
      } else plain+=text[i++];
    }
    flush();return result;
  }
  function render(container,tokens,onNavigate) {
    container.replaceChildren();
    tokens.forEach(token=>{
      if(token.type!=='card'){container.append(document.createTextNode(token.text));return;}
      const button=document.createElement('button');button.type='button';button.className='card-reference';
      button.dataset.cardId=token.id;button.textContent=token.text;button.setAttribute('aria-haspopup','dialog');
      button.onclick=()=>onNavigate(token.id);container.append(button);
    });
  }
  function bindPreview(root,resolveCard,onNavigate) {
    const popup=document.createElement('section');popup.className='card-popover';popup.hidden=true;
    popup.setAttribute('role','dialog');popup.setAttribute('aria-label','卡牌预览');document.body.append(popup);
    let anchor=null,timer;
    const hide=()=>{clearTimeout(timer);popup.hidden=true;anchor=null;};
    const later=()=>{clearTimeout(timer);timer=setTimeout(hide,180);};
    const show=target=>{
      const card=resolveCard(target.dataset.cardId);if(!card)return;
      clearTimeout(timer);if(anchor===target&&!popup.hidden)return;
      anchor=target;popup.replaceChildren();popup.hidden=false;
      const header=document.createElement('header'),title=document.createElement('strong'),close=document.createElement('button');
      title.textContent=card.name;close.type='button';close.textContent='×';close.setAttribute('aria-label','关闭预览');close.onclick=hide;
      header.append(title,close);popup.append(header);
      const content=document.createElement('div');content.className='popover-content';
      if(card.portrait){const img=document.createElement('img');img.src=card.portrait;img.alt=card.name;content.append(img);}
      for(const text of [`${card.cost??'未定'}费 · ${card.type??'未定'}${card.song?' · Song':''}`,card.base,`升级：${card.upgrade||'不变'}`]) {
        const p=document.createElement('p');p.textContent=text;content.append(p);
      }
      const go=document.createElement('button');go.className='popover-open';go.type='button';go.textContent='查看卡牌';
      go.onclick=()=>{const id=target.dataset.cardId;hide();onNavigate(id);};popup.append(content,go);
      const rect=target.getBoundingClientRect(),w=popup.offsetWidth,h=popup.offsetHeight;
      popup.style.left=`${Math.max(8,Math.min(innerWidth-w-8,rect.left))}px`;
      popup.style.top=`${Math.max(8,Math.min(innerHeight-h-8,rect.bottom+8))}px`;
    };
    const over=e=>{const target=e.target.closest?.('[data-card-id]');if(target)show(target);};
    const touch=e=>{const target=e.target.closest?.('[data-card-id]');if(e.pointerType==='touch'&&target){e.preventDefault();e.stopImmediatePropagation();show(target);}};
    const key=e=>{if(e.key==='Escape')hide();};
    const outside=e=>{if(!popup.contains(e.target)&&!e.target.closest?.('[data-card-id]'))hide();};
    root.addEventListener('pointerover',over);root.addEventListener('focusin',over);root.addEventListener('pointerout',later);
    root.addEventListener('click',touch,true);document.addEventListener('keydown',key);document.addEventListener('pointerdown',outside);
    popup.onpointerenter=()=>clearTimeout(timer);popup.onpointerleave=later;popup.onfocusin=()=>clearTimeout(timer);
    return ()=>{hide();popup.remove();root.removeEventListener('pointerover',over);root.removeEventListener('focusin',over);root.removeEventListener('pointerout',later);root.removeEventListener('click',touch,true);document.removeEventListener('keydown',key);document.removeEventListener('pointerdown',outside);};
  }
  return {tokenize,render,bindPreview};
});
