using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDPhysicsEngine
{
    // represents a primitive a detect collision against
    public class H1CollisionPrimitive
    {
        /// <summary>
        /// the rigid body that is represented by this primitive
        /// </summary>
        public H1Matrix Offset;

        /// <summary>
        /// the rigid body that is represented by this primitive
        /// </summary>
        public H1RigidBody Body;

        /// <summary>
        /// bindind collision shape (ex. sphere, box, ...)
        /// </summary>
        public H1ShapePrimitive Primitive;
    }
}
