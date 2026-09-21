extends SceneTree

var effects: Array[Node2D] = []
var samples: Array[float] = []
var sample_index := 0

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() < 1 or not ProjectSettings.load_resource_pack(args[0], true):
		push_error("THORN_RENDER_FAIL: expected PCK")
		quit(1)
		return
	var script_path := "res://scenes/vfx/togawasakiko/thorn_restraint.gd" if args.size() < 2 else args[1]
	for size in [0.41, 0.82, 1.23]:
		var effect := Node2D.new()
		effect.set_script(load(script_path))
		effect.set("auto_play", false)
		effect.set("effect_scale", size)
		effect.position = Vector2(280 + effects.size() * 340, 650)
		root.add_child(effect)
		effect.set("_playing", true)
		effects.append(effect)
	for i in range(1, 101): samples.append(0.25 + i * 0.000001)
	for i in range(1, 601): samples.append(0.25 + i * 0.0001)
	for i in range(481): samples.append(i / 600.0)
	process_frame.connect(_sample)

func _sample() -> void:
	if sample_index >= samples.size():
		print("THORN_RENDER_SWEEP_COMPLETE=", samples.size(), " scales=", effects.size())
		for effect in effects: effect.queue_free()
		quit()
		return
	for effect in effects:
		effect.set("_elapsed", samples[sample_index])
		effect.call("_sync_visuals")
		effect.queue_redraw()
	sample_index += 1
