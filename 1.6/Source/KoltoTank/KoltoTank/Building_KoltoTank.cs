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
using static HarmonyLib.Code;
using static UnityEngine.Random;

namespace KoltoTank
{
    public class Building_KoltoTank : Building_Casket, ISuspendableThingHolder
    {
        private List<string> tmpWillHealHediffs = new List<string>();

        private List<string> tmpCanHealHediffs = new List<string>();

        private List<Hediff> tmpHediffs = new List<Hediff>();
        private CompPower powerComp;
        private CompPowerTrader powerTraderComp;
        private CompRefuelable refuelableComp;
        public bool PowerOn => this.TryGetComp<CompPowerTrader>().PowerOn;
        public Building_KoltoTank Props => (Building_KoltoTank)Props;


        public Vector3 innerDrawOffset;
        public Vector3 waterDrawCenter;
        public Vector2 waterDrawSize;

        public enum KoltoTankState
        {
            Empty,
            StartFilling,
            Full
        }

        public KoltoTankState state = KoltoTankState.Empty;
        public float fillpct;

        public CompForbiddable forbiddable;

        public bool IsContainingThingPawn
        {
            get
            {
                if (!HasAnyContents)
                {
                    return false;
                }
                Pawn pawn = ContainedThing as Pawn;
                if (pawn != null)
                {
                    return true;
                }
                return false;
            }
        }

        public Pawn InnerPawn
        {
            get
            {
                if (!HasAnyContents)
                {
                    return null;
                }
                Pawn pawn = ContainedThing as Pawn;
                if (pawn != null)
                {
                    return pawn;
                }
                return null;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            forbiddable = GetComp<CompForbiddable>();
            fillpct = 0;
            KoltoTankDef tankDef = def as KoltoTankDef;
            if (tankDef != null)
            {
                innerDrawOffset = tankDef.innerDrawOffset;
                waterDrawCenter = tankDef.waterDrawCenter;
                waterDrawSize = tankDef.waterDrawSize;
            }

            // Initialize refuelableComp
            refuelableComp = GetComp<CompRefuelable>();
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                }
                state = KoltoTankState.StartFilling;
                Pawn pawn = thing as Pawn;
                return true;
            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
            {
                yield return o;
            }

            if (!PowerOn)
            {
                FloatMenuOption noPowerOption = new FloatMenuOption("NoPowerMessage".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return noPowerOption;
                yield break;
            }

            if (innerContainer.Count == 0)
            {
                if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return failer;
                }
                else
                {
                    JobDef jobDef = Kolto_DefOf.EnterKoltoTank;
                    string jobStr = "EnterKoltoTank".Translate();
                    Action jobAction = delegate ()
                    {
                        Job job = new Job(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
                }
            }
            yield break;
        }


        public static void AddCarryToKoltoTank(List<FloatMenuOption> opts, Pawn pawn, Pawn traveller)
        {
            if (!pawn.CanReserveAndReach(traveller, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
            {
                return;
            }
            Thing thing = FindKoltoTankFor(pawn, traveller);
            if (thing == null)
            {
                return;
            }
        }

        private Job MakeCarryToBiosculpterJob(Pawn willBeCarried)
        {
            return JobMaker.MakeJob(Kolto_DefOf.CarryToKoltoTank, willBeCarried, LocalTargetInfo.Invalid, this);
        }

        public override void EjectContents()
        {
            ThingDef filth_Slime = ThingDefOf.Filth_Slime;
            foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    pawn.filth.GainFilth(filth_Slime);
                    if (pawn.RaceProps.IsFlesh)
                    {
                        pawn.health.AddHediff(Kolto_DefOf.KoltoTank_Recovering, null, null, null);
                    }
                }
            }
            if (!base.Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }
            InnerPawn.health.hediffSet.hediffs.RemoveAll((Hediff x) => x.def == Kolto_DefOf.KoltoTank_Coma);
            state = KoltoTankState.Empty;
            base.EjectContents();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<KoltoTankState>(ref state, "state");
            Scribe_Values.Look<float>(ref fillpct, "fillpct");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                KoltoTankDef tankDef = def as KoltoTankDef;
                if (tankDef != null)
                {
                    innerDrawOffset = ((KoltoTankDef)def).innerDrawOffset;
                    waterDrawCenter = ((KoltoTankDef)def).waterDrawCenter;
                    waterDrawSize = ((KoltoTankDef)def).waterDrawSize;
                }
            }
            forbiddable = GetComp<CompForbiddable>();
            refuelableComp = GetComp<CompRefuelable>();
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public static Building_KoltoTank FindKoltoTankFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
                                               where typeof(Building_KoltoTank).IsAssignableFrom(def.thingClass)
                                               select def;

            foreach (ThingDef singleDef in enumerable)
            {
                Building_KoltoTank Building_KoltoTank = (Building_KoltoTank)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(singleDef), PathEndMode.InteractionCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing x)
                {
                    bool result;
                    if (!((Building_KoltoTank)x).HasAnyContents)
                    {
                        Pawn traveler2 = traveler;
                        LocalTargetInfo target = x;
                        bool ignoreOtherReservations2 = ignoreOtherReservations;
                        result = traveler2.CanReserve(target, 1, -1, null, ignoreOtherReservations2);
                    }
                    else
                    {
                        result = false;
                    }
                    return result;
                }, null, 0, -1, false, RegionType.Set_Passable, false);

                if (Building_KoltoTank != null && !Building_KoltoTank.forbiddable.Forbidden)
                {
                    KoltoTankDef koltoTankDef = Building_KoltoTank.def as KoltoTankDef;

                    if (koltoTankDef != null && (p.BodySize <= koltoTankDef.bodySizeMax) && (p.BodySize >= koltoTankDef.bodySizeMin))
                    {
                        return Building_KoltoTank;
                    }
                }
            }

