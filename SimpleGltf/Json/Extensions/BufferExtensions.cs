using SimpleGltf.Enums;

namespace SimpleGltf.Json.Extensions
{
    public static class BufferExtensions
    {
        public static BufferView CreateBufferView(this Buffer buffer, BufferViewTarget? target = null, string name = null)
        {
            return new(buffer, target, name);
        }
    }
}