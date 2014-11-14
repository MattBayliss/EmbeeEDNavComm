using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class StarSystem
    {
        public string Name { get; set; }
        public Coordinates Coordinates { get; set; }


        public StarSystem()
        {
            Name = "UNKNOWN";
            Coordinates = new Coordinates();
        }

        public StarSystem(string name)
        {
            Name = name;
            Coordinates = new Coordinates();
        }

        public StarSystem(string name, double x, double y, double z) : this(name)
        {
            Coordinates = new Coordinates(x, y, z);
        }
    }
}
