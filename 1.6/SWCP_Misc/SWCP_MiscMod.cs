using HarmonyLib;
using Verse;

namespace SWCP_Misc
{
    public class SWCP_MiscMod : Mod
    {
        public SWCP_MiscMod(ModContentPack pack) : base(pack)
        {
            new Harmony("SWCP_MiscMod").PatchAll();
        }
    }
}
