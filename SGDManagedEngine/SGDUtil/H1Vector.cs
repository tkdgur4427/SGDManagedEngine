using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDUtil
{
    public class H1Vector2
    {
        public H1Vector2()
        {
            Data = new Vector2();
        }

        protected H1Vector2(Vector2 InData)
        {
            Data = InData;
        }

        public static H1Vector2 operator +(H1Vector2 value)
        {
            return new H1Vector2(+value.Data);
        }

        public static H1Vector2 operator +(H1Vector2 left, H1Vector2 right)
        {
            return new H1Vector2(left.Data + right.Data);
        }

        public static H1Vector2 operator +(H1Vector2 value, float scalar)
        {
            return new H1Vector2(value.Data + scalar);
        }

        public static H1Vector2 operator +(float scalar, H1Vector2 value)
        {
            return new H1Vector2(scalar + value.Data);
        }

        public static H1Vector2 operator -(H1Vector2 value)
        {
            return new H1Vector2(-value.Data);
        }

        public static H1Vector2 operator -(H1Vector2 value, float scalar)
        {
            return new H1Vector2(value.Data - scalar);
        }

        public static H1Vector2 operator -(float scalar, H1Vector2 value)
        {
            return new H1Vector2(scalar - value.Data);
        }

        public static H1Vector2 operator -(H1Vector2 left, H1Vector2 right)
        {
            return new H1Vector2(left.Data - right.Data);
        }

        public static H1Vector2 operator *(H1Vector2 left, H1Vector2 right)
        {
            return new H1Vector2(left.Data * right.Data);
        }

        public static H1Vector2 operator *(H1Vector2 value, float scale)
        {
            return new H1Vector2(value.Data * scale);
        }

        public static H1Vector2 operator *(float scale, H1Vector2 value)
        {
            return new H1Vector2(scale * value.Data);
        }

        public static H1Vector2 operator /(H1Vector2 value, H1Vector2 scale)
        {
            return new H1Vector2(value.Data / scale.Data);
        }

        public static H1Vector2 operator /(float scale, H1Vector2 value)
        {
            return new H1Vector2(scale / value.Data);
        }

        public static H1Vector2 operator /(H1Vector2 value, float scale)
        {
            return new H1Vector2(value.Data / scale);
        }

        public static bool operator ==(H1Vector2 left, H1Vector2 right)
        {
            return left.Data == right.Data;
        }

        public static bool operator !=(H1Vector2 left, H1Vector2 right)
        {
            return left.Data != right.Data;
        }

        protected Vector2 Data;
    }

    public class H1Vector3
    {
        public H1Vector3()
        {
            Data = new Vector3();
        }

        public H1Vector3(float X, float Y, float Z)
        {
            Data = new Vector3(X, Y, Z);
        }

        public H1Vector3(Vector3 InData)
        {
            Data = InData;
        }

        public static H1Vector3 Normalize(H1Vector3 value)
        {
            return new H1Vector3(Vector3.Normalize(value.Data));
        }

        public static H1Vector3 Transform(H1Vector3 vector, H1Matrix transform)
        {
            Vector3 Result;
            Vector3.Transform(ref vector.Data, ref transform.Data, out Result);
            return new H1Vector3(Result);
        }

        public static H1Vector3 Cross(H1Vector3 left, H1Vector3 right)
        {
            return new H1Vector3(Vector3.Cross(left.Data, right.Data));
        }

        public static H1Vector3 operator +(H1Vector3 value)
        {
            return new H1Vector3(+value.Data);
        }

        public static H1Vector3 operator +(H1Vector3 left, H1Vector3 right)
        {
            return new H1Vector3(left.Data + right.Data);
        }

        public static H1Vector3 operator +(H1Vector3 value, float scalar)
        {
            return new H1Vector3(value.Data + scalar);
        }

        public static H1Vector3 operator +(float scalar, H1Vector3 value)
        {
            return new H1Vector3(scalar + value.Data);
        }

        public static H1Vector3 operator -(H1Vector3 value)
        {
            return new H1Vector3(-value.Data);
        }

        public static H1Vector3 operator -(H1Vector3 value, float scalar)
        {
            return new H1Vector3(value.Data - scalar);
        }

        public static H1Vector3 operator -(float scalar, H1Vector3 value)
        {
            return new H1Vector3(scalar - value.Data);
        }

        public static H1Vector3 operator -(H1Vector3 left, H1Vector3 right)
        {
            return new H1Vector3(left.Data - right.Data);
        }

        public static H1Vector3 operator *(H1Vector3 left, H1Vector3 right)
        {
            return new H1Vector3(left.Data * right.Data);
        }

        public static H1Vector3 operator *(float scale, H1Vector3 value)
        {
            return new H1Vector3(scale * value.Data);
        }

        public static H1Vector3 operator *(H1Vector3 value, float scale)
        {
            return new H1Vector3(value.Data * scale);
        }

        public static H1Vector3 operator /(H1Vector3 value, H1Vector3 scale)
        {
            return new H1Vector3(value.Data / scale.Data);
        }

        public static H1Vector3 operator /(float scale, H1Vector3 value)
        {
            return new H1Vector3(scale / value.Data);
        }

        public static H1Vector3 operator /(H1Vector3 value, float scale)
        {
            return new H1Vector3(value.Data / scale);
        }

        public static bool operator ==(H1Vector3 left, H1Vector3 right)
        {
            return left.Data == right.Data;
        }

        public static bool operator !=(H1Vector3 left, H1Vector3 right)
        {
            return left.Data != right.Data;
        }

        public Vector3 Data;
    }

    public class H1Vector4
    {
        public H1Vector4()
        {
            Data = new Vector4();
        }

        protected H1Vector4(Vector4 InData)
        {
            Data = InData;
        }

        public static H1Vector4 operator +(H1Vector4 value)
        {
            return new H1Vector4(+value.Data);
        }

        public static H1Vector4 operator +(H1Vector4 left, H1Vector4 right)
        {
            return new H1Vector4(left.Data + right.Data);
        }

        public static H1Vector4 operator +(H1Vector4 value, float scalar)
        {
            return new H1Vector4(value.Data + scalar);
        }

        public static H1Vector4 operator +(float scalar, H1Vector4 value)
        {
            return new H1Vector4(scalar + value.Data);
        }

        public static H1Vector4 operator -(H1Vector4 value)
        {
            return new H1Vector4(-value.Data);
        }

        public static H1Vector4 operator -(H1Vector4 value, float scalar)
        {
            return new H1Vector4(value.Data - scalar);
        }

        public static H1Vector4 operator -(float scalar, H1Vector4 value)
        {
            return new H1Vector4(scalar - value.Data);
        }

        public static H1Vector4 operator -(H1Vector4 left, H1Vector4 right)
        {
            return new H1Vector4(left.Data - right.Data);
        }

        public static H1Vector4 operator *(H1Vector4 left, H1Vector4 right)
        {
            return new H1Vector4(left.Data * right.Data);
        }

        public static H1Vector4 operator *(float scale, H1Vector4 value)
        {
            return new H1Vector4(scale * value.Data);
        }

        public static H1Vector4 operator *(H1Vector4 value, float scale)
        {
            return new H1Vector4(value.Data * scale);
        }

        public static H1Vector4 operator /(H1Vector4 value, H1Vector4 scale)
        {
            return new H1Vector4(value.Data / scale.Data);
        }

        public static H1Vector4 operator /(float scale, H1Vector4 value)
        {
            return new H1Vector4(scale / value.Data);
        }

        public static H1Vector4 operator /(H1Vector4 value, float scale)
        {
            return new H1Vector4(value.Data / scale);
        }

        public static bool operator ==(H1Vector4 left, H1Vector4 right)
        {
            return left.Data == right.Data;
        }

        public static bool operator !=(H1Vector4 left, H1Vector4 right)
        {
            return left.Data != right.Data;
        }

        Vector4 Data;
    }
}
