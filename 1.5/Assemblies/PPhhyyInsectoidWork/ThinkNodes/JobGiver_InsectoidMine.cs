namespace PPhhyyInsectoidWork
{
    public class JobGiver_InsectoidMine : ThinkNode
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            // The vanilla workgiver that we're gonna use to tell the insect to work
            WorkGiver_Miner workgiver = PPhhyyWorkGiverDefOfs.Mine.Worker as WorkGiver_Miner;
            TargetInfo targetInfo = TargetInfo.Invalid;
            Thing thing = null;

            // If the pawn can't do the job or the workgiver is telling it to skip we return no job
            if (workgiver.MissingRequiredCapacity(pawn) != null || workgiver.ShouldSkip(pawn))
            {
                return ThinkResult.NoJob;
            }
            else
            {
                IEnumerable<Thing> thingsGlobal = workgiver.PotentialWorkThingsGlobal(pawn);
                TraverseParms traverseParms = TraverseParms.For(pawn, workgiver.MaxPathDanger(pawn), TraverseMode.ByPawn);
                // Off of the potential things to work at, we get an actual thing to do the job at.
                thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, thingsGlobal, workgiver.PathEndMode, traverseParms);
                // If we couldn't find a thing, or the thing is out of bounds, we return no job.
                if (thing == null || !thing.Position.IsValid || !thing.Position.InBounds(pawn.Map))
                {
                    return ThinkResult.NoJob;
                }

                // Otherwise, we return a job
                Job mineJob;
                mineJob = workgiver.JobOnThing(pawn, thing, false);
                if (mineJob != null)
                {
                    return new ThinkResult(mineJob, this, new JobTag?(workgiver.def.tagToGive), false);
                }
            }
            return ThinkResult.NoJob;
        }

        public override float GetPriority(Pawn pawn)
        {
            return 9f;
        }
    }
}
