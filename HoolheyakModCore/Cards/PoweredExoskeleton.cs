using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class PoweredExoskeleton : HoolheyakBaseCard
{
    public PoweredExoskeleton() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [HoolheyakKeywords.Slough];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("PoweredExoskeleton-Magic", 3m)
    ];

    public override int ConditionalReplayCount =>
        IsMutable && Owner?.Creature is { CombatState: not null } creature &&
        SloughMechanics.Active(creature) ? 1 : 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCastAnimation();

        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["PoweredExoskeleton-Magic"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PoweredExoskeleton-Magic"].UpgradeValueBy(1);
    }
}
