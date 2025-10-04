using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;
using RimWorld.Planet;
namespace KoltoTank
{
    [StaticConstructorOnStartup]
    public static class KoltoTankPatches
    {
        private static readonly Type patchType = typeof(KoltoTankPatches);


        static KoltoTankPatches()
        {
            Harmony harmonyInstance = new Harmony("com.KoltoTank.rimworld.mod");
            harmonyInstance.Patch(
                original: AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                prefix: new HarmonyMethod(patchType, "Prefix_AddHumanlikeOrders", null));
        }

        public static bool Prefix_AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (LocalTargetInfo localTargetInfo3 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    LocalTargetInfo localTargetInfo4 = localTargetInfo3;
                    Pawn victim = (Pawn)localTargetInfo4.Thing;
                    if (victim.Downed && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) && Building_KoltoTank.FindKoltoTankFor(victim, pawn, true) != null)
                    {
                        string text4 = "CarryToKoltoTank".Translate(localTargetInfo4.Thing.LabelCap, localTargetInfo4.Thing);
                        JobDef jDef = Kolto_DefOf.CarryToKoltoTank;
                        Action action3 = delegate ()
                        {
                            Building_KoltoTank Building_KoltoTank = Building_KoltoTank.FindKoltoTankFor(victim, pawn, false);
                            if (Building_KoltoTank == null)
                            {
                                Building_KoltoTank = Building_KoltoTank.FindKoltoTankFor(victim, pawn, true);
                            }
                            if (Building_KoltoTank == null)
                            {
                                Messages.Message("CannotCarryToKoltoTank".Translate() + ": " + "NoKoltoTank".Translate(), victim, MessageTypeDefOf.RejectInput, false);
                                return;
                            }
                            Job job = new Job(jDef, victim, Building_KoltoTank);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        string label = text4;
                        Action action2 = action3;
                        Pawn revalidateClickTarget = victim;
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, revalidateClickTarget, 0f, null, null), pawn, victim, "ReservedBy"));
                    }
                }
            }
            return true;
        }
    }
}