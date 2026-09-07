using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Variables;

public interface IVariableCard
{
    IReadOnlyList<VariableChoice> GetVariableChoices(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        bool isAutoTriggered = false
    );

    // 原版默认允许被析因实验等效果自动触发。
    bool CanBeAutoTriggered => true;

    // 卡牌本身是否支持星图投影 All-In。
    bool CanAllIn => false;

    // 默认 All-In 直接执行所有普通 VariableChoice。
    // 某些具有互斥状态的卡可以在这里追加修正效果。
    Task OnAllIn(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay
    ) => Task.CompletedTask;
}