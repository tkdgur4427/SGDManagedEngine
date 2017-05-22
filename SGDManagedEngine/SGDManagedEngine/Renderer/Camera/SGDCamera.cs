using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public enum H1ViewTypes
    {
        Perspective,
    }

    public enum H1ProjectionTypes
    {
        Perspective,
        Orthographics,
    }

    class H1Camera
    {    
        /// <summary>
        /// construct a camera near the origin with default perspective projection
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lookAtPosition"></param>
        public H1Camera()
        {
            Set(new Vector3(1, 1, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));            
        }

        public Vector3 EyePosition
        {
            get { return m_EyePosition; }
            set
            {
                m_EyePosition = value;
                UpdateGeometry();
            }
        }

        public Vector3 LookAt
        {
            get { return m_LookAt; }
        }

        public Vector3 Right
        {
            get { return m_Right; }
        }

        public Vector3 Up
        {
            get { return m_Up; }
        }

        public Matrix AxisSystem
        {
            get { return m_AxisSystem; }
        }

        public Matrix InverseAxisSystem
        {
            get
            {
                Matrix inverseAxisSystem = new Matrix();
                inverseAxisSystem = Matrix.Transpose(m_AxisSystem);
                return inverseAxisSystem;
            }
        }

        public Matrix ViewMatrix
        {
            get
            {                
                return m_ViewMatrix;
            }
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                return m_ProjectionMatrix;
            }
        }

        public void Set(Vector3 eye, Vector3 lookAtPoint, Vector3 up)
        {
            m_EyePosition = eye;
            m_LookAtPoint = lookAtPoint;
            m_Up = up;            

            UpdateGeometry();
        }

        public void SetPerspective(float yFov, float aspectRatio, float nearZ, float farZ)
        {
            if (yFov <= 0 || aspectRatio <= 0 || nearZ <= 0 || farZ <= 0)
                throw new ArgumentOutOfRangeException();

            m_PerspectiveNearZ = nearZ;
            m_ProjectionType = H1ProjectionTypes.Perspective;
            m_Fov = yFov;
            m_AspectRatio = aspectRatio;
            m_Frustum.SetPerspective(m_Fov, m_AspectRatio, nearZ, farZ);

            // @TODO trigger camera changed event
        }

        public void SetState(H1ViewTypes viewType, Vector3 eye, Vector3 lookAtPoint, Vector3 upVector, float fov, float aspectRatio, float nearZ, float farZ, float focusRadius)
        {
            m_IsChangingCamera = true;

            try
            {
                m_ViewType = viewType;
                Set(eye, lookAtPoint, upVector);

                if (viewType == H1ViewTypes.Perspective)
                {
                    SetPerspective(fov, aspectRatio, nearZ, farZ);

                    // generate projection matrix
                    m_ProjectionMatrix = Matrix.PerspectiveFovLH(fov, m_AspectRatio, nearZ, farZ);
                }
            }
            finally
            {
                m_IsChangingCamera = false;
            }

            // @TODO - trigger camera change
        }

        private void UpdateGeometry()
        {                   
            // create orthonormal basis from lookAt and up vectors - based on 'right-hand system'
            m_LookAt = m_LookAtPoint - m_EyePosition;
            m_LookAt.Normalize();

            m_Right = Vector3.Cross(m_Up, m_LookAt);
            m_Right.Normalize();

            m_Up = Vector3.Cross(m_LookAt, m_Right);
            // no need to nomralize (already normalized!)

            m_LookAtDistance = Vector3.Distance(m_EyePosition, m_LookAtPoint);

            // construct axis system matrix (column-based matrix system)
            m_AxisSystem.Column1 = new Vector4(m_Right, 0.0f);
            m_AxisSystem.Column2 = new Vector4(m_Up, 0.0f);
            m_AxisSystem.Column3 = new Vector4(m_LookAt, 0.0f);
            m_AxisSystem.Column4 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            // view matrix
            Matrix invTranslation = Matrix.Translation(-m_EyePosition);
            Matrix invRotation = Matrix.Transpose(m_AxisSystem);
            //m_ViewMatrix = Matrix.Multiply(invTranslation, invRotation);
            // temporary
            m_ViewMatrix = Matrix.LookAtLH(m_EyePosition, m_LookAtPoint, m_Up);

            if (m_ProjectionType == H1ProjectionTypes.Orthographics)
            {
                // @TODO - orthogonal view
            }
            else
            {
                // @TODO - trigger the event (camera moved)
            }
        }        

        private H1ViewTypes m_ViewType = H1ViewTypes.Perspective;

        private Vector3 m_EyePosition;
        private Vector3 m_LookAtPoint;
        private Vector3 m_LookAt;
        private Vector3 m_Up;
        private Vector3 m_Right;
        private float m_LookAtDistance;
        private float m_FocusRadius;

        // column based matrix system
        // view matrix
        private Matrix m_AxisSystem = new Matrix();

        // @TODO - create camera controller!
        private Matrix m_ViewMatrix = new Matrix();
        private Matrix m_ProjectionMatrix = new Matrix();

        private float m_AspectRatio;
        private float m_Fov;
        private float m_PerspectiveNearZ = 0.01f, m_OrthogonalNearZ = -10000.0f;
        private H1ProjectionTypes m_ProjectionType;
        private readonly H1Frustum m_Frustum = new H1Frustum();

        private bool m_IsChangingCamera;

        // event handler for camera to change (with camera controller)
        public event EventHandler m_CameraChanged;
    }
}
