extends Node2D

signal hit_frame_reached
signal finished

const GENERATE_END := 0.08
const GROW_END := 0.25
const WRAP_END := 0.31
const HIT_END := 0.39
const DISSOLVE_END := 0.80

const THORN_TEXTURE: Texture2D = preload(
	"res://images/vfx/togawasakiko/thorn_cluster_alpha_v1_trimmed.png"
)
const SHADOW_SHADER: Shader = preload("res://scenes/vfx/togawasakiko/thorn_shadow.gdshader")
const SHADOW_BLACK := Color(0.018, 0.012, 0.025, 1.0)
const EDGE_WINE := Color(0.38, 0.045, 0.09, 1.0)
const HIT_GLOW := Color(0.70, 0.085, 0.16, 1.0)

@export var effect_scale := 0.82
@export var auto_play := true

var _elapsed := 0.0
var _playing := false
var _hit_emitted := false
var _cluster: Sprite2D
var _cluster_material: ShaderMaterial


func _ready() -> void:
	_cluster_material = ShaderMaterial.new()
	_cluster_material.shader = SHADOW_SHADER
	_cluster = Sprite2D.new()
	_cluster.texture = THORN_TEXTURE
	_cluster.material = _cluster_material
	_cluster.show_behind_parent = true
	add_child(_cluster)
	_sync_visuals()
	set_process(false)
	if auto_play:
		play()


func play() -> void:
	_elapsed = 0.0
	_playing = true
	_hit_emitted = false
	set_process(true)
	_sync_visuals()
	queue_redraw()


func get_hit_time_seconds() -> float:
	return WRAP_END


func get_duration_seconds() -> float:
	return DISSOLVE_END


func _process(delta: float) -> void:
	if not _playing:
		return

	_elapsed += delta
	if not _hit_emitted and _elapsed >= WRAP_END:
		_hit_emitted = true
		hit_frame_reached.emit()

	_sync_visuals()
	queue_redraw()
	if _elapsed >= DISSOLVE_END:
		_playing = false
		set_process(false)
		finished.emit()
		queue_free()


func _draw() -> void:
	if not _playing:
		return

	var generation := _ratio(0.0, GENERATE_END)
	var wrapping := _ratio(GROW_END, WRAP_END)
	var impact := _ratio(WRAP_END, HIT_END)
	var dissolve := _ratio(HIT_END, DISSOLVE_END)
	var alpha := 1.0 - _smoothstep(dissolve)

	_draw_ground_pool(generation, wrapping, impact, dissolve, alpha)
	_draw_constriction(wrapping, impact, dissolve, alpha)
	_draw_fragments(impact, dissolve, alpha)


func _sync_visuals() -> void:
	if not is_instance_valid(_cluster):
		return
	_cluster.visible = _playing and _elapsed < DISSOLVE_END
	_cluster.scale = Vector2.ONE * (410.0 * effect_scale / THORN_TEXTURE.get_height())
	_cluster.position = Vector2(0.0, -205.0 * effect_scale)
	_cluster_material.set_shader_parameter("elapsed", _elapsed)


func _draw_ground_pool(
	generation: float, wrapping: float, impact: float, dissolve: float, alpha: float
) -> void:
	var spread := _ease_out_cubic(generation) * (1.0 - dissolve * 0.18)
	if spread <= 0.0:
		return
	var pulse := sin(impact * PI)
	var pool := PackedVector2Array()
	for i in 64:
		var angle := TAU * float(i) / 64.0
		var edge := 1.0 + sin(angle * 7.0) * 0.055 + cos(angle * 11.0) * 0.04
		pool.append(Vector2(cos(angle) * 146.0 * spread, sin(angle) * 16.0) * edge * effect_scale)
	draw_colored_polygon(pool, Color(SHADOW_BLACK, alpha * (0.56 + wrapping * 0.16)))
	# Broken low red seams stay on the ground plane, never a vertical semicircle.
	for i in 4:
		var seam := PackedVector2Array()
		for j in 13:
			var angle := 0.22 + i * 1.6 + float(j) * 0.055
			seam.append(Vector2(cos(angle) * 137.0 * spread, sin(angle) * 14.0) * effect_scale)
		draw_polyline(seam, Color(EDGE_WINE, alpha * (0.35 + pulse * 0.35)), 1.2 * effect_scale, true)


