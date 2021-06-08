using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Extensions;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Accessor
    {
        internal readonly BufferView BufferView;
        internal readonly int ComponentCount;
        internal readonly int ComponentTypeLength;
        internal readonly int ElementSize;
        internal readonly GltfAsset GltfAsset;
        internal readonly bool MinMax;
        internal List<dynamic> Values;

        internal Accessor(BufferView bufferView, ComponentType componentType, AccessorType type, bool minMax,
            bool normalized, string name)
        {
            BufferView = bufferView;
            GltfAsset = BufferView.GltfAsset;
            GltfAsset.Accessors ??= new List<Accessor>();
            GltfAsset.Accessors.Add(this);
            ComponentType = componentType;
            Type = type;
            if (normalized)
                Normalized = true;
            Name = name;
            ComponentCount = Type.GetColumns() * Type.GetRows();
            ComponentTypeLength = ComponentType.GetSize();
            ElementSize = ComponentCount * ComponentTypeLength;
            MinMax = minMax;
        }

        [JsonPropertyName("bufferView")] public int BufferViewReference => GltfAsset.BufferViews.IndexOf(BufferView);

        public int? ByteOffset { get; internal set; }

        public ComponentType ComponentType { get; }

        public bool? Normalized { get; }

        public int Count { get; internal set; }

        [JsonConverter(typeof(AccessorTypeConverter))]
        public AccessorType Type { get; }

        public IList<dynamic> Max
        {
            get
            {
                if (!MinMax)
                    return null;
                var value = Values.Batch(ComponentCount).First();
                foreach (var element in Values.Batch(ComponentCount).Skip(1))
                    for (var i = 0; i < ComponentCount; i++)
                        if (element[i] > value[i])
                            value[i] = element[i];
                return value;
            }
        }

        public IList<dynamic> Min
        {
            get
            {
                if (!MinMax)
                    return null;
                var value = Values.Batch(ComponentCount).First();
                foreach (var element in Values.Batch(ComponentCount).Skip(1))
                    for (var i = 0; i < ComponentCount; i++)
                        if (element[i] < value[i])
                            value[i] = element[i];
                return value;
            }
        }

        public string Name { get; }
    }

    public class Accessor<T> : Accessor where T : struct, IComparable
    {
        internal Accessor(BufferView bufferView, AccessorType type, bool minMax, bool normalized, string name) : base(
            bufferView, typeof(T).GetComponentType(), type, minMax, normalized, name)
        {
            Values = new List<dynamic>();
        }
    }
}