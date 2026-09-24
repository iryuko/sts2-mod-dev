const {test,before,after}=require('node:test');
const assert=require('node:assert/strict');
const {chromium}=require('playwright');
const fs=require('node:fs');
const path=require('node:path');
const os=require('node:os');
const {spawn}=require('node:child_process');
const {once}=require('node:events');
const readline=require('node:readline');
let server,browser,temp,url,lines;
before(async()=>{
  temp=fs.mkdtempSync(path.join(os.tmpdir(),'sakiko-review-'));
  server=spawn(process.env.PYTHON || 'python3',['server.py','--port','0','--workspace',temp],{cwd:__dirname,stdio:['ignore','pipe','inherit']});
  lines=readline.createInterface({input:server.stdout});const [line]=await once(lines,'line');url=JSON.parse(line).url;
  browser=await chromium.launch({headless:true});
});
after(async()=>{if(browser)await browser.close();lines?.close();if(server?.exitCode===null){server.kill('SIGTERM');await once(server,'exit');}if(temp)fs.rmSync(temp,{recursive:true,force:true});});
async function fixture(){
  const page=await browser.newPage();page.setDefaultTimeout(5000);await page.goto(url);await page.waitForFunction(()=>sakikoWorkbench.api.workspace);
  await page.getByRole('button',{name:'新建草案',exact:true}).click();await page.getByLabel('卡牌名称',{exact:true}).fill('A');await page.getByLabel('基础费用',{exact:true}).fill('1');
  await page.getByLabel('基础效果',{exact:true}).fill('旧效果');await page.getByRole('button',{name:'保存草稿',exact:true}).click();await page.waitForFunction(()=>!sakikoWorkbench.editor.dirty);
  return page;
}
async function close(page){await page.evaluate(()=>sakikoWorkbench.editor.discard());await page.close();}

test('reselecting current draft uses the snapshot after flushing',async()=>{
  const page=await fixture();
  try{
    await page.getByLabel('基础效果',{exact:true}).fill('最新效果');
    await page.locator('#draft-list button[aria-current="true"]').click();
    await page.waitForFunction(()=>!sakikoWorkbench.editor.dirty);
    assert.equal(await page.getByLabel('基础效果',{exact:true}).inputValue(),'最新效果');
    await page.getByLabel('设计备注',{exact:true}).fill('不能回滚效果');await page.getByRole('button',{name:'保存草稿',exact:true}).click();
    await page.waitForFunction(()=>!sakikoWorkbench.editor.dirty);
    assert.equal(await page.evaluate(()=>sakikoWorkbench.api.workspace.entries[sakikoWorkbench.editor.id].working.base),'最新效果');
  }finally{await close(page);}
});

test('invalid input during a save stays dirty and later valid correction saves new text',async()=>{
  const page=await fixture();let release;const gate=new Promise(resolve=>release=resolve);
  try{
    await page.route('**/api/commands',async route=>{await gate;await route.continue();});
    await page.getByLabel('基础效果',{exact:true}).fill('第一版');await page.getByRole('button',{name:'保存草稿',exact:true}).click();
    await page.waitForFunction(()=>document.querySelector('#save-status').textContent==='保存中');
    await page.getByLabel('基础费用',{exact:true}).fill('invalid');await page.getByLabel('基础效果',{exact:true}).fill('最新未保存效果');release();
    await page.waitForFunction(()=>document.querySelector('#connection-status').textContent==='已保存');
    assert.equal(await page.evaluate(()=>sakikoWorkbench.editor.dirty),true);
    assert.notEqual(await page.locator('#save-status').textContent(),'已保存');
    await page.unroute('**/api/commands');await page.getByLabel('基础费用',{exact:true}).fill('2');
    await page.getByRole('button',{name:'保存草稿',exact:true}).click();await page.waitForFunction(()=>!sakikoWorkbench.editor.dirty);
    assert.equal(await page.evaluate(()=>sakikoWorkbench.api.workspace.entries[sakikoWorkbench.editor.id].working.base),'最新未保存效果');
  }finally{release();await close(page);}
});

test('late upload remains attached to its originating draft',async()=>{
  const page=await fixture();let release;const gate=new Promise(resolve=>release=resolve);
  try{
    const a=await page.evaluate(()=>sakikoWorkbench.editor.id);
    const b=await page.evaluate(async()=>{const r=await sakikoWorkbench.api.command({action:'create',card:{...DraftEditor.empty(),name:'B'}});return r.result.id;});
    await page.route('**/api/uploads',async route=>{await gate;await route.continue();});
    const requested=page.waitForRequest('**/api/uploads');const graph=JSON.parse(fs.readFileSync(path.join(__dirname,'graph.json')));
    await page.getByLabel('上传卡图',{exact:true}).setInputFiles(path.resolve(__dirname,graph.nodes.find(n=>n.portrait).portrait));await requested;
    await page.getByRole('button',{name:'B',exact:true}).click();release();
    await page.waitForFunction(id=>sakikoWorkbench.editor.id===id,b);
    await page.waitForFunction(()=>document.querySelector('#connection-status').textContent==='已保存');
    const entries=await page.evaluate(()=>sakikoWorkbench.api.workspace.entries);
    assert.ok(entries[a].working.image_id);assert.equal(entries[b].working.image_id,null);
  }finally{release();await close(page);}
});

test('copy preserves independent upgrade fields and unpublished draft identity',async()=>{
  const page=await fixture();
  try{
    await page.getByLabel('升级费用',{exact:true}).fill('0');
    await page.getByLabel('基础关键词',{exact:true}).fill('Exhaust');
    await page.getByLabel('升级关键词',{exact:true}).fill('Retain');
    await page.getByRole('button',{name:'保存草稿',exact:true}).click();await page.waitForFunction(()=>!sakikoWorkbench.editor.dirty);
    const origin=await page.evaluate(()=>sakikoWorkbench.editor.id);
    await page.getByRole('button',{name:'复制选中卡',exact:true}).click();
    await page.waitForFunction(id=>sakikoWorkbench.editor.id!==id,origin);
    assert.equal(await page.getByLabel('升级费用',{exact:true}).inputValue(),'0');
    assert.equal(await page.getByLabel('升级关键词',{exact:true}).inputValue(),'Retain');
    assert.equal(await page.evaluate(()=>sakikoWorkbench.api.workspace.entries[sakikoWorkbench.editor.id].origin_id),origin);
  }finally{await close(page);}
});
