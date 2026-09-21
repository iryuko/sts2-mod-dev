"""Single-process owner, revision checks and atomic local snapshots."""
from copy import deepcopy
import fcntl
import json
import os
from pathlib import Path
import tempfile
import threading
import uuid

from workbench_model import (empty_workspace, validate_workspace, reduce_command,
                             RevisionConflict, CorruptWorkspace, WorkspaceLocked)


class WorkspaceStore:
    def __init__(self, root: Path, baseline: dict, profiles=None):
        self.root=Path(root);self.root.mkdir(parents=True,exist_ok=True)
        self.baseline=baseline;self.profiles=profiles or {};self.mutex=threading.RLock()
        self.path=self.root/'workspace.json';self.closed=False
        self.lock=(self.root/'.workspace.lock').open('a+')
        try:
            fcntl.flock(self.lock,fcntl.LOCK_EX|fcntl.LOCK_NB)
        except BlockingIOError as error:
            self.lock.close();raise WorkspaceLocked('工作区已有服务在使用','workspace_locked') from error
        try:
            self.state=self._load() if self.path.exists() else empty_workspace(baseline)
        except Exception:
            self.close();raise

    def _load(self):
        try:
            return validate_workspace(json.loads(self.path.read_text(encoding='utf-8')),self.baseline)
        except (ValueError,OSError) as error:
            raise CorruptWorkspace('工作区无法读取，请检查JSON及备份','corrupt_workspace') from error

    def read(self):
        with self.mutex: return deepcopy(self.state)

    def _check(self, expected_revision):
        if type(expected_revision) is not int or expected_revision!=self.state['revision']:
            raise RevisionConflict('工作区已由其他页面修改，本页未保存内容已保留','revision_conflict')
        if self.path.exists() and self._load()!=self.state:
            raise RevisionConflict('磁盘内容已在服务外改变，请重新加载','external_change')

    def _commit(self, state):
        payload=(json.dumps(state,ensure_ascii=False,indent=2,allow_nan=False)+'\n').encode()
        history=self.root/'history';history.mkdir(exist_ok=True)
        if self.path.exists():
            backup=history/f'workspace-{self.state["revision"]:012d}.json'
            with backup.open('wb') as handle:
                handle.write(self.path.read_bytes());handle.flush();os.fsync(handle.fileno())
        temporary=None
        try:
            with tempfile.NamedTemporaryFile(dir=self.root,prefix='.workspace-',suffix='.tmp',delete=False) as handle:
                temporary=Path(handle.name);handle.write(payload);handle.flush();os.fsync(handle.fileno())
            os.replace(temporary,self.path)
        finally:
            if temporary: temporary.unlink(missing_ok=True)
        self.state=state
        # Rotation is housekeeping after a successful commit, not part of its outcome.
        for old in sorted(history.glob('workspace-[0-9]'+'[0-9]'*11+'.json'))[:-20]:
            try: old.unlink()
            except OSError: pass

    def apply(self, command, expected_revision):
        with self.mutex:
            self._check(expected_revision)
            state,result=reduce_command(self.state,command,self.baseline,self.profiles)
            self._commit(state)
            return {'workspace':self.read(),'result':result}

    def close(self):
        if not self.closed:
            self.closed=True
            fcntl.flock(self.lock,fcntl.LOCK_UN);self.lock.close()

    def add_image(self, data, expected_revision):
        from workbench_assets import validate_image
        metadata=validate_image(data)
        with self.mutex:
            self._check(expected_revision)
            identifier=str(uuid.uuid4());filename=f'{identifier}.{metadata["extension"]}'
            uploads=self.root/'uploads';uploads.mkdir(exist_ok=True)
            with (uploads/filename).open('xb') as handle:
                handle.write(data);handle.flush();os.fsync(handle.fileno())
            state=deepcopy(self.state);state['images'][identifier]={**metadata,'filename':filename}
            state['revision']+=1;self._commit(state)
            return {'workspace':self.read(),'image_id':identifier}
