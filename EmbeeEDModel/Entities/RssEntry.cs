using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDModel.Entities
{
    public class RssEntry
    {
        public string Id { get; set; }
        public bool Read { get; set; }
        public DateTime DatePosted { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
