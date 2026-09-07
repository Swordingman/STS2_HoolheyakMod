using BaseLib.Abstracts;
using Godot;

namespace HoolheyakMod.Code.Character;

public class HoolheyakModRelicPool : CustomRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://HoolheyakMod/images/charui/small_orb.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://HoolheyakMod/images/charui/energy_orb_p.png";
}