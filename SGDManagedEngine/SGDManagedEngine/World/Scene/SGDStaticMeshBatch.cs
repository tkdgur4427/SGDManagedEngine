using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1MeshElement
    {
        
    }

    // a batch of mesh elements, all with the same material and vertex buffer
    public class H1MeshBatch
    {
        private List<H1MeshElement> m_Elements = new List<H1MeshElement>(1);
    }

    // a mesh which is defined by a primitive at scene segment construction time and never changed
    // lights are attached and detached as the segment containing the mesh is added or removed from a scene
    public class H1StaticMeshBatch : H1MeshBatch
    {
        // an interface to a draw list's reference to this static mesh
        // used to remove the static mesh from the draw list without knowing the draw list type
        public class H1DrawListElementLink
        {
            public virtual void Remove() { }
            public virtual Boolean IsInDrawList(H1StaticMeshDrawListBase drawListRef) { return false; }
        }

        // the render info for the primitive which created this mesh
        private H1PrimitiveSceneInfo m_PrimtiveSceneInfoRef;
        private float m_MinDrawDistanceSquared;
        private float m_MaxDrawDistanceSquared;
        // the index of the mesh in the scene's static meshes array
        private Int32 m_Id;
        // links to the draw lists this mesh is an element of
        private List<H1DrawListElementLink> m_DrawListLinks = new List<H1DrawListElementLink>();
    }
}