func _draw_constriction(wrapping: float, impact: float, dissolve: float, alpha: float) -> void:
	if wrapping <= 0.0:
		return
	var wrap := _smoothstep(wrapping)
	var pulse := sin(impact * PI)
	var ribbon_alpha := alpha * (1.0 - _smoothstep(clampf(dissolve * 2.2, 0.0, 1.0)))
	# Two off-center tapered branches close in opposite directions; no closed rubber ring.
	for branch in 2:
		var points := PackedVector2Array()
		for i in 27:
			var along := float(i) / 26.0
			var angle := -0.35 + branch * PI + along * 2.22 * wrap
			var radius := lerpf(104.0, 68.0, wrap) - pulse * 3.0
			var center := Vector2(5.0, -161.0 - branch * 16.0)
			points.append((center + Vector2(cos(angle) * radius,
				sin(angle) * 35.0 + sin(angle * 3.0) * 5.0)) * effect_scale)
		_draw_tapered_ribbon(points, (5.5 - branch) * effect_scale, Color(SHADOW_BLACK, ribbon_alpha))
		var red_edge := points.duplicate()
		for i in red_edge.size():
			red_edge[i].y -= 1.4 * effect_scale
		_draw_tapered_ribbon(red_edge, 1.05 * effect_scale, Color(EDGE_WINE, ribbon_alpha * 0.8))
		for i in [6, 14, 22]:
			var at := points[i]
			var direction := (points[i + 1] - points[i - 1]).normalized()
			var normal := direction.orthogonal()
			draw_colored_polygon(PackedVector2Array([
				at - direction * 5.0 * effect_scale,
				at + direction * 4.0 * effect_scale,
				at + (normal * (10.0 + branch * 3.0) + direction * 8.0) * effect_scale,
			]), Color(SHADOW_BLACK, ribbon_alpha))
	if pulse > 0.0:
		for i in 3:
			var angle := -0.9 + i * 2.2
			var center := Vector2(6.0, -168.0) * effect_scale
			var direction := Vector2(cos(angle), sin(angle))
			var tip := center + direction * (25.0 + pulse * 14.0) * effect_scale
			_draw_tapered_ribbon(PackedVector2Array([
				center + direction * 16.0 * effect_scale,
				tip + direction.orthogonal() * 4.0 * effect_scale,
				center + direction * (54.0 + pulse * 12.0) * effect_scale,
			]), 2.7 * effect_scale, Color(HIT_GLOW, pulse * 0.9))


func _draw_fragments(impact: float, dissolve: float, _alpha: float) -> void:
	if impact <= 0.0:
		return
	for i in 19:
		var delay := float((i * 7) % 11) * 0.009
		var age := clampf((_elapsed - HIT_END - delay) / (0.30 - delay), 0.0, 1.0)
		if age <= 0.0 or age >= 1.0:
			continue
		var side := -1.0 if i % 2 == 0 else 1.0
		var y := -64.0 - float((i * 67) % 253)
		var x := side * (38.0 + float((i * 29) % 44))
		var drift := Vector2(side * (14.0 + i % 5 * 6.0), -19.0 - i % 7 * 4.0)
		var origin := Vector2(x, y) + drift * age + Vector2(sin(age * 4.0 + i) * 4.0, 0.0)
		var direction := drift.normalized()
		var length := (6.0 + i % 4 * 4.0) * (1.0 - age * 0.75)
		var fade := sin(age * PI) * (1.0 - dissolve) * 0.8
		var color := EDGE_WINE if i % 4 == 0 else SHADOW_BLACK
		_draw_tapered_ribbon(PackedVector2Array([
			(origin - direction * length) * effect_scale,
			origin * effect_scale,
			(origin + direction * length * 0.6) * effect_scale,
		]), (1.7 + i % 3) * (1.0 - age) * effect_scale, Color(color, fade))


func _draw_tapered_ribbon(points: PackedVector2Array, width: float, color: Color) -> void:
	# Near birth/fade, float32 points collapse and cannot form a valid polygon.
	# Both limits are below a pixel at the authored scale.
	if width < 0.01 * effect_scale or points[0].distance_to(points[-1]) < 0.5 * effect_scale:
		return
	var polygon := PackedVector2Array()
	var opposite := PackedVector2Array()
	for i in points.size():
		var tangent := points[mini(i + 1, points.size() - 1)] - points[maxi(i - 1, 0)]
		var normal := tangent.normalized().orthogonal()
		var along := float(i) / float(points.size() - 1)
		var taper := pow(sin(along * PI), 0.7) * (1.0 - along * 0.5)
		polygon.append(points[i] + normal * width * taper)
		opposite.append(points[i] - normal * width * taper)
	opposite.reverse()
	polygon.append_array(opposite)
	draw_colored_polygon(polygon, color)


func _ratio(start_time: float, end_time: float) -> float:
	return clampf(inverse_lerp(start_time, end_time, _elapsed), 0.0, 1.0)


func _ease_out_cubic(value: float) -> float:
	return 1.0 - pow(1.0 - value, 3.0)


func _smoothstep(value: float) -> float:
	return value * value * (3.0 - 2.0 * value)
