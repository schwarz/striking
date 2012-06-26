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
  using System.Reflection;
  using System.Text.RegularExpressions;

  /// <summary>
  /// TODO: Update summary.
  /// </summary>
  public class IniParser
  {
    /// <summary>
    /// A Dictionary containing Dictionaries. The key represents a section, the nested key an actual value.
    /// </summary>
    private Dictionary<string, Dictionary<string, string>> pairs = new Dictionary<string, Dictionary<string, string>>();

    /// <summary>
    /// Regular expression pattern to match sections.
    /// </summary>
    private readonly Regex sectionPattern = new Regex(@"^\[[a-zA-Z.]*\]$", RegexOptions.Compiled);

    // Add to this HashSet for additional comment delimiters
    private readonly HashSet<char> comments = new HashSet<char> { ';' };

    /// <summary>
    /// Gets or sets a string containing the path of the ini file.
    /// </summary>
    public string FilePath { get; set; }

    public IniParser(string filePath, bool parseImmediately = false)
    {
      this.FilePath = filePath;

      if (parseImmediately) this.Parse();
    }

    public Dictionary<string, string> this[string key]
    {
      get
      {
        return this.pairs[key];
      }
      set
      {
        this.pairs[key] = value;
      }
    }

    /// <summary>
    /// Fills an object's marked properties with values from the ini file.
    /// </summary>
    /// <param name="target">The object to operate on</param>
    public void Fill(object target)
    {
      foreach (var pair in this.getRelatedPropertiesAndAttributeData(target))
      {
        var customAttributeData = pair.Item2;
        var property = pair.Item1;

        var value = string.Empty;
        string section = string.Empty;
        string key = string.Empty;


        if (customAttributeData.ConstructorArguments.Count == 1)
        {
          // key
          key = customAttributeData.ConstructorArguments[0].Value.ToString();
          section = this.sectionOfKey(key);
        }
        else if (customAttributeData.ConstructorArguments.Count == 2)
        {
          // key, section
          section = customAttributeData.ConstructorArguments[0].Value.ToString();
          key = customAttributeData.ConstructorArguments[1].Value.ToString();
        }

        value = this[section][key];
        property.SetValue(target, value, null);
      } 
    }

    /// <summary>
    /// Returns a collection of properties and custom attribute data relevant to the parser.
    /// </summary>
    /// <param name="target">The target object</param>
    /// <returns>A collection of properties and custom attribute data</returns>
    private IEnumerable<Tuple<PropertyInfo, CustomAttributeData>> getRelatedPropertiesAndAttributeData(object target)
    {
      var properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

      foreach (var prop in properties)
      {
        // Only copy properties with public get and set methods
        if (!prop.CanRead || !prop.CanWrite || prop.GetGetMethod(false) == null || prop.GetSetMethod(false) == null) continue;
        // Skip indexers
        if (prop.GetGetMethod(false).GetParameters().Length > 0) continue;

        var customAttributes = prop.GetCustomAttributes(typeof(IniAttribute), true).ToArray();
        if (customAttributes.Length == 0)
        {
          continue; // no IniAttribute
        }

        for (int i = 0; i < prop.GetCustomAttributes(false).Length; i++)
        {
          if (prop.GetCustomAttributes(false).ElementAt(i).GetType() == typeof(IniAttribute))
          {
            yield return new Tuple<PropertyInfo, CustomAttributeData>(prop, prop.GetCustomAttributesData().ElementAt(i));
          }
        }
      }
    }

    /// <summary>
    /// Parses an ini file.
    /// </summary>
    public void Parse()
    {
      this.pairs.Clear();
      using (var sr = new StreamReader(this.FilePath))
      {
        string line;
        string section = string.Empty; // the current section
        while ((line = sr.ReadLine()) != null)
        {
          if (this.isIrrelevant(line)) continue;

          line = line.Trim();
          if (line[0] == '[')
          {
            if(sectionPattern.IsMatch(line))
            {
              section = line.Substring(1, line.Length - 2);
              this.pairs[section] = new Dictionary<string, string>();
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

              this.pairs[section][pair.Item1] = pair.Item2;
            }
            else
            {
              throw new InvalidDataException("Found no or more than one equal sign in a key=value line.");
            }
          }
        }
      }
    }

    /// <summary>
    /// A line is irrelevant if it only contains a comment, whitespace or is empty.
    /// </summary>
    /// <param name="line">The line</param>
    /// <returns>Whether the line is irrelevant or not.</returns>
    private bool isIrrelevant(string line)
    {
      if (string.IsNullOrWhiteSpace(line) ||
         comments.Contains(line.Trim()[0]))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Saves changes back to an ini file.
    /// </summary>
    /// <param name="objects">Optionally objects filled with ini data can be passed. Later object's data overwrites previous data.</param>
    public void Save(params object[] objects)
    {
      // Write the changes from the objects into the data set
      foreach (var target in objects)
      {
        foreach (var pair in this.getRelatedPropertiesAndAttributeData(target))
        {
          var customAttributeData = pair.Item2;
          var property = pair.Item1;

          var value = string.Empty;
          string section = string.Empty;
          string key = string.Empty;

          // Only properties with the section specified
          if (customAttributeData.ConstructorArguments.Count == 2)
          {
            // key, section
            section = customAttributeData.ConstructorArguments[0].Value.ToString();
            key = customAttributeData.ConstructorArguments[1].Value.ToString();

            this.pairs[section][key] = property.GetValue(target, null).ToString();
          }
        }
      }

      // Write to file
      using (var sw = new StreamWriter(this.FilePath,false))
      {
        foreach (var pair in this.pairs)
        {
          sw.WriteLine(string.Format("[{0}]", pair.Key));
          foreach (var ipair in pair.Value)
          {
            sw.WriteLine(string.Format("{0}={1}", ipair.Key, ipair.Value));
          }
        }
      }
    }

    /// <summary>
    /// Returns the name of the section a key first occurs in.
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>Section name</returns>
    private string sectionOfKey(string key)
    {
      foreach (var outer in this.pairs)
      {
        if (outer.Value.Keys.Contains(key))
        {
          return outer.Key;
        }
      }
      throw new KeyNotFoundException();
    }
  }
}