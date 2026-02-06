using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SWCPBiomes
{
    public class GenStep_WanderingCivilians : GenStep_HumanlikePawnsBase
    {
        public FactionDef factionDef;
        public IntRange initialCivilianCount = new IntRange(5, 10);

        public bool keepPopulationReplenished = false;
        public int maxPopulation = 15;

        public override int SeedPart => 8675309;

        public override void Generate(Map map, GenStepParams parms)
        {
            var faction = GetFaction(map, factionDef);
            if (faction == null)
            {
                Log.Error("SWCPBiomes: Could not find valid faction for GenStep_WanderingCivilians.");
                return;
            }

            var lordJob = new LordJob_WanderCivilians(
                map.Center,
                50f,
                keepPopulationReplenished,
                maxPopulation,
                faction,
                null
            );

            var lord = LordMaker.MakeNewLord(faction, lordJob, map);

            int countToSpawn = initialCivilianCount.RandomInRange;
            for (int i = 0; i < countToSpawn; i++)
            {
                var pawn = GenerateSinglePawn(map.Tile, faction);
                if (pawn != null)
                {
                    CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Fogged(map), map, 1000, out IntVec3 loc);
                    GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
                    lord.AddPawn(pawn);
                }
            }
        }
    }
}
