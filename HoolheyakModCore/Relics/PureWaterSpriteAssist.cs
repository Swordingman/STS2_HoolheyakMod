using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Monsters;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class PureWaterSpriteAssist : CustomRelicModel
{
    private Creature? _manifold;
    private int _reviveTurns;
    private bool _skipNextReviveTick;
    private int _nextMove = -1;
    public Creature? Manifold => _manifold;
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => _reviveTurns > 0;
    public override int DisplayAmount => _reviveTurns;
    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(PureWaterSpriteAssist)}.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _manifold = null;
        _reviveTurns = 0;
        _skipNextReviveTick = false;
        _nextMove = -1;
    }

    public override async Task BeforeCombatStart()
    {
        _manifold = null;
        _reviveTurns = 0;
        _skipNextReviveTick = false;
        _nextMove = -1;
        var player = Owner;
        if (player?.Creature.CombatState is not { } combat || player.PlayerCombatState == null) return;

        var manifold = combat.CreateCreature(ModelDb.Monster<FriendlyManifold>().ToMutable(),
            CombatSide.Player, null);
        manifold.PetOwner = player;
        player.PlayerCombatState.AddPetInternal(manifold);
        _manifold = manifold;
        await CreatureCmd.Add(manifold);
        await PowerCmd.Apply<PureWaterPower>(new ThrowingPlayerChoiceContext(), manifold, 1, player.Creature, null);
        Flash();
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner || _manifold == null) return;
        if (_manifold.IsDead)
        {
            // The upcoming player turn must elapse before it counts toward revival.
            if (_skipNextReviveTick)
            {
                _skipNextReviveTick = false;
                InvokeDisplayAmountChanged();
                return;
            }
            if (_reviveTurns <= 0 || --_reviveTurns > 0)
            {
                InvokeDisplayAmountChanged();
                return;
            }
            await CreatureCmd.Heal(_manifold, _manifold.MaxHp, true);
            NCombatRoom.Instance?.SetCreatureIsInteractable(_manifold, true);
            await CreatureCmd.TriggerAnim(_manifold, "Revive", 0.1f);
            Flash();
        }
        if (_manifold.Block > 0)
        {
#if STS2_BETA
            await CreatureCmd.LoseBlock(context, _manifold, _manifold.Block, null);
#else
            await CreatureCmd.LoseBlock(_manifold, _manifold.Block);
#endif
        }
        _nextMove = player.RunState.Rng.CombatCardSelection.NextInt(3);
        ((FriendlyManifold)_manifold.Monster!).ShowPlannedMove(_nextMove);
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side,
        IEnumerable<Creature> creatures)
    {
        var manifold = _manifold;
        if (side != CombatSide.Player || Owner?.Creature.IsAlive != true ||
            manifold == null || !manifold.IsAlive || _nextMove < 0) return;
        int move = _nextMove;
        _nextMove = -1;
        ((FriendlyManifold)manifold.Monster!).ShowPlannedMove(-1);
        Flash();
        switch (move)
        {
            case 0:
                await ((FriendlyManifold)manifold.Monster!).Attack(context);
                break;
            case 1:
                await CreatureCmd.GainBlock(manifold, 12, ValueProp.Move, null);
                break;
            case 2:
                await PowerCmd.Apply<StrengthPower>(context, Owner!.Creature, 1, manifold, null);
                await PowerCmd.Apply<StrengthPower>(context, manifold, 1, manifold, null);
                break;
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext context, Creature creature,
        bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature == Owner?.Creature)
        {
            _nextMove = -1;
            _reviveTurns = 0;
            _skipNextReviveTick = false;
            if (_manifold is { IsAlive: true }) await CreatureCmd.Kill(_manifold);
            InvokeDisplayAmountChanged();
            return;
        }
        if (creature == _manifold && !wasRemovalPrevented)
        {
            _reviveTurns = Owner?.Creature.IsAlive == true ? 3 : 0;
            _skipNextReviveTick = _reviveTurns > 0;
            _nextMove = -1;
            NCombatRoom.Instance?.SetCreatureIsInteractable(creature, false);
            InvokeDisplayAmountChanged();
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _manifold = null;
        _reviveTurns = 0;
        _skipNextReviveTick = false;
        _nextMove = -1;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterRemoved()
    {
        _manifold = null;
        _reviveTurns = 0;
        _skipNextReviveTick = false;
        _nextMove = -1;
        return Task.CompletedTask;
    }
}
