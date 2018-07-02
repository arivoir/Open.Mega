using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open.Mega
{
    internal static class MathEx
    {

        public static int Min(params int[] values)
        {
            if (values.Length < 1)
                throw new ArgumentException("values should have at least one element");
            var result = int.MaxValue;
            foreach (var v in values)
            {
                result = Math.Min(result, v);
            }
            return result;
        }
    }
}
