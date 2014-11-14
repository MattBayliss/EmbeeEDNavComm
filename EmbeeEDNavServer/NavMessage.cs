using EmbeeEDModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer
{
    public class NavMessage
    {
        public CommandEnum Command { get; set; }
        public string CourseId { get; set; }
        public string CurrentSystem { get; set; }
        public string NextSystem { get; set; }
        public string Value { get; set;}

        public NavMessage()
        {
            Command = 0;
            CourseId = string.Empty;
            CurrentSystem = string.Empty;
            NextSystem = string.Empty;
            Value = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}", CourseId, CurrentSystem, NextSystem, Value);
        }
    }
}
