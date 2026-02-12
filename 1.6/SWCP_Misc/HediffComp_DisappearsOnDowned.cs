using Verse;

namespace SWCP_Misc
{
    public class HediffComp_DisappearsOnDowned : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.Downed)
            {
                Pawn.health.RemoveHediff(parent);
            }
        }
    }
}
