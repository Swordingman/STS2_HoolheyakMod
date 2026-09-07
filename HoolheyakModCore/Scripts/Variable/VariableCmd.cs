using HoolheyakMod.Code.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Variables;

public static class VariableCmd
{
    public static async Task Choose(
        PlayerChoiceContext choiceContext,
        CardModel sourceCard,
        CardPlay cardPlay,
        bool isAutoTriggered = false)
    {
        // 兼容已有调用：
        // 自动触发绝对不能弹 Variable 选择 UI。
        if (isAutoTriggered)
        {
            await TriggerRandom(
                choiceContext,
                sourceCard,
                cardPlay
            );

            return;
        }

        if (sourceCard is not IVariableCard variableCard)
        {
            throw new InvalidOperationException(
                $"{sourceCard.Id} does not implement IVariableCard."
            );
        }

        IReadOnlyList<VariableChoice> choices =
            variableCard.GetVariableChoices(
                choiceContext,
                cardPlay,
                false
            );

        if (choices.Count == 0)
            return;

        List<CardModel> optionCards = [];

        for (int i = 0; i < choices.Count; i++)
        {
            VariableChoiceCard option =
                sourceCard.CombatState!
                    .CreateCard<VariableChoiceCard>(
                        sourceCard.Owner
                    );

            option.Configure(
                sourceCard,
                i,
                VariableLocalization.GetChoiceDescription(
                    sourceCard,
                    i
                )
            );

            optionCards.Add(option);
        }

        // 星图投影只负责“解锁”。
        // 卡本身是否支持 All-In 由 CanAllIn 决定。
        StarMapProjection? starMap = null;

        if (variableCard.CanAllIn)
        {
            starMap = sourceCard.Owner.Relics
                .OfType<StarMapProjection>()
                .FirstOrDefault(
                    relic => relic.CanUseAllIn(sourceCard)
                );
        }

        if (starMap != null)
        {
            int index = choices.Count;

            VariableChoiceCard option =
                sourceCard.CombatState!
                    .CreateCard<VariableChoiceCard>(
                        sourceCard.Owner
                    );

            option.Configure(
                sourceCard,
                index,
                GetAllInDescription(),
                true
            );

            optionCards.Add(option);
        }

        VariableChoiceCard? selected =
            await Select(
                choiceContext,
                sourceCard,
                optionCards
            );

        if (selected == null)
            return;

        if (selected.IsAllIn)
        {
            // 原版顺序：
            // 先额外扣 1 能量，再执行所有变量。
            await PlayerCmd.LoseEnergy(
                1,
                sourceCard.Owner
            );

            starMap?.OnAllIn();

            foreach (VariableChoice choice in choices)
            {
                await choice.Effect(choiceContext);
            }

            await variableCard.OnAllIn(
                choiceContext,
                cardPlay
            );
        }
        else
        {
            await choices[selected.ChoiceIndex]
                .Effect(choiceContext);
        }

        // 一整个选择只算“一次变量”。
        //
        // All-In 即使执行多个 Effect，
        // 副产品 / 互补实验也只触发一次。
        await VariableHooks.OnVariableTriggered(
            choiceContext,
            sourceCard
        );
    }

    /// <summary>
    /// 析因实验等效果使用。
    ///
    /// 不弹 UI。
    /// 不提供 All-In。
    /// 只随机执行一个普通 VariableChoice。
    /// 成功执行后仍然算进行了一次变量。
    /// </summary>
    public static async Task<bool> TriggerRandom(
        PlayerChoiceContext choiceContext,
        CardModel sourceCard,
        CardPlay cardPlay)
    {
        if (sourceCard is not IVariableCard variableCard)
            return false;

        if (!variableCard.CanBeAutoTriggered)
            return false;

        IReadOnlyList<VariableChoice> choices =
            variableCard.GetVariableChoices(
                choiceContext,
                cardPlay,
                true
            );

        if (choices.Count == 0)
            return false;

        int index = sourceCard.Owner.RunState.Rng.Shuffle
            .NextInt(choices.Count);

        await choices[index].Effect(choiceContext);

        await VariableHooks.OnVariableTriggered(
            choiceContext,
            sourceCard
        );

        return true;
    }

    private static async Task<VariableChoiceCard?> Select(
        PlayerChoiceContext choiceContext,
        CardModel sourceCard,
        List<CardModel> optionCards)
    {
        if (optionCards.Count <= 3)
        {
            return await CardSelectCmd
                .FromChooseACardScreen(
                    choiceContext,
                    optionCards,
                    sourceCard.Owner
                ) as VariableChoiceCard;
        }

        LocString prompt = new(
            "cards",
            $"{ModelDb.Card<VariableChoiceCard>().Id.Entry}.selectionScreenPrompt"
        );

        CardSelectorPrefs prefs =
            new(prompt, 1);

        return (
            await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                optionCards,
                sourceCard.Owner,
                prefs
            )
        ).FirstOrDefault() as VariableChoiceCard;
    }

    private static string GetAllInDescription()
    {
        LocString loc = new(
            "cards",
            $"{ModelDb.Card<VariableChoiceCard>().Id.Entry}.allInDescription"
        );

        return loc.GetFormattedText();
    }
}