﻿namespace LeagueToolkit.Helpers.Attributes;

/// <summary>
///     Used as an attribute on properties which are used to parse content from INI files
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IniPropertyAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new <see cref="IniPropertyAttribute" />
    /// </summary>
    /// <param name="name">The Name of the INI property this attribute is attached to</param>
    /// <param name="serializationType">The Serialization type of the INI property this attribute is attached to</param>
    public IniPropertyAttribute(string name, Type serializationType)
    {
        Name = name;
        SerializationType = serializationType;
    }

    /// <summary>
    ///     Name of the INI property
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Serialization type of the INI property
    /// </summary>
    public Type SerializationType { get; set; }
}