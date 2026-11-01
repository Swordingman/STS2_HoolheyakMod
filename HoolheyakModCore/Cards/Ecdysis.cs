using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Ecdysis : HoolheyakBaseCard, IVariableCard
{
    public Ecdysis() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true) { }
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext context, CardPlay play,
        bool isAutoTriggered = false) =>
    [
        new VariableChoice(c => PowerCmd.Apply<EcdysisStrengthPower>(c, Owner.Creature, 1, Owner.Creature, this)),
        new VariableChoice(c => PowerCmd.Apply<EcdysisDexterityPower>(c, Owner.Creature, 1, Owner.Creature, this))
    ];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await PlayCastAnimation();
        await VariableCmd.Choose(context, this, play);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
