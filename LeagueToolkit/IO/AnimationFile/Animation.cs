using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;
using LeagueToolkit.IO.SkeletonFile;

namespace LeagueToolkit.IO.AnimationFile;

public class Animation
{
    private List<List<ushort>> _jumpCaches = new();

    public Animation(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public Animation(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            var version = br.ReadUInt32();

            if (magic == "r3d2canm")
            {
                ReadCompressed(br);
            }
            else if (magic == "r3d2anmd")
            {
                if (version == 5)
                    ReadV5(br);
                else if (version == 4)
                    ReadV4(br);
                else
                    ReadLegacy(br);
            }
            else
            {
                throw new InvalidFileSignatureException();
            }
        }
    }

    public float FrameDuration { get; private set; }
    public float FPS => 1 / FrameDuration;

    public List<AnimationTrack> Tracks { get; } = new();

    private void ReadCompressed(BinaryReader br)
    {
        var resourceSize = br.ReadUInt32();
        var formatToken = br.ReadUInt32();
        var flags = br.ReadUInt32(); // 7 ?

        var jointCount = br.ReadInt32();
        var frameCount = br.ReadInt32();
        var jumpCacheCount = br.ReadInt32();

        var duration = br.ReadSingle();
        FrameDuration = 1 / br.ReadSingle();

        TransformQuantizationProperties rotationQuantizationProperties = new(br);
        TransformQuantizationProperties translationQuantizationProperties = new(br);
        TransformQuantizationProperties scaleQuantizationProperties = new(br);

        var translationMin = br.ReadVector3();
        var translationMax = br.ReadVector3();

        var scaleMin = br.ReadVector3();
        var scaleMax = br.ReadVector3();

        var framesOffset = br.ReadInt32();
        var jumpCachesOffset = br.ReadInt32(); // 5328
        var jointNameHashesOffset = br.ReadInt32();

        if (framesOffset <= 0) throw new Exception("Animation does not contain Frame data");
        //if (jumpCachesOffset <= 0) throw new Exception("Animation does not contain jump cache data");
        if (jointNameHashesOffset <= 0) throw new Exception("Animation does not contain joint data");

        // Read joint hashes
        br.BaseStream.Seek(jointNameHashesOffset + 12, SeekOrigin.Begin);
        List<uint> jointHashes = new(jointCount);
        for (var i = 0; i < jointCount; i++) jointHashes.Add(br.ReadUInt32());

        // Read frames
        br.BaseStream.Seek(framesOffset + 12, SeekOrigin.Begin);
        List<KeyValuePair<uint, Dictionary<float, Vector3>>> translations = new(jointCount);
        List<KeyValuePair<uint, Dictionary<float, Vector3>>> scales = new(jointCount);
        List<KeyValuePair<uint, Dictionary<float, Quaternion>>> rotations = new(jointCount);

        for (var i = 0; i < jointCount; i++)
        {
            var jointHash = jointHashes[i];

            translations.Add(new(jointHash, new Dictionary<float, Vector3>()));
            scales.Add(new(jointHash, new Dictionary<float, Vector3>()));
            rotations.Add(new(jointHash, new Dictionary<float, Quaternion>()));

            Tracks.Add(new AnimationTrack(jointHash));
        }

        for (var i = 0; i < frameCount; i++)
        {
            var compressedTime = br.ReadUInt16();

            var bits = br.ReadUInt16(); // JointHashIndex | TransformType
            var jointHashIndex = (byte) (bits & 0x3FFF);
            var transformType = (CompressedTransformType) (bits >> 14);

            var compressedTransform = br.ReadBytes(6);

            var jointHash = jointHashes[jointHashIndex];
            var uncompressedTime = UncompressFrameTime(compressedTime, duration);

            if (transformType == CompressedTransformType.Rotation)
            {
                var rotation = new QuantizedQuaternion(compressedTransform).Decompress();
                
                if (!rotations[jointHashIndex].Value.ContainsKey(uncompressedTime))
                    rotations[jointHashIndex].Value.Add(uncompressedTime, rotation);
            }
            else if (transformType == CompressedTransformType.Translation)
            {
                var translation = UncompressVector3(translationMin, translationMax, compressedTransform);
                
                if (!translations[jointHashIndex].Value.ContainsKey(uncompressedTime))
                    translations[jointHashIndex].Value.Add(uncompressedTime, translation);
            }
            else if (transformType == CompressedTransformType.Scale)
            {
                var scale = UncompressVector3(scaleMin, scaleMax, compressedTransform);
                
                if (!scales[jointHashIndex].Value.ContainsKey(uncompressedTime))
                    scales[jointHashIndex].Value.Add(uncompressedTime, scale);
            }
            else
            {
                throw new Exception("Encountered unknown transform type: " + transformType);
            }
        }

        // Build quantized tracks
        for (var i = 0; i < jointCount; i++)
        {
            var jointHash = jointHashes[i];
            var track = Tracks.First(x => x.JointHash == jointHash);

            track.Translations = translations[i].Value;
            track.Scales = scales[i].Value;
            track.Rotations = rotations[i].Value;
        }

        // Read jump caches
        //br.BaseStream.Seek(jumpCachesOffset + 12, SeekOrigin.Begin);
        //for(int i = 0; i < jumpCacheCount; i++)
        //{
        //    int count = 1332;
        //
        //    this._jumpCaches.Add(new List<ushort>());
        //    for(int j = 0; j < count; j++)
        //    {
        //        this._jumpCaches[i].Add(br.ReadUInt16());
        //    }
        //}

        DequantizeAnimationChannels(rotationQuantizationProperties, translationQuantizationProperties,
            scaleQuantizationProperties);
    }

