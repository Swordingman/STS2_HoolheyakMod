using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
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
public class AugmentationExperiment : HoolheyakBaseCard, IVariableCard
{
    public AugmentationExperiment() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("Augmentation-Analysis", 1m),
        new DynamicVar("Augmentation-BonusAnalysis", 2m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        return [
            new VariableChoice(async context => {
                await PowerCmd.Apply<AnalysisPower>(
                    context,
                    Owner.Creature,
                    DynamicVars["Augmentation-Analysis"].IntValue,
                    Owner.Creature,
                    this
                );
            }),
            new VariableChoice(async context => {
                var player = Owner;
                var combat = player?.Creature.CombatState;
                if (player == null || combat == null) return;

                // Generate a fresh card. Cloning this instance carries its cost
                // reductions, free-play flags, enchantment and replay count forward.
                var generated = combat.CreateCard<AugmentationExperiment>(player);
                if (IsUpgraded && generated.IsUpgradable)
                {
                    generated.UpgradeInternal();
                    generated.FinalizeUpgradeInternal();
                }
                await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, player);
                await PowerCmd.Apply<AnalysisPower>(
                    context,
                    player.Creature,
                    DynamicVars["Augmentation-BonusAnalysis"].IntValue,
                    player.Creature,
                    this
                );
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .WithHitFx(VfxCmd.starryImpactVfx)
            .Targeting(target)
            .Execute(choiceContext);

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
