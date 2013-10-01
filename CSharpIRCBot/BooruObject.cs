using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpIRCBot
{
    class BooruObject
    {
        public bool has_comments { get; set; }
        public object parent_id { get; set; }
        public string status { get; set; }
        public bool has_children { get; set; }
        public string created_at { get; set; }
        public string md5 { get; set; }
        public bool has_notes { get; set; }
        public string rating { get; set; }
        public string author { get; set; }
        public int creator_id { get; set; }
        public int width { get; set; }
        public string source { get; set; }
        public string preview_url { get; set; }
        public int score { get; set; }
        public string tags { get; set; }
        public int height { get; set; }
        public int file_size { get; set; }
        public int id { get; set; }
        public string file_url { get; set; }
    }
}
