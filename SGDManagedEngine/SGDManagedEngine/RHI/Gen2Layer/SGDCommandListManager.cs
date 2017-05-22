using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1CommandListManager
    {
        public H1CommandListManager()
        {

        }

        public Boolean Initialize()
        {
            if (!m_GraphicsQueue.Initialize())
                return false;
            if (!m_ComputeQueue.Initialize())
                return false;
            if (!m_CopyQueue.Initialize())
                return false;

            InitializePlatformDependent();

            return true;
        }

        public virtual void Destroy()
        {
            m_GraphicsQueue.Destroy();
            m_ComputeQueue.Destroy();
            m_CopyQueue.Destroy();
        }
               
        public H1CommandQueue GetQueue(H1CommandListType type)
        {
            H1CommandQueue commandQueue = null;
            switch (type)
            {
                case H1CommandListType.Compute:
                    commandQueue = m_ComputeQueue;
                    break;
                case H1CommandListType.Copy:
                    commandQueue = m_CopyQueue;
                    break;
                default:
                    commandQueue = m_GraphicsQueue;
                    break;
            }

            return commandQueue;
        }

        protected H1CommandQueue m_GraphicsQueue = new H1CommandQueue(H1CommandListType.Direct);
        protected H1CommandQueue m_ComputeQueue = new H1CommandQueue(H1CommandListType.Compute);
        protected H1CommandQueue m_CopyQueue = new H1CommandQueue(H1CommandListType.Copy);
    }
}
