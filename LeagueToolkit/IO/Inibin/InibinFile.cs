using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Cryptography;

namespace LeagueToolkit.IO.Inibin;

/// <summary>
///     Represents binary ini files such as Troybin, Inibin and Cfgbin
/// </summary>
public class InibinFile
{
    /// <summary>
    ///     Initializes a blank <see cref="InibinFile" />
    /// </summary>
    public InibinFile()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="InibinFile" /> with the specified sets
    /// </summary>
    /// <param name="sets">Sets to add</param>
    public InibinFile(Dictionary<InibinFlags, InibinSet> sets)
    {
        Sets = sets;
    }

    /// <summary>
    ///     Initializes a new <see cref="InibinFile" /> from the specified location
    /// </summary>
    /// <param name="fileLocation">The location to read from</param>
    public InibinFile(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="InibinFile" /> from a <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to read from</param>
    public InibinFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            uint version = br.ReadByte();

            uint stringDataLength = 0;
            InibinFlags flags = 0;
            if (version == 1)
            {
                br.ReadBytes(3);
                var valueCount = br.ReadUInt32();
                stringDataLength = br.ReadUInt32();

                Sets.Add(InibinFlags.StringList,
                    new InibinSet(br, InibinFlags.StringList, (uint) br.BaseStream.Length - stringDataLength,
                        valueCount));
            }
            else if (version == 2)
            {
                stringDataLength = br.ReadUInt16();
                flags = (InibinFlags) br.ReadUInt16();

                if (flags.HasFlag(InibinFlags.Int32List))
                    Sets.Add(InibinFlags.Int32List, new InibinSet(br, InibinFlags.Int32List));
                if (flags.HasFlag(InibinFlags.Float32List))
                    Sets.Add(InibinFlags.Float32List, new InibinSet(br, InibinFlags.Float32List));
                if (flags.HasFlag(InibinFlags.FixedPointFloatList))
                    Sets.Add(InibinFlags.FixedPointFloatList, new InibinSet(br, InibinFlags.FixedPointFloatList));
                if (flags.HasFlag(InibinFlags.Int16List))
                    Sets.Add(InibinFlags.Int16List, new InibinSet(br, InibinFlags.Int16List));
                if (flags.HasFlag(InibinFlags.Int8List))
                    Sets.Add(InibinFlags.Int8List, new InibinSet(br, InibinFlags.Int8List));
                if (flags.HasFlag(InibinFlags.BitList))
                    Sets.Add(InibinFlags.BitList, new InibinSet(br, InibinFlags.BitList));
                if (flags.HasFlag(InibinFlags.FixedPointFloatListVec3))
                    Sets.Add(InibinFlags.FixedPointFloatListVec3,
                        new InibinSet(br, InibinFlags.FixedPointFloatListVec3));
                if (flags.HasFlag(InibinFlags.Float32ListVec3))
                    Sets.Add(InibinFlags.Float32ListVec3, new InibinSet(br, InibinFlags.Float32ListVec3));
                if (flags.HasFlag(InibinFlags.FixedPointFloatListVec2))
                    Sets.Add(InibinFlags.FixedPointFloatListVec2,
                        new InibinSet(br, InibinFlags.FixedPointFloatListVec2));
                if (flags.HasFlag(InibinFlags.Float32ListVec2))
                    Sets.Add(InibinFlags.Float32ListVec2, new InibinSet(br, InibinFlags.Float32ListVec2));
                if (flags.HasFlag(InibinFlags.FixedPointFloatListVec4))
                    Sets.Add(InibinFlags.FixedPointFloatListVec4,
                        new InibinSet(br, InibinFlags.FixedPointFloatListVec4));
                if (flags.HasFlag(InibinFlags.Float32ListVec4))
                    Sets.Add(InibinFlags.Float32ListVec4, new InibinSet(br, InibinFlags.Float32ListVec4));
                if (flags.HasFlag(InibinFlags.StringList))
                    Sets.Add(InibinFlags.StringList,
                        new InibinSet(br, InibinFlags.StringList, (uint) br.BaseStream.Length - stringDataLength));
            }
            else
            {
                throw new Exception("This version is not supported");
            }
        }
    }

    /// <summary>
    ///     Sections of this <see cref="InibinFile" /> separated by value type
    /// </summary>
    public Dictionary<InibinFlags, InibinSet> Sets { get; } = new();

    /// <summary>
    ///     Writes this <see cref="InibinFile" /> to the specified location
    /// </summary>
    /// <param name="fileLocation">The location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="InibinFile" /> to the specified <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            ushort stringDataLength = 0;
            InibinFlags flags = 0;

            if (Sets.ContainsKey(InibinFlags.BitList)) flags |= InibinFlags.BitList;
            if (Sets.ContainsKey(InibinFlags.FixedPointFloatList)) flags |= InibinFlags.FixedPointFloatList;
            if (Sets.ContainsKey(InibinFlags.FixedPointFloatListVec2)) flags |= InibinFlags.FixedPointFloatListVec2;
            if (Sets.ContainsKey(InibinFlags.FixedPointFloatListVec3)) flags |= InibinFlags.FixedPointFloatListVec3;
            if (Sets.ContainsKey(InibinFlags.FixedPointFloatListVec4)) flags |= InibinFlags.FixedPointFloatListVec4;
            if (Sets.ContainsKey(InibinFlags.Float32List)) flags |= InibinFlags.Float32List;
            if (Sets.ContainsKey(InibinFlags.Float32ListVec2)) flags |= InibinFlags.Float32ListVec2;
            if (Sets.ContainsKey(InibinFlags.Float32ListVec3)) flags |= InibinFlags.Float32ListVec3;
            if (Sets.ContainsKey(InibinFlags.Float32ListVec4)) flags |= InibinFlags.Float32ListVec4;
            if (Sets.ContainsKey(InibinFlags.Int16List)) flags |= InibinFlags.Int16List;
            if (Sets.ContainsKey(InibinFlags.Int32List)) flags |= InibinFlags.Int32List;
            if (Sets.ContainsKey(InibinFlags.Int8List)) flags |= InibinFlags.Int8List;
            if (Sets.ContainsKey(InibinFlags.StringList))
            {
                flags |= InibinFlags.StringList;

                foreach (string dataString in Sets[InibinFlags.StringList].Properties.Values)
                    stringDataLength += (ushort) (dataString.Length + 1);
            }


            bw.Write((byte) 2);
            bw.Write(stringDataLength);
            bw.Write((ushort) flags);

            foreach (var set in Sets) set.Value.Write(bw);
        }
    }

    /// <summary>
    ///     Adds a value to this <see cref="InibinFile" />
    /// </summary>
    /// <param name="section">Section of the value being added</param>
    /// <param name="property">Name of the value being added</param>
    /// <param name="value">Value to add</param>
    public void AddValue(string section, string property, object value)
    {
        AddValue(Cryptography.SectionHash(section, property), value);
    }

    /// <summary>
    ///     Adds a value to this <see cref="InibinFile" />
    /// </summary>
    /// <param name="hash">Hash of the value being added</param>
    /// <param name="value">Value to add</param>
    public void AddValue(uint hash, object value)
    {
        InibinFlags valueType;
        var valueObjectType = value.GetType();

        if (valueObjectType == typeof(int))
        {
            valueType = InibinFlags.Int32List;
        }
        else if (valueObjectType == typeof(float))
        {
            valueType = InibinFlags.Float32List;
        }
        else if (valueObjectType == typeof(double))
        {
            var doubleValue = (double) value;
            if (doubleValue <= 25.5 && doubleValue >= 0.0)
                valueType = InibinFlags.FixedPointFloatList;
            else
                throw new FixedPointFloatOverflowException();
        }
        else if (valueObjectType == typeof(short))
        {
            valueType = InibinFlags.Int16List;
        }
        else if (valueObjectType == typeof(byte))
        {
            valueType = InibinFlags.Int8List;
        }
        else if (valueObjectType == typeof(bool))
        {
            valueType = InibinFlags.BitList;
        }
        else if (valueObjectType == typeof(byte[]))
        {
            var vector = (byte[]) value;
            if (vector.Length == 2)
                valueType = InibinFlags.FixedPointFloatListVec2;
            else if (vector.Length == 3)
                valueType = InibinFlags.FixedPointFloatListVec3;
            else if (vector.Length == 4)
                valueType = InibinFlags.FixedPointFloatListVec4;
            else
                throw new VectorOverflowException();
        }
        else if (valueObjectType == typeof(float[]))
        {
            var vector = (float[]) value;
            if (vector.Length == 2)
                valueType = InibinFlags.Float32ListVec2;
            else if (vector.Length == 3)
                valueType = InibinFlags.Float32ListVec3;
            else if (vector.Length == 4)
                valueType = InibinFlags.Float32ListVec4;
            else
                throw new VectorOverflowException();
        }
        else
        {
            throw new UnsupportedValueTypeException("The type of: " + valueObjectType + "is not supported");
        }

        if (!Sets.ContainsKey(valueType)) Sets.Add(valueType, new InibinSet(valueType));

        try
        {
            Sets[valueType].Properties.Add(hash, value);
        }
        catch (ArgumentException)
        {
            throw new Exception("The property you are trying to add already exists");
        }
    }
}

