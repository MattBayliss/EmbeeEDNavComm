using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeePathFinder
{
    public class PathFinder
    {
        private Universe _universe;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public PathFinder(Universe universe)
        {
            _universe = universe;
        }

        public List<JumpRoute> GetRoutes(double maxJump, string systemNameA, string systemNameB)
        {
            var systemA = _universe[systemNameA];
            var systemB = _universe[systemNameB];

            return GetRoutes(maxJump, systemA, systemB);
        }

        public List<JumpRoute> GetRoutes(double maxJump, StarSystem systemA, StarSystem systemB)
        {
            var allpaths = _universe.SystemsInRange(maxJump);

            var paths = new StarPaths(allpaths, systemB);

            var result = PlotRoutes(paths, systemA, systemB, maxJump);

            return result.OrderBy(r => r.TotalDistance).ToList();
        }

        private List<JumpRoute> PlotRoutes(StarPaths availablePaths, StarSystem startSystem, StarSystem targetSystem, double maxJump)
        {
            var fromstart = availablePaths.GetPathsFromSystem(startSystem.Name, targetSystem.Coordinates).Select(s => new JumpRoute(s)).ToList();
            var routesToSystem = new List<KeyValuePair<string, string>>();
            var jumpRoutes = new Dictionary<string, JumpRoute>();

            var minJumps = int.MaxValue;

            foreach (var jumproute in fromstart)
            {
                RecursivePlotRoutes(jumpRoutes, routesToSystem, availablePaths, jumproute, targetSystem, maxJump, ref minJumps);
            }

            return routesToSystem.Where(r => r.Key.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
                                       .Select(r => jumpRoutes[r.Value]).ToList();
        }

        private void RecursivePlotRoutes(Dictionary<string, JumpRoute> jumpRoutes, List<KeyValuePair<string, string>> routesToSystem, StarPaths availablePaths, JumpRoute currentRoute, StarSystem targetSystem, double maxJump, ref int minJumps)
        {
            var currentRouteId = currentRoute.ToString();
            Logger.Trace("CHECKING:   \t{0}", currentRouteId);

            var to = currentRoute.To.Name;

            // make sure we haven't looped back
            if (currentRoute.Previous != null)
            {
                var previousystems = currentRoute.Previous.GetSystemNames();
                if (previousystems.Contains(to))
                {
                    Logger.Trace("\tLOOPED:   \t{0}", currentRouteId);
                    return;
                }
            }

            var currentjumps = currentRoute.Jumps;

            // are there already routes to this system?
            var alternateroutes = routesToSystem.Where(kvp => kvp.Key.Equals(to, StringComparison.OrdinalIgnoreCase)).Select(r => jumpRoutes[r.Value]).ToList();
            if (alternateroutes.Count > 0)
            {
                // if the current route is shorter than existing routes, delete the existing routes
                // All alternate routes will be of the same length (longer ones are deleted)
                var altroute = alternateroutes.First();
                var altjumps = altroute.Jumps;
                if (currentjumps < altjumps)
                {
                    // Current route is shorter that existing ones - delete existing ones
                    foreach (var route in alternateroutes)
                    {
                        var routeid = route.ToString();
                        routesToSystem.RemoveAll(r => r.Value == routeid);
                        jumpRoutes.Remove(routeid);
                    }
                }
                else if (currentjumps > altjumps)
                {
                    // current route is longer than existing ones, ignore it
                    return;
                }
            }

            // if we've made it to the target, no need to go further!
            var reachedTarget = to.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase);
            if (reachedTarget)
            {
                // reached the destination!
                Logger.Trace("FOUND:   \t{0}", currentRouteId);
                minJumps = (new int[] { minJumps, currentRoute.Jumps }).Min();
            }
            else
            {
                // can this route even make it to the target with the distance and jumps left?
                if (minJumps < int.MaxValue)
                {
                    var jumpsLeft = minJumps - currentjumps;
                    if (jumpsLeft <= 0)
                    {
                        Logger.Trace("\tTOO LONG:   \t{0}", currentRouteId);
                        return;
                    }
                    var bestCaseDistance = maxJump * jumpsLeft;
                    var distanceLeft = currentRoute.To.Coordinates.DistanceTo(targetSystem.Coordinates);
                    if (bestCaseDistance < distanceLeft)
                    {
                        Logger.Trace("\tTOO FAR:   \t{0}", currentRouteId);
                        return;
                    }
                }
            }

            routesToSystem.Add(new KeyValuePair<string, string>(to, currentRouteId));
            jumpRoutes.Add(currentRouteId, currentRoute);

            if (reachedTarget)
            {
                return;
            }

            var fromcurrent = availablePaths.GetPathsFromSystem(currentRoute.To.Name, targetSystem.Coordinates);
            var nextroutes = new List<JumpRoute>();

            // check for deadend (checking for 1 route - the way we came)
            if (fromcurrent.Count <= 1)
            {
                Logger.Trace("\tDEAD END");
                return;
            }

            foreach (var s in fromcurrent)
            {
                if (!s.To.Name.Equals(currentRoute.From.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var newroute = new JumpRoute(s);
                    newroute.Previous = currentRoute;
                    RecursivePlotRoutes(jumpRoutes, routesToSystem, availablePaths, newroute, targetSystem, maxJump, ref minJumps);
                }
            }
        }

        private List<KeyValuePair<string, string>> GetSystemsRoutePassesThrough(JumpRoute route)
        {
            var routeid = route.ToString();
            var systems = route.GetSystemNames().Select(n => new KeyValuePair<string, string>(n, routeid));
            return systems.ToList();
        }

        private void KillRoute(Dictionary<string, JumpRoute> jumpRoutes, List<KeyValuePair<string, string>> routesToSystem, JumpRoute routeToKill)
        {
            var route = routeToKill;
            while (route != null)
            {
                var routeId = route.ToString();
                routesToSystem.RemoveAll(m => m.Value == routeId);
                jumpRoutes.Remove(routeId);
                route = route.Previous;
            }
        }
    }
}