            return null;
        }



        private int ticksBetweenHealing = GenDate.TicksPerHour; // Adjust the interval as needed
        private int ticksUntilNextHeal = 120;

        public override void Tick()
        {
            base.Tick();

            if (refuelableComp == null || !refuelableComp.HasFuel)
            {
                return;
            }

            switch (state)
            {
                case KoltoTankState.Empty:
                    SetPower();
                    break;
                case KoltoTankState.StartFilling:
                    if (refuelableComp != null && refuelableComp.HasFuel)
                    {
                        Hediff inducedComa = HediffMaker.MakeHediff(Kolto_DefOf.KoltoTank_Coma, InnerPawn);
                        InnerPawn.health.AddHediff(inducedComa);
                        fillpct += 0.01f;
                        if (fillpct >= 1)
                        {
                            state = KoltoTankState.Full;
                            fillpct = 0;
                        }
                    }
                    break;
                case KoltoTankState.Full:
                    SetPower();
                    if (InnerPawn.health == null && refuelableComp != null && refuelableComp.HasFuel)
                    {
                        return;
                    }

                    if (ticksUntilNextHeal > 0)
                    {
                        ticksUntilNextHeal--;
                        return;
                    }

                    bool healedOnce = false;

                    try
                    {
                        foreach (var hediff in InnerPawn.health.hediffSet.hediffs.ToList())
                        {
                            if (WillHeal(InnerPawn, hediff))
                            {
                                HealthUtility.Cure(hediff);
                                healedOnce = true;
                                break; // Break out of the loop after healing one injury
                            }
                            else if (hediff is Hediff_MissingPart { IsFresh: not false } hediff_MissingPart)
                            {
                                hediff_MissingPart.IsFresh = false;
                                InnerPawn.health.Notify_HediffChanged(hediff_MissingPart);
                                healedOnce = true;
                                break; // Break out of the loop after healing one injury
                            }
                        }

                        if (healedOnce)
                        {
                            Messages.Message("BiosculpterHealCompletedMessage".Translate(InnerPawn.Named("PAWN")), InnerPawn, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                    finally
                    {
                        ticksUntilNextHeal = ticksBetweenHealing;
                    }

                    break;
            }
        }

        public override void Draw()
        {
            PawnRenderFlags drawFlags = PawnRenderFlags.Cache;
            drawFlags |= PawnRenderFlags.Clothes;
            drawFlags |= PawnRenderFlags.Headgear;
            switch (state)
            {
                case KoltoTankState.Empty:
                    break;
                case KoltoTankState.StartFilling:
                    foreach (Thing t in innerContainer)
                    {
                        Pawn pawn = t as Pawn;
                        if (pawn != null)
                        {
                            DrawInnerThing(pawn, DrawPos + innerDrawOffset, drawFlags);
                            LiquidDraw(new Color32(123, 255, 233, 75), fillpct);
                        }
                    }
                    break;
                case KoltoTankState.Full:
                    foreach (Thing t in innerContainer)
                    {
                        Pawn pawn = t as Pawn;
                        if (pawn != null)
                        {
                            DrawInnerThing(pawn, DrawPos + innerDrawOffset, drawFlags);
                            if (refuelableComp != null && refuelableComp.HasFuel)
                            {
                                LiquidDraw(new Color32(123, 255, 233, 75), 1);
                            }
                            else
                            {
                                LiquidDraw(new Color32(123, 255, 233, 75), fillpct - 1);
                            }
                        }
                    }
                    break;
            }
            //Graphic.Draw(GenThing.TrueCenter(Position, Rot4.South, def.size, 11.7f), Rot4.South, this, 0f);
            Comps_PostDraw();
        }

        private void SetPower()
        {
            if (powerTraderComp == null)
            {
                powerTraderComp = this.TryGetComp<CompPowerTrader>();
            }
            if (powerComp == null)
            {
                powerComp = this.TryGetComp<CompPower>();
            }
            if (state == KoltoTankState.Full)
            {
                powerTraderComp.PowerOutput = 0f - powerComp.Props.PowerConsumption;
            }
            if (state == KoltoTankState.Empty)
            {
                powerTraderComp.PowerOutput = 0f - powerComp.Props.idlePowerDraw;
            }
        }

        public override void Print(SectionLayer layer)
        {
            //this.Graphic.Print(layer, this);
            Printer_Plane.PrintPlane(layer, GenThing.TrueCenter(Position, Rot4.South, def.size, 11.7f), Graphic.drawSize, Graphic.MatSingle, 0, false, null, null, 0.01f, 0f);
        }

        public virtual void LiquidDraw(Color color, float fillPct)
        {
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = DrawPos + waterDrawCenter;
            r.size = waterDrawSize;
            r.fillPercent = fillPct;
            r.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(color, false);
            r.unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0, 0, 0, 0), false);
            r.margin = 0f;
            Rot4 rotation = Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }

        public static MethodInfo pawnrender = AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[6]
    {
        typeof(Vector3),
        typeof(float),
        typeof(bool),
        typeof(Rot4),
        typeof(RotDrawMode),
        typeof(PawnRenderFlags)
    }, (Type[])null);
        public virtual void DrawInnerThing(Pawn pawn, Vector3 rootLoc, PawnRenderFlags renderFlags)
        {
            pawnrender.Invoke(pawn.Drawer.renderer, new object[6]
            {
            rootLoc,
            0,
            true,
            Rot4.South,
            RotDrawMode.Fresh,
            renderFlags
            });
        }
        private bool WillHeal(Pawn pawn, Hediff hediff)
        {
            if (!hediff.def.everCurableByItem)
            {
                return false;
            }
            if (hediff.def.chronic)
            {
                return false;
            }
            if (hediff.def.countsAsAddedPartOrImplant)
            {
                return false;
            }
            if (hediff.def == HediffDefOf.BloodLoss)
            {
                return true;
            }
            if (hediff is Hediff_Injury && !hediff.IsPermanent())
            {
                return true;
            }
            return false;
        }

