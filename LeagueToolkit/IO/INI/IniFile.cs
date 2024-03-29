﻿using System.Text;

namespace LeagueToolkit.IO.INI;

/// <summary>
///     Represents an Ini File which contains Sections and Properties
/// </summary>
public class IniFile
{
    /// <summary>
    ///     Initializes an empty <see cref="IniFile" />
    /// </summary>
    public IniFile()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="IniFile" /> with the specified Sections
    /// </summary>
    /// <param name="sections">Sections of this <see cref="IniFile" /></param>
    public IniFile(Dictionary<string, Dictionary<string, string>> sections)
    {
        Sections = sections;
    }

    /// <summary>
    ///     Initializes a new <see cref="IniFile" /> from the specified location
    /// </summary>
    /// <param name="fileLocation">Location to read from</param>
    public IniFile(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="IniFile" /> from a <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to read from</param>
    public IniFile(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine().Split(new[] { '[', ']', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (line.Length != 0 && line[0].Length != 0)
                {
                    Sections.Add(line[0], new Dictionary<string, string>());
                    ReadValues(sr, line[0]);
                }
            }
        }
    }

    /// <summary>
    ///     Sections of this <see cref="IniFile" />
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> Sections { get; set; } = new();

    /// <summary>
    ///     Reads the Properties and Values of the specified Section from the specified <see cref="StreamReader" />
    /// </summary>
    /// <param name="sr">The <see cref="StreamReader" /> to read from</param>
    /// <param name="section">Name of the Section to read</param>
    private void ReadValues(StreamReader sr, string section)
    {
        string[] line = null;

        while (sr.Peek() != '[')
        {
            if (!sr.EndOfStream)
            {
                if ((line = sr.ReadLine().Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)).Length != 0)
                {
                    Sections[section].Add(line[0], line[1]);
                }
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Writes this <see cref="IniFile" /> to the specified location
    /// </summary>
    /// <param name="fileLocation">Location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="IniFile" /> into a <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen))
        {
            foreach (var entry in Sections)
            {
                sw.WriteLine("[{0}]", entry.Key);
                foreach (var value in entry.Value)
                {
                    sw.WriteLine("{0}={1}", value.Key, value.Value);
                }
            }
        }
    }
}