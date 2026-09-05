extends SceneTree

const ANIMATION_ROOT := "res://animations/characters/togawasakiko/"
const ACTIONS := ["idle_loop", "attack", "cast", "hurt", "die", "relaxed_loop"]
const CARDS := ["uncommon/unspoken_words", "uncommon/lingering_resonance", "uncommon/until_next_act", "common/composed_response",
	"uncommon/octagram_dance", "uncommon/divine", "uncommon/in_your_blue_eyes", "uncommon/the_whole_blue_world"]
var _hit := false

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() != 2 or not ProjectSettings.load_resource_pack(args[0], true):
		_fail("Expected PCK and source Spine SHA256; mount failed")
		return
	var json_path := ANIMATION_ROOT + "togawasakiko_v2.spine-json"
	if FileAccess.get_sha256(json_path) != args[1]:
		_fail("PCK Spine differs from verified formal source")
		return
	var data: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(json_path))
	var die: Dictionary = data["animations"]["die"]
	if die["bones"]["death_fall_root"]["translate"][-1]["time"] != 0.21:
		_fail("Old death timeline in PCK")
		return
	var manifest: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://mod_manifest.json"))
	if manifest.get("version") != "0.2.2":
		_fail("Wrong embedded version")
		return
	var fall: Texture2D = load(ANIMATION_ROOT + "images/fall_prone_candidate_v3_idle_locked.png")
	if fall == null or fall.get_size() != Vector2(1024, 1536):
		_fail("Accepted V3 texture failed to load")
		return
	for card in CARDS:
		var texture: Texture2D = load("res://mod_assets/cards/normal/" + card + ".png")
		if texture == null or texture.get_width() <= 0:
			_fail("Card portrait missing: " + card)
			return
		print("CARD_TEXTURE_OK=", card)
	for icon in ["backstage_support_power", "lingering_resonance_power", "octagram_dance_power"]:
		if load("res://images/atlases/power_atlas.sprites/" + icon + ".tres") == null:
			_fail("Power icon missing: " + icon)
			return
	var skeleton_data: Resource = load(ANIMATION_ROOT + "togawasakiko_v2_skel_data.tres")
	if skeleton_data == null:
		_fail("Spine imported resources failed to load")
		return
	var sprite: Node2D = ClassDB.instantiate("SpineSprite")
	sprite.set("skeleton_data_res", skeleton_data)
	root.add_child(sprite)
	var state: Object = sprite.call("get_animation_state")
	for action in ACTIONS:
		if state.call("set_animation", action, false, 0) == null:
			_fail("Animation missing: " + action)
			return
		print("SPINE_ACTION_OK=", action)
	var thorn_scene: PackedScene = load("res://scenes/vfx/togawasakiko/thorn_restraint.tscn")
	if thorn_scene == null:
		_fail("Thorn scene failed to load")
		return
	var thorn: Node = thorn_scene.instantiate()
	thorn.connect("hit_frame_reached", func(): _hit = true)
	root.add_child(thorn)
	await create_timer(0.4).timeout
	if not _hit:
		_fail("Thorn hit signal missing")
		return
	await create_timer(0.5).timeout
	if is_instance_valid(thorn):
		_fail("Thorn did not free itself")
		return
	sprite.queue_free()
	await process_frame
	print("CONTENT_PCK_PASS: cards, icons, six animations, 0.21s death, thorn lifecycle, version")
	quit(0)

func _fail(message: String) -> void:
	push_error("CONTENT_PCK_FAIL: " + message)
	quit(1)