        public static IEnumerable<Gizmo> CopyPasteGizmosFor(StorageSettings s)
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings", true),
                defaultLabel = "CommandCopyKoltoTankSettingsLabel".Translate(),
                defaultDesc = "CommandCopyKoltoTankSettingsDesc".Translate(),
                action = delegate ()
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                    Copy(s);
                },
                hotKey = KeyBindingDefOf.Misc4
            };
            Command_Action command_Action = new Command_Action();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings", true);
            command_Action.defaultLabel = "CommandPasteKoltoTankSettingsLabel".Translate();
            command_Action.defaultDesc = "CommandPasteKoltoTankSettingsDesc".Translate();
            command_Action.action = delegate ()
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                PasteInto(s);
            };
            command_Action.hotKey = KeyBindingDefOf.Misc5;
            if (!HasCopiedSettings)
            {
                command_Action.Disable(null);
            }
            yield return command_Action;
            yield break;
        }

        private static StorageSettings clipboard = new StorageSettings();

        private static bool copied = false;

        public static bool HasCopiedSettings
        {
            get
            {
                return copied;
            }
        }

        bool ISuspendableThingHolder.IsContentsSuspended
        {
            get
            {
                return true;
            }
        }

        public static void Copy(StorageSettings s)
        {
            clipboard.CopyFrom(s);
            copied = true;
        }

        public static void PasteInto(StorageSettings s)
        {
            s.CopyFrom(clipboard);
        }

        public class AlienSettings
        {
            // Other properties and methods in AlienSettings...

            public CompatibilityClass compatibility { get; set; }
        }

        public class CompatibilityClass
        {
            // Other properties and methods in CompatibilityClass...

            public bool IsFleshPawn(Pawn pawn)
            {
                // Implement the logic to check if the pawn is flesh.
                // For example:
                if (pawn.RaceProps.IsFlesh)
                {
                    // Additional logic specific to your mod...
                    return true;
                }
                else
                {
                    // If the pawn is not flesh, return false.
                    return false;
                }
            }
        }
    }
    public class KoltoTankDef : ThingDef
    {

        public Vector3 innerDrawOffset;

        public Vector3 waterDrawCenter;

        public Vector2 waterDrawSize;

        public float bodySizeMin;

        public float bodySizeMax;

        public float color;
    }
}