/// <summary>
///     Represents an <see cref="Exception" /> that's thrown when the user is trying to add an unsupported value type to an
///     <see cref="InibinFile" />
/// </summary>
public class UnsupportedValueTypeException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="UnsupportedValueTypeException" /> with the specified error
    /// </summary>
    /// <param name="message">Message that describes the error</param>
    public UnsupportedValueTypeException(string message) : base(message)
    {
    }
}

/// <summary>
///     Represents an <see cref="Exception" /> that's thrown when a <see cref="InibinFlags.FixedPointFloatList" /> value is
///     overflowed
/// </summary>
public class FixedPointFloatOverflowException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="FixedPointFloatOverflowException" />
    /// </summary>
    public FixedPointFloatOverflowException() : base("The Fixed Point Float value has to be between 0.0 and 25.5")
    {
    }
}

/// <summary>
///     Represents an <see cref="Exception" /> that's thrown when a user tries to add a vector of unsupported size
/// </summary>
public class VectorOverflowException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="VectorOverflowException" />
    /// </summary>
    public VectorOverflowException() : base("A Vector can only have the size of 2, 3, and 4")
    {
    }
}

/// <summary>
///     Represents Value Types inside a <see cref="InibinFile" />
/// </summary>
[Flags]
public enum InibinFlags : ushort
{
    /// <summary>
    ///     Represents an <see cref="int" /> value
    /// </summary>
    Int32List = 1,

