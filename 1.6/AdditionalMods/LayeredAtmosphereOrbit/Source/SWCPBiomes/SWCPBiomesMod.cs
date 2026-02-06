using HarmonyLib;
using Verse;

namespace SWCPBiomes
{
    public class SWCPBiomesMod : Mod
    {
        public SWCPBiomesMod(ModContentPack pack) : base(pack)
        {
            new Harmony("SWCPBiomesMod").PatchAll();
        }
    }
}
