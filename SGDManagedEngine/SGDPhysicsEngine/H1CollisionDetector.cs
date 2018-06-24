using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDPhysicsEngine
{
    /// <summary>
    /// a wrapper class that holds the fine grained collision detection routines
    /// 
    /// each of the functions has the same format: it takes the details of two objects,
    /// and pointer to a contact array to fill. it returns the number of contacts it wrote
    /// into the array
    /// </summary>
    public class H1CollisionDetector
    {
        static uint BoxAndHalfSpace(
            H1CollisionPrimitive InBox, H1CollisionPrimitive InPlane, ref H1CollisionData OutData)
        {
            // making sure that transferring correct type of primitives
            Debug.Assert(InBox.Primitive.GetType() == typeof(H1Box));
            Debug.Assert(InPlane.Primitive.GetType() == typeof(H1Plane));

            // not implemented yet
            return 0;
        }
    }
}
