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
    public class DamageWorker_Mechanoids : DamageWorker_AddInjury // mechanoids only
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
            }
            return damageResult;
        }
    }

    public class DamageWorker_Organics : DamageWorker_AddInjury // all organic pawns
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

            if (thing is Pawn pawn && hediffDef != null && pawn.RaceProps.IsFlesh) // thank you erdelf#0001 in the RW discord for telling me how to target mechs
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

    public class DamageWorker_Humanlikes : DamageWorker_AddInjury // humanlikes
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

            if (thing is Pawn pawn && hediffDef != null && pawn.RaceProps.Humanlike) // thank you erdelf#0001 in the RW discord for telling me how to target mechs
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

    public class DamageWorker_Insectoids : DamageWorker_AddInjury // bugs only
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

            if (thing is Pawn pawn && hediffDef != null && pawn.RaceProps.Insect) // thank you erdelf#0001 in the RW discord for telling me how to target mechs
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

    public class DamageWorker_Animals : DamageWorker_AddInjury // animals only
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

            if (thing is Pawn pawn && hediffDef != null && pawn.RaceProps.Animal) // thank you erdelf#0001 in the RW discord for telling me how to target mechs
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
