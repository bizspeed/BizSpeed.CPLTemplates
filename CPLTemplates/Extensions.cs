using System;
using System.Collections.Generic;
using System.Linq;

namespace BizSpeed.CPLTemplates.Extensions
{
    public static class IntegerExtensions
    {
        public static int NextClosestDivisibleBy(this int n, int m)
        {
            // find the quotient 
            int q = n / m;

            // 1st possible closest number 
            int n1 = m * q;

            // 2nd possible closest number 
            int n2 = (n * m) > 0 ? (m * (q + 1)) : (m * (q - 1));

            // if true, then n1 is the required closest number 
            if (Math.Abs(n - n1) < Math.Abs(n - n2))
                return n1;

            // else n2 is the required closest number 
            return n2;
        }
	}

    public static class StringExtensions
    {
		/// <summary>
		/// inspired by https://stackoverflow.com/a/42709701/93230
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static IEnumerable<string> ToHex(this String s)
		{
			if (s == null)
				throw new ArgumentNullException("s");

			int mod4Len = s.Length % 8;
			if (mod4Len != 0)
			{
				// pad to length multiple of 8
				s = s.PadLeft(((s.Length / 8) + 1) * 8, '0');
			}

			int numBitsInByte = 8;
			for (var i = 0; i < s.Length; i += numBitsInByte)
			{
				string eightBits = s.Substring(i, numBitsInByte);
				yield return string.Format("{0:X2}", Convert.ToByte(eightBits, 2));
			}
		}

        public static IEnumerable<byte> ToByte(this String s)
        {
			if (s == null)
				throw new ArgumentNullException("s");

			int mod4Len = s.Length % 8;
			if (mod4Len != 0)
			{
				// pad to length multiple of 8
				s = s.PadLeft(((s.Length / 8) + 1) * 8, '0');
			}

			int numBitsInByte = 8;
			for (var i = 0; i < s.Length; i += numBitsInByte)
			{
				string eightBits = s.Substring(i, numBitsInByte);
				yield return Convert.ToByte(eightBits, 2);
			}
		}

		public static IEnumerable<string> HexToChar(this IEnumerable<string> hexValues)
        {
            foreach (var hex in hexValues)
            {
                var value = Convert.ToInt32(hex, 16);
                yield return Char.ConvertFromUtf32(value);
            }
        }

		public static List<string> Wrap(this string text, int partLength)
		{
            List<string> lines =
                text
                    .Split(' ')
                    .Aggregate(new[] { "" }.ToList(), (a, x) =>
                    {
                        var last = a[a.Count - 1];
                        if ((last + " " + x).Length > partLength)
                        {
                            a.Add(x);
                        }
                        else
                        {
                            a[a.Count - 1] = (last + " " + x).Trim();
                        }
                        return a;
                    });

            return lines;
        }
	}
}
