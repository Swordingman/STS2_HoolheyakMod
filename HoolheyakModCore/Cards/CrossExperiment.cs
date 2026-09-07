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
public class CrossExperiment : HoolheyakBaseCard, IVariableCard
{
    public CrossExperiment() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new BlockVar(4, ValueProp.Move),
        new DynamicVar("Cross-Growth", 1m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        int amount = DynamicVars["Cross-Growth"].IntValue;

        return [
            new VariableChoice(async context => {
                DynamicVars.Damage.UpgradeValueBy(amount);

                foreach (var deckCard in Owner.Deck.Cards.Where(c => c.Id == Id && !ReferenceEquals(c, this)))
                {
                    deckCard.DynamicVars.Damage.UpgradeValueBy(amount);
                }
            }),
            new VariableChoice(async context => {
                DynamicVars.Block.UpgradeValueBy(amount);

                foreach (var deckCard in Owner.Deck.Cards.Where(c => c.Id == Id && !ReferenceEquals(c, this)))
                {
                    deckCard.DynamicVars.Block.UpgradeValueBy(amount);
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

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cross-Growth"].UpgradeValueBy(1);
    }
}
