using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDManagedEngine.Commands
{
    public class H1DebugDrawCommandData
    {
        public enum EPrimitiveType
        {
            Invalid,
            Sphere,
            Box,
            Plane10x10,
            Plane,
        }

        public EPrimitiveType PrimitiveType = EPrimitiveType.Invalid;

        // transformation information
        public H1Vector3 Position = new H1Vector3();
        public H1Vector3 Scaling = new H1Vector3(1, 1, 1);
        public H1Quaternion Rotation = new H1Quaternion(0, 0, 0, 0);

        public SGD.H1Transform GetTransform()
        {
            // create new value (not copy by reference)
            SGD.H1Transform Result = new SGD.H1Transform();
            Result.Translation = new SharpDX.Vector3(Position.X, Position.Y, Position.Z);
            Result.Scaling = new SharpDX.Vector3(Scaling.X, Scaling.X, Scaling.Z);
            Result.Rotation = new SharpDX.Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);

            return Result;
        }
    }

    public class H1DebugDrawCommand_Box : H1DebugDrawCommandData
    {
        public H1DebugDrawCommand_Box()
        {
            PrimitiveType = EPrimitiveType.Box;
        }
    }

    public class H1DebugDrawCommand_Plane10x10 : H1DebugDrawCommandData
    {
        public H1DebugDrawCommand_Plane10x10()
        {
            PrimitiveType = EPrimitiveType.Plane10x10;
        }
    }

    // debug draw method for convenience usage
    public class H1DebugDrawCommand : H1Command<H1DebugDrawCommandData>
    {
        // getting only debug draw command data
        public H1DebugDrawCommand(H1DebugDrawCommandData InData)
            : base(InData)
        {
            // create delegate from class member method
            ExecuteObject = Execute;
        }

        // to prevent use of class member data, I declare this method as 'static'
        static void Execute(H1DebugDrawCommandData InData)
        {
            if (InData.PrimitiveType != H1DebugDrawCommandData.EPrimitiveType.Invalid)
            {
                SGD.H1Transform Transform = InData.GetTransform();

                // get the command list object from thread context
                SharpDX.Direct3D12.GraphicsCommandList CommandList = Thread.H1ThreadGlobal.ThreadContext.RendererContext.CurrCommandList.CommandList;

                switch (InData.PrimitiveType)
                {
                    case H1DebugDrawCommandData.EPrimitiveType.Box:
                        {
                            SGD.H1RenderUtils.DrawBox(CommandList, Transform);
                            break;
                        }
                    case H1DebugDrawCommandData.EPrimitiveType.Plane10x10:
                        {
                            SGD.H1RenderUtils.DrawPlane10x10(CommandList, new SharpDX.Vector2(0, 0), new SharpDX.Vector2(1, 1));
                            break;
                        }
                }
            }
        }
    }
}
