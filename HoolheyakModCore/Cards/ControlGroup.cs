using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ControlGroup : HoolheyakBaseCard, IVariableCard
{
    private CardType _variableType = CardType.Attack;

    public override CardType Type => _variableType;

    public ControlGroup() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("ControlGroup-Magic", 1m)
    ];

    public bool CanAllIn => true;
    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        return [
            new VariableChoice(async context => {
                _variableType = CardType.Attack;

                if (isAutoTriggered)
                    await GrantKeywordForType(context, CardType.Attack);
            }),

            new VariableChoice(async context => {
                _variableType = CardType.Skill;

                if (isAutoTriggered)
                    await GrantKeywordForType(context, CardType.Skill);
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        await VariableCmd.Choose(choiceContext, this, cardPlay);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await PowerCmd.Apply<WeightlessPower>(choiceContext, cardPlay.Target, DynamicVars["ControlGroup-Magic"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    public async Task OnAllIn(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // α + β 顺序执行后最终 Type == Skill，
        // 所以正常 AfterCardPlayed 会自动处理 Skill。
        //
        // 这里补上缺失的 Attack 对应关键词。
        await GrantKeywordForType(
            choiceContext,
            CardType.Attack
        );
    }

    private async Task GrantKeywordForType(PlayerChoiceContext choiceContext, CardType type)
    {
        bool quincunx =
            Owner.Creature.GetPower<QuincunxPower>() != null;

        if (type == CardType.Attack)
        {
            if (quincunx)
            {
                await PowerCmd.Apply<MeanderPower>(
                    choiceContext,
                    Owner.Creature,
                    1,
                    Owner.Creature,
                    this
                );
            }
            else
            {
                await PowerCmd.Apply<EruditionPower>(
                    choiceContext,
                    Owner.Creature,
                    1,
                    Owner.Creature,
                    this
                );
            }
        }
        else if (type == CardType.Skill)
        {
            if (quincunx)
            {
                await PowerCmd.Apply<EruditionPower>(
                    choiceContext,
                    Owner.Creature,
                    1,
                    Owner.Creature,
                    this
                );
            }
            else
            {
                await PowerCmd.Apply<MeanderPower>(
                    choiceContext,
                    Owner.Creature,
                    1,
                    Owner.Creature,
                    this
                );
            }
        }
    }
}