    private void ReadV4(BinaryReader br)
    {
        var resourceSize = br.ReadUInt32();
        var formatToken = br.ReadUInt32();
        var version = br.ReadUInt32();
        var flags = br.ReadUInt32();

        var trackCount = br.ReadInt32();
        var frameCount = br.ReadInt32();
        FrameDuration = br.ReadSingle();

        var tracksOffset = br.ReadInt32();
        var assetNameOffset = br.ReadInt32();
        var timeOffset = br.ReadInt32();
        var vectorsOffset = br.ReadInt32();
        var rotationsOffset = br.ReadInt32();
        var framesOffset = br.ReadInt32();

        if (vectorsOffset <= 0) throw new Exception("Animation does not contain Vector data");
        if (rotationsOffset <= 0) throw new Exception("Animation does not contain Rotation data");
        if (framesOffset <= 0) throw new Exception("Animation does not contain Frame data");

        var vectorsCount = (rotationsOffset - vectorsOffset) / 12;
        var rotationsCount = (framesOffset - rotationsOffset) / 16;

        br.BaseStream.Seek(vectorsOffset + 12, SeekOrigin.Begin);
        List<Vector3> vectors = new();
        for (var i = 0; i < vectorsCount; i++) vectors.Add(br.ReadVector3());

        br.BaseStream.Seek(rotationsOffset + 12, SeekOrigin.Begin);
        List<Quaternion> rotations = new();
        for (var i = 0; i < rotationsCount; i++) rotations.Add(br.ReadQuaternion());

        br.BaseStream.Seek(framesOffset + 12, SeekOrigin.Begin);
        List<(uint, ushort, ushort, ushort)> frames = new(frameCount * trackCount);
        for (var i = 0; i < frameCount * trackCount; i++)
        {
            frames.Add((br.ReadUInt32(), br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16()));
            br.ReadUInt16(); // padding
        }

        foreach (var (jointHash, translationIndex, scaleIndex, rotationIndex) in frames)
        {
            if (!Tracks.Any(x => x.JointHash == jointHash)) Tracks.Add(new AnimationTrack(jointHash));

            var track = Tracks.First(x => x.JointHash == jointHash);

            var trackFrameTranslationIndex = track.Translations.Count;
            var trackFrameScaleIndex = track.Scales.Count;
            var trackFrameRotationIndex = track.Rotations.Count;

            var translation = vectors[translationIndex];
            var scale = vectors[scaleIndex];
            var rotation = rotations[rotationIndex];
            
            track.Translations.Add(FrameDuration * trackFrameTranslationIndex, translation);
            track.Scales.Add(FrameDuration * trackFrameScaleIndex, scale);
            track.Rotations.Add(FrameDuration * trackFrameRotationIndex, rotation);
        }
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

        var jointHashesOffset = br.ReadInt32();
        var assetNameOffset = br.ReadInt32();
        var timeOffset = br.ReadInt32();
        var vectorsOffset = br.ReadInt32();
        var rotationsOffset = br.ReadInt32();
        var framesOffset = br.ReadInt32();

        if (jointHashesOffset <= 0) throw new Exception("Animation does not contain Joint hashes");
        if (vectorsOffset <= 0) throw new Exception("Animation does not contain Vector data");
        if (rotationsOffset <= 0) throw new Exception("Animation does not contain Rotation data");
        if (framesOffset <= 0) throw new Exception("Animation does not contain Frame data");

        var jointHashesCount = (framesOffset - jointHashesOffset) / sizeof(uint);
        var vectorsCount = (rotationsOffset - vectorsOffset) / 12;
        var rotationsCount = (jointHashesOffset - rotationsOffset) / 6;

        List<uint> jointHashes = new(jointHashesCount);
        List<Vector3> vectors = new(vectorsCount);
        List<Quaternion> rotations = new(rotationsCount);
        var frames = new List<(ushort, ushort, ushort)>(framesPerTrack * trackCount);

        // Read Joint Hashes
        br.BaseStream.Seek(jointHashesOffset + 12, SeekOrigin.Begin);
        for (var i = 0; i < jointHashesCount; i++) jointHashes.Add(br.ReadUInt32());

        // Read Vectors
        br.BaseStream.Seek(vectorsOffset + 12, SeekOrigin.Begin);
        for (var i = 0; i < vectorsCount; i++) vectors.Add(br.ReadVector3());

        // Read Rotations
        br.BaseStream.Seek(rotationsOffset + 12, SeekOrigin.Begin);
        for (var i = 0; i < rotationsCount; i++)
        {
            var rotation = new QuantizedQuaternion(br.ReadBytes(6)).Decompress();

            rotations.Add(Quaternion.Normalize(rotation));
        }

        // Read Frames
        br.BaseStream.Seek(framesOffset + 12, SeekOrigin.Begin);
        for (var i = 0; i < framesPerTrack * trackCount; i++)
            frames.Add((br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16()));

        // Create tracks
        for (var i = 0; i < trackCount; i++) Tracks.Add(new AnimationTrack(jointHashes[i]));

        for (var t = 0; t < trackCount; t++)
        {
            var track = Tracks[t];
            float currentTime = 0;
            for (var f = 0; f < framesPerTrack; f++)
            {
                (int translationIndex, int scaleIndex, int rotationIndex) = frames[f * trackCount + t];

                track.Translations.Add(currentTime, vectors[translationIndex]);
                track.Scales.Add(currentTime, vectors[scaleIndex]);
                track.Rotations.Add(currentTime, rotations[rotationIndex]);

                currentTime += FrameDuration;
            }
        }
    }

