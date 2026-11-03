using HoolheyakMod.Code.Patches;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace HoolheyakMod.Scripts.Character;

// A real merchant root is required by vanilla and other mods' scene loaders.
public partial class HoolheyakMerchant : NMerchantCharacter
{
    public override void _Ready() => PlayerSkinSceneHelper.PlayMerchantLoop(this);
}
