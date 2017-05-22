using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1Transform
    {
        public Matrix Transformation
        {
            get
            {
                // return transformation matrix
                Matrix scaleMtx = Matrix.Scaling(m_Scaling.X, m_Scaling.Y, m_Scaling.Z);
                Matrix rotMtx = Matrix.RotationQuaternion(m_Rotation);
                Matrix translationMtx = Matrix.Translation(m_Translation.X, m_Translation.Y, m_Translation.Z);

                return Matrix.Multiply(Matrix.Multiply(scaleMtx, rotMtx), translationMtx);
            }
        }

        public Vector3 Translation
        {
            get { return new Vector3(m_Translation.X, m_Translation.Y, m_Translation.Z); }
            set { m_Translation = new Vector4(value.X, value.Y, value.Z, 1.0f); }
        }

        public Vector3 Scaling
        {
            get { return new Vector3(m_Scaling.X, m_Scaling.Y, m_Scaling.Z); }
            set { m_Scaling = new Vector4(value.X, value.Y, value.Z, 1.0f); }
        }

        public Quaternion Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        // to align 16 to speed up
        private Vector4 m_Translation = new Vector4(0);
        private Quaternion m_Rotation = new Quaternion();
        private Vector4 m_Scaling = new Vector4(1); 
    }
}
