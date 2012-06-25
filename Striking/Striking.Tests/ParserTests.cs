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

namespace Striking.Tests
{
  using Striking;
  using Xunit;

  public class ParserTests
  {
    private IniParser parser;

    public ParserTests()
    {
      var file = "testdata.ini";
      this.parser = new IniParser(file);
      this.parser.Parse();
    }

    [Fact]
    public void GetValueUsingIndexer()
    {
      Assert.Equal<string>(this.parser["owner"]["name"], "John Doe");
    }

    [Fact]
    public void LegitimateParse()
    {
      Assert.DoesNotThrow(this.parser.Parse);
    }

    [Fact]
    public void TestFill()
    {
      var host = new SpecificAttribute();
      this.parser.Fill(host);
      Assert.Equal<string>(host.OwnerName, "John Doe");
      Assert.Equal<string>(host.IPAddress, "192.0.2.62");
    }

    [Fact]
    public void TestSimpleFill()
    {
      var simpleAttribute = new SimpleAttribute();
      this.parser.Fill(simpleAttribute);
      Assert.Equal<string>(simpleAttribute.Name, "John Doe");
      parser.Save();
    }

    private class SimpleAttribute
    {
      [Ini("name")]
      public string Name { get; set; }
    }

    private class SpecificAttribute
    {
      [Ini("owner", "name")]
      public string OwnerName { get; set; }

      [Ini("database", "server")]
      public string IPAddress { get; set; }
    }
  }
}
