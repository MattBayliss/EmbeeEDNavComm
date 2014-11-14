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

            var routes = new Dictionary<string, JumpTree>();

            var startMarker = new JumpTree(systemA.Name);
            var endMarker = new JumpTree(new StarPath(target, _targetMarker, 0));
            routes.Add(source, startMarker);
            routes.Add(_targetMarker, endMarker);

            //remove all paths TO source
            paths.RemoveAll(sp => sp.To.ToLower() == source);

            //remove all paths FROM target
            paths.RemoveAll(sp => sp.From.ToLower() == target);

            RecursiveRoutes(paths, new JumpRoute(startMarker), routes, target);

            if (routes[_targetMarker].Jumps > 1)
            {
                var foundRoutes = endMarker.ToJumpRoutes().Select(r => r.Previous).OrderBy(r => r.TotalDistance).ToList();
                return foundRoutes;
            }
            else
            {
                return new List<JumpRoute>();
            }
        }

        private void RecursiveRoutes(StarPaths unexaminedPaths, JumpRoute currentPath, Dictionary<string, JumpTree> routes, string target)
        {
            Task.Delay(100).Wait();

            var pathsToCheck = unexaminedPaths.PopPathsFromSystem(currentPath.To);

            if (pathsToCheck.Count == 0)
            {
                return;
            }
            if (unexaminedPaths.Count == 0)
            {
                return;
            }

            var nexttocheck = new List<StarPath>();
            bool reachedTarget = false;

            foreach (var sp in pathsToCheck)
            {
                var nextsystem = sp.To.ToLower();
                var fromsystem = sp.From.ToLower();

                // don't visit a system we've already gone through
                if (currentPath.Contains(nextsystem))
                {
                    Logger.Trace("{0} > {1} << LOOP - Abandon", currentPath.ToString(), nextsystem);
                    continue;
                }

                var from = routes[fromsystem];

                if (routes.ContainsKey(nextsystem))
                {
                    {
                        // if we've already seem the system, add an extra route to it
                        routes[nextsystem].AddPrecedingJump(from);
                    }
                }
                else
                {
                    var newroute = new JumpTree(sp);
                    newroute.AddPrecedingJump(from);
                    routes.Add(nextsystem, newroute);

                    if (nextsystem == target)
                    {
                        routes[_targetMarker].AddPrecedingJump(routes[target]);
                        reachedTarget = true;
                        break;
                    }
                    else
                    {
                        nexttocheck.Add(sp);
                    }
                }
            }
            if (reachedTarget)
            {
                Logger.Trace(string.Format("{0} > {1} << FOUND", currentPath.ToString(), target));
            }
            else if (nexttocheck.Count == 0)
            {
                Logger.Trace(currentPath.ToString() + " << DEAD END");
            }
            else
            {
                foreach (var next in nexttocheck)
                {
                    var croute = new JumpRoute(next);
                    croute.Previous = currentPath;
                    Logger.Trace(croute.ToString());
                    RecursiveRoutes(unexaminedPaths, croute, routes, target);
                }
            }
        }
    }
}