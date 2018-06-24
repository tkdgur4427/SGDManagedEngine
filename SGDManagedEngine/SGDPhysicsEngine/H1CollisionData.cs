using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDPhysicsEngine
{
    /*
     * a helper structure that contains information for the detector to use
     * in building its contact data
     */
    public class H1CollisionData
    {
        /*
         * holds the base of the collision data: first contact in the array
         * this is used so that the contact pointer (below) can be incremented
         * each time a contact is detected, while this pointer points to the
         * first contact found
         */
        public H1Contact ContactArray;

        /// <summary>
        /// holds the contact array to write into
        /// </summary>
        public H1Contact Contacts;

        /// <summary>
        /// holds the maximum number of contacts the array can take
        /// </summary>
        public int ContactsLeft;

        /// <summary>
        /// holds the number of contacts found so far
        /// </summary>
        public uint ContactCount;

        /// <summary>
        /// holds the friction value to write into any collisions
        /// </summary>
        public float Friction;

        /// <summary>
        /// holds the restitution value to write into any collisions
        /// </summary>
        public float Restitution;

        /// <summary>
        /// holds the collision tolerance, even uncolliding objects this close
        /// should have collisions generated
        /// </summary>
        public float Tolerance;
    }
}
