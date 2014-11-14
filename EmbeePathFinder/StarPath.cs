using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeePathFinder
{
    public class StarPath
    {
        private Dictionary<string, double?> _distances;
        private Dictionary<string, int?> _jumps;
        private Dictionary<string, StarPath> _previous;
        
        public StarPath()
        {
            _distances = new Dictionary<string, double?>();
            _jumps = new Dictionary<string, int?>();
            _previous = new Dictionary<string, StarPath>();
        }

        public StarPath(StarSystem starSystem)
            : this()
        {
            StarSystem = starSystem;
        }

        public StarSystem StarSystem { get; set; }
        
        public Dictionary<string, double?> DistanceTo
        {
            get { return _distances; }
        }

        public Dictionary<string, int?> JumpsTo
        {
            get { return _jumps; }
        }
    }
}
