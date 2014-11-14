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

            var result = PlotRoutes(paths, systemA.Name, systemB.Name);

            return result.OrderBy(r => r.TotalDistance).ToList();
        }


        private IEnumerable<JumpRoute> PlotRoutes(StarPaths availablePaths, string currentSystem, string targetSystem)
        {
            var fromstart = availablePaths.GetPathsFromSystem(currentSystem);
            var routes = new List<JumpRoute>();

            if (currentSystem.Equals(targetSystem, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            foreach (var s in fromstart)
            {
                // return straight away if we've already found our target
                if (s.To.Equals(targetSystem, StringComparison.OrdinalIgnoreCase))
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
                    var nextJumps = availablePaths.GetPathsFromSystem(r.To);
                    if (nextJumps.Count > 0)
                    {
                        var nextroutes = new List<JumpRoute>();
                        foreach (var nj in nextJumps)
                        {
                            if (nj.To.Equals(targetSystem, StringComparison.OrdinalIgnoreCase))
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
                    availablePaths.RemoveAll(sp => sp.To.ToLower() == r.From.ToLower());
                }
            }

            return routes.Where(r => r.To.ToLower() == targetSystem.ToLower()).ToList();
        }
    }
}