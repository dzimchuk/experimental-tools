using System.IO;
using System.Text;

namespace ExperimentalTools.Tests
{
    internal static class Extensions
    {
        public static string HomogenizeLineEndings(this string input)
        {
            var builder = new StringBuilder();

            using (var reader = new StringReader(input))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    builder.AppendLine(line);
                }
            }

            return builder.ToString();
        }
    }
}
