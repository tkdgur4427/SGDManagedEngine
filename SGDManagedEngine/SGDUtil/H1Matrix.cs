using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDUtil
{
    public class H1Matrix3x3
    {
        public Matrix3x3 Data;
    }

    public class H1Matrix
    {
        public H1Matrix()
        {
            Data = Matrix.Identity;
        }

        protected H1Matrix (Matrix InData)
        {
            Data = InData;
        }

        public H1Vector3 Look
        {
            get { return new H1Vector3(Data.Up); }
            set
            {
                Data.Up = value.Data;
            }
        }

        public H1Vector3 Right
        {
            get { return new H1Vector3(Data.Right); }
            set
            {
                Data.Right = value.Data;
            }
        }

        public H1Vector3 Up
        {
            get { return new H1Vector3(Data.Backward); }
            set
            {
                Data.Backward = value.Data;
            }
        }

        public H1Vector3 Axis0
        {
            get { return new H1Vector3(Data.Column1[0], Data.Column1[1], Data.Column1[2]); }
        }

        public H1Vector3 Axis1
        {
            get { return new H1Vector3(Data.Column2[0], Data.Column2[1], Data.Column2[2]); }
        }

        public H1Vector3 Axis2
        {
            get { return new H1Vector3(Data.Column3[0], Data.Column3[1], Data.Column3[2]); }
        }

        public H1Vector3 Axis3
        {
            get { return new H1Vector3(Data.Column4[0], Data.Column4[1], Data.Column4[2]); }
        }

        public H1Vector3 TranslationVector
        {
            get { return new H1Vector3(Data.TranslationVector); }
            set { Data.TranslationVector = value.Data; }
        }

        public static H1Matrix Invert(H1Matrix value)
        {
            return new H1Matrix(Matrix.Invert(value.Data));
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
