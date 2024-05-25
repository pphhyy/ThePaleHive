global using System;
global using System.Collections.Generic;
global using System.Reflection;
global using RimWorld;
global using Verse;
global using UnityEngine;
global using Verse.AI;
global using HarmonyLib;

namespace PPhhyyInsectoidWork
{
    [StaticConstructorOnStartup]
    public static class PPhhyyInsectoidWork
    {
        static PPhhyyInsectoidWork()
        {
            Harmony harmony = new("PPhhyy.InsectoidWork");
            harmony.PatchAll();
        }
    }
}
