using System.Drawing;

namespace Common.WebDefinitions.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHexadecimal(this Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
    }
}
