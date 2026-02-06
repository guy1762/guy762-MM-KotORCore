using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SWCPBiomes
{
    public abstract class GenStep_HumanlikePawnsBase : GenStep
    {
        protected List<PawnGenOption> options;

        protected Pawn GenerateSinglePawn(PlanetTile tile, Faction faction)
        {
            if (options.NullOrEmpty())
            {
                return null;
            }

            var validOptions = options.Where(o => o.kind != null).ToList();
            if (validOptions.Count == 0)
            {
                return null;
            }

            var selectedOption = validOptions.RandomElementByWeight(o => o.selectionWeight);
            if (selectedOption == null)
            {
                return null;
            }

            return PawnGenerator.GeneratePawn(selectedOption.kind, faction, tile);
        }

        protected Faction GetFaction(Map map, FactionDef factionDef)
        {
            if (factionDef != null) return Find.FactionManager.FirstFactionOfDef(factionDef);
            if (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) return map.ParentFaction;
            return Find.FactionManager.RandomEnemyFaction();
        }
    }
}
