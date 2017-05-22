using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    // @TODO - make abstract class for H1Controller
    class H1CameraController
    {
        public H1Camera Camera
        {
            get { return m_CameraRef; }
            set { m_CameraRef = value; }
        }

        public void Update(float deltaTime)
        {
            if (m_CameraRef != null)
            {
                HandleCameraInput();
            }
            else
                throw new NotSupportedException();
        }

        private void HandleCameraInput()
        {
            H1InputManagerWpf inputManagerRef = H1Global<H1InputManagerWpf>.Instance;

            // mouse rotation (while pressing left mouse button)
            if (inputManagerRef.IsMouseButtonDown(H1MouseButton.Left))
            {
                Vector2 mouseDelta = inputManagerRef.MouseDelta;
                float angleSize = 10.0f;

                // yaw - arbitrary axis Y
                Vector3 axisY = new Vector3(0.0f, 1.0f, 0.0f);
                Matrix yawMatrix = Matrix.RotationAxis(axisY, angleSize * mouseDelta.X);                

                // pitch - arbitrary axis X
                Vector3 axisX = new Vector3(1.0f, 0.0f, 0.0f);
                // @TODO - remove the gimbal lock ; currently, because of the gimbal lock problem, disable pitch matrix corresponding Yaxis delta value
                Matrix pitchMatrix = Matrix.Identity;
                //Matrix pitchMatrix = Matrix.RotationAxis(axisX, angleSize * mouseDelta.Y);

                Matrix finalMatrix = Matrix.Multiply(yawMatrix, pitchMatrix);
                Vector4 result = Vector3.Transform(m_CameraRef.EyePosition, finalMatrix);

                // final update (trigger UpdateGeometry())
                m_CameraRef.EyePosition = new Vector3(result.X, result.Y, result.Z);
            }

            // key board update
            if (inputManagerRef.IsKeyDown(H1Keys.W))
            {                
                m_CameraRef.EyePosition += m_CameraRef.LookAt * 10f;
            }

            if (inputManagerRef.IsKeyDown(H1Keys.S))
            {
                m_CameraRef.EyePosition -= m_CameraRef.LookAt * 10f;
            }            
        }

        private H1Camera m_CameraRef;
    }
}
