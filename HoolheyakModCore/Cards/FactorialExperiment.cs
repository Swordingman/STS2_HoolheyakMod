using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class FactorialExperiment :
    HoolheyakBaseCard,
    IVariableCard
{
    public bool CanAllIn => true;

    // 防止析因实验自动触发另一张析因实验，
    // 再触发析因实验，最终套娃爆炸。
    public bool CanBeAutoTriggered => false;

    public FactorialExperiment()
        : base(
            2,
            CardType.Attack,
            CardRarity.Rare,
            TargetType.AnyEnemy,
            true
        )
    {
    }

    protected override IEnumerable<DynamicVar>
        CanonicalVars => [
            new DamageVar(12, ValueProp.Move),
            new DynamicVar("Factorial-Count", 3m)
        ];

    public IReadOnlyList<VariableChoice>
        GetVariableChoices(
            PlayerChoiceContext choiceContext,
            CardPlay cardPlay,
            bool isAutoTriggered = false)
    {
        return [
            // α：
            // 触发当前手牌中所有允许自动触发的变量卡。
            new VariableChoice(async context => {
                var hand =
                    PileType.Hand.GetPile(Owner);

                var handCards =
                    hand.Cards.ToList();

                foreach (CardModel handCard in handCards)
                {
                    if (ReferenceEquals(handCard, this))
                        continue;

                    await VariableCmd.TriggerRandom(
                        context,
                        handCard,
                        cardPlay
                    );
                }
            }),

            // β：
            // 生成 3 张随机变量卡，
            // 本回合费用变为 0。
            new VariableChoice(async context => {
                int count =
                    DynamicVars[
                        "Factorial-Count"
                    ].IntValue;

                for (int i = 0; i < count; i++)
                {
                    CardModel card =
                        CreateRandomVariableCard();

                    card.SetToFreeThisTurn();

                    await CardPileCmd
                        .AddGeneratedCardToCombat(
                            card,
                            PileType.Hand,
                            Owner
                        );
                }
            })
        ];
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        // 原 Java 顺序：
        // 先伤害，再变量。
        await DamageCmd.Attack(
                DynamicVars.Damage.BaseValue
            )
            .FromCardCompatibility(
                this,
                cardPlay
            )
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await VariableCmd.Choose(
            choiceContext,
            this,
            cardPlay
        );
    }

    private CardModel CreateRandomVariableCard()
    {
        var combatState =
            Owner.Creature.CombatState
            ?? throw new InvalidOperationException(
                "No combat state."
            );

        var candidates =
            ModelDb.AllCards
                .Where(card =>
                    card is IVariableCard &&
                    (
                        card.Rarity == CardRarity.Common ||
                        card.Rarity == CardRarity.Uncommon ||
                        card.Rarity == CardRarity.Rare
                    )
                )
                .ToList();

        if (candidates.Count == 0)
        {
            return combatState
                .CreateCard<CrossExperiment>(
                    Owner
                );
        }

        CardModel canonical =
            candidates[
                Owner.RunState.Rng.Shuffle
                    .NextInt(candidates.Count)
            ];

        return combatState.CreateCard(
            canonical,
            Owner
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }
}