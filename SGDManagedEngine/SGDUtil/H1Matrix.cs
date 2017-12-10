using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDUtil
{
    public class H1Matrix
    {
        public H1Matrix()
        {
            Data = new Matrix();
        }

        protected H1Matrix (Matrix InData)
        {
            Data = InData;
        }

        public H1Vector3 Look
        {
            get { return new H1Vector3(Data.Down); }
        }

        public static H1Matrix operator +(H1Matrix value)
        {
            return new H1Matrix(+value.Data);
        }

        public static H1Matrix operator +(H1Matrix left, H1Matrix right)
        {
            return new H1Matrix(left.Data + right.Data);
        }

        public static H1Matrix operator -(H1Matrix value)
        {
            return new H1Matrix(-value.Data);
        }

        public static H1Matrix operator -(H1Matrix left, H1Matrix right)
        {
            return new H1Matrix(left.Data - right.Data);
        }

        public static H1Matrix operator *(float left, H1Matrix right)
        {
            return new H1Matrix(left * right.Data);
        }

        public static H1Matrix operator *(H1Matrix left, H1Matrix right)
        {
            return new H1Matrix(left.Data * right.Data);
        }

        public static H1Matrix operator *(H1Matrix left, float right)
        {
            return new H1Matrix(left.Data * right);
        }

        public static H1Matrix operator /(H1Matrix left, float right)
        {
            return new H1Matrix(left.Data / right);
        }

        public static H1Matrix operator /(H1Matrix left, H1Matrix right)
        {
            return new H1Matrix(left.Data / right.Data);
        }

        public static bool operator ==(H1Matrix left, H1Matrix right)
        {
            return left.Data == right.Data;
        }

        public static bool operator !=(H1Matrix left, H1Matrix right)
        {
            return left.Data != right.Data;
        }

        public Matrix Data;
    }
}
