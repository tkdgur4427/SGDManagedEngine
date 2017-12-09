using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDRayTracer
{
    public class H1ComponentDescriptor
    {
        public H1ComponentDescriptor()
        {
            Id = H1ComponentIdGenerator.GetId();

            // @TODO - temporary just setting value
            ComponentName = "";
        }

        public string ComponentName { get; }
        public Int64 Id { get; }
    }

    public static class H1ComponentIdGenerator
    {
        public static Int64 GetId()
        {
            return IdGenerator++;
        }

        private static Int64 IdGenerator = 0;
    }

    public abstract class H1Component
    {
        public H1Component(H1Entity InOwner)
        {
            Owner = InOwner;

            // create component descriptor each component
            ComponentDescriptor = new H1ComponentDescriptor();
        }

        // owner entity
        public H1Entity Owner
        {
            get; private set;
        }

        public H1ComponentDescriptor Descriptor
        {
            get { return ComponentDescriptor; }
        }

        // component descriptor
        protected H1ComponentDescriptor ComponentDescriptor;
    }
}
