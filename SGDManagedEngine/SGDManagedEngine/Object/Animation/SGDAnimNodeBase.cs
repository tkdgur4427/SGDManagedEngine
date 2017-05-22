using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1AnimationBaseContext
    {
        private H1AnimInstance m_AnimInstanceRef;
    }

    public class H1AnimationInitializeContext : H1AnimationBaseContext
    {
        
    }

    public class H1AnimationUpdateContext : H1AnimationBaseContext
    {
        public float DeltaTime;
        public float CurrentWeight;
    }

    public class H1PoseLinkBase
    {
        private Int32 m_LinkID;
        private H1AnimNodeBase m_LinkedNodeRef;
        private Boolean m_bProcessed;
    }

    public class H1PoseLink : H1PoseLinkBase
    {
        // a local-space pose link to another node
    }

    public class H1ComponentSpacePoseLink : H1PoseLinkBase
    {
        // a component-space pose link to another node
    }
    public class H1ExposeValueHandler
    {
        private String BindFuncName;
        public void Execute(H1AnimationBaseContext context)
        {

        }
    }

    public class H1AnimNodeBase
    {
        // FExposeValueHandler EvaluateGraphExposedInputs        
    }
}
