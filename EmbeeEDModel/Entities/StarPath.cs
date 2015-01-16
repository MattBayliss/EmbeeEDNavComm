using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class StarPath
    {
        private StarSystem[] _systems;
        private double _distance;

        public double Distance { get { return _distance; } }
        public StarSystem From { get { return _systems[0]; } }
        public StarSystem To { get { return _systems[1]; } }

        public StarPath(StarSystem from, StarSystem to)
        {
            _systems = new StarSystem[] { from, to };
            _distance = from.Coordinates.DistanceTo(to.Coordinates);

        }

        public virtual void SwapDirection()
        {
            Array.Reverse(_systems);
        }

        public double ProjectionOn(Coordinates target)
        {
            //vector from Start to target
            var targetVector = new Coordinates(target.X - From.Coordinates.X, target.Y - From.Coordinates.Y, target.Z - From.Coordinates.Z);
            var pathVector = new Coordinates(To.Coordinates.X - From.Coordinates.X, To.Coordinates.Y - From.Coordinates.Y, To.Coordinates.Z - From.Coordinates.Z);
            
            targetVector.Normalise();
            pathVector.Normalise();

            //projection of path onto target vector
            return pathVector.DotProduct(targetVector);
        }
    }
}
