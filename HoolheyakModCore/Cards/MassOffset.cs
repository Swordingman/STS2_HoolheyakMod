using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class MassOffset : HoolheyakBaseCard
{
    public MassOffset() : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MassOffset-Bonus", 0m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null || player.Creature == null || player.Creature.CombatState == null)
            return;

        int effect = ResolveEnergyXValue() + DynamicVars["MassOffset-Bonus"].IntValue;
        if (effect <= 0)
            return;

        foreach (var enemy in player.Creature.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<LiftPower>(choiceContext, enemy, effect, player.Creature, this);
        }

        await PowerCmd.Apply<AnalysisPower>(choiceContext, player.Creature, effect, player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MassOffset-Bonus"].UpgradeValueBy(1);
    }
}
