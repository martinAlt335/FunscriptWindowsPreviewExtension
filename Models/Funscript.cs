using System.Collections.Generic;

namespace FunscriptPreviewHandler.Models
{
    public class Funscript
    {
        public List<FunscriptAction> actions { get; set; } = new List<FunscriptAction>();
        public FunscriptMetadata metadata { get; set; }
        public string version { get; set; }
        public bool inverted { get; set; }
        public int range { get; set; }
    }

    public class FunscriptMetadata
    {
        public int? duration { get; set; }
        public double? average_speed { get; set; }
        public string creator { get; set; }
        public string description { get; set; }
        public string license { get; set; }
        public string notes { get; set; }
        public string[] performers { get; set; }
        public string script_url { get; set; }
        public string[] tags { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string video_url { get; set; }
        public FunscriptChapter[] chapters { get; set; }
    }
}