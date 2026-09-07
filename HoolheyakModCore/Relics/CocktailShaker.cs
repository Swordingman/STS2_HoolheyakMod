using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class CocktailShaker : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(CocktailShaker)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(CocktailShaker)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(CocktailShaker)}.png";

    // 装备时 +3 最大生命值（原版 Java 只有 onEquip，没有卸载回退）。
    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        if (Owner?.Creature != null)
        {
            await CreatureCmd.GainMaxHp(Owner.Creature, 3);
        }
    }

    // 打出非本角色颜色（含无色）的卡牌时回复 1 点生命。
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var card = cardPlay.Card;
        if (player == null || card.Owner != player || player.Creature == null)
            return;

        // 使用卡池是否相同近似“卡牌颜色是否不同”。
        if (card.Pool != player.Character.CardPool)
        {
            Flash();
            await CreatureCmd.Heal(player.Creature, 1);
        }
    }
}
