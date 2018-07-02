using System;

namespace Open.Mega
{
    public class MegaException : Exception
    {
        public MegaException(int[] codes)
        {
            Codes = codes;
        }

        public int[] Codes { get; private set; }
    }
}