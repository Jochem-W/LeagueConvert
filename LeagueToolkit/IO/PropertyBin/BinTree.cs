using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.PropertyBin;

public class BinTree
{
    private readonly List<BinTreeObject> _objects = new();

    public BinTree()
    {
        Objects = _objects.AsReadOnly();
    }

    public BinTree(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public BinTree(Stream stream) : this()
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (magic != "PROP" && magic != "PTCH") throw new InvalidFileSignatureException();

            if (magic == "PTCH")
            {
                IsOverride = true;

                var unknown = br.ReadUInt64();
                magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                if (magic != "PROP")
                    throw new InvalidFileSignatureException("Expected PROP section after PTCH, got: " + magic);
            }

            var version = br.ReadUInt32();
            if (version != 1 && version != 2 && version != 3) throw new UnsupportedFileVersionException();

            if (version >= 2)
            {
                var dependencyCount = br.ReadUInt32();
                for (var i = 0; i < dependencyCount; i++)
                    Dependencies.Add(Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt16())));
            }

            var objectCount = br.ReadUInt32();
            for (var i = 0; i < objectCount; i++)
            {
                var objectMetaClass = br.ReadUInt32();
                _objects.Add(new BinTreeObject(objectMetaClass));
            }

            foreach (var treeObject in _objects) treeObject.ReadData(br);
        }
    }

    public bool IsOverride { get; }

    public List<string> Dependencies { get; } = new();

    public ReadOnlyCollection<BinTreeObject> Objects { get; }

    public void Write(string fileLocation, Version version)
    {
        Write(File.OpenWrite(fileLocation), version);
    }

    public void Write(Stream stream, Version version, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            bw.Write(Encoding.ASCII.GetBytes("PROP"));
            bw.Write(version.PackToInt()); // version

            if (version.Major >= 2)
            {
                bw.Write(Dependencies.Count);
                foreach (var dependency in Dependencies)
                {
                    bw.Write((ushort) dependency.Length);
                    bw.Write(Encoding.UTF8.GetBytes(dependency));
                }
            }

            bw.Write(_objects.Count);
            foreach (var treeObject in _objects) bw.Write(treeObject.MetaClassHash);
            foreach (var treeObject in _objects) treeObject.WriteContent(bw);
        }
    }

    public void AddObject(BinTreeObject treeObject)
    {
        if (_objects.Any(x => x.PathHash == treeObject.PathHash))
            throw new InvalidOperationException("An object with the same path already exists");
        _objects.Add(treeObject);
    }

    public void RemoveObject(uint pathHash)
    {
        if (_objects.FirstOrDefault(x => x.PathHash == pathHash) is BinTreeObject treeObject)
            _objects.Remove(treeObject);
        else throw new ArgumentException("Failed to find an object with the specified path hash", nameof(pathHash));
    }
}

public interface IBinTreeParent
{
}