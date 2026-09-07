using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
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

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("PoweredExoskeleton-Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 3 层多层护甲
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["PoweredExoskeleton-Magic"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 多层护甲 3 -> 4
        DynamicVars["PoweredExoskeleton-Magic"].UpgradeValueBy(1);
    }
}
