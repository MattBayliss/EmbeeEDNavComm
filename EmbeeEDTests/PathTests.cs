using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmbeePathFinder;
using EmbeeEDModel.Entities;
using System.Collections.Generic;
using System.Linq;

namespace EmbeeEDTests
{
    [TestClass]
    public class PathTests
    {
        private PathFinder _pathfinder;

        [TestInitialize]
        public void Setup()
        {
            var stars = new List<StarSystem>();

            stars.Add(new StarSystem("Alpha", 0, 0, 0));
            stars.Add(new StarSystem("Zulu", 5, 0, 0));
            stars.Add(new StarSystem("Charlie", 10, 0, 0));
            stars.Add(new StarSystem("Disco", 15, 0, +1.1));
            stars.Add(new StarSystem("Echo", 15, 0, -1));
            stars.Add(new StarSystem("Foxtrot", 20, 0, 0));
            stars.Add(new StarSystem("Beta", 30, 0, 0));
            stars.Add(new StarSystem("Helo", 35, 0, 0));


            Coordinates gridstart = new Coordinates(100, 100, 100);

            // make a grid of 100 stars, at 5 LY intervals
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    var starname = string.Format("{0}-{1}", (char)(65 + x), y);
                    var star = new StarSystem(starname, gridstart.X + (x * 5), gridstart.Y + (y * 5), gridstart.Z);
                    stars.Add(star);
                }
            }


            var universe = new Universe(stars);

            _pathfinder = new PathFinder(universe);
        }

        [TestMethod]
        public void ShortRoute()
        {
            var routes =_pathfinder.GetRoutes(6.0, "Alpha", "Charlie");
            Assert.AreEqual(1, routes.Count());
            var route = routes[0];
            Assert.IsNotNull(route);
            var prevroute = route.Previous;
            Assert.IsNotNull(prevroute);
            var prevrevroute = prevroute.Previous;
            Assert.IsNull(prevrevroute);
            Assert.AreEqual(2, route.Jumps);
            Assert.AreEqual("Alpha", prevroute.From.Name);
            Assert.AreEqual("Zulu", prevroute.To.Name);
            Assert.AreEqual("Zulu", route.From.Name);
            Assert.AreEqual("Charlie", route.To.Name);
        }

        [TestMethod]
        public void BranchedRoute()
        {
            var routes = _pathfinder.GetRoutes(8, "Alpha", "Foxtrot");

            //should be two possible routes - one through Disco, one through Echo, Echo being the shortest
            Assert.AreEqual(2, routes.Count());
            var routeA = routes[0];
            var routeB = routes[1];

            //both end at Foxtrot
            Assert.AreEqual("Foxtrot", routeA.To.Name);
            Assert.AreEqual("Foxtrot", routeB.To.Name);

            Assert.AreEqual("Echo", routeA.From.Name);
            Assert.AreEqual("Disco", routeB.From.Name);

            var prevA = routeA.Previous;
            Assert.IsNotNull(prevA);

            var prevB = routeB.Previous;
            Assert.IsNotNull(prevB);

            Assert.AreEqual("Echo", prevA.To.Name);
            Assert.AreEqual("Disco", prevB.To.Name);

            Assert.AreEqual("Charlie", prevA.From.Name);
            Assert.AreEqual("Charlie", prevB.From.Name);
            
        }

        [TestMethod]
        public void ComplexPath()
        {
            //   C-D-E         // Path between A and G
            //  /|    \        // Trying direct routes means the PathFinder should
            // A-B     F-G     // go down the longer A-B-H route - B being the most direct route
            //    \     \|     // from A, and H being the most direct route from B
            //     H     L     //
            //     |    /      // The true shortest route is A-C-D-E-F-G
            //     I   K
            //      \ /
            //       J
            var stars = new List<StarSystem>();

            stars.Add(new StarSystem("C", 10, 10, 0));
            stars.Add(new StarSystem("D", 20, 15, 0)); // a bit up so not reachable from B
            stars.Add(new StarSystem("E", 30, 10, 0));

            stars.Add(new StarSystem("A", 0, 0, 0));
            stars.Add(new StarSystem("B", 10, 0, 0));
            stars.Add(new StarSystem("F", 40, 0, 0));
            stars.Add(new StarSystem("G", 50, 0, 0));

            stars.Add(new StarSystem("H", 20, -10, 0));
            stars.Add(new StarSystem("L", 50, -10, 0));

            stars.Add(new StarSystem("I", 20, -20, 0));
            stars.Add(new StarSystem("K", 40, -20, 0));

            stars.Add(new StarSystem("J", 30, -30, 0));

            var uni = new Universe(stars);
            var pf = new PathFinder(uni);

            // 14.2 is just big enough to include diagonals
            var routes = pf.GetRoutes(14.2, "A", "G");

            Assert.AreEqual(1, routes.Count);

            Assert.AreEqual(5, routes[0].Jumps);

            Assert.AreEqual("G", routes[0].To.Name);
            Assert.AreEqual("F", routes[0].From.Name);
            Assert.AreEqual("E", routes[0].Previous.From.Name);
            Assert.AreEqual("D", routes[0].Previous.Previous.From.Name);
            Assert.AreEqual("C", routes[0].Previous.Previous.Previous.From.Name);
            Assert.AreEqual("A", routes[0].Previous.Previous.Previous.Previous.From.Name);

            // total distance includes two diagonals, 3 straight bits
            Assert.AreEqual(Math.Sqrt(200) * 2 + Math.Sqrt(125) * 2 + 10, routes[0].TotalDistance);
        }


        [TestMethod]
        public void LotsOfRoutes()
        {
            //using the grid for this test - the diagonals are 7.07 long, so a Jump range of 7.1 will include the diagonals
            var routes = _pathfinder.GetRoutes(7.1, "A-5", "J-5");

            // there will be HEAPS of routes, but the first result will be the one with the least distance, a straight line
            Assert.IsTrue(routes.Count > 1);

            var fastestroute = routes.First();

            Assert.AreEqual("J-5", fastestroute.To.Name);
            Assert.AreEqual("I-5", fastestroute.From.Name);
            Assert.AreEqual("H-5", fastestroute.Previous.From.Name);
            Assert.AreEqual("G-5", fastestroute.Previous.Previous.From.Name);
            Assert.AreEqual("F-5", fastestroute.Previous.Previous.Previous.From.Name);
            Assert.AreEqual("E-5", fastestroute.Previous.Previous.Previous.Previous.From.Name);
            Assert.AreEqual("D-5", fastestroute.Previous.Previous.Previous.Previous.Previous.From.Name);
            Assert.AreEqual("C-5", fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.From.Name);
            Assert.AreEqual("B-5", fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.Previous.From.Name);
            Assert.AreEqual("A-5", fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.Previous.Previous.From.Name);
            
            Assert.IsNull(fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.Previous.Previous.Previous);

            Assert.AreEqual(45.0, fastestroute.TotalDistance);


            //longest routes should be going diagonals the whole way, except for one straight bit because odd number of jumps, equivalent to:
            var longestroute = routes.Last();
            Assert.AreEqual((Math.Sqrt(50) * 8) + 5, longestroute.TotalDistance);
        }
    }
}
