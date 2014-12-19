using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class Universe
    {
        private Dictionary<string, StarSystem> _stars;
        private Dictionary<double, List<StarPath>> _distances;

        public Universe(IEnumerable<StarSystem> stars)
        {
             _distances = new Dictionary<double,List<StarPath>>();
            _stars = new Dictionary<string, StarSystem>();
            foreach (var star in stars)
            {
                var key = star.Name.ToLower();
                if (!_stars.ContainsKey(key))
                {
                    _stars.Add(key, star);
                }
            }
        }

        public StarSystem this[string key]
        {
            get
            {
                var lkey = key.ToLower();
                if (_stars.ContainsKey(lkey))
                {
                    return _stars[lkey];
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<StarPath> SystemsInRange(double range)
        {
            //already calculated
            if(_distances.ContainsKey(range)) {
                return _distances[range];
            }

            if(range <= 0) {
                return new List<StarPath>();
            }

            // look for a list of star-paths already calculated for a larger range,
            // then we can filter off of that smaller set, rather than the entire
            // universe.

            // get the already fetched ranges
            var ranges = _distances.Keys.ToArray();
            Array.Sort(ranges);

            List<StarPath> systems;
            int r = 0;
            bool filterfound = false;

            // find the first larger-ranged StarPath set, so we can use that
            while (!filterfound && r < ranges.Length) {
                if (range < ranges[r])
                {
                    filterfound = true;
                }
                else
                {
                    r++;
                }
            }

            if (filterfound)
            {
                // filter found! We can simply do some LINQ to set the new subset of available paths
                systems = _distances[ranges[r]].Where(s => s.Distance < range).ToList();
            }
            else
            {
                // no suitable filter - have to look at all possible StarPaths
                systems = new List<StarPath>();
                var starnames = _stars.Keys.ToArray();
                var totalstars = starnames.Length;

                for (int x = 0; x < totalstars; x++)
                {
                    var stara = _stars[starnames[x]];
                    for (int y = x + 1; y < totalstars; y++)
                    {
                        var starb = _stars[starnames[y]];
                        var distance = stara.Coordinates.DistanceTo(starb.Coordinates);
                        if (distance < range)
                        {
                            systems.Add(new StarPath(stara, starb));
                        }
                    }
                }
            }

            _distances.Add(range, systems);

            return systems;
        }

        public IEnumerable<string> StarNames { get { return _stars.Keys; } }

    }
}
