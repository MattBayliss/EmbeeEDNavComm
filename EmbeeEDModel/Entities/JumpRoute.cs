using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class JumpRoute : StarPath, ICloneable
    {
        public JumpRoute(StarPath path)
            : base(path.From, path.To)
        { }

        public JumpRoute Previous { get; set; }

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

        public override string ToString()
        {
            if (Previous == null)
            {
                return string.Format("{0} > {1}", From.Name, To.Name);
            }
            else
            {
                return string.Format("{0} > {1}", Previous.ToString(), To.Name);
            }
        }

        public void ReplacePreviousRoute(JumpRoute newPreviousRoute)
        {
            if (Previous == null)
            {
                throw new ApplicationException("JumpRoute not found");
            }

            if (Previous.To.Name.Equals(newPreviousRoute.To.Name, StringComparison.OrdinalIgnoreCase))
            {
                Previous = null;
                Previous = newPreviousRoute;
            }
            else
            {
                Previous.ReplacePreviousRoute(newPreviousRoute);
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
