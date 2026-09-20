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
const SHADOW_BLACK := Color(0.012, 0.009, 0.015, 1.0)
const EDGE_WINE := Color(0.30, 0.035, 0.055, 1.0)
const HIT_GLOW := Color(0.95, 0.46, 0.24, 1.0)

@export var effect_scale := 0.82
@export var auto_play := true

var _elapsed := 0.0
var _playing := false
var _hit_emitted := false


func _ready() -> void:
	set_process(false)
	if auto_play:
		play()


func play() -> void:
	_elapsed = 0.0
	_playing = true
	_hit_emitted = false
	set_process(true)
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
	var growth := _ratio(GENERATE_END, GROW_END)
	var wrapping := _ratio(GROW_END, WRAP_END)
	var impact := _ratio(WRAP_END, HIT_END)
	var dissolve := _ratio(HIT_END, DISSOLVE_END)
	var alpha := 1.0 - _smoothstep(dissolve)

	_draw_ground_pool(generation, wrapping, impact, dissolve, alpha)
	_draw_cluster(growth, wrapping, impact, dissolve, alpha)
	_draw_constriction(wrapping, impact, dissolve, alpha)
	_draw_fragments(impact, dissolve, alpha)


func _draw_ground_pool(
	generation: float,
	wrapping: float,
	impact: float,
	dissolve: float,
	alpha: float
) -> void:
	var open_ratio := _ease_out_cubic(generation)
	var impact_pulse := sin(impact * PI)
	var radius := Vector2(150.0, 22.0) * effect_scale
	var pool_alpha := alpha * (0.60 + wrapping * 0.22 + impact_pulse * 0.15)
	var settle := lerpf(1.0, 0.80, dissolve)

	draw_set_transform(Vector2.ZERO, 0.0, Vector2(open_ratio * settle, 1.0))
	draw_colored_polygon(
		_ellipse_points(radius.x, radius.y, 44),
		Color(SHADOW_BLACK, pool_alpha)
	)
	draw_arc(
		Vector2.ZERO,
		radius.x,
		PI + 0.10,
		TAU - 0.10,
		48,
		Color(EDGE_WINE, pool_alpha * 0.82),
		2.8 * effect_scale,
		true
	)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)

	for crack_index in 5:
		var x := lerpf(-104.0, 104.0, float(crack_index) / 4.0)
		var side := -1.0 if crack_index % 2 == 0 else 1.0
		var crack := PackedVector2Array([
			Vector2(x * open_ratio, 0.0),
			Vector2((x + side * 10.0) * open_ratio, 7.0),
			Vector2((x + side * 23.0) * open_ratio, 12.0),
		])
		draw_polyline(
			crack,
			Color(EDGE_WINE, pool_alpha * 0.70),
			2.2 * effect_scale,
			true
		)


func _draw_cluster(
	growth: float,
	wrapping: float,
	impact: float,
	dissolve: float,
	alpha: float
) -> void:
	if growth <= 0.0:
		return

	var texture_size := THORN_TEXTURE.get_size()
	var target_height := 410.0 * effect_scale
	var target_width := target_height * texture_size.x / texture_size.y
	var grow_eased := _smoothstep(growth)
	var wrap_eased := _smoothstep(wrapping)
	var impact_pulse := sin(impact * PI)
	var horizontal_scale := (
		lerpf(0.72, 1.0, grow_eased)
		* lerpf(1.0, 0.84, wrap_eased)
		* (1.0 - impact_pulse * 0.07)
	)
	var vertical_scale := maxf(0.025, grow_eased) * (1.0 + impact_pulse * 0.025)
	var sink := dissolve * dissolve * 38.0 * effect_scale
	var tint := Color(
		1.0,
		1.0 - impact_pulse * 0.12,
		1.0 - impact_pulse * 0.20,
		alpha
	)

	draw_set_transform(
		Vector2(0.0, sink),
		0.0,
		Vector2(horizontal_scale, vertical_scale)
	)
	draw_texture_rect(
		THORN_TEXTURE,
		Rect2(
			Vector2(-target_width * 0.5, -target_height),
			Vector2(target_width, target_height)
		),
		false,
		tint
	)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_constriction(
	wrapping: float,
	impact: float,
	dissolve: float,
	alpha: float
) -> void:
	if wrapping <= 0.01:
		return

	var wrap_eased := _smoothstep(wrapping)
	var pulse := sin(impact * PI)
	var center := Vector2(
		8.0,
		-168.0 + pulse * 10.0 + dissolve * 36.0
	) * effect_scale
	var radius_x := lerpf(98.0, 55.0, wrap_eased) * effect_scale
	var radius_y := lerpf(64.0, 40.0, wrap_eased) * effect_scale
	var loop_points := PackedVector2Array()

	for sample_index in 42:
		var angle := TAU * float(sample_index) / 41.0
		var kink := sin(angle * 3.0 + wrapping * 2.0) * 7.0 * effect_scale
		loop_points.append(
			center + Vector2(cos(angle) * (radius_x + kink), sin(angle) * radius_y)
		)

	var loop_alpha := alpha * wrap_eased
	draw_polyline(loop_points, Color(EDGE_WINE, loop_alpha * 0.85), 15.0 * effect_scale, true)
	draw_polyline(loop_points, Color(SHADOW_BLACK, loop_alpha), 9.0 * effect_scale, true)

	for thorn_index in range(0, 42, 5):
		var point := loop_points[thorn_index]
		var normal := (point - center).normalized()
		var tangent := normal.rotated(PI * 0.5)
		var tip := point + normal * (16.0 + pulse * 8.0) * effect_scale
		draw_colored_polygon(
			PackedVector2Array([
				point + tangent * 5.0,
				point - tangent * 5.0,
				tip,
			]),
			Color(SHADOW_BLACK, loop_alpha)
		)

	if impact > 0.0:
		draw_arc(
			center,
			(69.0 - pulse * 18.0) * effect_scale,
			0.0,
			TAU,
			42,
			Color(HIT_GLOW, alpha * pulse * 0.80),
			5.0 * effect_scale,
			true
		)


func _draw_fragments(impact: float, dissolve: float, alpha: float) -> void:
	if impact <= 0.0 and dissolve <= 0.0:
		return

	var progress := maxf(impact, dissolve)
	for shard_index in 8:
		var angle := -PI + float(shard_index) * PI / 7.0
		var distance := (22.0 + float((shard_index * 19) % 42)) * progress
		var origin := Vector2(cos(angle) * distance, -120.0 + sin(angle) * distance)
		origin *= effect_scale
		var size := (5.0 - progress * 2.0) * effect_scale
		draw_colored_polygon(
			PackedVector2Array([
				origin + Vector2(-size, size),
				origin + Vector2(size, size * 0.4),
				origin + Vector2(0.0, -size * 1.8),
			]),
			Color(EDGE_WINE, alpha * 0.75)
		)


func _ratio(start_time: float, end_time: float) -> float:
	return clampf(inverse_lerp(start_time, end_time, _elapsed), 0.0, 1.0)


func _ease_out_cubic(value: float) -> float:
	return 1.0 - pow(1.0 - value, 3.0)


func _smoothstep(value: float) -> float:
	return value * value * (3.0 - 2.0 * value)


func _ellipse_points(radius_x: float, radius_y: float, count: int) -> PackedVector2Array:
	var points := PackedVector2Array()
	for point_index in count:
		var angle := TAU * float(point_index) / float(count)
		points.append(Vector2(cos(angle) * radius_x, sin(angle) * radius_y))
	return points
