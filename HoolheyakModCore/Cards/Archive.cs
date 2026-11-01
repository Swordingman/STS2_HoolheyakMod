using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Archive : HoolheyakBaseCard, IVariableCard
{
    private List<FatalPlay> _fatalPlays = [];

    private sealed class FatalPlay(CardPlay play, bool eligible)
    {
        public CardPlay Play { get; } = play;
        public bool Eligible { get; } = eligible;
        public bool Handled { get; set; }
    }

    public Archive() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move)
    ];

    public bool CanAllIn => true;
    public bool CanBeAutoTriggered => false;

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _fatalPlays = [];
    }

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        if (Owner?.RunState.CurrentRoom is not CombatRoom room)
            return [];

        return [
            new VariableChoice(async context => {
                var options = CardCreationOptions.ForNonCombatWithDefaultOdds(
                    ModelDb.AllCharacterCardPools.ToArray(),
                    card => card.Type is CardType.Attack or CardType.Skill or CardType.Power
                );
                var cardReward = new CardReward(options, 3, Owner);
                room.AddExtraReward(Owner, cardReward);
            }),
            new VariableChoice(async context => {
                var colorlessPool = ModelDb.CardPool<ColorlessCardPool>();
                var options = CardCreationOptions.ForNonCombatWithDefaultOdds(
                    new[] { colorlessPool },
                    card => card.Type is CardType.Attack or CardType.Skill or CardType.Power
                );
                var cardReward = new CardReward(options, 3, Owner);
                room.AddExtraReward(Owner, cardReward);
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        // Like native Fatal cards, snapshot powers that prevent a death from
        // counting as Fatal before damage can remove them.
        var fatal = new FatalPlay(cardPlay,
            target.CurrentHp > 0 && target.Powers.All(power => power.ShouldOwnerDeathTriggerFatal()));
        _fatalPlays.Add(fatal);
        try
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompatibility(this, cardPlay)
                .WithHitFx(VfxCmd.starryImpactVfx)
                .Targeting(target)
                .Execute(choiceContext);
        }
        finally
        {
            _fatalPlays.Remove(fatal);
        }
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (!ReferenceEquals(cardSource, this) || Owner?.Creature != dealer ||
            result.UnblockedDamage <= 0 || target.CurrentHp > 0)
            return;

        var fatal = _fatalPlays.LastOrDefault(play => play.Play.Target == target);
        if (fatal == null || !fatal.Eligible || fatal.Handled)
            return;

        fatal.Handled = true;
        // An ordinary choice context pauses the current action and releases the
        // executor while HP is already zero. Its victory check can then open loot
        // over the unfinished choice even though this hook is awaited. Keep this
        // particular Fatal choice inside the executing action until rewards exist.
        await VariableCmd.Choose(new BlockingPlayerChoiceContext(), this, fatal.Play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
