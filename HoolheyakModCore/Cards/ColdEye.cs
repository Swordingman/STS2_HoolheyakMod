using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
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
public class ColdEye : HoolheyakBaseCard
{
    public ColdEye() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move),
        new DynamicVar("ColdEye-Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || Owner.Creature == null) return;

        // 若本回合打出过攻击牌则获得解析，否则获得格挡
        bool hasDealtDamage = false;
        if (CombatState != null)
        {
            hasDealtDamage = CombatManager.Instance.History.CardPlaysStarted
                .Any(entry => entry.HappenedThisTurn(CombatState)
                    && entry.CardPlay.Card.Owner == Owner
                    && entry.CardPlay.Card.Type == CardType.Attack);
        }

        if (hasDealtDamage)
        {
            await PowerCmd.Apply<AnalysisPower>(choiceContext, Owner.Creature, DynamicVars["ColdEye-Magic"].IntValue, Owner.Creature, this);
        }
        else
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        // 格挡 12 -> 15，解析 3 -> 4
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["ColdEye-Magic"].UpgradeValueBy(1);
    }
}
