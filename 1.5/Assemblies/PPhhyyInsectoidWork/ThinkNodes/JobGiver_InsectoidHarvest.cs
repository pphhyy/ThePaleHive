namespace PPhhyyInsectoidWork
{
    public class JobGiver_InsectoidHarvest : ThinkNode
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            // The vanilla workgiver that we're gonna use to tell the insect to work
            WorkGiver_GrowerHarvest workgiver = PPhhyyWorkGiverDefOfs.GrowerHarvest.Worker as WorkGiver_GrowerHarvest;
            TargetInfo targetInfo = TargetInfo.Invalid;

            // If the pawn can't do the job or the workgiver is telling it to skip we return no job
            if (workgiver.MissingRequiredCapacity(pawn) != null || workgiver.ShouldSkip(pawn))
            {
                return ThinkResult.NoJob;
            }
            else
            {
                foreach (IntVec3 cell in workgiver.PotentialWorkCellsGlobal(pawn))
                {
                    // Checks if the cells to work are allowed for the pawn, inside the map and if they even have a job to do in them in the first place.
                    if (!cell.IsForbidden(pawn) && cell.InBounds(pawn.Map) && workgiver.HasJobOnCell(pawn, cell))
                    {
                        // Will skip cells that aren't reachable by the pawn if the workgiver doesn't allow a pawn to try to go to unreachable tiles
                        if (!workgiver.AllowUnreachable && !pawn.CanReach(cell, workgiver.PathEndMode, workgiver.MaxPathDanger(pawn)))
                        {
                            continue;
                        }
                        // If it finds a good targetinfo, it breaks out of the loop
                        targetInfo = new TargetInfo(cell, pawn.Map, false);
                        if (targetInfo.IsValid)
                        {
                            break;
                        }
                    }
                }

                // If that targetinfo is out of bounds we return no job
                if (!targetInfo.IsValid || !targetInfo.Cell.InBounds(pawn.Map))
                {
                    return ThinkResult.NoJob;
                }
                // Otherwise, we return a job
                Job harvestJob;
                harvestJob = workgiver.JobOnCell(pawn, targetInfo.Cell, false);
                if (harvestJob != null)
                {
                    return new ThinkResult(harvestJob, this, new JobTag?(workgiver.def.tagToGive), false);
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
