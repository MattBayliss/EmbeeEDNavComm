using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class JumpRoute : StarPath, ICloneable
    {
        private static long _lastRouteId = 0;
        private JumpRoute _previous;

        public JumpRoute(StarPath path)
            : base(path.From, path.To)
        {
            _previous = null;
        }

        public JumpRoute Previous
        {
            get
            {
                return _previous;
            }
            set
            {
                if (value != null)
                {
                    From = value.To;
                }
                _previous = value;
            }
        }

        public int Jumps
        {
            get
            {
                if (Previous == null)
                {
                    return 1;
                }
                else
                {
                    return Previous.Jumps + 1;
                }
            }
        }

        public IEnumerable<string> GetSystemNamesAfter(string systemName)
        {
            if(To.Name.Equals(systemName, StringComparison.OrdinalIgnoreCase)) {
                return new List<string>();
            } else if(From.Name.Equals(systemName, StringComparison.OrdinalIgnoreCase)) {
                return new List<string>() { To.Name };
            } else if (Previous == null)
            {
                throw new ApplicationException("System name isn't in route: " + systemName);
            }
            else
            {
                var names = new List<string>() { To.Name };
                names.AddRange(Previous.GetSystemNamesAfter(systemName));
                return names;
            }
        }

        public int JumpsTo(string systemName)
        {
            if (To.Name.Equals(systemName, StringComparison.OrdinalIgnoreCase))
            {
                return this.Jumps;
            }
            else if(Previous != null)
            {
                return Previous.JumpsTo(systemName);
            }
            else
            {
                throw new ArgumentException("systemName is not in the route: " + systemName);
            }
        }

        public double TotalDistance
        {
            get
            {
                if (Previous == null)
                {
                    return Distance;
                }
                else
                {
                    return Distance + Previous.TotalDistance;
                }
            }
        }

        public IEnumerable<string> GetSystemNames()
        {
            if (Previous == null)
            {
                return new List<string>() { From.Name, To.Name };
            }
            else
            {
                var names = new List<string>() { To.Name };
                names.InsertRange(0, Previous.GetSystemNames());
                return names;
            }
        }

        public override string ToString()
        {
            var names = GetSystemNames();
            return string.Join(" > ", names);
        }

        public JumpRoute ReplacePreviousRoute(JumpRoute newPreviousRoute)
        {
            if (From.Name.Equals(newPreviousRoute.To.Name, StringComparison.OrdinalIgnoreCase))
            {
                var replaced = Previous;
                _previous = null;
                Previous = newPreviousRoute;
                return replaced;
            }
            else if (Previous == null)
            {
                throw new ApplicationException("JumpRoute not found");
            }
            else
            {
                return Previous.ReplacePreviousRoute(newPreviousRoute);
            }
        }

        public JumpRoute CopyRouteAfter(StarSystem from)
        {
            if (from.Name.Equals(this.From.Name, StringComparison.OrdinalIgnoreCase))
            {
                return new JumpRoute(new StarPath(from, this.To));
            }
            else if (Previous == null)
            {
                throw new ApplicationException("JumpRoute not found");
            }
            else
            {
                var route = new JumpRoute(new StarPath(this.From, this.To));
                route.Previous = this.Previous.CopyRouteAfter(from);
                return route;
            }
        }

        public object Clone()
        {
            var route = new JumpRoute(new StarPath(this.From, this.To));
            if (Previous != null)
            {
                route.Previous = (JumpRoute)this.Previous.Clone();
            }

            return route;
        }
    }
}
