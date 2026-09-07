using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class DistantLight : HoolheyakBaseCard
{
    public DistantLight() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DistantLight-Magic", 1m)
    ];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DistantLightPower>(choiceContext, Owner.Creature, DynamicVars["DistantLight-Magic"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
