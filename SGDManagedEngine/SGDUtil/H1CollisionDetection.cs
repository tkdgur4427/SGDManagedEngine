using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDUtil
{
    public class H1CollisionContact
    {
        public H1Vector3 ContactPoint = new H1Vector3();
        public H1Vector3 ContactNormal = new H1Vector3();
        public float Penetration;
    }

    public class H1CollisionDetectResult
    {
        public List<H1CollisionContact> Contacts = new List<H1CollisionContact>();
    }

    public class H1CollisionDetection
    {
        // note that box vertices has 8 vertices
        static H1Vector3[] BoxVertices = new H1Vector3[]
        {
            new H1Vector3(1, 1, 1),
            new H1Vector3(-1, 1, 1),
            new H1Vector3(1, -1, 1),
            new H1Vector3(-1, -1, 1),
            new H1Vector3(1, 1, -1),
            new H1Vector3(-1, 1, -1),
            new H1Vector3(1, -1, -1),
            new H1Vector3(-1, -1, -1),
        };

        static uint BoxAndHalfSpace(H1Box InBox, H1Plane InPlane, ref H1CollisionDetectResult OutResult)
        {
            if (!H1PrimitiveIntersection.Intersect(InBox, InPlane))
            {
                return 0;
            }

            foreach (H1Vector3 BoxVertex in BoxVertices)
            {
                // calculate transformed box vertex
                H1Vector3 TransformedBoxVertex = H1Vector3.Transform(BoxVertex * InBox.HalfSize, InBox.Transform);

                // calculate the distance from the plane
                float VertexDistance = H1Vector3.Dot(TransformedBoxVertex, InPlane.Normal);

                // compare this to the plane's distance
                if (VertexDistance <= InPlane.Distance)
                {
                    // create the contact data
                    H1CollisionContact Contact = new H1CollisionContact();
                    Contact.ContactPoint = InPlane.Normal;
                    Contact.ContactPoint *= (VertexDistance - InPlane.Distance);
                    Contact.ContactPoint += TransformedBoxVertex;

                    Contact.ContactNormal = InPlane.Normal;
                    Contact.Penetration = InPlane.Distance - VertexDistance;

                    OutResult.Contacts.Add(Contact);
                }
            }

            return Convert.ToUInt32(OutResult.Contacts.Count);
        }
    }
}
