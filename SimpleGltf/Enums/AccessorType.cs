namespace SimpleGltf.Enums
{
    public class AccessorType
    {
        private AccessorType(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static AccessorType Scalar => new("SCALAR");
        public static AccessorType Vector2 => new("VEC2");
        public static AccessorType Vector3 => new("VEC3");
        public static AccessorType Vector4 => new("VEC4");
        public static AccessorType Matrix2x2 => new("MAT2");
        public static AccessorType Matrix3x3 => new("MAT3");
        public static AccessorType Matrix4x4 => new("MAT4");
    }
}