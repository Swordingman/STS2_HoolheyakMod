using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class ZodiacModel : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(ZodiacModel)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(ZodiacModel)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(ZodiacModel)}.png";

    // 近似：每回合开始时给所有敌人施加 2 层升力（原版基于意图变化触发）。
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner?.Creature?.CombatState == null || player != Owner) return;

        var source = Owner.Creature;
        var enemies = Owner.Creature.CombatState
            .GetOpponentsOf(source)
            .Where(e => e != null && !e.IsDead)
            .ToList();

        if (enemies.Count == 0) return;

        Flash();

        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<LiftPower>(choiceContext, enemy, 2, source, null);
        }
    }
}
