using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HoolheyakMod.Scripts;
using HoolheyakMod.Scripts.Cards;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class MeanderPower : CustomPowerModel
{
    public const int BaseThreshold = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(MeanderPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(MeanderPower)}.png";

    private bool _isTriggering;

    /// <summary>
    /// Amount 内部永久多保留 1 层哨兵。
    /// </summary>
    public int Progress => Math.Max(0, Amount - 1);

    /// <summary>
    /// Power 图标下方显示真实进度，而不是内部 Amount。
    /// </summary>
    public override int DisplayAmount => Progress;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new ComputedPowerVar<MeanderPower>("Amount", power => power.Progress),
        new ComputedPowerVar<MeanderPower>("Progress", power => power.Progress),
        new ComputedPowerVar<MeanderPower>("Threshold", power => power.IsMutable ? GetThreshold(power.Owner) : BaseThreshold)
    ];

    public override LocString Description
    {
        get
        {
            LocString description = base.Description;
            DynamicVars.AddTo(description);
            return description;
        }
    }

    public static int GetThreshold(Creature? owner)
    {
        int threshold = BaseThreshold;

        // 六合：逶迤触发需求 -2。
        if (owner?.GetPower<SextilePower>() != null)
            threshold -= 2;

        // 库库尔坎遗泽：每层上限 +3。
        var legacy = owner?.GetPower<KukulkanLegacyPower>();
        if (legacy != null)
            threshold += 3 * legacy.Amount;

        return Math.Max(1, threshold);
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 普通 PowerCmd.Apply 首次创建时，Amount 全部都是真实逶迤进度，因此额外补 1 层隐藏哨兵。
        SetAmount(Amount + 1, silent: true);
        await CheckAndTriggerAsync(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner?.Creature != Owner)
            return;

        if (cardPlay.Card.Tags.Contains(HoolheyakCardTags.Variable))
            return;

        bool isTriggerType = cardPlay.Card.Type == CardType.Skill;

        // 奎因库尼：逶迤改成由攻击牌提供。
        if (Owner.GetPower<QuincunxPower>() != null)
            isTriggerType = cardPlay.Card.Type == CardType.Attack;

        if (!isTriggerType)
            return;

        Flash();
        await PowerCmd.Apply<MeanderPower>(choiceContext, Owner, 1, Owner, null);
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!ReferenceEquals(power, this) || _isTriggering)
            return;

        await CheckAndTriggerAsync(choiceContext);
    }

    private async Task CheckAndTriggerAsync(PlayerChoiceContext choiceContext)
    {
        if (_isTriggering)
            return;

        _isTriggering = true;

        try
        {
            while (Progress >= GetThreshold(Owner))
            {
                int threshold = GetThreshold(Owner);

                // 只消耗真实进度，内部哨兵始终保留。
                SetAmount(Amount - threshold);
                Flash();

                await TriggerMeander.Trigger(choiceContext, Owner);
            }
        }
        finally
        {
            _isTriggering = false;
        }
    }
}
