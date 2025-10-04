using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace guy762_Ionization
{
    public class Projectile_IonBullet : Bullet
    {
        public ModExtention_HediffGiver Props => def.GetModExtension<ModExtention_HediffGiver>();
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing);
            if (Props != null && hitThing != null && hitThing is Pawn hitPawn && hitPawn.RaceProps.IsMechanoid) // thank you erdelf#0001 in the RW discord
            {
                float rand = Rand.Value; // everything below is identical to PlagueGun aside from some minor things I cut out
                if (rand <= Props.addHediffChance)
                {
                    Hediff plagueOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Props.hediffToAdd);
                    float randomSeverity = Rand.Range(0.15f, 0.30f);
                    if (plagueOnPawn != null)
                    {
                        plagueOnPawn.Severity += randomSeverity;
                    }
                    else
                    {
                        Hediff hediff = HediffMaker.MakeHediff(Props.hediffToAdd, hitPawn);
                        hediff.Severity = randomSeverity;
                        hitPawn.health.AddHediff(hediff);
                    }
                }
            }
        }
    }
}
