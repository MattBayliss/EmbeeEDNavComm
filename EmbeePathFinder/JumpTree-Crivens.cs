using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeePathFinder
{
    public class JumpTree : StarPath
    {
        private Dictionary<string, JumpTree> _precedingJumps;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public JumpTree(string startSystem) : base(null, startSystem, 0)
        {
            _precedingJumps = null;
        }

        public JumpTree(StarPath path)
            : base(path.From, path.To, path.Distance)
        {
            _precedingJumps = null;
        }

        public Dictionary<string, JumpTree> PrecedingJumps { get { return _precedingJumps; } }

        public int Jumps
        {
            get
            {
                int jumps = 0;
                if ((_precedingJumps == null) || (_precedingJumps.Count == 0))
                {
                    jumps = 1;
                }
                else if (string.IsNullOrEmpty(From) && Distance == 0.0)
                {
                    jumps = 0;
                }
                else
                {
                    jumps = _precedingJumps.Values.First().Jumps + 1;
                }
                return jumps;
            }
        }

        public void AddPrecedingJump(JumpTree jump)
        {
            if (string.IsNullOrEmpty(jump.From) || string.IsNullOrEmpty(this.From))
            {
                // encountered the start marker - can ignore it
                return;
            }

            var toKey = jump.To.ToLower();
            var newJumpJumps = jump.Jumps;
            var thisJumps = Jumps;
            if (thisJumps == 1)
            {
                _precedingJumps = new Dictionary<string, JumpTree> { { toKey, jump } };
            }
            else if(newJumpJumps < thisJumps - 1) {
                // new preceding jump is faster, clear the old ones, add the new
                _precedingJumps = null;
                _precedingJumps = new Dictionary<string,JumpTree>();
                _precedingJumps.Add(toKey, jump);
            }
            else if(newJumpJumps == thisJumps - 1)
            {
                // a jump of the same length, add to to the alternates
                if (_precedingJumps.ContainsKey(toKey))
                {
                    _precedingJumps[toKey] = jump;
                }
                else
                {
                    _precedingJumps.Add(toKey, jump);
                }
            }
            else
            {
                // more jumps? Do nothing (discard)
            }
        }

        public IEnumerable<JumpRoute> ToJumpRoutes()
        {
            var thisroute = new JumpRoute(this);
            if (_precedingJumps == null) {
                if (!string.IsNullOrEmpty(From))
                {
                    return new List<JumpRoute> { thisroute };
                }
                else
                {
                    return new List<JumpRoute>();
                }
            }

            var routes = new List<JumpRoute>();

            foreach (var jt in _precedingJumps.Values)
            {
                var newroutes = jt.ToJumpRoutes();
                foreach (var nr in newroutes)
                {
                    var newroute = (JumpRoute)thisroute.Clone();
                    newroute.Previous = nr;
                    routes.Add(newroute);
                }
            }

            return routes;
        }

    }
}