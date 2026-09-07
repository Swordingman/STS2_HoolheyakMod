using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
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
public class UniversalMapping : HoolheyakBaseCard
{
    public UniversalMapping() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        // EASY(0)=3, NORMAL(1)=2, HARD(2)=1；升级后 +1
        new DynamicVar("UniversalMapping-Count", HoolheyakConfig.CurrentDifficulty switch
        {
            0 => 3m,
            2 => 1m,
            _ => 2m
        })
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        var player = Owner;
        if (player == null || player.Creature?.CombatState == null)
            return;

        var candidates = ModelDb.AllCardPools
            .SelectMany(pool => pool.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card != null
                && card is not HoolheyakBaseCard
                && card is not UniversalMapping
                && card.Type == CardType.Attack
                && card.Rarity == CardRarity.Rare)
            .ToList();

        if (candidates.Count == 0)
            return;

        int count = DynamicVars["UniversalMapping-Count"].IntValue;
        for (int i = 0; i < count; i++)
        {
            var randomCard = CardFactory.GetDistinctForCombat(player, candidates, 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
            if (randomCard == null)
                continue;

            randomCard.SetToFreeThisTurn();
            await CardCmd.AutoPlay(choiceContext, randomCard, target);
            await Cmd.Wait(0.2f);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["UniversalMapping-Count"].UpgradeValueBy(1);
    }
}
