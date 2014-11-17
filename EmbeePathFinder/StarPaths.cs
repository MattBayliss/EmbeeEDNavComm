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
        }

        public int Count { get { return _availablePaths.Count; } }


        public List<StarPath> GetPathsFromSystem(string systemName)
        {
            var lname = systemName.ToLower();
            var paths = _availablePaths.Where(s => s.From.ToLower() == lname).ToList();
            paths.AddRange(_availablePaths.Where(s => s.To.ToLower() == lname).Select(s =>
                {
                    s.SwapDirection();
                    return s;
                }).ToList());

            return paths;
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
