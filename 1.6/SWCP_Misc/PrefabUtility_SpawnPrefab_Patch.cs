using HarmonyLib;
using RimWorld;
using Verse;

namespace SWCP_Misc
{
	[HarmonyPatch(typeof(PrefabUtility), nameof(PrefabUtility.SpawnPrefab))]
	public static class PrefabUtility_SpawnPrefab_Patch
	{
		public static void Prefix(PrefabDef prefab, ref Faction faction)
		{
			if (faction != null)
			{
				return;
			}
			PrefabDefExtension_Faction extension = prefab.GetModExtension<PrefabDefExtension_Faction>();
			if (extension == null || extension.factionDef == null)
			{
				return;
			}
			faction = Find.FactionManager.FirstFactionOfDef(extension.factionDef);
		}
	}
}
