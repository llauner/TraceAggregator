using System.IO;
using System.Text;

namespace TraceAggregator.Extension
{
    public static class StringExtension
    {

        public static Stream ToStream(this string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str ?? ""));
        }

        public static string ReadAsString(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
