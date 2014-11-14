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
        public void StarPathsMethods()
        {
            var path1 = new StarPath("from1", "to1", 5);
            var path2 = new StarPath("from2", "to2", 5);

            var starpaths = new StarPaths(new List<StarPath> { path1, path2 });

            //doubled for there and back
            Assert.AreEqual(4, starpaths.Count);

            // this will swap the order
            var fromto2 = starpaths.PopPathsFromSystem("to2");

            Assert.AreEqual(1, fromto2.Count);

            Assert.AreEqual("from2", fromto2[0].To);

            starpaths.RemovePath(fromto2[0]);

            Assert.AreEqual(3, starpaths.Count);

            var fromto1 = starpaths.PopPathsFromSystem("to1");
            Assert.AreEqual(1, fromto1.Count);
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
            Assert.AreEqual("Alpha", prevroute.From);
            Assert.AreEqual("Zulu", prevroute.To);
            Assert.AreEqual("Zulu", route.From);
            Assert.AreEqual("Charlie", route.To);
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
            Assert.AreEqual("Foxtrot", routeA.To);
            Assert.AreEqual("Foxtrot", routeB.To);

            Assert.AreEqual("Echo", routeA.From);
            Assert.AreEqual("Disco", routeB.From);

            var prevA = routeA.Previous;
            Assert.IsNotNull(prevA);

            var prevB = routeB.Previous;
            Assert.IsNotNull(prevB);

            Assert.AreEqual("Echo", prevA.To);
            Assert.AreEqual("Disco", prevB.To);

            Assert.AreEqual("Charlie", prevA.From);
            Assert.AreEqual("Charlie", prevB.From);
            
        }

        [TestMethod]
        public void LotsOfRoutes()
        {
            //using the grid for this test - the diagonals are 7.07 long, so a Jump range of 7.1 will include the diagonals
            var routes = _pathfinder.GetRoutes(7.1, "A-5", "J-5");

            // there will be HEAPS of routes, but the first result will be the one with the least distance, a straight line
            Assert.IsTrue(routes.Count > 1);

            var fastestroute = routes.First();

            Assert.AreEqual("J-5", fastestroute.To);
            Assert.AreEqual("I-5", fastestroute.From);
            Assert.AreEqual("H-5", fastestroute.Previous.From);
            Assert.AreEqual("G-5", fastestroute.Previous.Previous.From);
            Assert.AreEqual("F-5", fastestroute.Previous.Previous.Previous.From);
            Assert.AreEqual("E-5", fastestroute.Previous.Previous.Previous.Previous.From);
            Assert.AreEqual("D-5", fastestroute.Previous.Previous.Previous.Previous.Previous.From);
            Assert.AreEqual("C-5", fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.From);
            Assert.AreEqual("B-5", fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.Previous.From);
            Assert.AreEqual("A-5", fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.Previous.Previous.From);
            
            Assert.IsNull(fastestroute.Previous.Previous.Previous.Previous.Previous.Previous.Previous.Previous.Previous);

            Assert.AreEqual(45.0, fastestroute.TotalDistance);


            //longest routes should be going diagonals the whole way, except for one straight bit because odd number of jumps, equivalent to:
            var longestroute = routes.Last();
            Assert.AreEqual((Math.Sqrt(50) * 8) + 5, longestroute.TotalDistance);
        }
    }
}
