using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using HoolheyakMod.Scripts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ParasiticRegrowth : HoolheyakBaseCard
{
    public ParasiticRegrowth() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [HoolheyakKeywords.Slough];
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await PlayCastAnimation();

        bool slough = SloughMechanics.Active(Owner.Creature);
        var pools = ModelDb.AllCardPools.Where(pool => !pool.IsColorless)
            .Select(pool => pool.AllCardIds.Select(id => ModelDb.GetById<CardModel>(id))
                .Where(card => card != null && card.CanBeGeneratedInCombat &&
                    card.Type is CardType.Attack or CardType.Skill or CardType.Power).ToList())
            .Where(cards => cards.Count >= 3).ToList();
        if (pools.Count == 0) return;
        var candidates = pools[Owner.RunState.Rng.CombatCardGeneration.NextInt(pools.Count)];
        var choices = CardFactory.GetDistinctForCombat(Owner, candidates, 3,
            Owner.RunState.Rng.CombatCardGeneration).ToList();
        if (choices.Count == 0) return;
        if (IsUpgraded)
            foreach (var card in choices)
                CardCmd.Upgrade(card, default);

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
        var chosen = (await CardSelectCmd.FromSimpleGrid(context, choices, Owner, prefs)).FirstOrDefault();
        if (chosen == null) return;

        if (slough)
        {
            chosen.BaseReplayCount++;
            var temporary = Owner.Creature.GetPower<TemporaryReplayPower>();
            if (temporary == null)
            {
                temporary = (TemporaryReplayPower)ModelDb.Power<TemporaryReplayPower>().ToMutable();
                temporary.Track(chosen);
                await PowerCmd.Apply(context, temporary, Owner.Creature, 1, Owner.Creature, this);
            }
            else
                temporary.Track(chosen);
        }

        var destination = PileType.Hand.GetPile(Owner).Cards.Count < CardPile.MaxCardsInHand
            ? PileType.Hand : PileType.Discard;
        await CardPileCmd.AddGeneratedCardToCombat(chosen, destination, Owner);
    }

    protected override void OnUpgrade() { }
}
