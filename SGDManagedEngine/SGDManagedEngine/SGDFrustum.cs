using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1Frustum
    {
        // identifiers of the 6 planes that make up the frustum
        public const int iRight = 0, iLeft = 1, iTop = 2, iBottom = 3, iNear = 4, iFar = 5;

        public float NearZ
        {
            get { return m_Planes[iNear].D; }
            set
            {
                m_Planes[iNear].D = value;
            }
        }

        public float Right
        {
            get { return -m_Planes[iRight].D; }
        }

        public float Left
        {
            get { return m_Planes[iLeft].D; }
        }

        public float Top
        {
            get { return -m_Planes[iTop].D; }
        }

        public float Bottom
        {
            get { return m_Planes[iBottom].D; }
        }

        public float Near
        {
            get { return m_Planes[iNear].D; }
        }

        public float Far
        {
            get { return -m_Planes[iFar].D; }
        }

        public float FovX
        {
            get
            {
                float dot = Vector3.Dot(m_Planes[iLeft].Normal, new Vector3(1, 0, 0));
                float fovX = (float)Math.Acos(dot);
                return fovX * 2;
            }
        }

        public float FovY
        {
            get
            {
                float dot = Vector3.Dot(m_Planes[iBottom].Normal, new Vector3(0, 1, 0));
                float fovY = (float)Math.Acos(dot);
                return fovY * 2;
            }
        }

        public H1Frustum()
        {
            m_Planes = new Plane[6];
        }

        public H1Frustum(float fov, float aspectRatio, float near, float far)
        {
            SetPerspective(fov, aspectRatio, near, far);
        }

        public Plane GetPlane(int i)
        {
            return m_Planes[i];
        }

        public void SetPerspective(float fov, float aspectRatio, float near, float far)
        {
            float tanY = (float)Math.Tan((double)fov / 2.0);
            float tanX = tanY * aspectRatio;
            Vector3 xAxis = new Vector3(-1, 0, -tanX);
            Vector3 yAxis = new Vector3(0, -1, -tanY);
            float nX = xAxis.Length();
            float nY = yAxis.Length();

            m_Planes[iRight].Normal = new Vector3(-1, 0, -tanX) / nX;
            m_Planes[iRight].D = 0.0f;
            m_Planes[iLeft].Normal = new Vector3(1, 0, -tanX) / nX;
            m_Planes[iLeft].D = 0.0f;
            m_Planes[iTop].Normal = new Vector3(0, -1, -tanY) / nY;
            m_Planes[iTop].D = 0.0f;
            m_Planes[iBottom].Normal = new Vector3(0, 1, -tanY) / nY;
            m_Planes[iBottom].D = 0.0f;
            m_Planes[iNear].Normal = new Vector3(0, 0, -1);
            m_Planes[iNear].D = near;
            m_Planes[iFar].Normal = new Vector3(0, 0, 1);
            m_Planes[iFar].D = -far; 
        }

        //@TODO ... contain methods (sphere, box), cliping(...)

        // array of 6 frustum planes. the normals all point towards the inside of the frustum
        private readonly Plane[] m_Planes;
    }
}
