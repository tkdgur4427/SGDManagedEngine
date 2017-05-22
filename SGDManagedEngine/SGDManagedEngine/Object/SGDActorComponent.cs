using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{    
    public class H1ActorComponentRegistrator
    {
        public static Int32 GetComponentTypeIndex<T>() where T : H1ActorComponent
        {
            // when the type name is match, return the corresponding index
            return m_ActorComponentTypes.FindIndex(x => x.Name == typeof(T).Name);
        }

        private static Int32 AddActorComponentType<ComponentType>() where ComponentType : H1ActorComponent
        {
            Type componentType = typeof(ComponentType);
#if DEBUG
            if (!componentType.IsSubclassOf(typeof(H1ActorComponent)))
            {
                throw new InvalidOperationException("Not inherited from H1ActorComponent!");
            }
#endif
            // if the ActorComponent type exists, skip it
            if (m_ActorComponentTypes.Exists(x => x.Equals(typeof(ComponentType))))
                return -1;

            // add ActorComponent type
            Int32 componentTypeIndex = m_ActorComponentTypes.Count;
            m_ActorComponentTypes.Add(componentType);
            return componentTypeIndex;
        }

        public static void RegisterActorComponent<ComponentType>() where ComponentType : H1ActorComponent
        {
            // add actor component type       
            AddActorComponentType<ComponentType>();
        }

        // @TODO - need to boost up to search functionality for ActorComponentTypes
        protected static List<Type> m_ActorComponentTypes = new List<Type>();
    }

    public class H1ActorComponent : H1Object
    {
        public class UniqueData
        {
            public Int64 UniqueId;
        }

        // @TODO - pool structure generalization for C#
        public class UniqueDataGenerator
        {
            public Int64 GenerateUnqiueData()
            {
                Int64 newIndex = -1;
                if (m_FreeIndices.Count > 0)
                {
                    newIndex = m_FreeIndices.Dequeue();                 
                }
                else
                {
                    newIndex = m_UniqueDataList.Count;
                    m_UniqueDataList.Add(new UniqueData());
                }
                
                return newIndex;
            }

            public void RemoveUniqueData(Int64 index)
            {
                m_FreeIndices.Enqueue(index);
            }

            private List<UniqueData> m_UniqueDataList = new List<UniqueData>();
            private Queue<Int64> m_FreeIndices = new Queue<Int64>();
        }

        public H1ActorComponent()
        {
            // generate unique data for ActorComponent instance
            m_UnqiueDataIndex = m_UniqueDataGenerator.GenerateUnqiueData();
        }

        ~H1ActorComponent()
        {
            // remove unique data for ActorComponent instance
            m_UniqueDataGenerator.RemoveUniqueData(m_UnqiueDataIndex);
        }

        public virtual Boolean Tick(double dt)
        {
            return true;
        }        

        // unique data
        Int64 m_UnqiueDataIndex;
        protected static UniqueDataGenerator m_UniqueDataGenerator = new UniqueDataGenerator();                
    }
}
