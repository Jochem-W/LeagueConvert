using System.Diagnostics;
using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.AnimationFile;

public class Animation
{
    private float _fps;
    private float _frameDuration;

    public Animation(string filePath) : this(File.OpenRead(filePath))
    {
    }

    public Animation(Stream stream, bool leaveOpen = false)
    {
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen);
        var magic = new string(reader.ReadChars(8));
        var version = reader.ReadUInt32();
        switch (magic)
        {
            case "r3d2canm":
                ReadCompressed(reader);
                break;
            case "r3d2anmd" when version == 5:
                ReadV5(reader);
                break;
            case "r3d2anmd" when version == 4:
                ReadV4(reader);
                break;
            case "r3d2anmd":
                ReadLegacy(reader);
                break;
            default:
                throw new InvalidFileSignatureException($"The animation format '{magic}' isn't supported.");
        }
    }

    public float FrameDuration
    {
        get => _frameDuration;
        private set
        {
            _frameDuration = value;
            _fps = 1 / value;
        }
    }

    public float Fps
    {
        get => _fps;
        private set
        {
            _fps = value;
            _frameDuration = 1 / value;
        }
    }

    public float Duration { get; private set; }
    public IList<AnimationTrack> Tracks { get; } = new List<AnimationTrack>();

    private void ReadCompressed(BinaryReader br)
    {
        var resourceSize = br.ReadUInt32();
        var formatToken = new string(br.ReadChars(4));
        var flags = br.ReadUInt32(); // ?

        var jointCount = br.ReadInt32();
        var frameCount = br.ReadInt32();
        var jumpCacheCount = br.ReadInt32();

        Duration = br.ReadSingle();
        Fps = br.ReadSingle();

        // TODO: quantisation
        // TransformQuantizationProperties rotationQuantizationProperties = new(br);
        // TransformQuantizationProperties translationQuantizationProperties = new(br);
        // TransformQuantizationProperties scaleQuantizationProperties = new(br);
        br.BaseStream.Seek(6 * sizeof(float), SeekOrigin.Current);

        var minTranslation = br.ReadVector3();
        var maxTranslation = br.ReadVector3();
        var minScale = br.ReadVector3();
        var maxScale = br.ReadVector3();

        var frameOffset = br.ReadInt32();
        var jumpCacheOffset = br.ReadInt32();
        var jointNameHashesOffset = br.ReadInt32();

        if (frameOffset <= 0)
        {
            throw new InvalidDataException("The animation has no frames.");
        }

        if (jointNameHashesOffset <= 0)
        {
            throw new InvalidDataException("The animation has no joint name hashes.");
        }

        // Joint name hashes
        br.BaseStream.Seek(jointNameHashesOffset + 12, SeekOrigin.Begin);
        var jointNameHashes = new uint[jointCount];
        for (var i = 0; i < jointCount; i++)
        {
            jointNameHashes[i] = br.ReadUInt32();
            Tracks.Add(new AnimationTrack(jointNameHashes[i]));
        }

        // Frames
        br.BaseStream.Seek(frameOffset + 12, SeekOrigin.Begin);
        for (var i = 0; i < frameCount; i++)
        {
            var compressedTime = br.ReadUInt16();

            var bits = br.ReadUInt16();
            var jointIndex = (ushort)(bits & 0b_00111111_11111111);
            var transformType = (CompressedTransformType)(bits >> 14);

            var compressedTransform = br.ReadBytes(6);

            var time = DecompressTime(compressedTime, Duration);
            switch (transformType)
            {
                case CompressedTransformType.Rotation:
                    var rotation = new QuantizedQuaternion(compressedTransform);
                    Tracks[jointIndex].Rotations[time] = Quaternion.Normalize(rotation.Decompress());
                    break;
                case CompressedTransformType.Translation:
                    var translation = DecompressVector3(minTranslation, maxTranslation, compressedTransform);
                    Tracks[jointIndex].Translations[time] = translation;
                    break;
                case CompressedTransformType.Scale:
                    var scale = DecompressVector3(minScale, maxScale, compressedTransform);
                    Tracks[jointIndex].Scales[time] = scale;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // TODO: jump cache
        // reader.BaseStream.Seek(jumpCacheOffset + 12, SeekOrigin.Begin);
        // var ushortCount = (jointNameHashesOffset - jumpCacheOffset) / sizeof(ushort);
        // var data = new ushort[ushortCount];
        // for (var i = 0; i < ushortCount; i++) data[i] = reader.ReadUInt16();
        // var jumpCaches = data.Chunk(ushortCount / jumpCacheCount).ToList();

        Debug.Assert(formatToken == "canm");
    }

    private void ReadV4(BinaryReader br)
    {
        var resourceSize = br.ReadUInt32();
        var formatToken = br.ReadUInt32();
        var version = br.ReadUInt32();
        var flags = br.ReadUInt32();

        var trackCount = br.ReadInt32();
        var framesPerTrack = br.ReadInt32();
        FrameDuration = br.ReadSingle();

        var tracksOffset = br.ReadInt32();
        var assetNameOffset = br.ReadInt32();
        var timeOffset = br.ReadInt32();
        var vectorsOffset = br.ReadInt32();
        var rotationsOffset = br.ReadInt32();
        var framesOffset = br.ReadInt32();

        if (vectorsOffset <= 0)
        {
            throw new InvalidDataException("The animation has no vectors.");
        }

        if (rotationsOffset <= 0)
        {
            throw new InvalidDataException("The animation has no rotations.");
        }

        if (framesOffset <= 0)
        {
            throw new InvalidDataException("The animation has no frames.");
        }

        // Vectors
        br.BaseStream.Seek(vectorsOffset + 12, SeekOrigin.Begin);
        var vectorsCount = (rotationsOffset - vectorsOffset) / (sizeof(float) * 3);
        var vectors = new Vector3[vectorsCount];
        for (var i = 0; i < vectorsCount; i++)
        {
            vectors[i] = br.ReadVector3();
        }

        // Rotations
        br.BaseStream.Seek(rotationsOffset + 12, SeekOrigin.Begin);
        var rotationsCount = (framesOffset - rotationsOffset) / (sizeof(float) * 4);
        var rotations = new Quaternion[rotationsCount];
        for (var i = 0; i < rotationsCount; i++)
        {
            var rotation = br.ReadQuaternion();
            rotations[i] = Quaternion.Normalize(rotation);
        }

        // Frames
        br.BaseStream.Seek(framesOffset + 12, SeekOrigin.Begin);
        var time = 0f;
        for (var i = 0; i < framesPerTrack; i++)
        {
            for (var j = 0; j < trackCount; j++)
            {
                var jointHash = br.ReadUInt32();
                if (i == 0)
                {
                    Tracks.Add(new AnimationTrack(jointHash));
                }

                Tracks[j].Translations.Add(time, vectors[br.ReadUInt16()]);
                Tracks[j].Scales.Add(time, vectors[br.ReadUInt16()]);
                Tracks[j].Rotations.Add(time, rotations[br.ReadUInt16()]);

                var unknown = br.ReadUInt16(); // Might not be padding: not always 0.
            }

            time += FrameDuration;
        }

        Debug.Assert(formatToken is 3188167891 or 0);
        Debug.Assert(version == 0);
        Debug.Assert((formatToken == 3188167891 && tracksOffset == -1 && assetNameOffset == -1 && timeOffset == -1) ||
                     (formatToken == 0 && tracksOffset == 0 && assetNameOffset == 0 && timeOffset == 0));
    }

    private void ReadV5(BinaryReader br)
    {
        var resourceSize = br.ReadUInt32();
        var formatToken = br.ReadUInt32();
        var version = br.ReadUInt32();
        var flags = br.ReadUInt32();

        var trackCount = br.ReadInt32();
        var framesPerTrack = br.ReadInt32();

        FrameDuration = br.ReadSingle();

        var jointNameHashesOffset = br.ReadInt32();
        var assetNameOffset = br.ReadInt32();
        var timeOffset = br.ReadInt32();
        var vectorsOffset = br.ReadInt32();
        var rotationsOffset = br.ReadInt32();
        var framesOffset = br.ReadInt32();
        var reservedOffset1 = br.ReadInt32();
        var reservedOffset2 = br.ReadInt32();
        var reservedOffset3 = br.ReadInt32();

        if (jointNameHashesOffset <= 0)
        {
            throw new InvalidDataException("The animation has no joint name hashes.");
        }

        if (vectorsOffset <= 0)
        {
            throw new InvalidDataException("The animation has no vectors.");
        }

        if (rotationsOffset <= 0)
        {
            throw new InvalidDataException("The animation has no rotations.");
        }

        if (framesOffset <= 0)
        {
            throw new InvalidDataException("The animation has no frames.");
        }

        // Joint name hashes
        br.BaseStream.Seek(jointNameHashesOffset + 12, SeekOrigin.Begin);
        var jointNameHashes = new uint[trackCount];
        for (var i = 0; i < trackCount; i++)
        {
            jointNameHashes[i] = br.ReadUInt32();
            Tracks.Add(new AnimationTrack(jointNameHashes[i]));
        }

        // Vectors
        br.BaseStream.Seek(vectorsOffset + 12, SeekOrigin.Begin);
        var vectorsCount = (rotationsOffset - vectorsOffset) / (sizeof(float) * 3);
        var vectors = new Vector3[vectorsCount];
        for (var i = 0; i < vectorsCount; i++)
        {
            vectors[i] = br.ReadVector3();
        }

        // Rotations
        br.BaseStream.Seek(rotationsOffset + 12, SeekOrigin.Begin);
        var rotationsCount = (jointNameHashesOffset - rotationsOffset) / 6;
        var rotations = new Quaternion[rotationsCount];
        for (var i = 0; i < rotationsCount; i++)
        {
            var rotation = new QuantizedQuaternion(br.ReadBytes(6));
            rotations[i] = Quaternion.Normalize(rotation.Decompress());
        }

        // Frames
        br.BaseStream.Seek(framesOffset + 12, SeekOrigin.Begin);
        var time = 0f;
        for (var i = 0; i < framesPerTrack; i++)
        {
            for (var j = 0; j < trackCount; j++)
            {
                Tracks[j].Translations.Add(time, vectors[br.ReadUInt16()]);
                var scale = vectors[br.ReadUInt16()];
                if (float.IsNaN(scale.X) || float.IsNaN(scale.Y) || float.IsNaN(scale.Z))
                {
                    scale = Vector3.One;
                }

                Tracks[j].Scales.Add(time, scale);
                Tracks[j].Rotations.Add(time, rotations[br.ReadUInt16()]);
            }

            time += FrameDuration;
        }

        Debug.Assert(formatToken == 0);
        Debug.Assert(version == 0);
        Debug.Assert(assetNameOffset == 0 && timeOffset == 0);
        Debug.Assert((framesOffset - jointNameHashesOffset) / sizeof(uint) == trackCount);
        Debug.Assert(reservedOffset1 == 0 && reservedOffset2 == 0 && reservedOffset3 == 0);
    }

    private void ReadLegacy(BinaryReader br)
    {
        var skeletonId = br.ReadUInt32();

        var trackCount = br.ReadInt32();
        var frameCount = br.ReadInt32();

        Fps = br.ReadInt32();

        for (var i = 0; i < trackCount; i++)
        {
            var trackName = br.ReadPaddedString(32);
            var flags = br.ReadUInt32();

            Tracks.Add(new AnimationTrack(Cryptography.ElfHash(trackName)));
            var time = 0f;
            for (var j = 0; j < frameCount; j++)
            {
                var rotation = br.ReadQuaternion();
                var translation = br.ReadVector3();

                Tracks[i].Rotations.Add(time, Quaternion.Normalize(rotation));
                Tracks[i].Translations.Add(time, translation);

                time += FrameDuration;
            }
        }
    }

    private static Vector3 DecompressVector3(Vector3 min, Vector3 max, IReadOnlyList<byte> compressedData)
    {
        var uncompressed = max - min;
        var cX = (ushort)(compressedData[0] | (compressedData[1] << 8));
        var cY = (ushort)(compressedData[2] | (compressedData[3] << 8));
        var cZ = (ushort)(compressedData[4] | (compressedData[5] << 8));

        uncompressed.X *= cX / (float)ushort.MaxValue;
        uncompressed.Y *= cY / (float)ushort.MaxValue;
        uncompressed.Z *= cZ / (float)ushort.MaxValue;

        uncompressed += min;
        return uncompressed;
    }

    private static float DecompressTime(ushort compressedTime, float animationLength)
    {
        return compressedTime / (float)ushort.MaxValue * animationLength;
    }
}