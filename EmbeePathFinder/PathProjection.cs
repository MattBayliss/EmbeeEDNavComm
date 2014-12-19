using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeePathFinder
{
    public class PathProjection : StarPath
    {
        private Coordinates _target;

        public double Projection { get; protected set; }
        
        public PathProjection(StarSystem from, StarSystem to, Coordinates target) : base(from, to)
        {
            _target = target;
            Projection = this.ProjectionOn(_target);
        }

        public override void SwapDirection()
        {
            base.SwapDirection();
            Projection = this.ProjectionOn(_target);
        }
    }
}
