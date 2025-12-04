using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class RegionNames
    {
        private static string titleRegion = "TitleRegion";
        private static string contentRegion = "ContentRegion";

        public static string TitleRegion { get => titleRegion; set => titleRegion = value; }
        public static string ContentRegion { get => contentRegion; set => contentRegion = value; }
    }
}
