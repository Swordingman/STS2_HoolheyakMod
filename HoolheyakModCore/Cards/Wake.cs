using System;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
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
public class Wake : HoolheyakBaseCard
{
    public Wake() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5, ValueProp.Move),
        new DynamicVar("Wake-Magic", 1m)
    ];

    public override int ConditionalReplayCount
    {
        get
        {
            if (!IsMutable || Owner == null || CombatState == null)
                return 0;

            var lastPlay = CombatManager.Instance.History.CardPlaysStarted
                .Where(entry => entry.CardPlay.Card.Owner == Owner)
                .LastOrDefault();

            // The native play loop reads the replay count before adding this play to history.
            return lastPlay?.CardPlay.Card.Type == CardType.Skill ? 1 : 0;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCastAnimation();

        // 条件重放进入原生重放计数，整张牌由原生流程重复结算。
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
        await PowerCmd.Apply<AnalysisPower>(choiceContext, Owner.Creature, DynamicVars["Wake-Magic"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 格挡 5 -> 7
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
