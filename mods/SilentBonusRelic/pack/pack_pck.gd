extends SceneTree

func _initialize() -> void:
	var args: PackedStringArray = OS.get_cmdline_user_args()
	if args.size() < 1:
		push_error("Missing output path for SilentBonusRelic.pck")
		quit(1)
		return

	var output_path: String = args[0]
	var packer: PCKPacker = PCKPacker.new()
	var source_files: Array[String] = [
		"res://mod_manifest.json",
	]

	var start_code: int = packer.pck_start(output_path)
	if start_code != OK:
		push_error("pck_start failed: %s" % start_code)
		quit(1)
		return

	for source_path: String in source_files:
		if not FileAccess.file_exists(source_path):
			push_error("Missing source file: %s" % source_path)
			quit(1)
			return

		var pack_path: String = source_path.replace("res://", "")
		var add_code: int = packer.add_file(pack_path, source_path)
		if add_code != OK:
			push_error("add_file failed for %s: %s" % [source_path, add_code])
			quit(1)
			return

	var flush_code: int = packer.flush(true)
	if flush_code != OK:
		push_error("flush failed: %s" % flush_code)
		quit(1)
		return

	print("Packed SilentBonusRelic.pck to: %s" % output_path)
	quit()
