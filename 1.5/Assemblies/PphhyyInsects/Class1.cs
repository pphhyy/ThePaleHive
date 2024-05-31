using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI.Group;

namespace PphhyyInsects
{

    //please note: as of writing this comment, most of the below code is taken from various places in the vanilla code and edited to reflect the pale insect Defs.

    public class IncidentWorker_PaleInfestation : IncidentWorker
    {
        public const float HivePoints = 220f;

        public static readonly SimpleCurve PointsFactorCurve = new SimpleCurve
        { new CurvePoint(0f, 0.7f), new CurvePoint(5000f, 0.45f) };

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 cell;
            if (base.CanFireNowSub(parms) && Faction.OfInsects != null && HiveUtility.TotalSpawnedHivesCount(map) < 30)//Faction needs to be custom 'pale insect' faction? Also probably need to make my own HiveUtility, the modularity of which will depend on whether there are multiple insect factions/mod.
            {
                return InfestationCellFinder.TryFindCell(out cell, map);
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            parms.points *= PointsFactorCurve.Evaluate(parms.points);
            Thing thing = InfestationUtility.SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1), map, spawnAnywhereIfNoGoodCell: false, parms.infestationLocOverride.HasValue, null, parms.infestationLocOverride);
            SendStandardLetter(parms, thing);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }

        public static Thing SpawnTunnels(int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null, IntVec3? overrideLoc = null, float? insectsPoints = null)
        {
            IntVec3 loc = (overrideLoc.HasValue ? overrideLoc.Value : default(IntVec3));
            if (!overrideLoc.HasValue)
            {
                loc = FindRootTunnelLoc(map, spawnAnywhereIfNoGoodCell);
            }
            if (!loc.IsValid)
            {
                return null;
            }
            TunnelPaleHiveSpawner tunnelHiveSpawner = (TunnelPaleHiveSpawner)ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner);
            Thing thing = GenSpawn.Spawn(tunnelHiveSpawner, loc, map, WipeMode.FullRefund);
            if (insectsPoints.HasValue)
            {
                tunnelHiveSpawner.insectsPoints = insectsPoints.Value;
            }
            QuestUtility.AddQuestTag(thing, questTag);
            for (int i = 0; i < hiveCount - 1; i++)
            {
                loc = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive, ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, allowUnreachable: true);
                if (loc.IsValid)
                {
                    tunnelHiveSpawner = (TunnelPaleHiveSpawner)ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner);
                    thing = GenSpawn.Spawn(tunnelHiveSpawner, loc, map, WipeMode.FullRefund);
                    if (insectsPoints.HasValue)
                    {
                        tunnelHiveSpawner.insectsPoints = insectsPoints.Value;
                    }
                    QuestUtility.AddQuestTag(thing, questTag);
                }
            }
            return thing;
        }
        private static IntVec3 FindRootTunnelLoc(Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofIfNoGoodCell = false)
        {
            if (InfestationCellFinder.TryFindCell(out var cell, map))
            {
                return cell;
            }
            if (!spawnAnywhereIfNoGoodCell)
            {
                return IntVec3.Invalid;
            }
            Func<IntVec3, bool, bool> validator = delegate (IntVec3 x, bool canIgnoreRoof)
            {
                if (!x.Standable(map) || x.Fogged(map))
                {
                    return false;
                }
                if (!canIgnoreRoof)
                {
                    bool flag = false;
                    int num = GenRadial.NumCellsInRadius(3f);
                    for (int i = 0; i < num; i++)
                    {
                        IntVec3 c = x + GenRadial.RadialPattern[i];
                        if (c.InBounds(map))
                        {
                            RoofDef roof = c.GetRoof(map);
                            if (roof != null && roof.isThickRoof)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
                return true;
            };
            if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => validator(x, arg2: false), map, out cell))
            {
                return cell;
            }
            if (ignoreRoofIfNoGoodCell && RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => validator(x, arg2: true), map, out cell))
            {
                return cell;
            }
            return IntVec3.Invalid;
        }
    }
    [StaticConstructorOnStartup]
    public class TunnelPaleHiveSpawner : GroundSpawner //thing class of the hole that opens to become a pale hive
    {
        public bool spawnHive = true;

        public float insectsPoints;

        public bool spawnedByInfestationThingComp;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spawnHive, "spawnHive", defaultValue: true);
            Scribe_Values.Look(ref insectsPoints, "insectsPoints", 0f);
            Scribe_Values.Look(ref spawnedByInfestationThingComp, "spawnedByInfestationThingComp", defaultValue: false);
        }

        protected override void Spawn(Map map, IntVec3 loc)
        {
            if (spawnHive)
            {
                Hive obj = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("PaleHive")), loc, map, WipeMode.FullRefund);
                obj.SetFaction(Faction.OfInsects);//set to pale hive faction, when they have one
                obj.questTags = questTags;
                foreach (CompSpawner comp in obj.GetComps<CompSpawner>())
                {
                    if (comp.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly)
                    {
                        comp.TryDoSpawn();
                        break;
                    }
                }
            }
            if (!(insectsPoints > 0f))
            {
                return;
            }
            insectsPoints = Mathf.Max(insectsPoints, Hive.spawnablePawnKinds.Min((PawnKindDef x) => x.combatPower));
            float pointsLeft = insectsPoints;
            List<Pawn> list = new List<Pawn>();
            int num = 0;
            while (pointsLeft > 0f)
            {
                num++;
                if (num > 1000)
                {
                    Log.Error("Too many iterations.");
                    break;
                }
                if (!Hive.spawnablePawnKinds.Where((PawnKindDef x) => x.combatPower <= pointsLeft).TryRandomElement(out var result))
                {
                    break;
                }
                Pawn pawn = PawnGenerator.GeneratePawn(result, Faction.OfInsects);//faction
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(loc, map, 2), map);
                pawn.mindState.spawnedByInfestationThingComp = spawnedByInfestationThingComp;
                list.Add(pawn);
                pointsLeft -= result.combatPower;
                if (ModsConfig.BiotechActive)
                {
                    PollutionUtility.Notify_TunnelHiveSpawnedInsect(pawn);
                }
            }
            if (list.Any())
            {
                LordMaker.MakeNewLord(Faction.OfInsects, new LordJob_AssaultColony(Faction.OfInsects, canKidnap: true, canTimeoutOrFlee: false), map, list);//faction
            }
        }
    }
}
