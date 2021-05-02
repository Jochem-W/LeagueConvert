using System;
using System.Collections.Generic;
using System.Data;
using SimpleGltf.Enums;
using SimpleGltf.IO.Accessors;
using SimpleGltf.Json;

namespace SimpleGltf.IO
{
    public class SimplePrimitive
    {
        private readonly IDictionary<string, SimpleAccessor> _attributes;

        internal readonly Primitive Primitive;
        private BufferView _attributeBufferView;
        private BufferView _indicesBufferView;

        internal SimplePrimitive(Mesh mesh)
        {
            _attributes = new Dictionary<string, SimpleAccessor>();
            Primitive = new Primitive(mesh);
        }

        private BufferView IndicesBufferView => _indicesBufferView ??=
            new BufferView(Primitive.Mesh.GltfAsset, BufferViewTarget.ElementArrayBuffer);

        private BufferView AttributeBufferView => _attributeBufferView ??=
            new BufferView(Primitive.Mesh.GltfAsset, BufferViewTarget.ArrayBuffer);

        public IEnumerable<KeyValuePair<string, SimpleAccessor>> Attributes => _attributes;

        public SimpleAccessor Indices { get; private set; }

        public SimpleScalarAccessor<T> CreateIndicesAccessor<T>()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode != TypeCode.Byte && typeCode != TypeCode.UInt16 && typeCode != TypeCode.UInt32)
                throw new InvalidConstraintException("Only byte, ushort and uint are accepted!");
            var accessor = new SimpleScalarAccessor<T>(IndicesBufferView);
            Indices = accessor;
            Primitive.Indices = Indices.Accessor;
            return accessor;
        }

        public SimpleVector3Accessor<float> CreatePositionAccessor()
        {
            const string attribute = "POSITION";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector3Accessor<float>(AttributeBufferView, true);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }

        public SimpleVector3Accessor<float> CreateNormalAccessor()
        {
            const string attribute = "NORMAL";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector3Accessor<float>(AttributeBufferView);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }

        public SimpleVector4Accessor<float> CreateTangentAccessor()
        {
            const string attribute = "TANGENT";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector4Accessor<float>(AttributeBufferView);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }

        public SimpleVector2Accessor<T> CreateUvAccessor<T>()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode != TypeCode.Single && typeCode != TypeCode.Byte && typeCode != TypeCode.UInt16)
                throw new InvalidConstraintException("Only float, byte and ushort are accepted!");
            const string attribute = "TEXCOORD_0";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector2Accessor<T>(AttributeBufferView);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }

        public SimpleVector3Accessor<T> CreateRgbColorAccessor<T>()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode != TypeCode.Single && typeCode != TypeCode.Byte && typeCode != TypeCode.UInt16)
                throw new InvalidConstraintException("Only float, byte and ushort are accepted!");
            const string attribute = "COLOR_0";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector3Accessor<T>(AttributeBufferView, false, typeCode != TypeCode.Single);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }

        public SimpleVector4Accessor<T> CreateRgbaColorAccessor<T>()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode != TypeCode.Single && typeCode != TypeCode.Byte && typeCode != TypeCode.UInt16)
                throw new InvalidConstraintException("Only float, byte and ushort are accepted!");
            const string attribute = "COLOR_0";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector4Accessor<T>(AttributeBufferView, false, typeCode != TypeCode.Single);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }

        public SimpleVector4Accessor<T> CreateJointsAccessor<T>()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode != TypeCode.Byte && typeCode != TypeCode.UInt16)
                throw new InvalidConstraintException("Only byte and ushort are accepted!");
            const string attribute = "JOINTS_0";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector4Accessor<T>(AttributeBufferView);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }

        public SimpleVector4Accessor<T> CreateWeightsAccessor<T>()
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode != TypeCode.Single && typeCode != TypeCode.Byte && typeCode != TypeCode.UInt16)
                throw new InvalidConstraintException("Only float, byte and ushort are accepted!");
            const string attribute = "WEIGHTS_0";
            if (_attributes.ContainsKey(attribute))
                throw new NotImplementedException();
            var accessor = new SimpleVector4Accessor<T>(AttributeBufferView);
            _attributes[attribute] = accessor;
            Primitive.Attributes[attribute] = accessor.Accessor;
            return accessor;
        }
    }
}