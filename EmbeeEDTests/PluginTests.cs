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
            // from elitetradingtool.co.uk:
            // Eranin to CE Bootis (30.67 light years) Eranin → Aulin → Rakapila → LP 271-25 → Ross 52 → CE Bootis
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Eranin");
            _textValues.SetValue(VAPlugin.TEXT_JUMPRANGE, string.Empty);

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.JumpRange);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "8.0");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.TargetSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "CE Bootis");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Aulin", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Aulin");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Aulin", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Rakapila", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Rakapila");
            
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Rakapila", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("LP 271-25", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "LP 271-25");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("LP 271-25", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("Ross 52", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Ross 52");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Ross 52", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_NEXTSYSTEM].Equals("CE Bootis", StringComparison.OrdinalIgnoreCase));

            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)NavCommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "CE Bootis");

            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("CE Bootis", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_NEXTSYSTEM));
            Assert.IsTrue(string.IsNullOrEmpty(_textValues[VAPlugin.TEXT_NEXTSYSTEM]));

        }
    }
}
