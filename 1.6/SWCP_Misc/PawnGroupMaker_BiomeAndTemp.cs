using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace SWCP_Misc
{
    public class PawnGroupMaker_BiomeAndTemp : PawnGroupMaker
    {
        public List<BiomeDef> allowedBiomes;
        public FloatRange temperatureRange;

        [HarmonyPatch(typeof(PawnGroupMaker), "CanGenerateFrom")]
        public static class PawnGroupMaker_CanGenerateFrom_Patch
        {
            public static void Postfix(PawnGroupMaker __instance, ref bool __result, PawnGroupMakerParms parms)
            {
                if (__instance is PawnGroupMaker_BiomeAndTemp biomeAndTemp)
                {
                    if (biomeAndTemp.allowedBiomes != null && biomeAndTemp.allowedBiomes.Count > 0 && parms.tile.Valid)
                    {
                        if (!biomeAndTemp.allowedBiomes.Contains(Find.WorldGrid[parms.tile].PrimaryBiome))
                        {
                            __result = false;
                            return;
                        }
                    }
                    if (biomeAndTemp.temperatureRange != default(FloatRange) && parms.tile.Valid)
                    {
                        float temperature = GetTemperatureForTile(parms.tile);
                        if (!biomeAndTemp.temperatureRange.Includes(temperature))
                        {
                            __result = false;
                            return;
                        }
                    }
                }
            }

            private static float GetTemperatureForTile(PlanetTile tile)
            {
                foreach (Map map in Find.Maps)
                {
                    if (map.Tile == tile)
                    {
                        return map.mapTemperature.OutdoorTemp;
                    }
                }
                return Find.WorldGrid[tile].temperature;
            }
        }
    }
}
