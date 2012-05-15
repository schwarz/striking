// -----------------------------------------------------------------------
// <copyright file="IniBase.cs" company="">
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
  using System.Linq;
  using System.Reflection;

  /// <summary>
  /// TODO: Update summary.
  /// </summary>
  public class IniBase
  {

    public void Fill(IniParser parser)
    {
      parser.Parse();

      var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

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
            var customAttribute = prop.GetCustomAttributesData().ElementAt(i);

            var value = string.Empty;
            // key, section
            if (customAttribute.ConstructorArguments.Count == 2)
            {
              var section = customAttribute.ConstructorArguments[1].Value.ToString();
              var key = customAttribute.ConstructorArguments[0].Value.ToString();

              value = parser.Pairs[section + IniParser.Delimiter + key];
            }

            prop.SetValue(this, value, null);
          }
        }
      }
    }
  }
}
