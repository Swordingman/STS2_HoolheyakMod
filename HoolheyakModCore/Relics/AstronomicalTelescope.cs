using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class AstronomicalTelescope : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(AstronomicalTelescope)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(AstronomicalTelescope)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(AstronomicalTelescope)}.png";

    // 装备时把一张起始打击/防御转化为随机卡牌奖励（近似原版无色卡转化）。
    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        if (Owner == null) return;

        var starters = Owner.Deck.Cards
            .Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CardTag.Defend))
            .ToList();

        CardModel? toRemove = null;
        if (starters.Count > 0)
        {
            toRemove = starters[System.Random.Shared.Next(starters.Count)];
            await CardPileCmd.RemoveFromDeck(toRemove);
        }

        var options = CardCreationOptions.ForNonCombatWithDefaultOdds(
            new[] { Owner.Character.CardPool },
            null);

        var cardReward = new CardReward(options, 3, Owner);
        await RewardsCmd.OfferCustom(Owner, new List<Reward> { cardReward });
    }
}
