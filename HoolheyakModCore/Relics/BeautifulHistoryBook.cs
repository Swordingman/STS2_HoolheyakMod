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
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class BeautifulHistoryBook : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(BeautifulHistoryBook)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(BeautifulHistoryBook)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(BeautifulHistoryBook)}.png";

    // 使用字符串保存已交战过的敌人 ID 列表，兼容 STS2 的存档序列化。
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string Hoolheyak_FoughtEnemies { get; set; } = "";

    private bool ContainsEnemy(string id)
    {
        if (string.IsNullOrEmpty(Hoolheyak_FoughtEnemies)) return false;
        return Hoolheyak_FoughtEnemies.Split(',').Contains(id);
    }

    private bool AddEnemy(string id)
    {
        if (ContainsEnemy(id)) return false;
        Hoolheyak_FoughtEnemies = string.IsNullOrEmpty(Hoolheyak_FoughtEnemies)
            ? id
            : Hoolheyak_FoughtEnemies + "," + id;
        return true;
    }

    // 战斗开始时，对已经交战过的敌人降低 50% 最大生命值。
    public override async Task BeforeCombatStart()
    {
        var creature = Owner?.Creature;
        if (creature?.CombatState == null) return;

        bool activated = false;
        foreach (var enemy in creature.CombatState.Enemies.Where(e => e.IsAlive))
        {
            var id = enemy.Monster?.Id.Entry;
            if (id != null && ContainsEnemy(id))
            {
                activated = true;
                await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), enemy, enemy.MaxHp / 2, false);
            }
        }

        if (activated)
        {
            Flash();
        }
    }

    // 战斗结束后记录本场出现的敌人 ID。
    public override Task AfterCombatEnd(CombatRoom room)
    {
        var state = Owner?.Creature?.CombatState;
        if (state == null) return Task.CompletedTask;

        bool recordedNew = false;
        foreach (var enemy in state.Enemies)
        {
            var id = enemy.Monster?.Id.Entry;
            if (id != null && AddEnemy(id))
            {
                recordedNew = true;
            }
        }

        if (recordedNew)
        {
            Flash();
        }

        return Task.CompletedTask;
    }

    // 原版“右键查看已交战敌人列表”的 UI 尚未迁移；核心记录/减血机制已实现。
}
