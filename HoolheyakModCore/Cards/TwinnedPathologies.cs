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
public class TwinnedPathologies : HoolheyakBaseCard, IVariableCard
{
    public TwinnedPathologies() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [HoolheyakKeywords.Slough];
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move)];

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext context, CardPlay play, bool isAutoTriggered = false)
    {
        if (isAutoTriggered && !SloughMechanics.Active(Owner.Creature)) return [];
        return [
        new VariableChoice(async c =>
        {
            await SloughMechanics.ChangeRandomDebuff(c, Owner.Creature, this, -1);
            await CardPileCmd.Draw(c, 1, Owner);
        }),
        new VariableChoice(async c =>
        {
            await SloughMechanics.ChangeRandomDebuff(c, Owner.Creature, this, 1);
            await PowerCmd.Apply<AnalysisPower>(c, Owner.Creature, 1, Owner.Creature, this);
        })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await PlayCastAnimation();

        bool slough = SloughMechanics.Active(Owner.Creature);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        if (slough)
            await VariableCmd.Choose(context, this, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}
