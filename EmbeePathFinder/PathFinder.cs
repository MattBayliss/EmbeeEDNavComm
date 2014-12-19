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
        private const string _targetMarker = "<< target";
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

            var result = PlotRoutes(paths, systemA.Name, systemB);

            return result.OrderBy(r => r.TotalDistance).ToList();
        }


        private IEnumerable<JumpRoute> PlotRoutes(StarPaths availablePaths, string currentSystem, StarSystem targetSystem)
        {
            var fromstart = availablePaths.GetPathsFromSystem(currentSystem, targetSystem.Coordinates);
            var routes = new List<JumpRoute>();

            if (currentSystem.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            foreach (var s in fromstart)
            {
                // return straight away if we've already found our target
                if (s.To.Name.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return new List<JumpRoute> { new JumpRoute(s) };
                }
                routes.Add(new JumpRoute(s));                
            }

            bool targetFound = false;

            while ((availablePaths.Count > 0) && (!targetFound))
            {
                var currentRoutes = new JumpRoute[routes.Count];
                routes.CopyTo(currentRoutes);
                routes = new List<JumpRoute>();
                foreach(var r in currentRoutes) {
                    var nextJumps = availablePaths.GetPathsFromSystem(r.To.Name, targetSystem.Coordinates);
                    if (nextJumps.Count > 0)
                    {
                        var nextroutes = new List<JumpRoute>();
                        foreach (var nj in nextJumps)
                        {
                            if (nj.To.Name.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                targetFound = true;
                                var nextroute = new JumpRoute(nj);
                                var prevroute = (JumpRoute)r.Clone();
                                nextroute.Previous = prevroute;
                                nextroutes = new List<JumpRoute> { nextroute };
                                Logger.Debug("FOUND:   \t{0}", nextroute.ToString());
                                break;
                            }
                            else if(!targetFound)
                            {
                                //if target wasn't found on anther iteration, keep looking
                                var nextroute = new JumpRoute(nj);
                                var prevroute = (JumpRoute)r.Clone();
                                nextroute.Previous = prevroute;
                                Logger.Trace("CHECK:   \t{0}", nextroute.ToString());
                                nextroutes.Add(nextroute);
                            }
                        }
                        routes.AddRange(nextroutes);
                    }
                    else
                    {
                        Logger.Trace("DEAD END:\t{0}", r.ToString());
                    }
                    availablePaths.RemoveAll(sp => sp.To.Name.ToLower() == r.From.Name.ToLower());
                }
            }

            return routes.Where(r => r.To.Name.ToLower() == targetSystem.Name.ToLower()).ToList();
        }


        private void RecursivePlotRoutes(Dictionary<string, List<JumpRoute>> routesThroughSystem, StarPaths availablePaths, JumpRoute currentRoute, StarSystem targetSystem)
        {
            if (currentRoute.To.Name.Equals(targetSystem.Name, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Trace("FOUND:   \t{0}", currentRoute.ToString());
                return;
            }

            var fromcurrent = availablePaths.GetPathsFromSystem(currentRoute.To.Name, targetSystem.Coordinates);

            foreach (var s in fromcurrent)
            {
                var to = s.To.Name;

                // see if new path is shorter than existing ones
                var newroute = new JumpRoute(s);
                newroute.Previous = currentRoute;

                if(routesThroughSystem.ContainsKey(to)) {
                    var sampleroute = routesThroughSystem[to].First();
                    var existingjumps = sampleroute.JumpsTo(to);
                    var newjumps = newroute.Jumps;

                    if (existingjumps > newjumps)
                    {
                        //var oldroutes =  TODO: need to find old routes, and replace the start with 
                        // the new route, and then stop this branch, because we've already checked
                        // to the end of it previously
                        
                        Logger.Trace("SHORTER:   \t{0}", newroute.ToString());
                        continue;
                    }
                    else if (existingjumps == newjumps)
                    {
                        //var oldroutes =  TODO: need to find old routes, and add this route 
                        //  and then stop this branch, because we've already checked
                        // to the end of it previously
                        routesThroughSystem[to].Add(newroute);
                        Logger.Trace("ALTERNATE:   \t{0}", newroute.ToString());
                        continue;
                    }
                    else
                    {
                        // new route is longer, it ends here
                        routesThroughSystem[to].Add(newroute);
                        Logger.Trace("LONGER:   \t{0}", newroute.ToString());
                        continue;
                    }
                }
                else
                {
                    routesThroughSystem.Add(to, new List<JumpRoute>() { newroute });
                }

                RecursivePlotRoutes(routesThroughSystem, availablePaths, newroute, targetSystem);
            }

           
        }

    }
}