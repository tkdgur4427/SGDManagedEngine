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
            // add draw commands
            H1DebugDrawCommand_Plane10x10 DataPlane10x10 = new H1DebugDrawCommand_Plane10x10();
            H1DebugDrawCommand Command = new H1DebugDrawCommand(DataPlane10x10);

            H1Global<H1CommandManager>.Instance.AddCommand(Command);
        }
    }
}
