using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1Actor : H1Object
    {
        public H1Actor()
        {

        }

        public virtual Boolean Tick(double dt)
        {
            foreach (H1ActorComponent component in m_ActorComponents)
                component.Tick(dt);

            return true;
        }

        public Int32 AddActorComponent<T>(T actorComponent) where T : H1ActorComponent
        {
            Int32 index = H1ActorComponentRegistrator.GetComponentTypeIndex<T>();
            m_ActorComponents.Insert(index, actorComponent);
            return index;
        }

        public T GetActorComponent<T>() where T : H1ActorComponent
        {
            Int32 index = H1ActorComponentRegistrator.GetComponentTypeIndex<T>();
            if (index <= m_ActorComponents.Count)
            {                
                return m_ActorComponents[index] as T;
            }
            return null;
        }

        public Matrix LocalToWorld()
        {
            Matrix translationMtx = Matrix.Translation(m_Location);
            Matrix rotationMtx = Matrix.RotationYawPitchRoll(m_Rotation.Z, m_Rotation.X, m_Rotation.Y);
            Matrix scaleMtx = Matrix.Scaling(m_Scale);
            
            return Matrix.Multiply(Matrix.Multiply(scaleMtx, rotationMtx), translationMtx);
        }

        private List<H1ActorComponent> m_ActorComponents = new List<H1ActorComponent>();

        // transformation properties
        private Vector3 m_Location;
        private Vector3 m_Rotation;
        private Vector3 m_Scale;
    }
}
