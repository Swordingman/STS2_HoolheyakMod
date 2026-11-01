extends Node2D
## Shared lifetime and optional flight for the standalone Ho'olheyak effects.
## Configure a projectile before adding it to the scene tree.

@export_range(0.05, 10.0) var duration: float = 0.7
@export var travel_distance: float = 0.0
@export var auto_free: bool = true

var _age: float = 0.0
var _origin: Vector2


func configure_projectile(start: Vector2, destination: Vector2, seconds: float = 0.42) -> void:
	position = start
	_origin = start
	rotation = (destination - start).angle()
	travel_distance = start.distance_to(destination)
	duration = maxf(seconds, 0.05)
	_age = 0.0
	if is_inside_tree():
		_sync_animation_speed()


func _ready() -> void:
	_origin = position
	_sync_animation_speed()


func _sync_animation_speed() -> void:
	var player := get_node_or_null("AnimationPlayer") as AnimationPlayer
	if player != null and player.has_animation("play"):
		player.speed_scale = player.get_animation("play").length / duration
		player.play("play")


func _process(delta: float) -> void:
	_age += delta
	if not is_zero_approx(travel_distance):
		position = _origin + Vector2.RIGHT.rotated(rotation) * travel_distance * minf(_age / duration, 1.0)
	if auto_free and _age >= duration:
		queue_free()