    /// <summary>
    ///     Represents a <see cref="float" /> value
    /// </summary>
    Float32List = 1 << 1,

    /// <summary>
    ///     Represents a <see cref="double" /> value with the range of 0.0 - 25.5
    /// </summary>
    FixedPointFloatList = 1 << 2,

    /// <summary>
    ///     Represents a <see cref="short" /> value
    /// </summary>
    Int16List = 1 << 3,

    /// <summary>
    ///     Represents a <see cref="byte" /> value
    /// </summary>
    Int8List = 1 << 4,

    /// <summary>
    ///     Represents a <see cref="bool" /> value
    /// </summary>
    BitList = 1 << 5,

    /// <summary>
    ///     Represents a <see cref="byte" /> Vector3 value
    /// </summary>
    FixedPointFloatListVec3 = 1 << 6,

    /// <summary>
    ///     Represents a <see cref="float" /> Vector3 value
    /// </summary>
    Float32ListVec3 = 1 << 7,

    /// <summary>
    ///     Represents a <see cref="byte" /> Vector2 value
    /// </summary>
    FixedPointFloatListVec2 = 1 << 8,

    /// <summary>
    ///     Represents a <see cref="float" /> Vector2 value
    /// </summary>
    Float32ListVec2 = 1 << 9,

    /// <summary>
    ///     Represents a <see cref="byte" /> Vector4 value
    /// </summary>
    FixedPointFloatListVec4 = 1 << 10,

    /// <summary>
    ///     Represents a <see cref="float" /> Vector4 value
    /// </summary>
    Float32ListVec4 = 1 << 11,

    /// <summary>
    ///     Represents a <see cref="string" /> value
    /// </summary>
    StringList = 1 << 12
}