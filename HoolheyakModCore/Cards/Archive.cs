using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Archive : HoolheyakBaseCard, IVariableCard
{
    public Archive() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move)
    ];

    public bool CanAllIn => true;
    public bool CanBeAutoTriggered => false;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        if (Owner == null)
            return [];

        return [
            new VariableChoice(async context => {
                var options = CardCreationOptions.ForNonCombatWithDefaultOdds(
                    new[] { Owner.Character.CardPool },
                    null
                );
                var cardReward = new CardReward(options, 3, Owner);
                await RewardsCmd.OfferCustom(Owner, new List<Reward> { cardReward });
            }),
            new VariableChoice(async context => {
                var colorlessPool = ModelDb.CardPool<ColorlessCardPool>();
                var options = CardCreationOptions.ForNonCombatWithDefaultOdds(
                    new[] { colorlessPool },
                    null
                );
                var cardReward = new CardReward(options, 3, Owner);
                await RewardsCmd.OfferCustom(Owner, new List<Reward> { cardReward });
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

        if (target.CurrentHp <= 0 && Owner != null)
        {
            await VariableCmd.Choose(choiceContext, this, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
