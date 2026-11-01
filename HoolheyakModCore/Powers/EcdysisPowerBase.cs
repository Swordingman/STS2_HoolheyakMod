using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public abstract class EcdysisPowerBase : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{GetType().Name}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{GetType().Name}.png";

    public async Task OnDebuffRemoved()
    {
        if (Owner == null) return;
        Flash();
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<AnalysisPower>(context, Owner, Amount, Owner, null);
        await GrantStat(context);
    }

    protected abstract Task GrantStat(PlayerChoiceContext context);
}
