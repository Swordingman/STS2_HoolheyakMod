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

public class MetaplasticSkinPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(MetaplasticSkinPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(MetaplasticSkinPower)}.png";
    private HashSet<CardPlay> _sloughPlays = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _sloughPlays = new HashSet<CardPlay>(_sloughPlays);
    }

    public override Task BeforeCardPlayed(CardPlay play)
    {
        if (Owner?.Player == play.Card.Owner && SloughMechanics.Active(Owner))
            _sloughPlays.Add(play);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (!_sloughPlays.Remove(play)) return;
        if (play.Card.Type == CardType.Attack)
            await PowerCmd.Apply<EruditionPower>(context, Owner, Amount, Owner, null);
        else if (play.Card.Type == CardType.Skill)
            await PowerCmd.Apply<MeanderPower>(context, Owner, Amount, Owner, null);
    }
}
