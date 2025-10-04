using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace guy762_Ionization
{
    public class DamageWorker_Ionization : DamageWorker_AddInjury // stuns and hediffs mechanoids
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing) // thank you Aelanna#0001 in RW discord for showing me a DamageWorker that actually works
        {
            HediffDef hediffDef = null;
            ModExtension_HediffGiver options = dinfo.Def.GetModExtension<ModExtension_HediffGiver>();
            if (options != null)
            {
                hediffDef = options.hediffToAdd;
            }
            DamageResult damageResult = base.Apply(dinfo, thing);

            if (thing is Pawn pawn && hediffDef != null && pawn.RaceProps.IsMechanoid) // thank you erdelf#0001 in the RW discord for telling me how to target mechs
            {
                Hediff hediff;
                float severity = options.severityFixed;
                if (options.hediffResistanceStat != null)
                {
                    float statValue = pawn.GetStatValue(options.hediffResistanceStat);
                    if (options.hediffResistanceStat.defaultBaseValue > 0f)
                    {
                        severity *= statValue;
                    }
                    else
                    {
                        severity *= (1 - statValue);
                    }
                }
                if (severity > 0f)
                {
                    if (options.severityVariesBySize)
                    {
                        severity /= pawn.BodySize;
                    }
                    if (options.hediffAppliedToWholeBody)
                    {
                        hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = severity;
                        pawn.health.AddHediff(hediff, null, dinfo);
                    }
                    else
                    {
                        foreach (BodyPartRecord bodyPart in damageResult.parts)
                        {
                            hediff = HediffMaker.MakeHediff(hediffDef, pawn, bodyPart);
                            hediff.Severity = severity;
                            pawn.health.AddHediff(hediff, bodyPart, dinfo);
                        }
                    }
                }
                damageResult.stunned = true;
            }
            return damageResult;
        }
    }

    public class DamageWorker_OrganicStun : DamageWorker // all organic pawns
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim) // taken from DamageWorker_Stun
        {
            DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);

            if (victim is Pawn pawn && pawn.RaceProps.IsFlesh) 
            {
                damageResult.stunned = true;
            }
            return damageResult;
        }
    }
}
