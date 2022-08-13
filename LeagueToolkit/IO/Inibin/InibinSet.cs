using System.Text;

namespace LeagueToolkit.IO.Inibin;

/// <summary>
///     Represents a set of values inside of a <see cref="InibinFile" />
/// </summary>
public class InibinSet
{
    /// <summary>
    ///     Initializes a blank <see cref="InibinSet" />
    /// </summary>
    public InibinSet()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="InibinSet" /> with the specified type
    /// </summary>
    /// <param name="type">Type of this <see cref="InibinSet" /></param>
    public InibinSet(InibinFlags type)
    {
        Type = type;
    }

    /// <summary>
    ///     Initializes a new <see cref="InibinSet" />
    /// </summary>
    /// <param name="type">Type of this <see cref="InibinSet" /></param>
    /// <param name="properties">Values of this <see cref="InibinSet" /></param>
    public InibinSet(InibinFlags type, Dictionary<uint, object> properties)
    {
        Type = type;
        Properties = properties;
    }

    /// <summary>
    ///     Initializes a new <see cref="InibinSet" /> from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    /// <param name="type">The type of this <see cref="InibinSet" /></param>
    public InibinSet(BinaryReader br, InibinFlags type)
    {
        Type = type;
        var valueCount = br.ReadUInt16();
        var hashes = new List<uint>();

        for (var i = 0; i < valueCount; i++)
        {
            var hash = br.ReadUInt32();
            Properties.Add(hash, null);
            hashes.Add(hash);
        }

        byte boolean = 0;
        for (var i = 0; i < valueCount; i++)
        {
            if (Type == InibinFlags.Int32List)
            {
                Properties[hashes[i]] = br.ReadInt32();
            }
            else if (Type == InibinFlags.Float32List)
            {
                Properties[hashes[i]] = br.ReadSingle();
            }
            else if (Type == InibinFlags.FixedPointFloatList)
            {
                Properties[hashes[i]] = br.ReadByte() * 0.1;
            }
            else if (Type == InibinFlags.Int16List)
            {
                Properties[hashes[i]] = br.ReadInt16();
            }
            else if (Type == InibinFlags.Int8List)
            {
                Properties[hashes[i]] = br.ReadByte();
            }
            else if (Type == InibinFlags.BitList)
            {
                boolean = i % 8 == 0 ? br.ReadByte() : (byte)(boolean >> 1);
                Properties[hashes[i]] = Convert.ToBoolean(0x1 & boolean);
            }
            else if (Type == InibinFlags.FixedPointFloatListVec3)
            {
                Properties[hashes[i]] = new[] { br.ReadByte(), br.ReadByte(), br.ReadByte() };
            }
            else if (Type == InibinFlags.Float32ListVec3)
            {
                Properties[hashes[i]] = new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
            }
            else if (Type == InibinFlags.FixedPointFloatListVec2)
            {
                Properties[hashes[i]] = new[] { br.ReadByte(), br.ReadByte() };
            }
            else if (Type == InibinFlags.Float32ListVec2)
            {
                Properties[hashes[i]] = new[] { br.ReadSingle(), br.ReadSingle() };
            }
            else if (Type == InibinFlags.FixedPointFloatListVec4)
            {
                Properties[hashes[i]] = new[] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
            }
            else if (Type == InibinFlags.Float32ListVec4)
            {
                Properties[hashes[i]] = new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
            }
        }
    }

    /// <summary>
    ///     Initializes a new legacy <see cref="InibinSet" /> from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    /// <param name="type">The type of this <see cref="InibinSet" /></param>
    /// <param name="stringOffset">Offset to the string data</param>
    /// <param name="valueCount">Amount of values in this <see cref="InibinSet" /></param>
    public InibinSet(BinaryReader br, InibinFlags type, uint stringOffset, uint? valueCount = null)
    {
        Type = type;
        var hashes = new List<uint>();
        if (valueCount == null)
        {
            valueCount = br.ReadUInt16();

            for (var i = 0; i < valueCount; i++)
            {
                var hash = br.ReadUInt32();
                Properties.Add(hash, null);
                hashes.Add(hash);
            }
        }

        for (var i = 0; i < valueCount; i++)
        {
            uint offset = br.ReadUInt16();
            var oldPos = br.BaseStream.Position;
            br.BaseStream.Seek(offset + stringOffset, SeekOrigin.Begin);

            var s = "";
            char c;
            while ((c = br.ReadChar()) != '\u0000')
            {
                s += c;
            }

            Properties[hashes[i]] = s;
            br.BaseStream.Seek(oldPos, SeekOrigin.Begin);
        }
    }

