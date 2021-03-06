﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class JumpRoute : StarPath, ICloneable
    {
        public JumpRoute(StarPath path)
            : base(path.From, path.To, path.Distance)
        {
        }

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

        public JumpRoute Start()
        {
            if (this.Previous == null)
            {
                return this;
            }
            else
            {
                return this.Previous.Start();
            }
        }

        public override string ToString()
        {
            if (Previous == null)
            {
                return string.Format("{0} > {1}", From, To);
            }
            else
            {
                return string.Format("{0} > {1}", Previous.ToString(), To);
            }
        }

        public bool Contains(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
            {
                return false;
            }

            if (To.Equals(systemName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Previous == null)
            {
                if (string.IsNullOrEmpty(From))
                {
                    return false;
                }
                else
                {
                    return From.Equals(systemName, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                return Previous.Contains(systemName);
            }
        }

        public override object Clone()
        {
            var route = new JumpRoute(new StarPath(this.From, this.To, this.Distance));
            if (Previous != null)
            {
                route.Previous = (JumpRoute)this.Previous.Clone();
            }

            return route;
        }
    }
}
