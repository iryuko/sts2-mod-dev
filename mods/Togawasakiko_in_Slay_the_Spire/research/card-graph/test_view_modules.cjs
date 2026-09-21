const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');

test('edge kinds remain distinguishable without colour', () => {
  assert.ok(fs.existsSync(`${__dirname}/relation-style.js`), 'style module is available');
  const style = require('./relation-style.js');
  const kinds = ['supply','payoff','sequence','replay','generation','tradeoff','conflict'];
  const shapes = kinds.map(k => {const s=style.styles(k); return `${s.lineStyle}/${s.arrowShape}`;});
  assert.equal(new Set(shapes).size, 7);
  assert.equal(new Set(kinds.map(k=>style.styles(k).color)).size, 7);
  assert.deepEqual(style.legend().map(s=>s.kind), kinds);
  assert.throws(()=>style.styles('unknown'));
});

test('explicit identities win over ambiguous names and missing references',()=>{
  assert.ok(fs.existsSync(`${__dirname}/card-references.js`),'reference module is available');
  const refs=require('./card-references.js');
  const index=new Map([['Face',{id:'Face',name:'颜'}],['A',{id:'A',name:'同名'}],['B',{id:'B',name:'同名'}]]);
  assert.deepEqual(refs.tokenize('颜不是攻击，同名',index,{}),[{type:'text',text:'颜不是攻击，同名'}]);
  assert.equal(refs.tokenize('[[card:Face]]',index,{})[0].id,'Face');
  assert.equal(refs.tokenize('[[card:missing]]',index,{})[0].type,'missing');
  assert.equal(refs.tokenize('[[card:B]]',index,{})[0].id,'B');
  assert.equal(refs.tokenize('长名称',index,{'长名':'A','长名称':'B'})[0].id,'B');
  assert.deepEqual(refs.tokenize('<img onerror=bad()>',index,{}),[{type:'text',text:'<img onerror=bad()>'}]);
});
