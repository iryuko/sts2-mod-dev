const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const path = require('node:path');
const fs = require('node:fs');
const { pathToFileURL } = require('node:url');

(async () => {
  const out = path.resolve(__dirname, '../../../../local/card-graph-2026-09-21');
  fs.mkdirSync(out, { recursive: true });
  const browser = await chromium.launch({ headless: true });
  const errors = [];
  try {
    for (const [name, width, height] of [['desktop', 1440, 1000], ['mobile', 390, 844]]) {
      const page = await browser.newPage({ viewport: { width, height } });
      page.on('pageerror', error => errors.push(error.message));
      await page.goto(pathToFileURL(path.join(__dirname, 'index.html')).href);
      await page.waitForFunction(() => window.cardGraphView?.cy.nodes().length > 0);
      assert.equal(await page.locator('.card-item').count(), 75);
      await page.locator('#search').fill('不存在的卡牌');
      assert.equal(await page.locator('.card-item').count(), 0);
      assert.ok((await page.locator('#card-list').textContent()).includes('没有匹配'));
      await page.locator('#search').fill('CrucifixX');
      assert.equal(await page.locator('.card-item .card-cost').textContent(), 'X费');
      await page.locator('#search').fill('');
      await page.getByRole('button', { name: '中伤', exact: true }).click();
      assert.equal(await page.locator('#graph-title').textContent(), '中伤');
      assert.ok(await page.locator('.relation-row.negative').count() > 0);
      await page.locator('#relation-filter').selectOption('tradeoff');
      assert.ok(await page.evaluate(() => window.cardGraphView.cy.edges().length) > 0);
      await page.locator('.relation-row').first().click();
      assert.ok((await page.locator('#detail-content').textContent()).includes('成立条件'));
      assert.ok(await page.evaluate(()=>cardGraphView.cy.nodes('.dim').length)>0);
      assert.ok(await page.locator('#detail-content a[href*="057b6396"]').count() > 0);
      await page.locator('#search').fill('RewrittenAccent');
      assert.equal(await page.locator('.card-item').count(), 1);
      await page.getByRole('button', { name: '改写重拍（草案）', exact: true }).click();
      assert.ok((await page.locator('#detail-content').textContent()).includes('未批准'));
      await page.locator('#proposal-filter').uncheck();
      assert.equal(await page.evaluate(() => window.cardGraphView.state.selected), 'Completeness');
      await page.locator('#search').fill('');
      assert.equal(await page.locator('.card-item').count(), 72);
      await page.locator('#type-filter').selectOption('Attack');
      assert.equal(await page.locator('.card-item').count(), 19);
      await page.locator('#type-filter').selectOption('all');
      await page.locator('#relation-filter').selectOption('all');
      await page.getByRole('button', { name: '全局', exact: true }).click();
      assert.equal(await page.evaluate(() => window.cardGraphView.cy.nodes().length), 72);
      const edgeStyles=await page.evaluate(()=>RelationStyle.legend().map(s=>{const e=cardGraphView.cy.edges().filter(e=>e.data('kind')===s.kind).first();return [e.style('line-style'),e.style('target-arrow-shape'),e.style('line-color')];}));
      assert.equal(new Set(edgeStyles.map(s=>s.join('/'))).size,7);
      assert.equal(await page.locator('#relation-legend button').count(),7);
      assert.equal(await page.locator('#direction-filter').isDisabled(), true);
      assert.equal(await page.evaluate(() => window.cardGraphView.cy.nodes().filter(n => n.style('label') !== '').length), 1);
      assert.equal(await page.evaluate(() => { const n=window.cardGraphView.cy.getElementById('Slander');n.emit('mouseover');return n.style('label'); }), '中伤');
      await page.evaluate(() => window.cardGraphView.cy.getElementById('Slander').emit('mouseout'));
      await page.locator('#graph').screenshot({ path: path.join(out, `${name}-global.png`) });
      await page.getByRole('button', { name: '选中牌关系', exact: true }).click();
      await page.getByRole('button', { name: '完美无缺', exact: true }).click();
      await page.locator('#direction-filter').selectOption('out');
      assert.equal(await page.evaluate(() => window.cardGraphView.cy.edges().every(e => e.data('source') === 'Completeness')), true);
      await page.locator('#direction-filter').selectOption('both');
      const downloadPromise = page.waitForEvent('download');
      await page.getByRole('button', { name: '导出当前关系 JSON' }).click();
      const download = await downloadPromise;
      const exportPath = path.join(out, `${name}-subgraph.json`);
      await download.saveAs(exportPath);
      const exported = JSON.parse(fs.readFileSync(exportPath, 'utf8'));
      assert.equal(exported.nodes.length, await page.evaluate(() => window.cardGraphView.cy.nodes().length));
      assert.ok(exported.edges.length > 0);
      await page.waitForFunction(() => [...document.images].every(image => image.complete && image.naturalWidth > 0));
      const dimensions = await page.evaluate(() => ({ scroll: document.documentElement.scrollWidth, width: innerWidth,
        canvas: document.querySelector('#graph canvas')?.getBoundingClientRect().toJSON(),
        nodes: window.cardGraphView.cy.nodes().length, edges: window.cardGraphView.cy.edges().length }));
      assert.ok(dimensions.scroll <= dimensions.width, `${name}: horizontal overflow`);
      assert.ok(dimensions.canvas?.width > 200 && dimensions.canvas?.height > 300, `${name}: blank-size canvas`);
      assert.ok(dimensions.nodes > 3 && dimensions.edges > 2);
      const shot = path.join(out, `${name}.png`);
      await page.screenshot({ path: shot, fullPage: true });
      const graphShot = path.join(out, `${name}-graph.png`);
      await page.locator('#graph').screenshot({ path: graphShot });
      console.log(JSON.stringify({ name, ...dimensions, screenshot: shot, graphScreenshot: graphShot }));
      await page.close();
    }
    assert.deepEqual(errors, []);
    console.log('PASS desktop/mobile graph interactions, counts, X cost, filters, export, evidence links, images and overflow');
  } finally {
    await browser.close();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
