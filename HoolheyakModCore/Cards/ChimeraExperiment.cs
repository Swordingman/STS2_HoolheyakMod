using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ChimeraExperiment : HoolheyakBaseCard, IVariableCard
{
    public ChimeraExperiment() : base(-1, CardType.Skill, CardRarity.Rare, TargetType.None, true)
    {
    }

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Chimera-Effect", 0m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        int effect = isAutoTriggered ? 2 : ResolveEnergyXValue();
        if (IsUpgraded) effect += 1;

        return [
            new VariableChoice(async context => {
                await PlayTopTypeCards(context, CardType.Attack, effect);
            }),
            new VariableChoice(async context => {
                await PlayTopTypeCards(context, CardType.Skill, effect);
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || Owner.Creature?.CombatState == null)
            return;

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    private async Task PlayTopTypeCards(PlayerChoiceContext choiceContext, CardType type, int count)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        var cards = drawPile.Cards.Where(c => c.Type == type).Take(count).ToList();

        foreach (var card in cards)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
            await Cmd.Wait(0.15f);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果由 IsUpgraded 提供：额外 +1 次
    }
}
