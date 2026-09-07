using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Variables;

[Pool(typeof(HoolheyakModCardPool))]
public class VariableChoiceCard : HoolheyakBaseCard
{
    private CardModel? _sourceCard;
    private string _choiceDescription = "";

    public int ChoiceIndex { get; private set; }
    public bool IsAllIn { get; private set; }

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override int MaxUpgradeLevel => 0;
    public override string PortraitPath => _sourceCard?.PortraitPath ?? CardModel.MissingPortraitPath;

    public VariableChoiceCard() : base(-1, CardType.Status, CardRarity.Token, TargetType.None, false)
    {
    }

    public void Configure(CardModel sourceCard, int index, string description, bool isAllIn = false)
    {
        AssertMutable();

        _sourceCard = sourceCard;
        ChoiceIndex = index;
        IsAllIn = isAllIn;
        _choiceDescription = description;

        TitleLocString.Add("Symbol", VariableSymbols.Get(index));

        if (isAllIn)
            EnergyCost.SetCustomBaseCost(1);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        description.Add("ChoiceDescription", _choiceDescription);
    }
}