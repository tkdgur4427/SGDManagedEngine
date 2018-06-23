using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDPhysicsEngine
{
    /// <summary>
    /// a rigid body is the basic simulation object in physics core
    /// 
    /// it has position and orientation data, along with first derivatives
    /// it can be integrated forward through time, and have forces, torques and impulses
    /// (linear or angular) applied to it
    /// 
    /// the rigid body manages its states and allows access through a set of methods
    /// </summary>
    public class H1RigidBody
    {
        /*
         * @name characteristic data and state
         * 
         * this data holds the state of the rigid body
         * there are two sets of data: characteristics and state
         * 
         * characteristics are properties of the rigid body independent of its current kinematic
         * situation. this includes mass, moment of inertia and damping properties. two identical
         * rigid bodys will have the same values for their characteristics
         * 
         * state includes all the characteristics and also includes the kinematric situation of
         * the rigid body in the current simulation. by setting the whole state data, a rigid body's
         * exact game state can be replicated. note that state does not include any forces applied
         * to the body. two identical rigid bodies in the same simulation will not share the same
         * state values
         * 
         * the state values make up the smallest set of independent data for the rigid body
         * other state data is calculated from their current values. when state data is changed the
         * dependent values need to be updated: this can be achieved either by integrating the 
         * simulation, or by calling the calculateInternals function. this two stage process is
         * used because recalculating internals can be costly process: all state changes should be
         * carried out at the same time allowing for a single call
         * 
         * @see calculateInternals
         */

        /// <summary>
        /// holds the inverse of the mass of the rigid body. it is more useful to hold the inverse
        /// mass because integration is simpler, and because in real time simulation it is more 
        /// useful to have bodies with infinite mass (immovable) than zero mass
        /// (completely unstable in numerical simulation)
        /// </summary>
        float InverseMass;

        /// <summary>
        /// holds the inverse of the body's inertia tensor. the inertia tensor provided must not be 
        /// degenerate (that would mean the body had zero inertia for spinning along one axis).
        /// as long as the tensor is finite, it will be invertible. the inverse tensor is used for
        /// similar reasons to the use of inverse mass
        /// 
        /// the inertia tensor, unlike the other variables that define a rigid body, is given in
        /// boy space
        /// </summary>
        H1Matrix3x3 InverseInertiaTensor;

        /// <summary>
        /// holds the amount of damping applied linear(angular) motion
        /// damping is required to remove energy added through numerical instability in the 
        /// integrator
        /// </summary>
        float LinearDamping;
        float AngularDamping;

        /// <summary>
        /// linear position of the rigid body
        /// </summary>
        H1Vector3 Position;

        /// <summary>
        /// angular orientation of the rigid body
        /// </summary>
        H1Quaternion Quaternion;

        /// <summary>
        /// holds the linear(angular) velocity in world space
        /// </summary>
        H1Vector3 Velocity;
        H1Vector3 Rotation;

        /*
         * derived data
         * 
         * these data members hold information that is derived from the other data in the class
         */

        /// <summary>
        /// holds the inverse inertia tensor of the body in world space
        /// the inverse inertia tensor member is specified in the body's local space
        /// </summary>
        H1Matrix3x3 InverseInertiaTensorWorld;

        /// <summary>
        /// holds the amount of motion of the body
        /// this is a recency weighted mean that can be used to put a body to sleap
        /// </summary>
        float Motion;

        /// <summary>
        /// a body can be put to sleep to avoid it being updated by the integration functions
        /// or affected by collisions with the world
        /// </summary>
        bool IsAwake;

        /// <summary>
        /// some bodies may never be allowed to fall asleep
        /// user controlled bodies, for example, should be always awake
        /// </summary>
        bool CanSleep;

        /// <summary>
        /// holds a transform matrix for converting body space into world space and vice versa
        /// this can be achieved by calling the getPointIn*Space functions
        /// </summary>
        H1Matrix TransformMatrix;

        /*
         * @name force and torque accumulators
         * 
         * these data members store the current force, torque and acceleration of the rigid body
         * forces can be added to the rigid body in any order, and the class decomposes them into 
         * their constituents, accumulating them for the next simulation step. at the simulation
         * step, the accelerations are calculated and stored to be applied to the rigid body
         */

        /// <summary>
        /// holds the accumulated (force/torque) to be applied at the next integration step
        /// </summary>
        H1Vector3 ForceAccum;
        H1Vector3 TorqueAccum;

        /// <summary>
        /// holds the acceleration of the rigid body
        /// this value can be used to set acceleration due to gravity (its primary use)
        /// , or any other constant acceleration
        /// </summary>
        H1Vector3 Acceleration;

        /// <summary>
        /// holds the linear acceleration of the rigid body, for the previous frame
        /// </summary>
        H1Vector3 LastFrameAcceleration;
    }
}
