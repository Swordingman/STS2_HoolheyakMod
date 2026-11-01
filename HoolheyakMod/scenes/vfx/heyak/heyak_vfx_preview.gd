extends Node2D
## Editor preview only. The four effect scenes do not depend on this script.

const BASE := "res://HoolheyakMod/scenes/vfx/heyak/"
const EFFECTS := ["HoolheyakProjectileVfx", "HoolheyakHitVfx", "HoolheyakStormVfx", "HoolheyakCycloneVfx"]
var _next_spawn: float = 0.0


func _ready() -> void:
	_spawn()


func _process(delta: float) -> void:
	_next_spawn -= delta
	if _next_spawn <= 0.0:
		_spawn()


func _spawn() -> void:
	_next_spawn = 2.3
	for index in EFFECTS.size():
		var packed := load(BASE + EFFECTS[index] + ".tscn") as PackedScene
		if packed == null:
			continue
		var effect := packed.instantiate() as Node2D
		match index:
			0:
				effect.call("configure_projectile", Vector2(130, 265), Vector2(490, 265), 0.65)
			1:
				effect.position = Vector2(950, 265)
			2:
				effect.position = Vector2(320, 603)
			3:
				effect.position = Vector2(950, 625)
		add_child(effect)


func _draw() -> void:
	var font := ThemeDB.fallback_font
	draw_rect(Rect2(0, 0, 1280, 800), Color("09121b"))
	draw_string(font, Vector2(42, 48), "HO'OLHEYAK / VFX SCENES", HORIZONTAL_ALIGNMENT_LEFT, -1, 26, Color("78e9de"))
	draw_string(font, Vector2(42, 78), "Original Arknights textures / Godot 2D reconstruction / effects repeat automatically", HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color("9caebd"))
	var rects := [Rect2(40, 108, 580, 290), Rect2(660, 108, 580, 290), Rect2(40, 424, 580, 344), Rect2(660, 424, 580, 344)]
	var labels := ["01  BASIC ATTACK / projectile", "02  BASIC ATTACK / impact", "03  SKILL 3 / storm", "04  CYCLONE / rising wind"]
	for index in rects.size():
		var rect: Rect2 = rects[index]
		draw_rect(rect, Color("101f2b"))
		draw_rect(rect, Color("294551"), false, 1.0)
		draw_string(font, rect.position + Vector2(18, 30), labels[index], HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Color("d0dfeb"))
		var center := rect.get_center()
		draw_line(center - Vector2(10, 0), center + Vector2(10, 0), Color("294551"))
		draw_line(center - Vector2(0, 10), center + Vector2(0, 10), Color("294551"))
