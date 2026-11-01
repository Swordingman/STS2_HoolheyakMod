using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
public class Surveying : HoolheyakBaseCard, IVariableCard
{
    private List<CardModel> _selectedCards = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _selectedCards = new List<CardModel>(_selectedCards);
    }

    public Surveying() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar("Surveying-Magic", 1m)
    ];

    public bool CanBeAutoTriggered => false;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext,
        CardPlay cardPlay, bool isAutoTriggered = false) =>
        StargazingLogic.GetVariableChoices(_selectedCards);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 8 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .WithHitFx(VfxCmd.starryImpactVfx)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await StargazingLogic.Execute(choiceContext, cardPlay, this, SelectionScreenPrompt,
            DynamicVars["Surveying-Magic"].IntValue, _selectedCards);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Surveying-Magic"].UpgradeValueBy(2);
    }
}
