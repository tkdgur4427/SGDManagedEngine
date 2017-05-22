using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{    
    public class H1World : H1Object
    {
        public H1Level PersistentLevel
        {
            get { return m_PersistentLevel; }
            set { m_PersistentLevel = value; }
        }

        public H1World()
        {

        }

        public H1Level GetLevel(Int32 index)
        {
            return m_Levels[index];
        }
        
        public Int32 AddLevel(H1Level level)
        {
            Int32 newIndex = m_Levels.Count;
            m_Levels.Add(level);

            return newIndex;
        }

        public virtual Boolean Tick(double dt)
        {
            if (m_PersistentLevel != null)
                m_PersistentLevel.Tick(dt);

            return true;
        }

        // current level
        private H1Level m_PersistentLevel;
        // streaming levels
        private List<H1Level> m_Levels = new List<H1Level>();
        // scene manager
        private H1Scene m_Scene;
    }
}
