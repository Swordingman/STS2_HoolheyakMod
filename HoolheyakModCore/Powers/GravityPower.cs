using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class GravityPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(GravityPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(GravityPower)}.png";

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner != null)
        {
            Owner.MaxHpChanged -= OnMaxHpChanged;
            Owner.MaxHpChanged += OnMaxHpChanged;
        }

        RecalculateGravity();

        var lift = Owner?.GetPower<LiftPower>();
        if (lift != null)
            await lift.CheckLevitateAsync(new ThrowingPlayerChoiceContext());
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        oldOwner.MaxHpChanged -= OnMaxHpChanged;
        return Task.CompletedTask;
    }

    public static int CalculateGravity(Creature owner)
    {
        int baseGravity = Math.Max(1, owner.MaxHp / 10);

        bool hasOpposition = owner.CombatState?.PlayerCreatures
            .Any(c => c.GetPower<OppositionPower>() != null) ?? false;

        return hasOpposition ? Math.Max(1, baseGravity / 2) : baseGravity;
    }

    public void RecalculateGravity()
    {
        if (Owner == null)
            return;

        SetAmount(CalculateGravity(Owner));
    }

    private void OnMaxHpChanged(int oldMaxHp, int newMaxHp)
    {
        RecalculateGravity();

        var lift = Owner?.GetPower<LiftPower>();
        if (lift != null)
            TaskHelper.RunSafely(lift.CheckLevitateAsync(new ThrowingPlayerChoiceContext()));
    }

    public async Task RecalculateAndCheckLevitateAsync(PlayerChoiceContext choiceContext)
    {
        RecalculateGravity();

        var lift = Owner?.GetPower<LiftPower>();
        if (lift != null)
            await lift.CheckLevitateAsync(choiceContext);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner?.Player == player)
            RecalculateGravity();

        return Task.CompletedTask;
    }
}