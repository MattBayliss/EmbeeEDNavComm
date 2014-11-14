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
            _conditions.SetValue(VAPlugin.COND_NAVCOMMAND, (short)CommandEnum.CurrentSystem);
            _textValues.SetValue(VAPlugin.TEXT_NAVVALUE, "Eranin");
            VAPlugin.VA_Invoke1("", ref _state, ref _conditions, ref _textValues, ref _extendedValues);
            Assert.IsTrue(_textValues.ContainsKey(VAPlugin.TEXT_CURRENTSYSTEM));
            Assert.IsTrue(_textValues[VAPlugin.TEXT_CURRENTSYSTEM].Equals("Eranin", StringComparison.OrdinalIgnoreCase));
        }
    }
}
