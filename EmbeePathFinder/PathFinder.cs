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

            var result = PlotRoutes2(paths, systemA, systemB);

            return result.OrderBy(r => r.TotalDistance).ToList();
        }

        private List<JumpRoute> PlotRoutes2(StarPaths availablePaths, StarSystem startSystem, StarSystem targetSystem)
        {
            var fromstart = availablePaths.GetPathsFromSystem(startSystem.Name, targetSystem.Coordinates);
            var startroutes = new List<JumpRoute>();
            var routesThroughSystem = new List<KeyValuePair<string, long>>();
            var jumpRoutes = new Dictionary<long, JumpRoute>();

            foreach (var starpath in fromstart)
            {
                var jumproute = new JumpRoute(starpath);
                var to = starpath.To.Name;
                if (to.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
                {
                    //can reach it in one jump - don't go any further
                    return new List<JumpRoute>() { jumproute };
                }
                routesThroughSystem.Add(new KeyValuePair<string, long>(to, jumproute.RouteId));
                jumpRoutes.Add(jumproute.RouteId, jumproute);
                startroutes.Add(jumproute);
            }
            var minJumps = int.MaxValue;
            foreach (var route in startroutes)
            {
                RecursivePlotRoutes(jumpRoutes, routesThroughSystem, availablePaths, route, targetSystem, ref minJumps);
            }

            return routesThroughSystem.Where(r => r.Key.Equals(targetSystem.Name)).Select(r => jumpRoutes[r.Value]).ToList();
        }

        private void RecursivePlotRoutes(Dictionary<long, JumpRoute> jumpRoutes, List<KeyValuePair<string, long>> routesThroughSystem, StarPaths availablePaths, JumpRoute currentRoute, StarSystem targetSystem, ref int minJumps)
        {
            Logger.Trace("CHECKING:   \t{0} [{1}]", currentRoute.ToString(), currentRoute.RouteId);

            if (currentRoute.To.Name.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
            {
                // reached the destination!
                // don't need to check any other nodes
                Logger.Trace("FOUND:   \t{0}", currentRoute.ToString());
                minJumps = (new int[] { minJumps, currentRoute.Jumps }).Min();
                return;
            }

            var currentjumps = currentRoute.Jumps;

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
                var to = s.To.Name;

                // make sure the route isn't doubling back
                var systems = currentRoute.GetSystemNames();
                if (systems.Contains(to))
                {
                    Logger.Trace("\tLOOPED:   \t{0} > {1}", currentRoute.ToString(), to);
                    continue;
                }

                // see if new path is shorter than existing ones
                var newroute = new JumpRoute(s);
                newroute.Previous = currentRoute;
                var newrouteid = newroute.RouteId;
                jumpRoutes.Add(newrouteid, newroute);

                var routes = routesThroughSystem.Where(r => (r.Key.Equals(to, StringComparison.OrdinalIgnoreCase)) && (r.Value != currentRoute.RouteId)).Select(r => r.Value).Distinct().Select(r => jumpRoutes[r]).ToList();

                if (routes.Count > 0)
                {
                    var sampleroute = routes.First();
                    var existingjumps = sampleroute.JumpsTo(to);
                    var newjumps = currentjumps + 1;
                    var totalroutes = routes.Count;

                    if (existingjumps > newjumps)
                    {
                        Logger.Trace("\tSHORTER:   \t{0}", newroute.ToString());
                        for (var i = 0; i < totalroutes; i++)
                        {
                            var route = routes[i];

                            Logger.Trace("\t\tREPLACING:\t{0}", route.ToString());

                            JumpRoute replaced;
                            var oldrouteid = route.RouteId;
                            var oldjumps = route.Jumps;

                            // clear the systems that the route used to go through
                            replaced = route;
                            while (replaced != null)
                            {
                                routesThroughSystem.RemoveAll(m => m.Value == replaced.RouteId);
                                replaced = replaced.Previous;
                            }

                            if (to.Equals(route.To.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                routesThroughSystem.AddRange(GetSystemsRoutePassesThrough(newroute));
                            }
                            else
                            {
                                replaced = route.ReplacePreviousRoute(newroute);
                                routesThroughSystem.AddRange(GetSystemsRoutePassesThrough(newroute));

                                var replacedjumps = replaced.Jumps;
                                if (replaced.To.Name.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    Logger.Trace("SHORTFOUND:   \t{0}", replaced.ToString());
                                    if (replacedjumps < minJumps)
                                    {
                                        minJumps = replacedjumps;
                                    }
                                }
                                else if ((oldjumps > minJumps) && (replacedjumps < minJumps))
                                {
                                    //this route may have been abandoned because it got too long - 
                                    //but it's been shortened enough to be a contender again
                                    Logger.Trace("SHORTENED:   \t{0}", replaced.ToString());
                                    nextroutes.Add(replaced);
                                }


                            }

                        }
                    }
                    else if (existingjumps == newjumps)
                    {
                        Logger.Trace("\tALTERNA:   \t{0}", newroute.ToString());
                        for (var i = 0; i < totalroutes; i++)
                        {
                            var route = routes[i];

                            if (to.Equals(route.To.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                routesThroughSystem.AddRange(GetSystemsRoutePassesThrough(newroute));
                            }
                            else
                            {
                                var altroute = route.CopyRouteAfter(s.To);
                                var routeid = altroute.RouteId;

                                var altprev = altroute;
                                while (altprev != null)
                                {
                                    jumpRoutes.Add(altprev.RouteId, altprev);
                                    altprev = altprev.Previous;
                                }

                                altroute.ReplacePreviousRoute(newroute);

                                // register the alternate route's systems
                                routesThroughSystem.AddRange(GetSystemsRoutePassesThrough(altroute));

                                if (altroute.To.Name.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    Logger.Trace("ALTFOUND:   \t{0}", altroute.ToString());
                                }
                            }

                        }
                    }
                    else
                    {
                        // new route is longer, it ends here
                        Logger.Trace("\tLONGER:   \t{0}", newroute.ToString());
                    }
                }
                else
                {
                    // haven't seen this system yet
                    var prevsystems = newroute.GetSystemNames();
                    routesThroughSystem.Add(new KeyValuePair<string, long>(to, newrouteid));
                    nextroutes.Add(newroute);
                }
            }

            foreach (var nextroute in nextroutes)
            {
                if (nextroute.Jumps < minJumps)
                {
                    RecursivePlotRoutes(jumpRoutes, routesThroughSystem, availablePaths, nextroute, targetSystem, ref minJumps);
                }
            }

        }

        private List<KeyValuePair<string, long>> GetSystemsRoutePassesThrough(JumpRoute route)
        {
            var routeid = route.RouteId;
            var systems = route.GetSystemNames().Select(n => new KeyValuePair<string, long>(n, routeid));
            return systems.ToList();
        }

    }
}