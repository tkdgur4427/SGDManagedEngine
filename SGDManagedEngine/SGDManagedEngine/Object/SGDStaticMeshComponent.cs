using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    class H1StaticMeshComponent : H1ActorComponent 
    {
        public H1StaticMeshComponent()
            : base()
        {
            H1ActorComponentRegistrator.RegisterActorComponent<H1StaticMeshComponent>();
        }

        public H1StaticMesh StaticMesh
        {
            get { return m_StaticMesh; }
            set { m_StaticMesh = value; }
        }

        public override bool Tick(double dt)
        {
            return base.Tick(dt);
        }
                 
        private H1StaticMesh m_StaticMesh;        
    }
}
