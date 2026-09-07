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
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Deepthink : HoolheyakBaseCard
{
    public Deepthink() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Deepthink-Analysis", 2m),
        new DynamicVar("Deepthink-Magic", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 2 层解析，以及 2 层博览和 2 层逶迤
        await PowerCmd.Apply<AnalysisPower>(choiceContext, Owner.Creature, DynamicVars["Deepthink-Analysis"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<EruditionPower>(choiceContext, Owner.Creature, DynamicVars["Deepthink-Magic"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<MeanderPower>(choiceContext, Owner.Creature, DynamicVars["Deepthink-Magic"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 解析 2 -> 4
        DynamicVars["Deepthink-Analysis"].UpgradeValueBy(2);
    }
}
