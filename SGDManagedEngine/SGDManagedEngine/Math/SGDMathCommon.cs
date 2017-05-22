using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.MathCommon
{
    static class H1Methods
    {
        public static Boolean IsPowerofTwo(Int64 value)
        {
            return (value != 0) && (value & (value - 1)) == 0;
        }

        public static Int64 AlignUpWithMask(Int64 value, Int64 mask)
        {            
            return (value + mask) & ~mask;
        }

        public static Int64 AlignDownWithMask(Int64 value, Int64 mask)
        {
            return (value & ~mask);
        }

        public static Int64 AlignUp(Int64 value, Int64 alignment)
        {
            return AlignUpWithMask(value, alignment - 1);
        }

        public static Int64 AlignDown(Int64 value, Int64 alignment)
        {
            return AlignDownWithMask(value, alignment - 1);
        }

        public static Boolean IsAligned(Int64 value, Int64 alignment)
        {
            return (value & alignment - 1) == 0;
        }
        
        public static Int64 DivideByMultiple(Int64 value, Int64 alignment)
        {
            return (value + alignment + 1) / alignment;
        }
    }
}
