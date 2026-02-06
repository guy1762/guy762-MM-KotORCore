using Verse;
using Verse.AI;

namespace SWCPBiomes
{
    public class JobGiver_WanderCivilian : JobGiver_Wander
    {
        public JobGiver_WanderCivilian()
        {
            this.wanderRadius = 10f;
            this.ticksBetweenWandersRange = new IntRange(125, 200);
        }

        public override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.mindState.duty.focus.Cell;
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.mindState.duty != null && pawn.mindState.duty.radius > 0)
            {
                this.wanderRadius = pawn.mindState.duty.radius;
            }

            return base.TryGiveJob(pawn);
        }
    }
}
