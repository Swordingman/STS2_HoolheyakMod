using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(StatusCardPool))]
public class SquareCard : HoolheyakBaseCard
{
    private bool _autoConsumed;

    public SquareCard() : base(-2, CardType.Status, CardRarity.Token, TargetType.None, true)
    {
    }

    // 相位牌不可被手动打出
    protected override bool IsPlayable => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    /// <summary>
    /// 抽到手中时自动消耗并施加对应相位 Power。
    /// </summary>
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (!ReferenceEquals(card, this) || _autoConsumed)
            return;

        _autoConsumed = true;
        try
        {
            await ApplyPhase(choiceContext);
        }
        finally
        {
            _autoConsumed = false;
        }
    }

    /// <summary>
    /// 施加相位；供“过境”等选牌效果直接调用。
    /// </summary>
    public async Task ApplyPhase(PlayerChoiceContext choiceContext)
    {
        if (Owner?.Creature == null)
            return;

        await PowerCmd.Apply<SquarePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

        // 抽到时自动消耗：仅当确实在手牌中才消耗，避免在生成/预览阶段误删。
        if (Pile?.Type == PileType.Hand)
        {
            await CardCmd.Exhaust(choiceContext, this, causedByEthereal: false);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 正常情况下不可打出；若被特殊效果强制打出，也施加相位。
        await ApplyPhase(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 相位牌不参与升级
    }
}
