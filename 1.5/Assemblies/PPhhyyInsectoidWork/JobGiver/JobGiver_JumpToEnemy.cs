namespace PPhhyyInsectoidWork
{
    public class JobGiver_JumpToEnemy : ThinkNode_JobGiver
    {
        private AbilityDef ability;

        protected override Job TryGiveJob(Pawn pawn)
        {
            Ability ability = pawn?.abilities?.GetAbility(this.ability);
            if (ability is null || !ability.CanCast)
            {
                return null;
            }

            Pawn target = GetClosestEnemy(pawn);

            IntVec3 cell = GenAdjFast.AdjacentCells8Way(target).RandomElement();
            if (cell.InBounds(pawn.Map) && cell.IsValid)
            {
                Log.Message("JUMP");
                return ability.GetJob(target, cell);
            }
            Log.Message($"Cell at {cell} is invalid. Gotten from {target}");
            return null;
        }

        public static Pawn GetClosestEnemy(Pawn thisPawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(thisPawn,
                TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable,
                (Thing x) => x is Pawn && (int)x.def.race.intelligence >= 1,
                0f,
                9999f,
                thisPawn.Position,
                float.MaxValue,
                true,
                canTakeTargetsCloserThanEffectiveMinRange: true,
                true);
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_JumpToEnemy obj = (JobGiver_JumpToEnemy)base.DeepCopy(resolve);
            obj.ability = ability;
            return obj;
        }
    }
}
