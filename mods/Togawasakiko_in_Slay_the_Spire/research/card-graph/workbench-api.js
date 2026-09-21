/* Same-origin token and the only mutation queue. Responses never replace editor text. */
(() => {
  'use strict';
  let token,workspace,capabilities,queue=Promise.resolve(),blocked=false,status='未连接';
  const listeners=new Set();
  const notify=()=>listeners.forEach(fn=>fn({workspace,status,blocked,capabilities}));
  async function request(path,body,headers={}) {
    const options=body===undefined?{}:{method:'POST',headers:{'X-Workbench-Token':token,...headers},body};
    let response;
    try{response=await fetch(path,options);}catch(error){throw new Error('服务连接中断，本页未保存内容已保留。');}
    const data=await response.json();
    if(!response.ok){const error=new Error(data.error?.message||'请求失败');error.status=response.status;error.fields=data.error?.fields;throw error;}
    return data;
  }
  const post=(path,body)=>request(path,JSON.stringify(body),{'Content-Type':'application/json'});
  function write(operation) {
    const next=queue.then(async()=>{
      if(blocked)throw new Error('保存已暂停，请先重新连接或处理版本冲突。');
      status='保存中';notify();
      try{const result=await operation();workspace=result.workspace;status='已保存';notify();return result;}
      catch(error){blocked=true;status=error.message;notify();throw error;}
    });
    queue=next.catch(()=>{});return next;
  }
  window.WorkbenchApi={
    async connect({reload=false}={}){
      await queue;
      const data=await request('/api/bootstrap');
      if(workspace&&!reload&&workspace.revision!==data.workspace.revision){blocked=true;status='版本冲突：本页内容已保留。下载本页草稿后可重新加载。';notify();throw new Error(status);}
      token=data.token;workspace=data.workspace;capabilities=data.capabilities;blocked=false;status='已保存';notify();return data;
    },
    command:command=>write(()=>post('/api/commands',{expected_revision:workspace.revision,command})),
    upload:file=>write(()=>request('/api/uploads',file,{'Content-Type':file.type||'application/octet-stream','X-Workspace-Revision':String(workspace.revision)})),
    analyze:(draft_id,card)=>post('/api/analyze',{draft_id,card}),
    preview:(draft_id,card)=>post('/api/preview',{draft_id,card}),
    graph:()=>request('/api/graph'),
    get workspace(){return workspace;},get blocked(){return blocked;},
    subscribe(fn){listeners.add(fn);return()=>listeners.delete(fn);}
  };
})();
