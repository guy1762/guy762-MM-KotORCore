using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using AlienRace;

namespace guy762_Ionization
{
    public static class FleshTypeDefOf
    {
        public static FleshTypeDef ATR_AndroidTier;
    }

    public class RacePropertiesATR : RaceProperties
    {
        public bool IsATRAndroid
        {
            get
            {
                return this.FleshType == FleshTypeDefOf.ATR_AndroidTier;
            }
        }
    }


    public class DamageWorker_AllDroids : DamageWorker_AddInjury // compilation worker for mechanoids + all robotic FleshTypes added by other mods
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

            if (thing is Pawn pawn && hediffDef != null && pawn.RaceProps.IsMechanoid || thing is  pawn.RaceProps.IsATRAndroid) // thank you erdelf#0001 in the RW discord for telling me how to target mechs
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
            }
            return damageResult;
        }
    }
}
