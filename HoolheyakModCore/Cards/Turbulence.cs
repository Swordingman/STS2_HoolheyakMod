using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Turbulence : HoolheyakBaseCard
{
    public Turbulence() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar("Turbulence-Magic", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var combat = CombatState;
        if (cardPlay.Target == null || player == null || combat == null) return;

        // The native history already contains this play. Decide before awaiting
        // damage, which can trigger other cards and alter the history.
        bool shouldGenerate = ShouldGenerate(combat, player);

        // 造成 3 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .WithHitFx(VfxCmd.sandyImpactPath)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (!shouldGenerate || CombatManager.Instance.IsOverOrEnding)
            return;

        for (int i = 0; i < DynamicVars["Turbulence-Magic"].IntValue; i++)
        {
            // CreateCard assigns the owner and registers the card with this combat.
            // ToMutable alone leaves Owner/CombatState unset and cannot be added.
            var copy = combat.CreateCard<Turbulence>(player);
            if (IsUpgraded && copy.IsUpgradable)
            {
                copy.UpgradeInternal();
                copy.FinalizeUpgradeInternal();
            }

            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, player);
        }
    }

    internal static bool ShouldGenerate(ICombatState combat, Player player)
    {
        var plays = CombatManager.Instance.History.CardPlaysStarted
            .Where(entry => entry.Actor == player.Creature && entry.CardPlay.Card is Turbulence)
            .ToList();
        if (plays.Count == 0 || plays.Count % 2 != 0)
            return false;

        // At most the first pair completed during a turn produces cards. History
        // keeps this shared across all copies, without per-card or static counters.
        return !plays.Where((entry, index) => index < plays.Count - 1 && index % 2 == 1)
            .Any(entry => entry.HappenedThisTurn(combat));
    }

    protected override void OnUpgrade()
    {
        // 生成数量系数 1 -> 2
        DynamicVars["Turbulence-Magic"].UpgradeValueBy(1);
    }
}
