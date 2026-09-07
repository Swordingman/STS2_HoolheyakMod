using BaseLib.Abstracts;
using HoolheyakMod.Code.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using System.Collections.Generic;

namespace HoolheyakMod.Code.Encounters;

public sealed class EnemyManifoldEncounter : CustomEncounterModel
{
    public EnemyManifoldEncounter() : base(RoomType.Monster)
    {
    }

    public override string? CustomScenePath => "res://HoolheyakMod/scenes/EnemyManifoldEncounter.tscn";
    public override bool IsValidForAct(ActModel act) => true;

    public override IReadOnlyList<string> Slots => [ "main" ];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ ModelDb.Monster<EnemyManifold>() ];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
    [
        (ModelDb.Monster<EnemyManifold>().ToMutable(), "main")
    ];
}
