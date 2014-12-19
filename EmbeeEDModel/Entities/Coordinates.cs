using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class Coordinates : ICloneable
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Coordinates()
        {
            X = Y = Z = 0.0;
        }

        public Coordinates(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public object Clone()
        {
            return new Coordinates(X,Y,Z);
        }

        public double Length
        {
            get { return DistanceTo(new Coordinates(0, 0, 0)); }
        }

        public double DistanceTo(Coordinates coords)
        {
            return Math.Sqrt(Sqrd(coords.X - X) + Sqrd(coords.Y - Y) + Sqrd(coords.Z - Z));
        }

        public double DotProduct(Coordinates other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        private double Sqrd(double x)
        {
            return x * x;
        }
    }
}
