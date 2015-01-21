using EmbeeEDModel.Entities;
using EmbeePathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer
{
    public class Controller
    {
        private Dictionary<string, JumpRoute> _jumpRoutes;
        private Universe _universe;
        private PathFinder _pathFinder;
        private double _jumpRange;
        private string _lastTarget;
        private string _current;

        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Controller()
        {
            _universe = null;
            _pathFinder = null;
            _jumpRange = 0; // TODO: save this value between sessions?
            _lastTarget = string.Empty;
            _current = string.Empty;
            _jumpRoutes = new Dictionary<string, JumpRoute>();
        }

        public async Task InitAsync()
        {
            var starLoader = new StarLoader();
            var stars = await starLoader.GetStarsAsync();
            _universe = new Universe(stars);
            _pathFinder = new PathFinder(_universe);

            // TODO: this takes way too long, and doesn't seem to actually work
            //AddSystemsToSpeechDictionary();
        }

        public async Task<string> ProcessMessageAsync(string incomingMessage)
        {
            //if for some reason Init() wasn't run
            if (_universe == null)
            {
                Logger.Trace("Universe wasn't loaded - running InitAsync()");
                await InitAsync();
            }

            var result = await Task.Run<string>(() =>
            {
                Logger.Trace("Parsing message: {0}", incomingMessage);
                var msg = ParseMessage(incomingMessage);

                switch (msg.Command)
                {
                    case CommandEnum.CurrentSystem:
                        msg = CommandCurrentSystem(msg);
                        break;
                    case CommandEnum.TargetSystem:
                        msg = CommandTargetSystem(msg);
                        break;

                    case CommandEnum.JumpRange:
                        msg = CommandJumpRange(msg);
                        break;

                    default:
                        msg.Value = "Invalid command code received";
                        msg.CourseId = string.Empty;
                        msg.NextSystem = string.Empty;
                        break;
                }

                return msg.ToString();

            });

            return result;

        }

        private NavMessage ParseMessage(string message)
        {
            var msg = new NavMessage();

            //parse message

            var messagebits = message.Split(new char[] { '|' });
            if (messagebits.Length < 3)
            {
                //not enough message parts!
                throw new ArgumentException("The message is incorrectly formatted");
            }

            CommandEnum command;
            if (!Enum.TryParse(messagebits[0], out command))
            {
                //command needs to be a number!
                throw new ArgumentException("The message is incorrectly formatted");
            }
            else
            {
                msg.Command = command;
            }

            msg.CourseId = messagebits[1].Trim();

            // if messagebits.Length is greater than 3, it means there might be
            // commas in what's meant to be the voice input - let's put them back
            if (messagebits.Length > 3)
            {
                string[] endbits = new string[messagebits.Length - 2];
                Array.Copy(messagebits, 2, endbits, 0, endbits.Length);

                msg.Value = string.Join("|", endbits).Trim();
            }
            else
            {
                msg.Value = messagebits[2].Trim();
            }

            msg.CurrentSystem = _current;

            return msg;
        }

        #region Command Processing

        private NavMessage CommandCurrentSystem(NavMessage msg)
        {
            var current = msg.Value;
            StarSystem currentSystem = null;

            if (!string.IsNullOrEmpty(current))
            {
                currentSystem = _universe[current];
            }

            if (currentSystem == null)
            {
                msg.CurrentSystem = string.Empty;
                _current = string.Empty;
                msg.Value = string.Format("The system name {0} is unrecognised. Please re-state", current);
                return msg;
            }

            msg.CurrentSystem = _current = currentSystem.Name;

            if (string.IsNullOrEmpty(msg.CourseId))
            {
                msg.CourseId = Guid.NewGuid().ToString();
                msg.NextSystem = string.Empty;
                msg.Value = string.Format("Current system set to {0}", msg.CurrentSystem);
            }
            else
            {
                // previously had a course
                if (_jumpRoutes.ContainsKey(msg.CourseId))
                {
                    var route = _jumpRoutes[msg.CourseId];

                    // check to see if we reached the target
                    if (route.To.Name.Equals(msg.CurrentSystem, StringComparison.OrdinalIgnoreCase))
                    {
                        msg.NextSystem = string.Empty;
                        _jumpRoutes.Remove(msg.CourseId);
                        msg.CourseId = null;
                        msg.Value = "You have reached your destination";
                    }
                    else if (route.From.Name.Equals(msg.CurrentSystem, StringComparison.OrdinalIgnoreCase))
                    {
                        msg.NextSystem = route.To.Name;
                        msg.Value = string.Format("jump to your destination, {0}", msg.NextSystem);
                    }
                    else
                    {
                        var jump = route;
                        bool keeplooking = true;
                        while (keeplooking)
                        {
                            if (jump.From.Name.Equals(msg.CurrentSystem, StringComparison.OrdinalIgnoreCase))
                            {
                                keeplooking = false;
                                msg.NextSystem = route.To.Name;
                                msg.Value = string.Format("jump to your destination, {0}", msg.NextSystem);
                            }
                            else if (jump.Previous == null)
                            {
                                msg.Value = string.Format("you are off course. You'll have to plot a new course to {0}", route.To);
                                keeplooking = false;
                                msg.NextSystem = string.Empty;
                                _jumpRoutes.Remove(msg.CourseId);
                                msg.CourseId = null;
                            }
                            else
                            {
                                jump = jump.Previous;
                            }
                        }

                    }
                }
                else
                {
                    // course id is missing!
                    msg.Value = "I can't remember where you wanted to go. Please plot a new course.";
                    msg.CourseId = string.Empty;
                    msg.NextSystem = string.Empty;
                }
            }
            return msg;
        }

        private NavMessage CommandTargetSystem(NavMessage msg)
        {
            var target = msg.Value;

            ClearRoute(msg.CourseId);
            msg.NextSystem = string.Empty;

            if (target.Equals(msg.CurrentSystem, StringComparison.OrdinalIgnoreCase))
            {
                msg.Value = string.Format("You're already in the {0} system", target); //TODO: add responses to a config file
                return msg;
            }

            // can we find the target system?
            var targetSystem = _universe[target];

            if (targetSystem == null)
            {
                msg.Value = string.Format("I don't recognise the system {0}. Please repeat.", target); //TODO: add responses to a config file
                return msg;
            }

            // make sure we actually have a jump range
            if (_jumpRange <= 0)
            {
                msg.Value = string.Format("I don't know your jump range"); //TODO: add responses to a config file
                _lastTarget = target;
                return msg;
            }
            var current = msg.CurrentSystem;
            var currentSystem = _universe[current];

            if (currentSystem == null)
            {
                msg.Value = string.Format("What system are we in currently?");
                msg.NextSystem = string.Empty;
                return msg;
            }

            var routes = _pathFinder.GetRoutes(_jumpRange, currentSystem, targetSystem);

            if (routes.Count == 0)
            {
                msg.Value = string.Format("There are no possible routes to {0}. You need a better jump range.");
                return msg;
            }

            // TODO: give commander options - but for now, just the first route
            msg.CourseId = Guid.NewGuid().ToString();
            var route = routes.First();

            _jumpRoutes.Add(msg.CourseId, route);

            var step = route;
            while ((!step.From.Name.Equals(current, StringComparison.OrdinalIgnoreCase)) && (step.Previous != null))
            {
                step = step.Previous;
            }

            msg.NextSystem = step.To.Name;

            return msg;
        }

        private NavMessage CommandJumpRange(NavMessage msg)
        {
            var rangeStr = msg.Value;

            // is the jump range valid?
            double range;

            bool rangeisvalid = false;

            if (double.TryParse(rangeStr, out range))
            {
                rangeisvalid = (range > 0);
            }
            if (!rangeisvalid) {
                msg.Value = "Invalid jump range specified. Please repeat.";
                return msg;
            }

            _jumpRange = range;

            var target = _lastTarget;
            JumpRoute oldRoute = null;

            // if we already have a course, get the target from there
            if (!string.IsNullOrEmpty(msg.CourseId))
            {
                if (_jumpRoutes.ContainsKey(msg.CourseId))
                {
                    oldRoute = _jumpRoutes[msg.CourseId];
                    target = oldRoute.To.Name;
                }
            }

            // we might not have a course - might just be setting the range. All good
            if (string.IsNullOrEmpty(target))
            {
                msg.Value = string.Format("Jump range set to {0}", rangeStr);
                return msg;
            }

            // can we find the target system?
            var targetSystem = _universe[target];

            if (targetSystem == null)
            {
                msg.Value = string.Format("I don't recognise the system {0}. Please restate your destination.", target); //TODO: add responses to a config file
                return msg;
            }

            var current = msg.CurrentSystem;
            var currentSystem = _universe[current];

            if (currentSystem == null)
            {
                msg.Value = string.Format("What system are we in currently?");
                msg.NextSystem = string.Empty;
                return msg;
            }

            var routes = _pathFinder.GetRoutes(_jumpRange, currentSystem, targetSystem);

            if (routes.Count == 0)
            {
                msg.Value = string.Format("There are no possible routes to {0}. You need a better jump range.", target);
                return msg;
            }

            // TODO: give commander options - but for now, just the first route
            var route = routes.First();

            if (oldRoute == null) {
                msg.CourseId = Guid.NewGuid().ToString();
                _jumpRoutes.Add(msg.CourseId, route);
            }
            else
            {
                // reusing old courseId
                oldRoute = null;
                _jumpRoutes[msg.CourseId] = route;
            }

            var step = route;
            while ((!step.From.Name.Equals(current, StringComparison.OrdinalIgnoreCase)) && (step.Previous != null))
            {
                step = step.Previous;
            }

            msg.NextSystem = step.To.Name;

            var jumps = route.Jumps;
            if (jumps == 1)
            {
                msg.Value = string.Format("{0} is within range - we can jump straight there", route.To);
            }
            else
            {
                msg.Value = string.Format("A {0} jump route has been plotted to {1}. For the first leg, jump to {2}", jumps, route.To, msg.NextSystem);
            }

            return msg;
        }

        #endregion

        private void ClearRoute(string courseId)
        {
            if (!string.IsNullOrEmpty(courseId))
            {
                if (_jumpRoutes.ContainsKey(courseId))
                {
                    _jumpRoutes.Remove(courseId);
                }
            }
        }

        private void AddSystemsToSpeechDictionary()
        {
            var wordsToAdd = new List<string>();
            foreach (var star in _universe.StarNames)
            {
                var words = star.Split(new char[] { ' ', '.', ',' });
                foreach (var word in words)
                {
                    if (word.Length < 3) continue;

                    long testnumber;
                    if (long.TryParse(word, out testnumber))
                    {
                        continue;
                    }
                    wordsToAdd.Add(word);
                }
            }

            EmbeeSpeechDictionaryUtils.SpeechUtilities.AddWords(wordsToAdd.Distinct().ToList());
        }


    }
}
