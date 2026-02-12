using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace SWCP_Misc
{
	public class Building_LifeSupport : Building_TempControl
	{
		private bool oxygenMode = true;
		public override void TickRare()
		{
			if (!Spawned || !compPowerTrader.PowerOn)
			{
				return;
			}
			Room room = this.GetRoom();
			if (room == null)
			{
				return;
			}
			ComputeTempChange(room);
			ComputeVacuum(room);
		}

		private void ComputeTempChange(Room room)
		{
			float ambientTemperature = AmbientTemperature;
			float efficiency = (ambientTemperature < Building_LifeSupportUnit.TargetTemperature) ? 1f : ((!(ambientTemperature > Building_LifeSupportUnit.TargetTemperature + Building_LifeSupportUnit.EfficiencyFalloffSpan)) ? Mathf.InverseLerp(Building_LifeSupportUnit.TargetTemperature + Building_LifeSupportUnit.EfficiencyFalloffSpan, Building_LifeSupportUnit.TargetTemperature, ambientTemperature) : 0f);
			float tempDiff = compTempControl.TargetTemperature - ambientTemperature;
			float energyLimit = compTempControl.Props.energyPerSecond * efficiency * CompOxygenPusher.IntervalToPerSecond * (tempDiff > 0f ? 1f : -1f);
			float tempChange = GenTemperature.ControlTemperatureTempChange(Position, Map, energyLimit, compTempControl.TargetTemperature);
			bool operating = !Mathf.Approximately(tempChange, 0f);
			if (operating)
			{
				room.Temperature += tempChange;
				compPowerTrader.PowerOutput = -compPowerTrader.Props.PowerConsumption;
			}
			else
			{
				compPowerTrader.PowerOutput = (-compPowerTrader.Props.PowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
			}
			compTempControl.operatingAtHighPower = operating;
		}

		private void ComputeVacuum(Room room)
		{
			if (!Map.Biome.inVacuum || room.ExposedToSpace)
			{
				return;
			}
			float vacuumChange = (100f / room.CellCount * Building_LifeSupportUnit.AirPerSecondPerHundredCells * CompOxygenPusher.IntervalToPerSecond) * (oxygenMode ? -1f : 1f);
			room.Vacuum = Mathf.Clamp01(room.Vacuum + vacuumChange);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref oxygenMode, "oxygenMode", true);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (Faction == Faction.OfPlayer)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "SWCP_LifeSupport_OxygenMode".Translate(),
					defaultDesc = "SWCP_LifeSupport_OxygenMode_Desc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Icons/ToggleOxygenMode"),
					isActive = () => oxygenMode,
					toggleAction = delegate
					{
						oxygenMode = !oxygenMode;
					}
				};
			}
		}
	}
}
