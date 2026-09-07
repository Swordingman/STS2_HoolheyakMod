using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class StarMapProjection : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(StarMapProjection)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(StarMapProjection)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(StarMapProjection)}.png";

    public bool CanUseAllIn(CardModel sourceCard)
    {
        return sourceCard.Owner.PlayerCombatState?.Energy >= 1;
    }

    public void OnAllIn()
    {
        Flash();
    }
}
