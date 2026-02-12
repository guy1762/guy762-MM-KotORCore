using System.Linq;
using Verse;

namespace SWCP_Misc
{
    public class Verb_SuppressingFire : Verb_Shoot
    {
        public override bool TryCastShot()
        {
            bool result = base.TryCastShot();
            if (result && EquipmentSource != null && EquipmentSource.def != null && EquipmentSource.def.modExtensions != null)
            {
                VerbProperties_SuppressingFire props = EquipmentSource.def.modExtensions.OfType<VerbProperties_SuppressingFire>().FirstOrDefault();
                if (props != null && props.hediffDef != null && verbProps.ForcedMissRadius > 0f)
                {
                    foreach (Pawn pawn in GenRadial.RadialDistinctThingsAround(currentTarget.Cell, caster.Map, verbProps.ForcedMissRadius, useCenter: true).OfType<Pawn>())
                    {
                        if (pawn != caster && pawn.health != null && !pawn.health.hediffSet.HasHediff(props.hediffDef))
                        {
                            Hediff hediff = HediffMaker.MakeHediff(props.hediffDef, pawn);
                            pawn.health.AddHediff(hediff);
                            hediff = pawn.health.hediffSet.GetFirstHediffOfDef(props.hediffDef);
                            if (hediff != null && props.severityToAdd.HasValue)
                            {
                                hediff.Severity += props.severityToAdd.Value;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