    /// <summary>
    ///     Type of the values of this <see cref="InibinSet" />
    /// </summary>
    public InibinFlags Type { get; }

    /// <summary>
    ///     Values of this <see cref="InibinSet" />
    /// </summary>
    public Dictionary<uint, object> Properties { get; } = new();

    /// <summary>
    ///     Writes this <see cref="InibinSet" /> into a <see cref="BinaryWriter" />
    /// </summary>
    /// <param name="bw">The <see cref="BinaryWriter" /> to write to</param>
    public void Write(BinaryWriter bw)
    {
        bw.Write((ushort)Properties.Count);

        foreach (var hash in Properties.Keys)
        {
            bw.Write(hash);
        }

        if (Type == InibinFlags.BitList)
        {
            var booleans = Properties.Values.Cast<bool>().ToList();
            byte value = 0;

            while (booleans.Count % 8 != 0)
            {
                booleans.Add(false);
            }

            for (int i = 0, j = 0; i < Properties.Count; i++)
            {
                if (booleans[i])
                {
                    value |= (byte)(1 << j);
                }

                j++;

                if (j == 8 || i == Properties.Count - 1)
                {
                    bw.Write(value);
                    j = value = 0;
                }
            }
        }
        else if (Type == InibinFlags.StringList)
        {
            ushort offset = 0;
            foreach (string value in Properties.Values)
            {
                bw.Write(offset);
                offset += (ushort)(value.Length + 1);
            }

            foreach (string value in Properties.Values)
            {
                bw.Write(Encoding.ASCII.GetBytes(value));
                bw.Write((byte)0);
            }
        }
        else
        {
            foreach (var value in Properties.Values)
            {
                if (Type == InibinFlags.Int32List)
                {
                    bw.Write((int)value);
                }
                else if (Type == InibinFlags.Float32List)
                {
                    bw.Write((float)value);
                }
                else if (Type == InibinFlags.FixedPointFloatList)
                {
                    bw.Write(Convert.ToByte((double)value * 10));
                }
                else if (Type == InibinFlags.Int16List)
                {
                    bw.Write((short)value);
                }
                else if (Type == InibinFlags.Int8List)
                {
                    bw.Write((byte)value);
                }
                else if (Type == InibinFlags.FixedPointFloatListVec3)
                {
                    var valueList = (byte[])value;

                    bw.Write(valueList[0]);
                    bw.Write(valueList[1]);
                    bw.Write(valueList[2]);
                }
                else if (Type == InibinFlags.Float32ListVec3)
                {
                    var valueList = (float[])value;

                    bw.Write(valueList[0]);
                    bw.Write(valueList[1]);
                    bw.Write(valueList[2]);
                }
                else if (Type == InibinFlags.FixedPointFloatListVec2)
                {
                    var valueList = (byte[])value;

                    bw.Write(valueList[0]);
                    bw.Write(valueList[1]);
                }
                else if (Type == InibinFlags.Float32ListVec2)
                {
                    var valueList = (float[])value;

                    bw.Write(valueList[0]);
                    bw.Write(valueList[1]);
                }
                else if (Type == InibinFlags.FixedPointFloatListVec4)
                {
                    var valueList = (byte[])value;

                    bw.Write(valueList[0]);
                    bw.Write(valueList[1]);
                    bw.Write(valueList[2]);
                    bw.Write(valueList[3]);
                }
                else if (Type == InibinFlags.Float32ListVec4)
                {
                    var valueList = (float[])value;

                    bw.Write(valueList[0]);
                    bw.Write(valueList[1]);
                    bw.Write(valueList[2]);
                    bw.Write(valueList[3]);
                }
            }
        }
    }
}