using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public enum H1MouseState
    {
        /// <summary>
        /// The pointer just started to hit the digitizer.
        /// </summary>
        Down,

        /// <summary>
        /// The pointer is moving onto the digitizer.
        /// </summary>
        Move,

        /// <summary>
        /// The pointer just released pressure to the digitizer.
        /// </summary>
        Up,

        /// <summary>
        /// The pointer is out of the digitizer.
        /// </summary>
        Out,

        /// <summary>
        /// The pointer has been canceled.
        /// </summary>
        Cancel,
    }
}
