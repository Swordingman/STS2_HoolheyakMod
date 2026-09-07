using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class BottledCloud : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(BottledCloud)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(BottledCloud)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(BottledCloud)}.png";

}
