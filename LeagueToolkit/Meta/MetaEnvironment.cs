using System.Collections.ObjectModel;
using System.Reflection;
using LeagueToolkit.Helpers.Hashing;
using LeagueToolkit.Meta.Attributes;

namespace LeagueToolkit.Meta;

public sealed class MetaEnvironment
{
    private readonly List<Type> _registeredMetaClasses = new();
    private readonly Dictionary<uint, IMetaClass> _registeredObjects = new();

    internal MetaEnvironment(ICollection<Type> metaClasses, IEnumerable<KeyValuePair<uint, string>> hashes)
    {
        if (metaClasses is null)
        {
            throw new ArgumentNullException(nameof(metaClasses));
        }

        if (hashes is null)
        {
            throw new ArgumentNullException(nameof(hashes));
        }

        _registeredMetaClasses = metaClasses.ToList();
        RegisteredMetaClasses = _registeredMetaClasses.AsReadOnly();
        RegisteredObjects = new ReadOnlyDictionary<uint, IMetaClass>(_registeredObjects);
        RegisteredHashes = new Dictionary<uint, string>(hashes);

        foreach (var metaClass in RegisteredMetaClasses)
        {
            var metaClassAttribute = metaClass.GetCustomAttribute(typeof(MetaClassAttribute)) as MetaClassAttribute;
            if (metaClassAttribute is null)
            {
                throw new ArgumentException($"MetaClass: {metaClass.Name} does not have MetaClass Attribute");
            }
        }
    }

    public ReadOnlyCollection<Type> RegisteredMetaClasses { get; }
    public ReadOnlyDictionary<uint, IMetaClass> RegisteredObjects { get; }
    public Dictionary<uint, string> RegisteredHashes { get; set; }

    public static MetaEnvironment Create(ICollection<Type> metaClasses)
    {
        if (metaClasses is null)
        {
            throw new ArgumentNullException(nameof(metaClasses));
        }

        return new MetaEnvironment(metaClasses, new Dictionary<uint, string>());
    }

    public static MetaEnvironment Create(ICollection<Type> metaClasses, IEnumerable<string> hashes)
    {
        if (metaClasses is null)
        {
            throw new ArgumentNullException(nameof(metaClasses));
        }

        if (hashes is null)
        {
            throw new ArgumentNullException(nameof(hashes));
        }

        Dictionary<uint, string> hashDictionary = new();
        foreach (var hash in hashes)
        {
            hashDictionary.Add(Fnv1a.HashLower(hash), hash);
        }

        return Create(metaClasses, hashDictionary);
    }

    public static MetaEnvironment Create(ICollection<Type> metaClasses, IEnumerable<KeyValuePair<uint, string>> hashes)
    {
        if (metaClasses is null)
        {
            throw new ArgumentNullException(nameof(metaClasses));
        }

        if (hashes is null)
        {
            throw new ArgumentNullException(nameof(hashes));
        }

        return new MetaEnvironment(metaClasses, hashes);
    }

    public void RegisterObject<T>(string path, T metaObject)
        where T : IMetaClass
    {
        var metaClassAttribute =
            metaObject.GetType().GetCustomAttribute(typeof(MetaClassAttribute)) as MetaClassAttribute;
        if (metaClassAttribute is null)
        {
            throw new ArgumentException($"{nameof(metaObject)} does not have a Meta Class attribute");
        }

        var pathHash = Fnv1a.HashLower(path);
        if (_registeredObjects.TryAdd(pathHash, metaObject) is false)
        {
            throw new ArgumentException("An object with the same path is already registered", nameof(pathHash));
        }
    }

    public void RegisterObject<T>(uint pathHash, T metaObject)
        where T : IMetaClass
    {
        var metaClassAttribute =
            metaObject.GetType().GetCustomAttribute(typeof(MetaClassAttribute)) as MetaClassAttribute;
        if (metaClassAttribute is null)
        {
            throw new ArgumentException($"{nameof(metaObject)} does not have a Meta Class attribute");
        }

        if (_registeredObjects.TryAdd(pathHash, metaObject) is false)
        {
            throw new ArgumentException("An object with the same path is already registered", nameof(pathHash));
        }
    }

    public bool DeregisterObject(string path)
    {
        return DeregisterObject(Fnv1a.HashLower(path));
    }

    public bool DeregisterObject(uint pathHash)
    {
        return _registeredObjects.Remove(pathHash);
    }

    public Type FindMetaClass(uint classNameHash)
    {
        return RegisteredMetaClasses.FirstOrDefault(x =>
        {
            var metaClassAttribute = x.GetCustomAttribute(typeof(MetaClassAttribute)) as MetaClassAttribute;

            return metaClassAttribute?.NameHash == classNameHash;
        });
    }

    public T FindObject<T>(string path)
        where T : IMetaClass
    {
        return FindObject<T>(Fnv1a.HashLower(path));
    }

    public T FindObject<T>(uint pathHash)
        where T : IMetaClass
    {
        return (T)_registeredObjects.GetValueOrDefault(pathHash);
    }

    public string ResolveHash(uint hash)
    {
        return RegisteredHashes.GetValueOrDefault(hash);
    }
}