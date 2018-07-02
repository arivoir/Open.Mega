using System.Text;

namespace Open.Mega
{
    internal static class ByteEx
    {
        public static string ToUTF8String(this byte[] data)
        {
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static byte[] ToBytes(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
