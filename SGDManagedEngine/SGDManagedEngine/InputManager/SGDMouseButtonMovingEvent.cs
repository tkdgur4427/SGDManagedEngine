using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    class H1MouseButtonMovingEvent : EventArgs
    {
        internal readonly static Queue<H1MouseButtonMovingEvent> Pool = new Queue<H1MouseButtonMovingEvent>();

        public int MouseButtonId { get; internal set; }
        public Vector2 Position { get; internal set; }
        public Vector2 DeltaPosition { get; internal set; }
        public TimeSpan DeltaTime { get; internal set; }
        public H1MouseState State { get; internal set; }        

        internal static H1MouseButtonMovingEvent GetOrCreateMouseButtonEvent()
        {
            return Pool.Count > 0 ? Pool.Dequeue() : new H1MouseButtonMovingEvent();
        }

        public H1MouseButtonMovingEvent Clone()
        {
            var clone = GetOrCreateMouseButtonEvent();

            clone.MouseButtonId = MouseButtonId;
            clone.Position = Position;
            clone.DeltaPosition = DeltaPosition;
            clone.DeltaTime = DeltaTime;
            clone.State = State;

            return clone;
        }
    }
}
