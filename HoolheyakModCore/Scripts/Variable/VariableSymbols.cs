namespace HoolheyakMod.Code.Variables;

public static class VariableSymbols
{
    private static readonly string[] Symbols = {
        "α", "β", "γ", "δ", "ε", "ζ", "η", "θ",
        "ι", "κ", "λ", "μ", "ν", "ξ", "ο", "π",
        "ρ", "σ", "τ", "υ", "φ", "χ", "ψ", "ω"
    };

    public static string Get(int index)
    {
        if (index >= 0 && index < Symbols.Length) return Symbols[index];
        return (index + 1).ToString();
    }
}