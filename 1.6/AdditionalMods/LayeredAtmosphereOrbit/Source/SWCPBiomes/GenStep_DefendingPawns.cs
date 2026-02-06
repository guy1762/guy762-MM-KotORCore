using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SWCPBiomes
{
    public class GenStep_DefendingPawns : GenStep_HumanlikePawnsBase
    {
        public FactionDef factionDef;

        public List<ThingDef> beaconBuildingDefs;
        public IntRange pawnsPerBeacon = new IntRange(2, 4);
        public float defendRadius = 10f;

        public override int SeedPart => 8675309;

        public override void Generate(Map map, GenStepParams parms)
        {
            var faction = GetFaction(map, factionDef);
            if (faction == null)
            {
                Log.Error("SWCPBiomes: Could not find valid faction for GenStep_DefendingPawns.");
                return;
            }

            if (beaconBuildingDefs.NullOrEmpty()) return;

            var allBeacons = new List<Thing>();
            foreach (var beaconDef in beaconBuildingDefs)
            {
                allBeacons.AddRange(map.listerThings.ThingsOfDef(beaconDef));
            }

            if (allBeacons.Count == 0)
            {
                Log.Warning($"SWCPBiomes: GenStep_DefendingPawns found 0 beacon buildings. Check GenStep Order. Pawns running at {this.def.order}.");
                return;
            }

            foreach (var beacon in allBeacons)
            {
                int countToSpawn = pawnsPerBeacon.RandomInRange;
                if (countToSpawn <= 0) continue;

                var lord = LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(beacon.Position, defendRadius), map);

                for (int i = 0; i < countToSpawn; i++)
                {
                    Pawn newPawn = GenerateSinglePawn(map.Tile, faction);

                    if (newPawn != null)
                    {
                        IntVec3 spawnLoc = CellFinder.RandomClosewalkCellNear(beacon.Position, map, (int)defendRadius, null);
                        if (!spawnLoc.Standable(map)) spawnLoc = beacon.Position;

                        GenSpawn.Spawn(newPawn, spawnLoc, map);
                        lord.AddPawn(newPawn);
                    }
                }
            }
        }
    }
}
