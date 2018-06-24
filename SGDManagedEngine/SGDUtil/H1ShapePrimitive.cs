using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDUtil
{
    public class H1ShapePrimitive
    {
        public H1Matrix Transform;
    }

    public class H1Sphere : H1ShapePrimitive
    {
        /// <summary>
        /// the radius of the sphere
        /// </summary>
        public float Radius;
    }

    public class H1Box : H1ShapePrimitive
    {
        /// <summary>
        /// holds the half-sizes of the box along each of its local axes
        /// </summary>
        public H1Vector3 HalfSize;
    }
}
