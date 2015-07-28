using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.App
{
    public class OutputUtils
    {
        /// <summary>
        /// Gets the text representation of the given memory value.
        /// </summary>
        /// <param name="memorySize">Size of the memory.</param>
        /// <returns>The text representation of the given memory value</returns>
        public static string GetMemoryText(long memorySize)
        {
            var units = new[] { "B", "KB", "MB", "GB", "TB" };
            var index = 0;
            double size = memorySize;
            while (size > 1024 && index < units.Length - 1)
            {
                size /= 1024;
                index++;
            }

            if (size / 100 > 1)
            {
                return string.Format("{0:0.0} {1}", size, units[index]);
            }
            else if (size / 10 > 1)
            {
                return string.Format("{0:0.00} {1}", size, units[index]);
            }
            else
            {
                return string.Format("{0:0.000} {1}", size, units[index]);
            }
        }
    }
}
