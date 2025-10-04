using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace guy762_Ionization
{
    public class ModExtension_HediffGiver : DefModExtension
    {
        public HediffDef hediffToAdd;
        public StatDef hediffResistanceStat;
        public float severityFixed = 0.1f;
        public bool severityVariesBySize = true;
        public bool hediffAppliedToWholeBody = true;
    }
}
