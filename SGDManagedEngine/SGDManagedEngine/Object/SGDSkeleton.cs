using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1MeshBoneInfo
    {
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Int32 ParentIndex
        {
            get { return m_ParentIndex; }
            set { m_ParentIndex = value; }
        }

        private String m_Name;
        private Int32 m_ParentIndex;
    }

    public class H1ReferenceSkeleton
    {
        public List<H1MeshBoneInfo> RefBoneInfoList
        {
            get { return m_RefBoneInfos; }
        }

        public List<H1Transform> RefBoneBases
        {
            get { return m_RefBoneBases; }
        }

        public List<H1Transform> RefOffsetBases
        {
            get { return m_RefBoneOffsetTransforms; }
        }

        public Int32 GetIndexByBoneName(String boneName)
        {
            return m_NameToIndexMap[boneName];
        }

        public Boolean NameToIndexMapIsEmpty()
        {
            return m_NameToIndexMap.Count == 0;
        }

        public void AddNameToIndexPair(String boneName, Int32 index)
        {
            m_NameToIndexMap.Add(boneName, index);
        }

        // reference bone related to info serialized
        private List<H1MeshBoneInfo> m_RefBoneInfos = new List<H1MeshBoneInfo>();
        private List<H1Transform> m_RefBoneBases = new List<H1Transform>();
        private List<H1Transform> m_RefBoneOffsetTransforms = new List<H1Transform>();
        // dictionary to look up bone index from bone name
        private Dictionary<String, Int32> m_NameToIndexMap = new Dictionary<String, Int32>();
    }

    public class H1Skeleton : H1Object
    {       
        public H1ReferenceSkeleton RefSkeleton
        {
            get { return m_ReferenceSkeleton; }
        }

        public Guid Guid
        {
            get { return m_Guid; }
        }

        private H1ReferenceSkeleton m_ReferenceSkeleton = new H1ReferenceSkeleton();
        private Guid m_Guid;
    }
}
