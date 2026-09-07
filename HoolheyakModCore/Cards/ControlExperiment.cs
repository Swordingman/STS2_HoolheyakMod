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
public class ControlExperiment : HoolheyakBaseCard, IVariableCard
{
    public ControlExperiment() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Control-Draw", 2m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        return [
            new VariableChoice(async context => {
                var phase = GetRandomPhaseCard();
                await CardPileCmd.AddGeneratedCardToCombat(phase, PileType.Draw, Owner, CardPilePosition.Random);
                await CardPileCmd.Draw(context, DynamicVars["Control-Draw"].IntValue, Owner);
            }),
            new VariableChoice(async context => {
                var phase = GetRandomPhaseCard();
                await CardPileCmd.AddGeneratedCardToCombat(phase, PileType.Discard, Owner);

                var discard = PileType.Discard.GetPile(Owner);
                foreach (var card in discard.Cards.ToList())
                {
                    await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random);
                }
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    private CardModel GetRandomPhaseCard()
    {
        var owner = Owner ?? throw new InvalidOperationException("No owner.");
        var combatState = owner.Creature.CombatState ?? throw new InvalidOperationException("No combat state.");
        int roll = owner.RunState.Rng.Shuffle.NextInt(6);

        return roll switch
        {
            0 => combatState.CreateCard<ConjunctionCard>(owner),
            1 => combatState.CreateCard<OppositionCard>(owner),
            2 => combatState.CreateCard<QuincunxCard>(owner),
            3 => combatState.CreateCard<SextileCard>(owner),
            4 => combatState.CreateCard<SquareCard>(owner),
            _ => combatState.CreateCard<TrineCard>(owner)
        };
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
