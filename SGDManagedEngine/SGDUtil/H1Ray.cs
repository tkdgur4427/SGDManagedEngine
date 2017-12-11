using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDUtil
{
    /// <summary>
    /// ray object
    /// </summary>
    public class H1Ray
    {
        public H1Ray()
        {
            Origin = new H1Vector3(0, 0, 0);
            Direction = new H1Vector3(0, 0, 0);
        }

        public H1Ray(H1Vector3 InOrigin, H1Vector3 InDirection)
        {
            Origin = InOrigin;
            Direction = InDirection;
        }

        public H1Vector3 Origin;
        public H1Vector3 Direction;
    }
}
