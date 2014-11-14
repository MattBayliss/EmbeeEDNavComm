using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class StarPath : ICloneable
    {
        private string[] _systemNames;
        private double _distance;

        public double Distance { get { return _distance; } }
        public string From { get { return _systemNames[0]; } }
        public string To { get { return _systemNames[1]; } }

        public StarPath(string from, string to, double distance)
        {
            _systemNames = new string[] { from, to };
            _distance = distance;
        }

        public void SwapDirection()
        {
            Array.Reverse(_systemNames);
        }

        public virtual object Clone()
        {
            return new StarPath(From, To, Distance);
        }
    }
}
