using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDManagedEngine.Commands;
using SGDManagedEngine.SGD;

namespace SGDManagedEngine.Physics.Test
{
    public class TestPhysicsSimulator
    {
        public void Update()
        {
            if (bSimulate)
            {
                // when simulation is enabled, run the physics
                SimulatePhysics();
            }
            else
            {
                // otherwise advancing frame by frame for phyiscs simulation
                if (PrevFrameNumber < FrameNumber)
                {
                    // simulate the physics by frames 
                    for (Int64 Count = PrevFrameNumber; Count < FrameNumber; ++Count)
                    {
                        SimulatePhysics();
                    }

                    // update the previously simulated frame number
                    PrevFrameNumber = FrameNumber;
                }
            }

            // render physics every frame
            RenderPhysics();
        }

        public void IncreaseFrameNumber()
        {
            FrameNumber++;
        }

        public void SimulatePhysics()
        {

        }

        public void RenderPhysics()
        {
            // add draw commands (it executes every frame)

            // basic plane 10x10
            H1DebugDrawCommand_Plane10x10 DataPlane10x10 = new H1DebugDrawCommand_Plane10x10();
            H1DebugDrawCommand Command = new H1DebugDrawCommand(DataPlane10x10);

            H1Global<H1CommandManager>.Instance.AddCommand(Command);


        }

        public bool bSimulate = false;
        public Int64 FrameNumber = 0;

        // this is for only inner tracking purpose
        protected Int64 PrevFrameNumber = 0;
    }
}
