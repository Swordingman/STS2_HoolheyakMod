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
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class CrossExperiment : HoolheyakBaseCard, IVariableCard
{
    public CrossExperiment() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Move),
        new BlockVar(2, ValueProp.Move),
        new DynamicVar("Cross-Growth", 1m)
    ];

    public bool CanAllIn => true;

    private int _damageGrowth;
    private int _blockGrowth;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int Hoolheyak_DamageGrowth
    {
        get => _damageGrowth;
        set
        {
            AssertMutable();
            DynamicVars.Damage.BaseValue += value - _damageGrowth;
            _damageGrowth = value;
        }
    }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int Hoolheyak_BlockGrowth
    {
        get => _blockGrowth;
        set
        {
            AssertMutable();
            DynamicVars.Block.BaseValue += value - _blockGrowth;
            _blockGrowth = value;
        }
    }

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        int amount = DynamicVars["Cross-Growth"].IntValue;

        return [
            new VariableChoice(async context => {
                Hoolheyak_DamageGrowth += amount;
                if (DeckVersion is CrossExperiment deckCard && !ReferenceEquals(deckCard, this))
                    deckCard.Hoolheyak_DamageGrowth += amount;
            }),
            new VariableChoice(async context => {
                Hoolheyak_BlockGrowth += amount;
                if (DeckVersion is CrossExperiment deckCard && !ReferenceEquals(deckCard, this))
                    deckCard.Hoolheyak_BlockGrowth += amount;
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .WithHitFx(VfxCmd.starryImpactVfx)
            .Targeting(target)
            .Execute(choiceContext);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cross-Growth"].UpgradeValueBy(1);
    }
}
