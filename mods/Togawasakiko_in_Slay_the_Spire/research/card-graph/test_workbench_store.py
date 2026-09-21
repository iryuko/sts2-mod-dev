import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from test_workbench_model import BASE, card
from workbench_model import RevisionConflict, CorruptWorkspace, WorkspaceLocked
from workbench_store import WorkspaceStore


class StoreTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory(); self.root=Path(self.temp.name)
        self.store=WorkspaceStore(self.root,BASE)
    def tearDown(self): self.store.close(); self.temp.cleanup()

    def test_revision_persistence_and_lock(self):
        with self.assertRaises(WorkspaceLocked): WorkspaceStore(self.root,BASE)
        result=self.store.apply({'action':'create','card':card()},0)
        with self.assertRaises(RevisionConflict): self.store.apply({'action':'create','card':card()},0)
        self.store.close();self.store=WorkspaceStore(self.root,BASE)
        self.assertIn(result['result']['id'],self.store.read()['entries'])
        detached=self.store.read();detached['entries'].clear()
        self.assertEqual(len(self.store.read()['entries']),1)

    def test_failed_replace_never_advances_revision(self):
        self.store.apply({'action':'create','card':card()},0)
        before=(self.root/'workspace.json').read_bytes()
        with patch('workbench_store.os.replace',side_effect=OSError('disk unavailable')):
            with self.assertRaises(OSError): self.store.apply({'action':'create','card':card()},1)
        self.assertEqual(self.store.read()['revision'],1)
        self.assertEqual((self.root/'workspace.json').read_bytes(),before)

    def test_backup_limit_and_bad_workspace_not_overwritten(self):
        for i in range(23): self.store.apply({'action':'create','card':card()},i)
        self.assertEqual(len(list((self.root/'history').glob('workspace-*.json'))),20)
        self.store.close()
        for payload in ['broken', json.dumps({'schema_version':999}), json.dumps({'schema_version':1})]:
            (self.root/'workspace.json').write_text(payload)
            with self.assertRaises(CorruptWorkspace): WorkspaceStore(self.root,BASE)
            self.assertEqual((self.root/'workspace.json').read_text(),payload)

    def test_symlinked_storage_folders_never_write_outside_root(self):
        for folder in ['history','uploads']:
            with self.subTest(folder=folder), tempfile.TemporaryDirectory() as root, tempfile.TemporaryDirectory() as outside:
                store=WorkspaceStore(Path(root),BASE)
                try:
                    (Path(root)/folder).symlink_to(outside,target_is_directory=True)
                    if folder=='history':
                        with self.assertRaises((OSError,CorruptWorkspace)):
                            store.apply({'action':'create','card':card()},0)
                    else:
                        from io import BytesIO
                        from PIL import Image
                        data=BytesIO();Image.new('RGB',(2,2)).save(data,format='PNG')
                        with self.assertRaises((OSError,CorruptWorkspace)): store.add_image(data.getvalue(),0)
                    self.assertEqual(list(Path(outside).iterdir()),[])
                    self.assertEqual(store.read()['revision'],0)
                finally: store.close()


if __name__=='__main__': unittest.main()
