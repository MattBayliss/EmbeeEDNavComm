using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
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

            var routes = new Dictionary<string, List<JumpRoute>>();

            PlotRoutes2(routes, paths, systemA, systemB);

            List<JumpRoute> result;

            if (routes.ContainsKey(systemB.Name))
            {
                result = routes[systemB.Name];
            }
            else
            {
                result = new List<JumpRoute>();
            }

            return result.OrderBy(r => r.TotalDistance).ToList();
        }

        private void PlotRoutes2(Dictionary<string, List<JumpRoute>> routesThroughSystem, StarPaths availablePaths, StarSystem startSystem, StarSystem targetSystem)
        {
            var fromstart = availablePaths.GetPathsFromSystem(startSystem.Name, targetSystem.Coordinates);
            var startroutes = new List<JumpRoute>();
            foreach (var starpath in fromstart)
            {
                var jumproute = new JumpRoute(starpath);
                routesThroughSystem.Add(starpath.To.Name, new List<JumpRoute>() {jumproute});
                startroutes.Add(jumproute);
            }
            foreach (var route in startroutes)
            {
                RecursivePlotRoutes(routesThroughSystem, availablePaths, route, targetSystem);
            }
        }


        private void RecursivePlotRoutes(Dictionary<string, List<JumpRoute>> routesThroughSystem, StarPaths availablePaths, JumpRoute currentRoute, StarSystem targetSystem)
        {
            Logger.Trace("\tCHECKING:   \t{0}", currentRoute.ToString());

            var fromcurrent = availablePaths.GetPathsFromSystem(currentRoute.To.Name, targetSystem.Coordinates);
            var nextroutes = new List<JumpRoute>();
            foreach (var s in fromcurrent)
            {
                var to = s.To.Name;

                // see if new path is shorter than existing ones
                var newroute = new JumpRoute(s);
                newroute.Previous = currentRoute;

                if(routesThroughSystem.ContainsKey(to)) {
                    var routes = routesThroughSystem[to];
                    var sampleroute = routes.First();
                    var existingjumps = sampleroute.JumpsTo(to);
                    var newjumps = newroute.Jumps;
                    var totalroutes = routes.Count;

                    if (existingjumps > newjumps)
                    {
                        Logger.Trace("\tSHORTER:   \t{0}", newroute.ToString());
                        for (var i = 0; i < totalroutes; i++)
                        {
                            var route = routes[i];
                            Logger.Trace("\t\tREPLACING:\t{0}", route.ToString());
                            if (to.Equals(route.To.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                route = newroute;
                            }
                            else
                            {
                                route.ReplacePreviousRoute(newroute);
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
                                routesThroughSystem[to].Add(newroute);
                            }
                            else
                            {
                                var altroute = (JumpRoute)route.Clone();
                                altroute.ReplacePreviousRoute(newroute);
                                
                                routesThroughSystem[to].Add(altroute);
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
                    routesThroughSystem.Add(to, new List<JumpRoute>() { newroute });
                    nextroutes.Add(newroute);
                }

                if (to.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
                {
                    // reached the destination!
                    // don't need to check any other nodes
                    Logger.Trace("FOUND:   \t{0}", newroute.ToString());
                    nextroutes = new List<JumpRoute>();
                    break;
                }
            }

            foreach (var nextroute in nextroutes)
            {
                RecursivePlotRoutes(routesThroughSystem, availablePaths, nextroute, targetSystem);
            }

           
        }

    }
}