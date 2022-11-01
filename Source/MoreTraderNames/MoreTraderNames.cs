using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;

namespace MoreTraderNames
{
    [StaticConstructorOnStartup]
    public static class MoreTraderNames
    {
        public static string DOMAIN_NAME = "8z.moretradernames";

        static MoreTraderNames()
        {
            var harmony = new Harmony(DOMAIN_NAME);
            harmony.PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(TradeShip), MethodType.Constructor, new Type[] { typeof(TraderKindDef), typeof(Faction) })]
    public static class TradeShipConstructorPatch
    {
        static FieldInfo NamerTraderGeneral = AccessTools.Field(typeof(RulePackDefOf), "NamerTraderGeneral");

        // Replace GenerateName's first parameter from NamerTraderGeneral to our custom namer
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction i in instructions)
            {
                if (i.opcode == OpCodes.Ldsfld && i.LoadsField(NamerTraderGeneral))
                {
                    // Instead of loading RimWorld.RulePackDefOf.NamerTraderGeneral, use the return value of our function
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TradeShipConstructorPatch), nameof(GetTraderNamer)));
                } else
                {
                    yield return i;
                }
            }
        }

        static RulePackDef GetTraderNamer(TraderKindDef def)
        {
            if (def.defName == "Orbital_CombatSupplier")
            {
                return (RulePackDef) GenDefDatabase.GetDef(typeof(RulePackDef), "NamerTraderCombat");
            }
            else if(def.defName == "Orbital_Exotic")
            {
                return (RulePackDef)GenDefDatabase.GetDef(typeof(RulePackDef), "NamerTraderExotic");
            }
            else if (def.defName == "Orbital_PirateMerchant")
            {
                return (RulePackDef)GenDefDatabase.GetDef(typeof(RulePackDef), "NamerTraderPirate");
            }
            return RulePackDefOf.NamerTraderGeneral;
        }
    }
}