using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class PureWaterSpriteAssist : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(PureWaterSpriteAssist)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(PureWaterSpriteAssist)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(PureWaterSpriteAssist)}.png";

    // 近似：每回合随机执行水精灵的“加 12 格挡”或“给玩家 +1 力量”。
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player != Owner) return;

        Flash();

        int move = System.Random.Shared.Next(2);
        if (move == 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, 12, ValueProp.Move, null);
        }
        else
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
        }
    }
}
