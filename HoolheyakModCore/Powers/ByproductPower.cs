using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class ByproductPower : CustomPowerModel, IVariableTriggeredPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(ByproductPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(ByproductPower)}.png";

    /// <summary>
    /// 是否给予升级后的随机牌
    /// </summary>
    public bool IsUpgraded { get; set; }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Byproduct-Upgraded", 0m)
    ];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        RefreshDynamicVars();
        return Task.CompletedTask;
    }

    private void RefreshDynamicVars()
    {
        if (DynamicVars == null)
            return;
        DynamicVars["Byproduct-Upgraded"].BaseValue = IsUpgraded ? 1m : 0m;
        InvokeDisplayAmountChanged();
    }

    /// <summary>
    /// 由 VariableChoiceCard 调用的触发器：随机生成牌加入手牌
    /// </summary>
    public async Task OnVariableTriggered(
        PlayerChoiceContext choiceContext,
        CardModel sourceCard)
    {
        if (Owner?.Player == null || Amount <= 0)
            return;

        Flash();

        var player = Owner.Player;

        var candidates = ModelDb.AllCards
            .Where(c =>
                c.Type != CardType.Status &&
                c.Type != CardType.Curse &&
                c.Rarity != CardRarity.Token
            )
            .ToList();

        if (candidates.Count == 0)
            return;

        var generatedCards =
            CardFactory.GetDistinctForCombat(
                player,
                candidates,
                Amount,
                player.RunState.Rng.CombatCardGeneration
            ).ToList();

        foreach (var card in generatedCards)
        {
            if (IsUpgraded && card.IsUpgradable)
            {
                card.UpgradeInternal();
                card.FinalizeUpgradeInternal();
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                player
            );
        }
    }
}
