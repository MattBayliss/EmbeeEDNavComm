using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using EmbeeEDNav;
using EmbeeEDModel.Entities;

namespace EmbeeEDTests
{
    [TestClass]
    public class PluginTests
    {
        private Dictionary<string, Int16?> _conditions;
        private Dictionary<string, object> _state;
        private Dictionary<string, string> _textValues;
        private Dictionary<string, object> _extendedValues;

        [TestInitialize]
        public void Init()
        {
            _conditions = new Dictionary<string, short?>();
            _state = new Dictionary<string, object>();
            _textValues = new Dictionary<string, string>();
            _extendedValues = new Dictionary<string, object>();

            EmbeeEDNav.VAPlugin.VA_Init1(ref _state, ref _conditions, ref _textValues, ref _extendedValues);

        }

        [TestMethod]
        public void StateWhereIAm()
        {
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Eranin");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void PlotRouteWithoutRange()
        {
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Eranin");
            _textValues.SetValue(VAPlugin.TEXT_JUMPRANGE, string.Empty);

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.TargetSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "I Bootis");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            // no range set, so there should be no next system
            Assert.IsTrue(string.IsNullOrEmpty(_textValues.GetValue(VAPlugin.TEXT_NEXTSYSTEM, null)));

            // but keep hold of target system for when a range is added later
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_TARGETSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_TARGETSYSTEM].Equals("I Bootis", StringComparison.OrdinalIgnoreCase));

            // now add a jump range
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.JumpRange);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "8.0");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            // now we should have a route
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("I Bootis", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_TARGETSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_TARGETSYSTEM].Equals("I Bootis", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void PlotRouteAndDoIt()
        {
            // from Galaxy Map, Fastest Routes
            // LHS 262 to Melaie
            // LHS 262 -> LHS 2211 -> YIN SECTOR CL-Y D143 -> WYRD -> MORGOR -> MELIAE
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "LHS 262");
            _textValues.SetValue(VAPlugin.TEXT_JUMPRANGE, string.Empty);

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("LHS 262", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.JumpRange);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "12.0");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.TargetSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "MELIAE");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("LHS 2211", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "LHS 2211");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("LHS 2211", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("YIN SECTOR CL-Y D143", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "YIN SECTOR CL-Y D143");
            
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("YIN SECTOR CL-Y D143", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("WYRD", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "WYRD");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("WYRD", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("MORGOR", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "MORGOR");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("MORGOR", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("MELIAE", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "MELIAE");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("MELIAE", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(string.IsNullOrEmpty(_textValues[VAPlugin.TEXT_NEXTSYSTEM]));

        }


        [TestMethod]
        public void PlotRouteFromEraninToLave()
        {
            // from Galaxy Map, Fastest Routes
            // LHS 262 to Melaie
            // LHS 262 -> LHS 2211 -> YIN SECTOR CL-Y D143 -> WYRD -> MORGOR -> MELIAE
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Eranin");
            _textValues.SetValue(VAPlugin.TEXT_JUMPRANGE, string.Empty);

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.JumpRange);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "15.0");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.TargetSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Lave");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

        }

    }
}
