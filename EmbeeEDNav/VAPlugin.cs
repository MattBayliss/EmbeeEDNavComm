using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmbeeEDNav
{
    public class VAPlugin
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public const string COND_NAVCOMMAND = "embee.NavCommand";
        public const string TEXT_NAVVALUE = "embee.NavText";
        public const string TEXT_VOICERESPONSE = "embee.VoiceReponse";
        public const string TEXT_TARGETSYSTEM = "embee.TargetSystem";
        public const string TEXT_CURRENTSYSTEM = "embee.CurrentSystem";
        public const string TEXT_NEXTSYSTEM = "embee.NextSystem";
        public const string TEXT_JUMPRANGE = "embee.JumpRange";

        private const string STATE_COURSEID = "embee.CourseId";

        public static string VA_DisplayName()
        {
            return "Embee NavCom";
        }

        public static string VA_DisplayInfo()
        {
            return "Embee NavCom:\r\nA Navigation helper for Elite: Dangerous.";
        }

        public static Guid VA_Id()
        {
            return new Guid("{2A4A5F10-E9BE-4B2F-9059-16F77933C7E1}");
        }

        public static void VA_Init1(ref Dictionary<string, object> state, ref Dictionary<string, Int16?> conditions, ref Dictionary<string, string> textValues, ref Dictionary<string, object> extendedValues)
        {
            Logger.Debug("VA_Init1 called");

            state.SetValue(STATE_COURSEID, string.Empty);
            conditions.SetValue<Int16?>(COND_NAVCOMMAND, 0);
            textValues.SetValue(TEXT_VOICERESPONSE, string.Empty);

            try
            {
                NavServerConnection.EnsureServerIsRunning();
            }
            catch(Exception ex)
            {
                Logger.Error("Problem with NavServer", ex);
                textValues.SetValue(TEXT_VOICERESPONSE, ex.Message);
            }
        }

        public static void VA_Exit1(ref Dictionary<string, object> state)
        {
            Logger.Debug("VA_Exit1 called");
        }

        public static void VA_Invoke1(String context, ref Dictionary<string, object> state, ref Dictionary<string, Int16?> conditions, ref Dictionary<string, string> textValues, ref Dictionary<string, object> extendedValues)
        {
            Logger.Debug("VA_Invoke1 called");

            var courseid = string.Empty;
            object courseidobj;
            if(state.TryGetValue(STATE_COURSEID, out courseidobj)) {
                if(courseidobj != null) {
                    courseid = (string)courseidobj;
                }
            };

            Int16? command = null;
            if(conditions.TryGetValue(COND_NAVCOMMAND, out command)) {
                if(command == null) {
                    command = 0;
                }
                //command parsed
            }

            // the value passed along (ie system name)
            string navvalue = string.Empty;
            if(textValues.TryGetValue(TEXT_NAVVALUE, out navvalue)) {
                if(navvalue == null) {
                    navvalue = string.Empty;
                }
            }

            var cmd = (NavCommandEnum)command;

            switch (cmd)
            {
                case NavCommandEnum.TargetSystem:
                    textValues.SetValue(TEXT_TARGETSYSTEM, navvalue);
                    break;
                default:
                    break;
            }

            var message = string.Format("{0}|{1}|{2}", command, courseid, navvalue);

            // setting something for VA to say, incase this plugin crashes horribly
            textValues.SetValue(TEXT_VOICERESPONSE, "Something went terribly wrong");

            try
            {
                var response = NavServerConnection.SendMessageToServerAsync(message).Result;
                // response is of the format:
                // [courseid],[voice response]

                if (!string.IsNullOrEmpty(response))
                {
                    var responsebits = response.Split(new char[] { '|' });

                    if (responsebits.Length == 4)
                    {
                        courseid = responsebits[0].Trim();
                        var currentsystem = responsebits[1].Trim();
                        var nextsystem = responsebits[2].Trim();
                        var voiceresponse = responsebits[3].Trim();

                        state.SetValue(STATE_COURSEID, courseid);
                        textValues.SetValue(TEXT_CURRENTSYSTEM, currentsystem);
                        textValues.SetValue(TEXT_NEXTSYSTEM, nextsystem);
                        textValues.SetValue(TEXT_VOICERESPONSE, voiceresponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in communication with NavServer", ex);
                textValues.SetValue(TEXT_VOICERESPONSE, string.Format("error occurred with the Nav Computer interface. {0}", ex.Message));
            }
        }
    }
}
