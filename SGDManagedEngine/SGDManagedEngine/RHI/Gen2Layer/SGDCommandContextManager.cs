using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public abstract class H1CommandContextManager
    {


        protected List<H1CommandContext>[] m_CommandContextPools = new List<H1CommandContext>[Convert.ToInt32(H1CommandListType.Num)];
        protected Queue<H1CommandContext>[] m_AvailableCommandContextPools = new Queue<H1CommandContext>[Convert.ToInt32(H1CommandListType.Num)];
    }
}
