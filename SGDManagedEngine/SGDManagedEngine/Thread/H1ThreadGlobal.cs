using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.Thread
{
    public class H1ThreadGlobal
    {
        // thread local storage accessor
        [ThreadStatic]
        public static H1ThreadContext ThreadContext = new H1ThreadContext();
    }
}