    private void ReadLegacy(BinaryReader br)
    {
        var skeletonId = br.ReadUInt32();

        var trackCount = br.ReadInt32();
        var frameCount = br.ReadInt32();

        FrameDuration = 1.0f / br.ReadInt32(); // FPS

        for (var i = 0; i < trackCount; i++)
        {
            var trackName = br.ReadPaddedString(32);
            var flags = br.ReadUInt32();

            var track = new AnimationTrack(Cryptography.ElfHash(trackName));

            var frameTime = 0f;
            for (var j = 0; j < frameCount; j++)
            {
                track.Rotations.Add(frameTime, br.ReadQuaternion());
                track.Translations.Add(frameTime, br.ReadVector3());
                track.Scales.Add(frameTime, new Vector3(1, 1, 1));

                frameTime += FrameDuration;
            }

            Tracks.Add(track);
        }
    }

    private Vector3 UncompressVector3(Vector3 min, Vector3 max, byte[] compressedData)
    {
        var uncompressed = max - min;
        var cX = (ushort) (compressedData[0] | (compressedData[1] << 8));
        var cY = (ushort) (compressedData[2] | (compressedData[3] << 8));
        var cZ = (ushort) (compressedData[4] | (compressedData[5] << 8));

        uncompressed.X *= cX / 65535.0f;
        uncompressed.Y *= cY / 65535.0f;
        uncompressed.Z *= cZ / 65535.0f;

        uncompressed += min;

        return uncompressed;
    }

    private float UncompressFrameTime(ushort compressedTime, float animationLength)
    {
        return compressedTime / 65535.0f * animationLength;
    }

    // ------------ DEQUANTIZATION ------------ \\
    private void DequantizeAnimationChannels(
        TransformQuantizationProperties rotationQuantizationProperties,
        TransformQuantizationProperties translationQuantizationProperties,
        TransformQuantizationProperties scaleQuantizationProperties)
    {
        // TODO

        //TranslationDequantizationRound(translationQuantizationProperties);
    }

    private void TranslationDequantizationRound(TransformQuantizationProperties quantizationProperties)
    {
        foreach (var track in Tracks)
        {
            List<(float, Vector3)> interpolatedFrames = new();
            for (var i = 0; i < track.Translations.Count; i++)
            {
                // Do not process last frame
                if (i + 1 >= track.Translations.Count) return;

                var frameA = track.Translations.ElementAt(i + 0);
                var frameB = track.Translations.ElementAt(i + 1);

                // Check if interpolation is needed

                interpolatedFrames.Add(InterpolateTranslationFrames((frameA.Key, frameA.Value),
                    (frameB.Key, frameB.Value)));
            }

            foreach (var (time, value) in interpolatedFrames)
                track.Translations.Add(time, value);
        }
    }

    private (float, Vector3) InterpolateTranslationFrames((float, Vector3) a, (float, Vector3) b)
    {
        var time = (a.Item1 + b.Item1) / 2;
        return (time, Vector3.Lerp(a.Item2, b.Item2, 0.5f));
    }

    public bool IsCompatibleWithSkeleton(Skeleton skeleton)
    {
        foreach (var track in Tracks)
            if (!skeleton.Joints.Any(x => Cryptography.ElfHash(x.Name) == track.JointHash))
                return false;

        return true;
    }

    private struct TransformQuantizationProperties
    {
        internal float ErrorMargin { get; }
        internal float DiscontinuityThreshold { get; }

        internal TransformQuantizationProperties(BinaryReader br)
        {
            ErrorMargin = br.ReadSingle();
            DiscontinuityThreshold = br.ReadSingle();
        }
    }

    private enum CompressedTransformType : byte
    {
        Rotation = 0,
        Translation = 1,
        Scale = 2
    }
}