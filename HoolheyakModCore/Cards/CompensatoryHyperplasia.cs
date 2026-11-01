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
public class CompensatoryHyperplasia : HoolheyakBaseCard
{
    public CompensatoryHyperplasia() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [HoolheyakKeywords.Slough];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("CompensatoryHyperplasia-Draw", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await PlayCastAnimation();

        bool slough = SloughMechanics.Active(Owner.Creature);
        await CardPileCmd.Draw(context, DynamicVars["CompensatoryHyperplasia-Draw"].IntValue, Owner);
        var card = await SloughMechanics.SelectHandCard(context, Owner, this);
        if (card != null)
            await CardCmd.Exhaust(context, card);
        if (slough)
            await CardPileCmd.Draw(context, 1, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["CompensatoryHyperplasia-Draw"].UpgradeValueBy(1);
}
