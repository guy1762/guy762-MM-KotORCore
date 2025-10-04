using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace guy762_Ionization
{
    public class DamageDef_Stun_guy762 : StunHandler
    {
        public DamageDef_Stun_guy762(Thing parent) : base(parent)
        {
        }

        public new void Notify_DamageApplied(DamageInfo dinfo)
        {
            if (dinfo.Def == DamageDefOf_guy762.guy762_GrenadeDamage_stun)
            {
                this.StunFor(Mathf.RoundToInt(dinfo.Amount * 30f), dinfo.Instigator, true, true);
                return;
            }
        }
    }
}
