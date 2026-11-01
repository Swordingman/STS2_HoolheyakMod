using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NativePowers = MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Refutation : HoolheyakBaseCard
{
    public Refutation() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 14 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .WithHitFx(VfxCmd.rockShatterPath)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (!cardPlay.Target.IsAlive) return;
        var bonuses = GetReducibleBonuses(cardPlay.Target);
        if (bonuses.Count > 0)
        {
            var bonus = Owner.RunState.Rng.CombatCardSelection.NextItem(bonuses);
            if (bonus?.StackType == PowerStackType.None)
                await PowerCmd.Remove(bonus);
            else if (bonus != null)
                await PowerCmd.ModifyAmount(choiceContext, bonus, GetWeakeningDelta(bonus), Owner.Creature, this);
        }
    }

    // Innate attributes and powers that drive death/phase transitions are not
    // ordinary bonuses. Never let dispelling a buff bypass a combat's lifecycle.
    private static bool IsProtectedMechanic(PowerModel power) => power is
        GravityPower or NativePowers.AdaptablePower or NativePowers.ReattachPower or
        NativePowers.IllusionPower or NativePowers.SurprisePower or
        PureWaterPower or EnemyPureWaterPower;

    internal static List<PowerModel> GetReducibleBonuses(Creature target) =>
        target.Powers.Where(power => power.Amount > 0 && !IsProtectedMechanic(power) &&
            power.TypeForCurrentAmount == PowerType.Buff).ToList();

    // These amounts are damage caps or counters towards an enemy's next benefit.
    // Lowering them strengthens the enemy, so weakening instead raises them.
    // Charges/durations such as Slippery, Intangible and Flutter still lose a stack.
    internal static int GetWeakeningDelta(PowerModel power) => power is
        NativePowers.HardenedShellPower or NativePowers.HardToKillPower or
        NativePowers.AsleepPower or NativePowers.SlumberPower or
        NativePowers.HatchPower or NativePowers.EscapeArtistPower or
        NativePowers.BattlewornDummyTimeLimitPower or NativePowers.SandpitPower or
        NativePowers.TheBombPower ? 1 : -1;

    protected override void OnUpgrade()
    {
        // 伤害 14 -> 17
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
