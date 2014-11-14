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
        private List<StarPath> _availablePaths;

        public StarPaths(IEnumerable<StarPath> availablePaths)
        {
            _availablePaths = availablePaths.ToList();
            _availablePaths.AddRange(availablePaths.Select(p =>
            {
                var n = (StarPath)p.Clone();
                n.SwapDirection();
                return n;
            }).ToList());
        }

        public int Count { get { return _availablePaths.Count; } }

        public List<StarPath> PopPathsFromSystem(string systemName) {
            var lname = systemName.ToLower();
            var popped = new List<StarPath>();
            var paths = _availablePaths.Where(s => s.From.ToLower() == lname).ToList();

            foreach(var p in paths)
            {
                popped.Add(p);
                _availablePaths.Remove(p);
            }

            return popped;
        }

        public void RemoveAll(Func<StarPath, bool> predicate)
        {
            _availablePaths.RemoveAll(p => predicate(p));
        }

        public void RemovePath(StarPath path)
        {
            if (path != null)
            {
                _availablePaths.Remove(path);
            }
        }

        public void RemovePaths(IEnumerable<StarPath> starPaths)
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
