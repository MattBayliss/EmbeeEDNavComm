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

            var paths = new StarPaths(allpaths);

            var source = systemA.Name.ToLower();
            var target = systemB.Name.ToLower();

            var routes = new List<KeyValuePair<string, JumpRoute>>();

            Recurse2(paths, null, systemA.Name, systemB.Name, routes);

            return routes.Where(kvp => kvp.Key == target).Select(kvp => kvp.Value).OrderBy(r => r.TotalDistance).ToList();
        }

        private void Recurse2(StarPaths availablePaths, JumpRoute currentRoute, string currentSystem, string targetSystem, List<KeyValuePair<string, JumpRoute>> routes)
        {
            if (currentRoute != null)
            {
                availablePaths.RemoveAll(sp => (sp.From == currentRoute.From) && (sp.To == currentRoute.To));
                var current = currentRoute.To.ToLower();

                var existingroutes = routes.Where(kvp => kvp.Key == current).Select(kvp => kvp.Value).ToList();
                if (existingroutes.Count > 0)
                {
                    //already have a route to this system, let's see if the new one is quicker.
                    var existingroute = existingroutes[0];
                    if (currentRoute.Jumps < existingroute.Jumps)
                    {
                        Logger.Trace(currentRoute.ToString() + " << SHORTER ROUTE");
                        //better route! Replace existing ones
                        routes.RemoveAll(kvp => kvp.Key == current);
                    }
                    else if (currentRoute.Jumps == existingroute.Jumps)
                    {
                        //an equal length jump, add it to the list
                    }
                    else
                    {
                        // a longer route, do nothing
                        return;
                    }
                }
                routes.Add(new KeyValuePair<string, JumpRoute>(current, currentRoute));

                if (currentRoute.To.Equals(targetSystem, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Trace(currentRoute.ToString() + " << FOUND");
                    return;
                }
            }
            if (availablePaths.Count == 0)
            {
                return;
            }
            var pathsFromCurrent = availablePaths.GetPathsFromSystem(currentSystem);

            if (pathsFromCurrent.Count == 0)
            {
                Logger.Trace(currentRoute.ToString() + " << DEAD END");
                return;
            }

            var newpaths = new List<JumpRoute>();

            foreach (var nextPath in pathsFromCurrent)
            {
                var to = nextPath.To.ToLower();
                var newroute = new JumpRoute(nextPath);
                if (currentRoute != null)
                {
                    newroute.Previous = (JumpRoute)currentRoute.Clone();
                }
                Recurse2(availablePaths, newroute, nextPath.To, targetSystem, routes);
            }
        }

    //    private void RecursiveRoutes(StarPaths unexaminedPaths, JumpRoute currentPath, Dictionary<string, JumpTree> routes, string target)
    //    {
    //        Task.Delay(100).Wait();

    //        var pathsToCheck = unexaminedPaths.PopPathsFromSystem(currentPath.To);

    //        if (pathsToCheck.Count == 0)
    //        {
    //            return;
    //        }
    //        if (unexaminedPaths.Count == 0)
    //        {
    //            return;
    //        }

    //        var nexttocheck = new List<StarPath>();
    //        bool reachedTarget = false;

    //        foreach (var sp in pathsToCheck)
    //        {
    //            var nextsystem = sp.To.ToLower();
    //            var fromsystem = sp.From.ToLower();

    //            // don't visit a system we've already gone through
    //            if (currentPath.Contains(nextsystem))
    //            {
    //                Logger.Trace("{0} > {1} << LOOP - Abandon", currentPath.ToString(), nextsystem);
    //                continue;
    //            }

    //            var from = routes[fromsystem];

    //            if (routes.ContainsKey(nextsystem))
    //            {
    //                {
    //                    // if we've already seem the system, add an extra route to it
    //                    routes[nextsystem].AddPrecedingJump(from);
    //                }
    //            }
    //            else
    //            {
    //                var newroute = new JumpTree(sp);
    //                newroute.AddPrecedingJump(from);
    //                routes.Add(nextsystem, newroute);

    //                if (nextsystem == target)
    //                {
    //                    routes[_targetMarker].AddPrecedingJump(routes[target]);
    //                    reachedTarget = true;
    //                    break;
    //                }
    //                else
    //                {
    //                    nexttocheck.Add(sp);
    //                }
    //            }
    //        }
    //        if (reachedTarget)
    //        {
    //            Logger.Trace(string.Format("{0} > {1} << FOUND", currentPath.ToString(), target));
    //        }
    //        else if (nexttocheck.Count == 0)
    //        {
    //            Logger.Trace(currentPath.ToString() + " << DEAD END");
    //        }
    //        else
    //        {
    //            foreach (var next in nexttocheck)
    //            {
    //                var croute = new JumpRoute(next);
    //                croute.Previous = currentPath;
    //                Logger.Trace(croute.ToString());
    //                RecursiveRoutes(unexaminedPaths, croute, routes, target);
    //            }
    //        }
    //    }
    }
}