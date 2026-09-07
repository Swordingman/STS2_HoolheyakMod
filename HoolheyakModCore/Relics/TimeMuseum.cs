using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class TimeMuseum : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(TimeMuseum)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(TimeMuseum)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(TimeMuseum)}.png";

    private bool _attackPlayed;
    private bool _skillPlayed;
    private bool _powerPlayed;

    // 每场战斗开始时重置“首次打出”标记。
    public override Task BeforeCombatStart()
    {
        _attackPlayed = false;
        _skillPlayed = false;
        _powerPlayed = false;
        return Task.CompletedTask;
    }

    // 战斗内第一次打出攻击/技能/能力时，将其额外复读一次。
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner != Owner) return playCount;

        bool isFirst = false;
        switch (card.Type)
        {
            case CardType.Attack when !_attackPlayed:
                _attackPlayed = true;
                isFirst = true;
                break;
            case CardType.Skill when !_skillPlayed:
                _skillPlayed = true;
                isFirst = true;
                break;
            case CardType.Power when !_powerPlayed:
                _powerPlayed = true;
                isFirst = true;
                break;
        }

        if (isFirst)
        {
            Flash();
            return playCount + 1;
        }

        return playCount;
    }
}
