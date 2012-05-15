// -----------------------------------------------------------------------
// <copyright file="IniParser.cs" company="">
// Copyright (c) 2012 Bernhard Schwarz

// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// -----------------------------------------------------------------------

namespace Striking
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.RegularExpressions;

  /// <summary>
  /// TODO: Update summary.
  /// </summary>
  public class IniParser
  {
    public const string Delimiter = ";;";

    /// <summary>
    /// Gets or sets a string containing the path of the .ini.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Gets or sets a Dictionary containing all the key/value pairs. The key contains both the section and the key. For example: "General;;Name"
    /// </summary>
    public Dictionary<string, string> Pairs { get; set; }

    public IniParser(string filePath)
    {
      this.FilePath = filePath;
      this.Pairs = new Dictionary<string, string>();
    }

    public void Parse()
    {
      this.Pairs.Clear();
      // Add to this HashSet for additional comment delimiters
      var comments = new HashSet<char> { ';' };

      using (var sr = new StreamReader(this.FilePath))
      {
        string line;
        string section = string.Empty; // the current section
        while ((line = sr.ReadLine()) != null)
        {
          // Skip empty lines
          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          // Skip comment lines
          if (comments.Contains(line.Trim()[0]))
          {
            continue;
          }

          line = line.Trim();
          if (line[0] == '[')
          {
            if (Regex.IsMatch(line, @"^\[[a-zA-Z.]*\]$", RegexOptions.Compiled))
            {
              section = line.Substring(1, line.Length - 2);
            }
          }
          else
          {
            // Exclude everything after comment-signs
            for (int i = 0; i < line.Length; i++)
            {
              if (comments.Contains(line[i]))
              {
                line = line.Substring(0, i);
                line.TrimEnd(' ');
                break;
              }
            }

            // Make sure there is only one equal sign, could probably be optimized
            if (line.Count(c => c == '=') == 1)
            {
              var keyValueSplit = line.Split('=');
              var pair = new Tuple<string, string>(keyValueSplit[0].TrimEnd(' '),
                                                   keyValueSplit[1].TrimStart(' '));
              this.Pairs[section + Delimiter + pair.Item1] = pair.Item2;
            }
            else
            {
              throw new InvalidDataException("Found no or more than one equal sign in a key=value line.");
            }
          }
        }
      }
    }
  }
}
