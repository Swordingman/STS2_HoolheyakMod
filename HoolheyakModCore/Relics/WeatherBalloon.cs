using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class WeatherBalloon : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(WeatherBalloon)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(WeatherBalloon)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(WeatherBalloon)}.png";

    // 供 Patch 在敌人获得“漂浮/升空”时调用：额外给予 5 层 LiftPower。
    public async Task TriggerOnEnemyLevitate(Creature target)
    {
        if (target == null || target.IsDead) return;

        var source = Owner?.Creature;
        if (source == null) return;

        Flash();
        await PowerCmd.Apply<LiftPower>(
            new ThrowingPlayerChoiceContext(),
            target,
            5,
            source,
            null);
    }
}
