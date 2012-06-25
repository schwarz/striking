# Striking

Striking is a small C# library for handling INI files. 
It's easy to extend and contains various features to ease the development process.

If you run into performance issues you can use Striking as a base to taylor your own problem-specific solution.

# Usage

_The following examples work if you use the testdata.ini file from the test library._

To load an ini file and access certain keys you would do this:

```csharp
IniParser parser = new IniParser("testdata.ini");
parser.Parse();
string name = parser["owner"]["name"];
```

You can also tell the constructor to immediately parse the specified file like this:

```csharp
IniParser parser = new IniParser("testdata.ini", true);
```

The first indexer specifies the section, the while the second specified the key:

```csharp
string name = parser["owner"]["name"];
```

# Install

# Requirements

* .NET Framework 4 or higher (has only been tested on this version)

# License

Striking is released under the MIT license. See LICENSE for details.
