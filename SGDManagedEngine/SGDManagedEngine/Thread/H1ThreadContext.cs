using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SGDManagedEngine.Thread
{
    // custom thread-local layout
    //  - it contains all data for thread-local storage
    public class H1ThreadContext
    {
        public Renderer.H1RendererContext RendererContext = new Renderer.H1RendererContext();
    }
}
