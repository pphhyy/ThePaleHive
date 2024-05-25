namespace PPhhyyInsectoidWork
{
    public class JobGiver_InsectoidDeliverResourcesToBlueprints : ThinkNode
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            // The vanilla workgiver that we're gonna use to tell the insect to work
            WorkGiver_ConstructDeliverResourcesToBlueprints workgiver = PPhhyyWorkGiverDefOfs.ConstructDeliverResourcesToBlueprints.Worker as WorkGiver_ConstructDeliverResourcesToBlueprints;
            TargetInfo targetInfo = TargetInfo.Invalid;
            Thing thing = null;

            // If the pawn can't do the job or the workgiver is telling it to skip we return no job
            if (workgiver.MissingRequiredCapacity(pawn) != null || workgiver.ShouldSkip(pawn))
            {
                return ThinkResult.NoJob;
            }
            else
            {
                ThingRequest frame = workgiver.PotentialWorkThingRequest;

                // If the workgiver isn't skipped but there's no things to work at, we return no job
                if (frame.IsUndefined)
                {
                    return ThinkResult.NoJob;
                }

                // Off of the thingrequest, we get an actual thing to do the job at.
                TraverseParms traverseParms = TraverseParms.For(pawn, workgiver.MaxPathDanger(pawn), TraverseMode.ByPawn);
                thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, frame, workgiver.PathEndMode, traverseParms);
                if (thing == null || !thing.Position.IsValid || !thing.Position.InBounds(pawn.Map))
                {
                    // If we couldn't find a thing or if that thing's position isn't valid we return no job
                    return ThinkResult.NoJob;
                }

                // Otherwise, we do a new job.
                Job buildFrameJob;
                if (thing != null)
                {
                    buildFrameJob = workgiver.JobOnThing(pawn, thing, false);
                    if (buildFrameJob != null)
                    {
                        return new ThinkResult(buildFrameJob, this, new JobTag?(workgiver.def.tagToGive), false);
                    }
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
