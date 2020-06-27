using System;
using System.Linq;

namespace PolyConverter
{
    static class ByteArrayExtensions
    {
        /// <summary>A bit wasteful but it gets the job done.</summary>
        public static byte[] Replace(this byte[] bytes, string source, string replacement)
        {
            string hex = BitConverter.ToString(bytes);
            hex = hex.Replace(source, replacement);
            return hex.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
        }
    }
}
