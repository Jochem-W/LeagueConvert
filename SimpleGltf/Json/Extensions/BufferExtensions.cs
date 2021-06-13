using System.Collections.Generic;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Extensions
{
    public static class BufferExtensions
    {
        public static BufferView CreateBufferView(this Buffer buffer, BufferViewTarget? target = null)
        {
            return new(buffer) {Target = target};
        }
    }
}