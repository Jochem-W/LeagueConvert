using System.Text;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.LightGrid;

public class LightGridFile
{
    public LightGridFile(string fileLocation)
        : this(File.OpenRead(fileLocation))
    {
    }

    public LightGridFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var version = br.ReadUInt32();
            if (version != 3)
            {
                throw new InvalidFileSignatureException();
            }

            var gridOffset = br.ReadUInt32();
            Width = br.ReadUInt32();
            Heigth = br.ReadUInt32();
            XBound = br.ReadSingle();
            YBound = br.ReadSingle();
            Sun = new LightGridSun(br);

            br.BaseStream.Seek(gridOffset, SeekOrigin.Begin);
            var cellCount = Width * Heigth;
            for (var i = 0; i < cellCount; i++)
            {
                Lights.Add(new[]
                {
                    br.ReadColor(ColorFormat.RgbaU8), br.ReadColor(ColorFormat.RgbaU8),
                    br.ReadColor(ColorFormat.RgbaU8),
                    br.ReadColor(ColorFormat.RgbaU8), br.ReadColor(ColorFormat.RgbaU8), br.ReadColor(ColorFormat.RgbaU8)
                });
            }
        }
    }

    public uint Width { get; }
    public uint Heigth { get; }
    public float XBound { get; }
    public float YBound { get; }
    public LightGridSun Sun { get; }
    public List<Color[]> Lights { get; } = new();

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            bw.Write((uint)3);
            bw.Write((uint)76);
            bw.Write(Width);
            bw.Write(Heigth);
            bw.Write(XBound);
            bw.Write(YBound);
            Sun.Write(bw);

            foreach (var cell in Lights)
            {
                for (var i = 0; i < 6; i++)
                {
                    bw.WriteColor(cell[i], ColorFormat.RgbaU8);
                }
            }
        }
    }

    public void WriteTexture(string fileLocation)
    {
        using (var bw = new BinaryWriter(File.OpenWrite(fileLocation)))
        {
            bw.Write((byte)0); //ID Length
            bw.Write((byte)0); //ColorMap Type
            bw.Write((byte)2); //DataType Code
            bw.Write((uint)0); //ColorMap Origin and Length
            bw.Write((byte)0); //ColorMap Depth
            bw.Write((uint)0); //X and Y Origin
            bw.Write((ushort)Width);
            bw.Write((ushort)Heigth);
            bw.Write((byte)32); //Bits Per Color
            bw.Write((byte)0); //Image Descriptor

            foreach (var cell in Lights)
            {
                for (var i = 0; i < 6; i++)
                {
                    bw.WriteColor(cell[i], ColorFormat.RgbaU8);
                }
            }
        }
    }
}