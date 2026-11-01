# 霍尔海雅 Godot 特效场景

使用用户提供的 `battle/prefabs/effects/heyak.ab` 中原始贴图，在 Godot 4.5.1 中重建的第一版 2D 特效。
风暴和气旋的粒子、材质、运动由 Godot 重新实现；不是完整 Unity prefab 的一比一转换。
原包仍缺少公共材质、贴图和 Shader 依赖，因此暂未复现所有原版细节。

## 场景

| 文件 | 用途 | 默认时长 | 原点 |
|---|---|---|---|
| `HoolheyakProjectileVfx.tscn` | 向右飞行的普攻弹道 | 0.42 秒 | 发射位置 |
| `HoolheyakHitVfx.tscn` | 羽状冲击、扩散光环和碎光 | 0.62 秒 | 命中中心 |
| `HoolheyakStormVfx.tscn` | 三技能风暴风格，旋转羽片和锥形风带 | 1.65 秒 | 风暴中心 |
| `HoolheyakCycloneVfx.tscn` | 气旋、向上盘旋的风片和光粒 | 1.05 秒 | 效果中心 |
| `HoolheyakVfxPreview.tscn` | 四个效果自动循环的预览 | 循环 | 1280×800 预览画布 |

效果根节点均为 `Node2D`，加入场景树后自动播放、到期自动释放。
纯 Godot 场景、GDScript 与 Shader，不增加 C# 辅助工程或编译依赖。
`auto_free = false` 可关闭自动释放，由调用方管理生命周期。
`duration` 可在加入场景树前调整，动画播放速度会对应调整。
Shader 使用逐实例动画时间，没有使用全局 `TIME`，可随场景暂停；每个实例的材质独立。

## 在编辑器中预览

打开 `HoolheyakVfxPreview.tscn` 并按 F6。无需进入游戏，不依赖 Spine 或游戏程序集。
纹理位于 `res://HoolheyakMod/images/vfx/heyak/`，PNG 与提取结果逐字节相同。
图集区域通过 `AtlasTexture` 选择，没有改写或重新生成原始图片。

## 调用方式

使用 `ResourceLoader.Load<PackedScene>()` 加载完整资源路径，实例化为 `Node2D` 后添加到目标的特效容器，设置 `GlobalPosition` 为目标特效中心。

```csharp
var packed = ResourceLoader.Load<PackedScene>(
    "res://HoolheyakMod/scenes/vfx/heyak/HoolheyakHitVfx.tscn");
var effect = packed.Instantiate<Node2D>();
vfxContainer.AddChild(effect);
effect.GlobalPosition = hitPosition;
```

弹道可在添加到树之前调用 `configure_projectile(start, destination, seconds)`：

```csharp
var effect = packedProjectile.Instantiate<Node2D>();
effect.Call("configure_projectile", localStart, localDestination, 0.42f);
vfxContainer.AddChild(effect);
```

`start`、`destination` 均为相对所选父容器的局部坐标；方向、距离和时长会自动计算。
命中场景需在抵达时由调用方单独播放。弹道场景不处理伤害或命中判定。

原生 `VfxCmd` 的 `SceneHelper.GetScenePath()` 会补 `res://scenes/` 和 `.tscn`，不能直接把这里的完整 `res://...tscn` 路径传给它。
战斗调用统一由 `HoolheyakModCore/Scripts/Vfx/HoolheyakVfx.cs` 处理，直接加载完整路径。

已接入：

- 博览每次命中调用风暴；同一目标同时只保留一个风暴，避免连续触发叠得过亮。
- 逶迤每次命中调用冲击效果。
- 稀有攻击牌「博览者的狂语」在攻击动作后发射弹道，抵达后继续原有命中与伤害结算，每次重放都会播放。
- 稀有技能牌「飞起来！」施放时在持有者身上播放气旋。

自动按目标战斗节点的位置摆放效果；弹道将双方位置换算到所选特效容器的局部坐标。
战斗显示比例已统一放大为接入初版的两倍：风暴、命中和狂语弹道为场景原始比例的1.3倍，气旋为1.8倍。弹道飞行距离、抵达位置及播放时长不受该显示缩放影响。
博览风暴单独相对目标的特效中心上移130个画布坐标单位，使放大后的底部光环靠近目标脚下。其他三种特效保持原有锚点。
无战斗节点或在原生 TestMode 下跳过视觉与飞行等待。伤害、升力、随机目标及解析快照计算保持原样。

## 验证

Godot 4.5.1 独立项目实际渲染检查：四个场景可实例化，动画自动播放，图像非空，到期无残留节点，多实例 ShaderMaterial 不共用。
源图片及场景均不依赖本机绝对路径。预览与检查工程、截图放在项目外的独立工作目录。
