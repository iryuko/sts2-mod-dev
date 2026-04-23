extends SceneTree

func _initialize() -> void:
	var args: PackedStringArray = OS.get_cmdline_user_args()
	if args.size() < 1:
		push_error("Missing output path for Togawasakiko_in_Slay_the_Spire.pck")
		quit(1)
		return

	var output_path: String = args[0]
	var packer: PCKPacker = PCKPacker.new()
	var root: String = ProjectSettings.globalize_path("res://")

	var start_code: int = packer.pck_start(output_path)
	if start_code != OK:
		push_error("pck_start failed: %s" % start_code)
		quit(1)
		return

	var pending: Array[String] = ["res://"]
	while pending.size() > 0:
		var current: String = pending.pop_back()
		var dir: DirAccess = DirAccess.open(current)
		if dir == null:
			push_error("Failed to open %s" % current)
			quit(1)
			return

		dir.list_dir_begin()
		while true:
			var entry: String = dir.get_next()
			if entry == "":
				break
			if _should_skip_entry(current, entry):
				continue

			var child: String = current.path_join(entry)
			if dir.current_is_dir():
				pending.push_back(child)
				continue

			if child.ends_with("project.godot") or child.ends_with("pack_pck.gd"):
				continue

			var add_code: int = packer.add_file(child.replace("res://", ""), child)
			if add_code != OK:
				push_error("add_file failed for %s: %s" % [child, add_code])
				quit(1)
				return
		dir.list_dir_end()

	var flush_code: int = packer.flush(true)
	if flush_code != OK:
		push_error("flush failed: %s" % flush_code)
		quit(1)
		return

	print("Packed Togawasakiko_in_Slay_the_Spire.pck to: %s" % output_path)
	quit()


func _should_skip_entry(current: String, entry: String) -> bool:
	if current == "res://":
		return entry.begins_with(".") and entry != ".godot"

	if current == "res://.godot":
		return entry != "imported"

	return entry.begins_with(".")
