using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeePathFinder
{
    public class StarPaths
    {
        private List<PathProjection> _availablePaths;

        public StarPaths(IEnumerable<StarPath> availablePaths, StarSystem targetSystem)
        {
            _availablePaths = availablePaths.Select(s => new PathProjection(s.From, s.To, targetSystem.Coordinates)).ToList();
        }

        public int Count { get { return _availablePaths.Count; } }

        /// <summary>
        /// Gets paths from a system, ordered by those most directly heading to the target coordinates
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public List<PathProjection> GetPathsFromSystem(string systemName, Coordinates target)
        {
            var lname = systemName.ToLower();
            var paths = _availablePaths.Where(s => s.From.Name.ToLower() == lname).ToList();
            paths.AddRange(_availablePaths.Where(s => s.To.Name.ToLower() == lname).Select(s =>
                {
                    s.SwapDirection();
                    return s;
                }).ToList());

            return paths.OrderByDescending(p => p.Projection).ThenBy(p => p.Distance).ToList();
        }

        public void RemoveAll(Func<PathProjection, bool> predicate)
        {
            _availablePaths.RemoveAll(p => predicate(p));
        }

        public void RemovePath(PathProjection path)
        {
            if (path != null)
            {
                _availablePaths.Remove(path);
            }
        }

        public void RemovePaths(IEnumerable<PathProjection> starPaths)
        {
            if (starPaths != null)
            {
                foreach (var sp in starPaths)
                {
                    _availablePaths.Remove(sp);
                }
            }
        }
    }
}
