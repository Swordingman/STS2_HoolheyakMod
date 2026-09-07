using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class StarryRevelation : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(StarryRevelation)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(StarryRevelation)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(StarryRevelation)}.png";

    // 战斗开始时，按敌方已有 GravityPower 层数给予一半 LiftPower（近似原版精英/Boss 触发）。
    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature?.CombatState == null) return;

        Flash();

        var ctx = new ThrowingPlayerChoiceContext();
        var source = Owner.Creature;

        var enemies = Owner.Creature.CombatState
            .GetOpponentsOf(source)
            .Where(e => e != null && !e.IsDead)
            .ToList();

        foreach (var enemy in enemies)
        {
            var gravity = enemy.GetPower<GravityPower>();
            if (gravity == null || gravity.Amount <= 0) continue;

            int liftAmount = (int)(gravity.Amount / 2);
            if (liftAmount > 0)
            {
                await PowerCmd.Apply<LiftPower>(ctx, enemy, liftAmount, source, null);
            }
        }
    }
}
