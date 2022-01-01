using System;
using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.Helpers.Structures;

/// <summary>
///     Represents a Time Gradient
/// </summary>
public class TimeGradient
{
    /// <summary>
    ///     Initializes a new <see cref="TimeGradient" />
    /// </summary>
    /// <param name="type">Type of this <see cref="TimeGradient" /></param>
    /// <param name="values">The Values of this <see cref="TimeGradient" /></param>
    public TimeGradient(int type, TimeGradientValue[] values)
    {
        if (values.Length != 8) throw new ArgumentException("There must be 8 values");

        Type = type;
        Values = values;
    }

    /// <summary>
    ///     Creates a clone of a <see cref="TimeGradient" /> object
    /// </summary>
    /// <param name="timeGradient">The <see cref="TimeGradient" /> to clone</param>
    public TimeGradient(TimeGradient timeGradient)
    {
        Type = timeGradient.Type;
        for (var i = 0; i < timeGradient.Values.Length; i++) Values[i] = new TimeGradientValue(timeGradient.Values[i]);
    }

    /// <summary>
    ///     Initializes a new <see cref="TimeGradient" /> from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    public TimeGradient(BinaryReader br)
    {
        Type = br.ReadInt32();
        var usedValueCount = br.ReadUInt32();
        for (var i = 0; i < usedValueCount; i++) Values[i] = new TimeGradientValue(br);

        for (var i = 0; i < 8 - usedValueCount; i++)
        {
            var value = new TimeGradientValue(br);
        }
    }

    /// <summary>
    ///     Type of this <see cref="TimeGradient" />
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    ///     The Values of this <see cref="TimeGradient" />
    /// </summary>
    public TimeGradientValue[] Values { get; } = new TimeGradientValue[8];

    /// <summary>
    ///     Gets the <see cref="TimeGradientValue" /> count of this <see cref="TimeGradient" />
    /// </summary>
    /// <remarks>
    ///     This method should be used instead of assuming each gradient has 8 values, as most of the gradients won't have
    ///     all 8 values set
    /// </remarks>
    public uint GetValueCount()
    {
        uint count = 0;
        for (var i = 0; i < 8; i++)
            if (Values[i] != null)
                count++;

        return count;
    }

    /// <summary>
    ///     Gets a value from this <see cref="TimeGradient" /> at the specified time
    /// </summary>
    /// <param name="time">The time at which to get the value</param>
    public Vector4 GetValue(float time)
    {
        if (time > 0)
        {
            var valueCount = GetValueCount();

            if (time < 1)
            {
                var gradientValueIndex = 0;
                float accValue = 0;

                for (var i = 0;; i++)
                {
                    accValue = Values[gradientValueIndex].Value.Y;
                    if (i >= valueCount || Values[gradientValueIndex].Time >= time) break;

                    gradientValueIndex++;
                }

                var minTime = Values[gradientValueIndex - 1].Time;
                var maxTime = Values[gradientValueIndex].Time;
                var minX = Values[gradientValueIndex - 1].Value.X;
                var minY = Values[gradientValueIndex - 1].Value.Y;
                var minZ = Values[gradientValueIndex - 1].Value.Z;
                var minW = Values[gradientValueIndex - 1].Value.W;
                var maxX = Values[gradientValueIndex].Value.X;
                var maxY = Values[gradientValueIndex].Value.Y;
                var maxZ = Values[gradientValueIndex].Value.Z;
                var maxW = Values[gradientValueIndex].Value.W;

                var timeFraction = (time - minTime) / (maxTime - minTime);
                var x = (maxX - minX) * timeFraction + minX;
                var y = (maxY - minY) * timeFraction + minY;
                var z = (maxZ - minZ) * timeFraction + minZ;
                var w = (maxW - minW) * timeFraction + minW;

                return new Vector4(x, y, z, w);
            }

            return Values[valueCount].Value;
        }

        return Values[0].Value;
    }

    /// <summary>
    ///     Writes this <see cref="TimeGradient" /> into a <see cref="BinaryWriter" />
    /// </summary>
    /// <param name="bw">The <see cref="BinaryWriter" /> to write to</param>
    public void Write(BinaryWriter bw)
    {
        bw.Write(Type);

        uint usedValueCount = 0;
        for (var i = 0; i < 8; i++) usedValueCount += Values[i] == null ? (uint) 1 : 0;
        bw.Write(usedValueCount);

        foreach (var value in Values)
            if (value != null)
                value.Write(bw);
            else
                new TimeGradientValue(0, new Vector4(0, 0, 0, 0)).Write(bw);
    }
}

/// <summary>
///     Represents a value inside a <see cref="TimeGradient" />
/// </summary>
public class TimeGradientValue
{
    /// <summary>
    ///     Initializes a new <see cref="TimeGradientValue" />
    /// </summary>
    /// <param name="time">The time at which this value starts</param>
    /// <param name="value">The value of this <see cref="TimeGradientValue" /></param>
    public TimeGradientValue(float time, Vector4 value)
    {
        if (time > 1 || time < 0) throw new ArgumentException("Time must be normalized in a range from 0 - 1");

        Time = time;
        Value = value;
    }

    /// <summary>
    ///     Initializes a new <see cref="TimeGradientValue" /> from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    public TimeGradientValue(BinaryReader br)
    {
        Time = br.ReadSingle();
        Value = br.ReadVector4();
    }

    /// <summary>
    ///     Creates a clone of a <see cref="TimeGradientValue" /> object
    /// </summary>
    /// <param name="timeGradientValue">The <see cref="TimeGradientValue" /> to clone</param>
    public TimeGradientValue(TimeGradientValue timeGradientValue)
    {
        Time = timeGradientValue.Time;
        Value = timeGradientValue.Value;
    }

    /// <summary>
    ///     The time at which this value starts
    /// </summary>
    public float Time { get; set; }

    /// <summary>
    ///     The value of this <see cref="TimeGradientValue" />
    /// </summary>
    public Vector4 Value { get; set; }

    /// <summary>
    ///     Writes this <see cref="TimeGradientValue" /> into a <see cref="BinaryWriter" />
    /// </summary>
    /// <param name="bw">The <see cref="BinaryWriter" /> to write to</param>
    public void Write(BinaryWriter bw)
    {
        bw.Write(Time);
        bw.WriteVector4(Value);
    }
}