using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;
using SharpDX.DXGI;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1SwapChain
    {
        public Int32 CurrFrameIndex
        {
            // return current back buffer frame index
            get { return m_DXGISwapChain.CurrentBackBufferIndex; }
        }     
        
        public SwapChain3 SwapChain
        {
            get { return m_DXGISwapChain; }
        }

        public H1SwapChain(H1AsyncCommandQueue asyncCommandQueue, H1CommandListPool commandListPool)
        {
            m_AsyncQueueRef = asyncCommandQueue;
            m_CommandListPoolRef = commandListPool;
        }

        public void Initialize(Int32 width, Int32 height, IntPtr outputHandle)
        {
            // provide a convinent syntax that ensures the correct use of IDisposable objects
            // only factory is enabled in this scope
            using (var factory = new Factory4())
            {
                m_Desc = new SwapChainDescription()
                {
                    BufferCount = FrameCount,
                    ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                    Usage = Usage.RenderTargetOutput,
                    OutputHandle = outputHandle,
                    SwapEffect = SwapEffect.FlipDiscard,
                    SampleDescription = new SampleDescription(1, 0), //@TODO enable 4xMSAA
                    IsWindowed = true,
                };

                using (var tempSwapChain = new SwapChain(factory, m_CommandListPoolRef.CommandQueue, m_Desc))
                // @TODO - temporary
                //CommandQueue cmdQueue = H1Global<H1ManagedRenderer>.Instance.m_CommandQueue;
                //using (var tempSwapChain = new SwapChain(factory, cmdQueue, m_Desc))
                {
                    m_DXGISwapChain = tempSwapChain.QueryInterface<SwapChain3>();
                }
            }

            // set the back buffer resources
            for (Int32 n = 0; n < FrameCount; ++n)
            {
                m_BackBuffers.Add(m_DXGISwapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(n));
            }

            // @TODO need to move this chunk of codes to SwapChain
            #region Temp
            //@TODO - temp          
            //H1DX12Device deviceDX12 = m_CommandListPoolRef.Device;
            H1DX12Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Dx12Device;

            // create RTV descriptor heap            
            deviceDX12.RenderTargetDescriptorCache.Initialize(deviceDX12.Device, FrameCount, false, H1ViewType.RenderTargetView);

            // create DSV descriptor heap            
            deviceDX12.DepthStencilDescriptorCache.Initialize(deviceDX12.Device, 1, false, H1ViewType.DepthStencilView);

            // create CBV descriptor heap
            //H1DescriptorHeap generalHeap = new H1DescriptorHeap();
            //generalHeap.Initialize(m_Device, 2, true, H1ViewType.ConstantBufferView);
            //m_GlobalDescriptorHeaps.Add(generalHeap);

            // create frame resource
            // 1. RTV
            for (Int32 n = 0; n < FrameCount; ++n)
            {
                // get available alloc cpu address (internally update cursor in H1DescriptorHeap)
                CpuDescriptorHandle rtvHandle = deviceDX12.RenderTargetDescriptorCache.GetAvailableAllocCpuAddress();
                SharpDX.Direct3D12.Resource resourceForRTV = GetBackBuffer(n);
                deviceDX12.Device.CreateRenderTargetView(resourceForRTV, null, rtvHandle);
            }

            // 2. DSV            
            DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription();
            dsvDesc.Format = SharpDX.DXGI.Format.D32_Float;
            dsvDesc.Dimension = DepthStencilViewDimension.Texture2D;
            dsvDesc.Flags = DepthStencilViewFlags.None;

            ClearValue depthOptimizedClearValue = new ClearValue();
            depthOptimizedClearValue.Format = SharpDX.DXGI.Format.D32_Float;
            depthOptimizedClearValue.DepthStencil.Depth = 1.0f;
            depthOptimizedClearValue.DepthStencil.Stencil = 0;

            m_DepthStencil = deviceDX12.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                ResourceDescription.Texture2D(SharpDX.DXGI.Format.D32_Float, width, height, 1, 0, 1, 0, ResourceFlags.AllowDepthStencil),
                ResourceStates.DepthWrite,
                depthOptimizedClearValue);

            CpuDescriptorHandle dsvHandle = deviceDX12.DepthStencilDescriptorCache.GetAvailableAllocCpuAddress();
            deviceDX12.Device.CreateDepthStencilView(m_DepthStencil, dsvDesc, dsvHandle);

            #endregion
        }

        public SharpDX.Direct3D12.Resource GetBackBuffer(Int32 index)
        {
            return m_BackBuffers[index];
        }

        #region Temp
        SharpDX.Direct3D12.Resource m_DepthStencil;
        #endregion

        // @TODO - I need to change this
        private const Int32 FrameCount = 2;        

        private H1AsyncCommandQueue m_AsyncQueueRef;
        private H1CommandListPool m_CommandListPoolRef;

        private SwapChainDescription m_Desc;
        private UInt32 m_CurrentBackBufferIndex;

        SwapChain3 m_DXGISwapChain;

        // @TODO change Resource to H1DX12Resource
        private List<SharpDX.Direct3D12.Resource> m_BackBuffers = new List<SharpDX.Direct3D12.Resource>();
        //private List<H1DX12Resource> H1DX12Resource = new List<H1DX12Resource>();
        private List<H1View> m_BackBufferRTVs = new List<H1View>();
        private List<H1View> m_BackBufferSRVs = new List<H1View>();
    }
}
