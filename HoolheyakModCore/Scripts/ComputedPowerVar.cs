using System;
using System.Globalization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

/// <summary>
/// 实时读取 Power 当前状态的 DynamicVar。
/// 用于 smartDescription 中显示 Progress、Threshold 等动态数据。
/// </summary>
internal sealed class ComputedPowerVar<TPower> : DynamicVar
    where TPower : PowerModel
{
    private readonly Func<TPower, decimal> _getter;

    public ComputedPowerVar(string name, Func<TPower, decimal> getter)
        : base(name, 0m)
    {
        _getter = getter;
    }

    private decimal CurrentValue
    {
        get
        {
            if (_owner is TPower power)
                return _getter(power);

            return BaseValue;
        }
    }

    protected override decimal GetBaseValueForIConvertible()
    {
        return CurrentValue;
    }

    public override string ToString()
    {
        return CurrentValue.ToString(CultureInfo.InvariantCulture);
    }
}