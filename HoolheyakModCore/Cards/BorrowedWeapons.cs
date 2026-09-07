using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class BorrowedWeapons : HoolheyakBaseCard
{
    public BorrowedWeapons() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 9 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 随机生成一张非本职业攻击牌加入手牌
        var player = Owner;
        if (player != null)
        {
            var candidates = ModelDb.AllCards
                .Where(c => c.Type == CardType.Attack
                         && c.Pool != ModelDb.CardPool<HoolheyakModCardPool>())
                .ToList();

            if (candidates.Count > 0)
            {
                var generated = CardFactory.GetDistinctForCombat(
                    player,
                    candidates,
                    1,
                    player.RunState.Rng.CombatCardGeneration
                ).ToList();

                foreach (var card in generated)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 9 -> 12
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
