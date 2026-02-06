using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using System.Linq;

namespace SWCPBiomes
{
    public class LordJob_WanderCivilians : LordJob
    {
        private IntVec3 wanderCenter;
        private float wanderRadius;

        private bool keepPopulationReplenished = false;
        private int maxPopulation = 15;
        private Faction targetFaction;
        private PawnGroupKindDef pawnGroupKind;

        private int checkInterval = 2500;

        public LordJob_WanderCivilians() { }

        public LordJob_WanderCivilians(IntVec3 wanderCenter, float wanderRadius, bool replenishPop, int maxPop, Faction faction, PawnGroupKindDef groupKind)
        {
            this.wanderCenter = wanderCenter;
            this.wanderRadius = wanderRadius;
            this.keepPopulationReplenished = replenishPop;
            this.maxPopulation = maxPop;
            this.targetFaction = faction;
            this.pawnGroupKind = groupKind ?? PawnGroupKindDefOf.Peaceful;
        }

        public override bool ShouldExistWithoutPawns => keepPopulationReplenished;

        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();

            var toilWander = new LordToil_WanderCivilians(wanderCenter, wanderRadius);
            stateGraph.AddToil(toilWander);

            var toilFlee = new LordToil_PanicFlee();
            stateGraph.AddToil(toilFlee);

            var transitionDamaged = new Transition(toilWander, toilFlee);
            transitionDamaged.AddTrigger(new Trigger_PawnHarmed(0.5f, false, null));
            stateGraph.AddTransition(transitionDamaged);

            stateGraph.StartingToil = toilWander;
            return stateGraph;
        }

        public override void LordJobTick()
        {
            base.LordJobTick();

            if (!keepPopulationReplenished) return;
            if (lord.Map == null) return;

            if (Find.TickManager.TicksGame % checkInterval != 0) return;

            if (lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer)) return;

            int currentCount = lord.ownedPawns.Count;
            if (currentCount < maxPopulation)
            {
                TrySpawnReplacementPawn();
            }
        }

        private void TrySpawnReplacementPawn()
        {
            Map map = lord.Map;

            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 spawnLoc, map, CellFinder.EdgeRoadChance_Neutral))
            {
                return;
            }

            var newPawn = GenerateSinglePawn(map.Tile, targetFaction ?? lord.faction, pawnGroupKind);

            if (newPawn != null)
            {
                GenSpawn.Spawn(newPawn, spawnLoc, map, Rot4.Random);
                lord.AddPawn(newPawn);
            }
        }

        private Pawn GenerateSinglePawn(int tile, Faction faction, PawnGroupKindDef groupKind)
        {
            var parms = new PawnGroupMakerParms
            {
                tile = tile,
                faction = faction,
                groupKind = groupKind
            };

            if (!PawnGroupMakerUtility.TryGetRandomPawnGroupMaker(parms, out var pawnGroupMaker, ignoreCommonality: true))
            {
                return null;
            }

            if (pawnGroupMaker.options.NullOrEmpty())
            {
                return null;
            }

            var validOptions = pawnGroupMaker.options.Where(o => o.kind != null).ToList();
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref wanderCenter, "wanderCenter");
            Scribe_Values.Look(ref wanderRadius, "wanderRadius", 50f);
            Scribe_Values.Look(ref keepPopulationReplenished, "keepPopulationReplenished", false);
            Scribe_Values.Look(ref maxPopulation, "maxPopulation", 15);
            Scribe_References.Look(ref targetFaction, "targetFaction");
            Scribe_Defs.Look(ref pawnGroupKind, "pawnGroupKind");
        }
    }

    public class LordToil_WanderCivilians : LordToil
    {
        private IntVec3 wanderCenter;
        private float wanderRadius;

        public LordToil_WanderCivilians(IntVec3 wanderCenter, float wanderRadius)
        {
            this.wanderCenter = wanderCenter;
            this.wanderRadius = wanderRadius;
        }

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DefsOf.WanderCivilian, wanderCenter, wanderRadius);
            }
        }
    }
}
