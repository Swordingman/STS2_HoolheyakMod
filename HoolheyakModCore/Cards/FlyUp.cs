using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using HoolheyakMod.Scripts.Vfx;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class FlyUp : HoolheyakBaseCard, IVariableCard
{
    public FlyUp() : base(-1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("FlyUp-Analysis", 1m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        int effect = isAutoTriggered ? 2 : ResolveEnergyXValue();
        if (IsUpgraded) effect++;

        // 一次变量选择（包括 All-In）共用解析快照；传承记忆的成长留给后续出牌。
        int analysisSnapshot = Owner.Creature.GetPower<AnalysisPower>()?.Amount ?? 0;

        return [
            new VariableChoice(async context =>
            {
                for (int i = 0; i < effect; i++)
                    await TriggerErudition.Trigger(context, Owner.Creature, analysisSnapshot);
            }),
            new VariableChoice(async context =>
            {
                for (int i = 0; i < effect; i++)
                    await TriggerMeander.Trigger(context, Owner.Creature, analysisSnapshot);
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCastAnimation();
        HoolheyakVfx.PlayCyclone(Owner.Creature);

        await PowerCmd.Apply<AnalysisPower>(choiceContext, Owner.Creature, DynamicVars["FlyUp-Analysis"].IntValue, Owner.Creature, this);

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 升级效果由 IsUpgraded 提供：额外 +1 次
    }
}
