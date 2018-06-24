using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDPhysicsEngine
{
    /*
     * a contact represents two bodies in contact
     * resolving a contact removes their penetration, and applies sufficient impulse to keep them apart
     * collising bodies may also rebound
     * contacts can be used to represent positional joints, by making the contact constraint keep the bodies
     * in their correct orientation
     * 
     * it can be a good idea to create a contact object even when the contact isn't violated. because resolving
     * one contact can violate another; that way if one resolution moves the body, the contact may be violated
     * and can be resolved. if the contact is not violated, it will not be resolved, so you only loose a small
     * amount of execution time
     * 
     * the contact has no callable functions, it just holds the contact details
     * to resolve a set of contacts, use the contact resolver class
     */
    public class H1Contact
    {
        /// <summary>
        /// holds the bodies that are involved in the contact
        /// the second of these can be NULL, for contacts with the scenery
        /// 
        /// note that contact only holds two rigid bodies
        /// </summary>
        public H1RigidBody[] Bodies = new H1RigidBody[2];

        /// <summary>
        /// holds the lateral friction coefficient at the contact
        /// </summary>
        public float Friction;
        
        /// <summary>
        /// holds the normal restitution coefficient at the contact
        /// </summary>
        public float Restitution;

        /// <summary>
        /// holds the position of the contact in world space
        /// </summary>
        public H1Vector3 ContactPoint = new H1Vector3();

        /// <summary>
        /// holds the direction of the contact in world coordinates
        /// </summary>
        public H1Vector3 ContactNormal = new H1Vector3();

        /// <summary>
        /// holds the depth of penetration at the contact point
        /// if both bodies are specified then the contact point should be midway between the
        /// inter-penetrating points
        /// </summary>
        public float Penetration;

        /*
         * protected characteristics         
         */

        /// <summary>
        /// a transformation matrix that converts coordinates in the contact's frame of reference to world
        /// coordinates. the column of this matrix form an othonormal set of vectors
        /// </summary>
        protected H1Matrix3x3 ContactToWorld;

        /// <summary>
        /// holds the closing velocity at the point of contact
        /// this is set when the calculateInternals function is run
        /// </summary>
        protected H1Vector3 ContactVelocity;

        /// <summary>
        /// holds the required change in velocity for this contact to be resolved
        /// </summary>
        protected float DesiredDeltavelocity;

        /// <summary>
        /// holds the world space position of the contact point relative to centre of each body
        /// this is set when the calculateInternals function is run
        /// </summary>
        protected H1Vector3[] RelativeContactPosition = new H1Vector3[2];
    }
}
