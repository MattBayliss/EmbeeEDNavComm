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
            // LHS 2651 to Melaie
            // LHS 2651 - Loucetios - Meliae - Dahan - Asellus Primus - Eranin
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "LHS 2651");
            _textValues.SetValue(VAPlugin.TEXT_JUMPRANGE, string.Empty);

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("LHS 2651", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.JumpRange);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "21.17");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.TargetSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Eranin");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Loucetios", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Loucetios");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Loucetios", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Meliae", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Meliae");
            
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Meliae", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Dahan", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Dahan");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Dahan", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Asellus Primus", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Asellus Primus");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Asellus Primus", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Eranin");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(string.IsNullOrEmpty(_textValues[VAPlugin.TEXT_NEXTSYSTEM]));

        }


        [TestMethod]
        public void PlotRouteFromAliothToLave()
        {
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Alioth");
            _textValues.SetValue(VAPlugin.TEXT_JUMPRANGE, string.Empty);

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Alioth", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.JumpRange);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "15.0");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.TargetSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Lave");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

        }

    }
}
