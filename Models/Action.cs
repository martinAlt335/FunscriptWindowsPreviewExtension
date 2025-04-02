using System.Collections.Generic;

namespace FunscriptPreviewHandler.Models
{
    public class FunscriptAction
    {
        public int at { get; set; }
        public int pos { get; set; }
        public string type { get; set; }
        public List<FunscriptAction> subActions { get; set; }
    }
}