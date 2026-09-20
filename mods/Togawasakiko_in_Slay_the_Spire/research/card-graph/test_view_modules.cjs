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
