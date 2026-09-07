using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class OldNotes : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(OldNotes)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(OldNotes)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(OldNotes)}.png";

    private int _attackDrawCount;
    private int _skillDrawCount;

    // 每抽到第 5 张攻击牌给予 1 层 EruditionPower，每抽到第 5 张技能牌给予 1 层 MeanderPower。
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner != Owner) return;

        var creature = Owner?.Creature;
        if (creature == null) return;

        if (card.Type == CardType.Attack)
        {
            _attackDrawCount++;
            if (_attackDrawCount >= 5)
            {
                _attackDrawCount = 0;
                Flash();
                await PowerCmd.Apply<EruditionPower>(choiceContext, creature, 1, creature, null);
            }
        }
        else if (card.Type == CardType.Skill)
        {
            _skillDrawCount++;
            if (_skillDrawCount >= 5)
            {
                _skillDrawCount = 0;
                Flash();
                await PowerCmd.Apply<MeanderPower>(choiceContext, creature, 1, creature, null);
            }
        }
    }
}
