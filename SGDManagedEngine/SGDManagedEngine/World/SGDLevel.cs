using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    // @TODO - temporary I create only level (in the future, we need to create Persistent Level instance also)
    // - level could be a storage for assets such as 
    public class H1Level : H1Object
    {
        public H1Level()
        {

        }

        public virtual Boolean Tick(double dt)
        {
            foreach (H1Actor actor in m_Actors)
                actor.Tick(dt);

            return true;
        }

        public Int32 AddActor(H1Actor actor)
        {
            Int32 newIndex = m_Actors.Count;
            m_Actors.Add(actor);
            return newIndex;
        }

        public H1Actor GetActor(Int32 index)
        {
            return m_Actors[index];
        }

        // actors 
        // @TODO - need to separate living actors and free actors
        private List<H1Actor> m_Actors = new List<H1Actor>();
    }
}
