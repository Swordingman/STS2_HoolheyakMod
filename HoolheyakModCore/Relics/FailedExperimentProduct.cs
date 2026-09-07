using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class FailedExperimentProduct : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(FailedExperimentProduct)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(FailedExperimentProduct)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(FailedExperimentProduct)}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("FailureCount", 1m)
    ];

    // 实验计数：初始为 1，可被外部逻辑调用 IncrementCounter 增加。
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int Hoolheyak_ExperimentCounter { get; set; } = 1;

    public override bool ShowCounter => Hoolheyak_ExperimentCounter > 0;
    public override int DisplayAmount => Hoolheyak_ExperimentCounter;

    public override Task BeforeCombatStart()
    {
        RefreshDynamicVars();
        return Task.CompletedTask;
    }

    // 自定义的增加计数方法：同步刷新遗物右上角的数字。
    public void IncrementCounter()
    {
        if (Hoolheyak_ExperimentCounter < 0)
        {
            Hoolheyak_ExperimentCounter = 0;
        }

        Hoolheyak_ExperimentCounter++;
        RefreshDynamicVars();
        InvokeDisplayAmountChanged();
        Flash();
    }

    private void RefreshDynamicVars()
    {
        if (DynamicVars == null)
            return;
        DynamicVars["FailureCount"].BaseValue = Hoolheyak_ExperimentCounter;
    }
}
