using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextControlBox_WinUI.Helper
{
    internal class WordWrapHelper
    {
        public static string[] WordWrap(IEnumerable<string> lines, int maxWidth)
        {
            List<string> wrappedLines = new List<string>();

            foreach (var line in lines)
            {
                StringBuilder currentLine = new StringBuilder();
                int currentLength = 0;

                foreach (char character in line)
                {
                    if (character == ' ' && currentLength > 0 && currentLength + 1 > maxWidth)
                    {
                        wrappedLines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentLength = 0;
                    }
                    else
                    {
                        currentLine.Append(character);
                        currentLength++;

                        if (currentLength == maxWidth)
                        {
                            wrappedLines.Add(currentLine.ToString());
                            currentLine.Clear();
                            currentLength = 0;
                        }
                    }
                }

                if (currentLength > 0)
                {
                    wrappedLines.Add(currentLine.ToString());
                }
            }

            return wrappedLines.ToArray();
        }
    }
}