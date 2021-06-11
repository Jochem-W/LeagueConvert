using System;
using System.Collections.Generic;
using SimpleGltf.Enums;

namespace SimpleGltf.Json
{
    public class Sampler
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;

        internal Sampler(GltfAsset gltfAsset, ScaleFilter? magFilter, ScaleFilter? minFilter, WrappingMode wrapS,
            WrappingMode wrapT, string name)
        {
            if (magFilter != ScaleFilter.Linear && magFilter != ScaleFilter.Nearest && magFilter != null)
                throw new ArgumentOutOfRangeException(nameof(magFilter), magFilter,
                    "Only Linear and Nearest ScaleFilter are allowed for magFilter!");
            GltfAsset = gltfAsset;
            GltfAsset.Samplers ??= new List<Sampler>();
            Index = GltfAsset.Samplers.Count;
            GltfAsset.Samplers.Add(this);
            MagFilter = magFilter;
            MinFilter = minFilter;
            WrapS = wrapS == WrappingMode.Repeat ? wrapS : null;
            WrapT = wrapT == WrappingMode.Repeat ? wrapT : null;
            Name = name;
        }

        public ScaleFilter? MagFilter { get; }

        public ScaleFilter? MinFilter { get; }

        public WrappingMode? WrapS { get; }

        public WrappingMode? WrapT { get; }

        public string Name { get; }
    }
}