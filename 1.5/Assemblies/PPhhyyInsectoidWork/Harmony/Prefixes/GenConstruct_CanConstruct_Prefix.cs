namespace PPhhyyInsectoidWork
{
    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanConstruct), typeof(Thing), typeof(Pawn), typeof(WorkTypeDef), typeof(bool), typeof(JobDef))]
    internal static class GenConstruct_CanConstruct_Prefix
    {
        /// <summary>
        /// We use this to skip the check inside the CanConstruct which returns an NRE because insects don't have worksettings.<br></br>
        /// This patch only applies if the thinktree has either
        /// JobGiver_InsectoidDeliverResourcesToBlueprints or JobGiver_InsectoidDeliverResourcesToFrames,<br></br>
        /// and ModExtension_PPhhyyInsectoids in the thinktree's def
        /// <br><br></br></br>
        /// The patch checks for that modextension and if the pawn is an insect, if it is, sets __result (the method's return type) as true, <br></br>
        /// while also returning false (in a prefix, this means it skips the method).
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        static bool AlwaysReturnTrueForInsectoids(Pawn pawn, ref bool __result)
        {
            if (pawn.RaceProps.Insect && pawn.thinker.MainThinkTree.HasModExtension<ModExtension_PPhhyyInsectoids>())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
