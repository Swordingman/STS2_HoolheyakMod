using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class TailCareKit : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(TailCareKit)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(TailCareKit)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(TailCareKit)}.png";

    // 战斗开始时给所有存活敌人 7 层 DeconstructionPower。
    public override async Task BeforeCombatStart()
    {
        var creature = Owner?.Creature;
        if (creature?.CombatState == null) return;

        Flash();

        foreach (var enemy in creature.CombatState.Enemies.Where(e => e.IsAlive))
        {
            await PowerCmd.Apply<DeconstructionPower>(
                new ThrowingPlayerChoiceContext(),
                enemy,
                7,
                creature,
                null);
        }
    }
}
