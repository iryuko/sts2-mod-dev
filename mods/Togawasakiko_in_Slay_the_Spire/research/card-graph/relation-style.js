(function(root, factory) {
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  else root.RelationStyle = api;
})(globalThis, () => {
  'use strict';
  const palette = {
    supply: ['资源供给','#177d70','solid',[1,0],'triangle'],
    payoff: ['效果收益','#2965b3','solid',[1,0],'diamond'],
    sequence: ['牌序编排','#947208','dashed',[9,4],'triangle'],
    replay: ['重复打出','#96528c','dashed',[4,3],'vee'],
    generation: ['生成候选','#198da6','dotted',[1,3],'circle'],
    tradeoff: ['资源取舍','#b96b22','dashed',[9,4],'diamond'],
    conflict: ['流程冲突','#bd3f4c','solid',[1,0],'tee']
  };
  function styles(kind) {
    if (!Object.hasOwn(palette, kind)) throw new Error(`Unknown relation kind: ${kind}`);
    const [,color,lineStyle,dashPattern,arrowShape] = palette[kind];
    return {color,lineStyle,dashPattern:[...dashPattern],arrowShape};
  }
  function sample(kind) {
    const style=styles(kind), span=document.createElement('span');
    span.className=`relation-sample shape-${style.arrowShape}`;
    span.style.setProperty('--edge-color',style.color);
    span.style.setProperty('--edge-line',style.lineStyle);
    span.setAttribute('aria-hidden','true');
    return span;
  }
  return {styles,sample,legend:()=>Object.keys(palette).map(kind=>({kind,label:palette[kind][0],...styles(kind)}))};
});
