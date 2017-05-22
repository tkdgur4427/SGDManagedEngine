using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;
using SharpDX.DXGI;

namespace SGDManagedEngine.SGD.Direct3D12
{
    class H1GlobalDX12Definitions
    {
        public const UInt32 CMDQUEUE_GRAPHICS = 0;
        public const UInt32 CMDQUEUE_COMPUTE = 1;
        public const UInt32 CMDQUEUE_COPY = 2;
        public const UInt32 CMDQUEUE_NUM = 3;

        public const UInt32 CMDTYPE_READ = 0;
        public const UInt32 CMDTYPE_WRITE = 1;
        public const UInt32 CMDTYPE_ANY = 2;
        public const UInt32 CMDTYPE_NUM = 3;

        public const UInt64 MAX_FRAME_LATENCY = 1;
        public const UInt64 MAX_FRAMES_IN_FLIGHT = MAX_FRAME_LATENCY + 1;

        public const UInt64 FRAME_FENCES = 32;
        public const UInt64 FRAME_FENCE_LATENCY = 32;        
        public const UInt64 FRAME_FENCE_INFLIGHT = MAX_FRAMES_IN_FLIGHT;
                
        public const UInt32 VERTEX_INPUT_RESOURCE_SLOT_COUNT = 32;
        public const UInt32 COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT = 14;
        public const UInt32 COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT = 128;
        public const UInt32 COMMONSHADER_SAMPLER_SLOT_COUNT = 16;
        public const UInt32 IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT = 32;
        public const UInt32 VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE = 16;
        public const UInt32 SIMULTANEOUS_RENDER_TARGET_COUNT = 8;

        static public ClearValue GetDXGIFormatClearValue(Format format, Boolean depth)
        {
            ClearValue clearValue = new ClearValue();

            clearValue.Color.X = 0.0f;
            clearValue.Color.Y = 0.0f;
            clearValue.Color.Z = 0.0f;
            clearValue.Color.W = 0.0f;

            switch (format)
            {
                // special handling for format conversion
                case Format.R32_Typeless:
                    format = depth ? Format.D32_Float : Format.R32_Float;
                    break;
            }

            clearValue.Format = format;
            return clearValue;
        }
    }
}
