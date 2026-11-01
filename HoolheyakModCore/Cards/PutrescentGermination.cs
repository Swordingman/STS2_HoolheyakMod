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
public class PutrescentGermination : HoolheyakBaseCard
{
    public PutrescentGermination() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PutrescentGermination-Count", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await PlayCastAnimation();

        var card = await SloughMechanics.SelectHandCard(context, Owner, this);
        if (card != null)
            await CardCmd.Exhaust(context, card);

        var debuffs = SloughMechanics.Debuffs(Owner.Creature);
        int count = Math.Min(DynamicVars["PutrescentGermination-Count"].IntValue, debuffs.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Owner.RunState.Rng.CombatCardSelection.NextInt(debuffs.Count);
            var debuff = debuffs[index];
            debuffs.RemoveAt(index);
            await SloughMechanics.ChangeDebuff(context, debuff, this, -1);
        }
    }

    protected override void OnUpgrade() => DynamicVars["PutrescentGermination-Count"].UpgradeValueBy(1);
}
