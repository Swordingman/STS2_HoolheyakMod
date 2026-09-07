using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
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
public class Convection : HoolheyakBaseCard
{
    public Convection() : base(4, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(18, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 18 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

    }

    /// <summary>
    /// 本回合每打出 1 张攻击牌，本牌费用 -1（最低 0）。
    /// </summary>
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (card == this && CombatState != null)
        {
            int attackCount = CombatManager.Instance.History.CardPlaysStarted
                .Count(entry =>
                    entry.HappenedThisTurn(CombatState) &&
                    entry.CardPlay.Card.Type == CardType.Attack);

            modifiedCost = System.Math.Max(0, originalCost - attackCount);
            return true;
        }

        modifiedCost = originalCost;
        return false;
    }

    protected override void OnUpgrade()
    {
        // 费用 4 -> 3，伤害 18 -> 22
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
