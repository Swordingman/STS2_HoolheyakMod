using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class BeautifulHistoryBook : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(BeautifulHistoryBook)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(BeautifulHistoryBook)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(BeautifulHistoryBook)}.png";

    // Keep the existing save key for old books. The game's own map history
    // supplies battles from before obtaining this relic, including after loading.
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string Hoolheyak_FoughtEnemies { get; set; } = "";
    private HashSet<string> _battleEnemyIds = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _battleEnemyIds = [];
    }

    private bool AddEnemy(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || RecordedIds().Contains(id)) return false;
        Hoolheyak_FoughtEnemies = string.IsNullOrEmpty(Hoolheyak_FoughtEnemies)
            ? id : Hoolheyak_FoughtEnemies + "," + id;
        return true;
    }

    private HashSet<string> RecordedIds() => Hoolheyak_FoughtEnemies
        .Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToHashSet();

    private IEnumerable<string> HistoryIds(bool excludeCurrentCombat)
    {
        if (!IsMutable || Owner == null) yield break;
        var run = Owner.RunState;
        var current = excludeCurrentCombat ? run.CurrentMapPointHistoryEntry?.Rooms.LastOrDefault() : null;
        foreach (var act in run.MapPointHistory)
            foreach (var point in act)
                foreach (var room in point.Rooms)
                {
                    // Initial monsters are written before combat starts. Do not
                    // treat the current encounter as an earlier completed battle.
                    if (ReferenceEquals(room, current)) continue;
                    foreach (var monster in room.MonsterIds) yield return monster.Entry;
                }
    }

    internal bool SyncHistory(bool excludeCurrentCombat)
    {
        bool changed = false;
        foreach (var id in HistoryIds(excludeCurrentCombat)) changed |= AddEnemy(id);
        return changed;
    }

    public IReadOnlyList<string> GetRecordedEnemyIds()
    {
        // Inspecting is local UI: never change a saved gameplay property here.
        var ids = RecordedIds();
        ids.UnionWith(HistoryIds(Owner?.Creature.CombatState != null && CombatManager.Instance.IsInProgress));
        return ids.OrderBy(id => id, System.StringComparer.Ordinal).ToArray();
    }

    public override async Task AfterObtained()
    {
        await base.AfterObtained();
        SyncHistory(Owner?.Creature.CombatState != null && CombatManager.Instance.IsInProgress);
        if (Owner?.Creature.CombatState is { } state)
            foreach (var enemy in state.Enemies)
                if (enemy.Monster is { } monster) _battleEnemyIds.Add(monster.Id.Entry);
    }

    public override async Task BeforeCombatStart()
    {
        var state = Owner?.Creature.CombatState;
        if (state == null) return;
        SyncHistory(excludeCurrentCombat: true);
        _battleEnemyIds.Clear();
        var recorded = RecordedIds();
        bool activated = false;
        foreach (var enemy in state.Enemies.Where(enemy => enemy.IsAlive))
        {
            if (enemy.Monster is not { } monster) continue;
            _battleEnemyIds.Add(monster.Id.Entry);
            if (!recorded.Contains(monster.Id.Entry)) continue;
            activated = true;
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), enemy, enemy.MaxHp / 2, false);
        }
        if (activated) Flash();
    }

    public override Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.Side == CombatSide.Enemy && creature.Monster is { } monster)
            _battleEnemyIds.Add(monster.Id.Entry);
        return Task.CompletedTask;
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        bool changed = SyncHistory(excludeCurrentCombat: false);
        foreach (var id in _battleEnemyIds) changed |= AddEnemy(id);
        _battleEnemyIds.Clear();
        if (changed) Flash();
        return Task.CompletedTask;
    }
}
