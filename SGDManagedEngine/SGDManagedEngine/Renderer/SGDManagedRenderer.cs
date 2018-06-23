using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;

// @TODO temporary
using System.IO;
using System.Reflection;

namespace SGDManagedEngine.SGD
{
    // using directX 12
    using SharpDX.Direct3D12;

    public class H1ManagedRenderer : IDisposable
    {
        public Device Device
        {
            get { return m_DeviceContext.Dx12Device.Device; }
        }

        public Direct3D12.H1DX12Device Dx12Device
        {
            get { return m_DeviceContext.Dx12Device; }
        }

        public H1GPUResourceManager ResourceManager
        {
            get { return m_ResourceManager; }
        }

        public H1ShaderManager ShaderManager
        {
            get { return m_ShaderManager; }
        }

        public Gen2Layer.H1Gen2Layer Gen2Layer
        {
            get { return m_Gen2Layer; }
        }

        #region DeviceContext // device context for Direct3D12

        // Initialize device context
        void InitializeDeviceContext()
        {
            // create & initialize device context
            m_DeviceContext = new Direct3D12.H1DX12DeviceContext();
            m_DeviceContext.Intialize();
        }

        // dx12 device context
        Direct3D12.H1DX12DeviceContext m_DeviceContext;

        #endregion

        #region SwapChain

        void InitializeSwapChain(IntPtr handle)
        {
            m_SwapChainDX12.Initialize(m_Width, m_Height, handle);
        }

        Direct3D12.H1SwapChain m_SwapChainDX12;

        #endregion

        // manager class for the renderer
        private H1GPUResourceManager m_ResourceManager;
        private H1ShaderManager m_ShaderManager;

        // @TODO - remove this
        // temp static mesh
        //private H1StaticMesh m_TempStaticMesh;
        // ...
        const int FrameCount = 2;

        private int m_Height, m_Width;
        private ViewportF m_Viewport;
        private Rectangle m_SissorRect;

        // pipeline objects
        private DescriptorHeap m_ConstantBufferViewHeap;
        private Int32 m_ConstantBufferDescriptorSize;
        // temp
        private Direct3D12.H1CommandList m_CommandList;
        private PipelineState m_PipelineState;

        private RootSignature m_RootSignature;

        // app resources
        Resource m_ConstantBuffer;

        // @TODO - temporary loading images
        H1ImageWrapper m_tempImageLoader;

        // synchronization objects
        private int m_FrameIndex;

        public class TransformationCB
        {
            public Matrix viewProjectionMatrix;
            public Matrix[] BoneMatrices = new Matrix[100];
        };

        // @TODO - I need to make this CB outside this class
        public Matrix ViewProjectionMatrix
        {
            set
            {
                m_TransformationCB.viewProjectionMatrix = value;
            }
        }

        public void SetBoneMatrix(Int32 index, Matrix boneMatrix)
        {
            m_TransformationCB.BoneMatrices[index] = boneMatrix;
        }

        private TransformationCB m_TransformationCB = new TransformationCB();
        private IntPtr m_TransformationCBPointer;

        // assessors
        public int Width
        {
            get { return m_Width; }
        }

        public int Height
        {
            get { return m_Height; }
        }

        public void CopyTextureRegion(H1Texture2D texObject, H1GeneralBuffer generalBuffer)
        {
            // reuse the memory associated with command recording
            // we can only reset when the associated command lists have finished execution on the GPU
            m_CommandList.CommandAllocator.Reset();

            // a command list can be reset after it has been added to the command queue via ExecuteCommandList
            m_CommandList.CommandList.Reset(m_CommandList.CommandAllocator, null);

            // @TODO - I need to change this into parallel copy texture region by managing multiple command queue
            H1GPUResourceManager refResourceManager = H1Global<H1ManagedRenderer>.Instance.ResourceManager;

            // create destination and source locations
            // TextureCopyLocation - describe a portion of texture for the purpose of texture copies
            TextureCopyLocation destLocation = new TextureCopyLocation(refResourceManager.GetTexture2D(Convert.ToInt32(texObject.Index)), 0);
            TextureCopyLocation srcLocation = new TextureCopyLocation(generalBuffer.Resource,
                new PlacedSubResourceFootprint() // describes the footprint of a placed subresource, including the offset and the D3D12_SUBRESOURCE_FOOTPRINT
                {
                    Offset = 0, // the offset of the subresource within the parent resource in bytes
                    Footprint = new SubResourceFootprint() // describes the format, with, height, depth and row-pitch of the subresource into the parent resource
                    {
                        Width = Convert.ToInt32(texObject.Width),
                        Height = Convert.ToInt32(texObject.Height),
                        Depth = 1,
                        Format = H1RHIDefinitionHelper.ConvertToFormat(texObject.PixelFormat),
                        RowPitch = Convert.ToInt32(texObject.Stride),
                    }
                });

            m_CommandList.CommandList.ResourceBarrierTransition(texObject.Resource, ResourceStates.GenericRead, ResourceStates.CopyDestination);
            m_CommandList.CommandList.CopyTextureRegion(destLocation, 0, 0, 0, srcLocation, null);
            m_CommandList.CommandList.ResourceBarrierTransition(texObject.Resource, ResourceStates.CopyDestination, ResourceStates.GenericRead);

            m_CommandList.CommandList.Close();

            m_DeviceContext.MainCommandListPool.CommandQueue.ExecuteCommandList(m_CommandList.CommandList);
        }        

        public void Initialize(int height, int width, IntPtr handle)
        {
            m_Height = height;
            m_Width = width;

            m_Viewport.Width = m_Width;
            m_Viewport.Height = m_Height;
            m_Viewport.MaxDepth = 1.0f;

            m_SissorRect.Right = m_Width;
            m_SissorRect.Bottom = m_Height;

#if DEBUG
            // enable the d3d12 debug layer
            DebugInterface.Get().EnableDebugLayer();
#endif            
            m_DeviceContext = new Direct3D12.H1DX12DeviceContext();
            m_DeviceContext.Intialize();

            m_SwapChainDX12 = new Direct3D12.H1SwapChain(null, m_DeviceContext.MainCommandListPool);
            m_SwapChainDX12.Initialize(m_Width, m_Height, handle);

            // create Gen2Layer 
#if SGD_DX12
            m_Gen2Layer = new Gen2Layer.H1Gen2Layer();
            m_Gen2Layer.Initialize();
#endif

            var cbvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = 2,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
            };

            m_ConstantBufferViewHeap = Device.CreateDescriptorHeap(cbvHeapDesc);
            m_ConstantBufferDescriptorSize = Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
                      
            // create resource manager
            m_ResourceManager = new H1GPUResourceManager();
            m_ResourceManager.Initialize(this); // set the owner as this renderer

            m_ShaderManager = new H1ShaderManager();
            m_ShaderManager.Initialize(this);

            // temp
            //m_TempStaticMesh = new H1StaticMesh(H1Global<H1AssimpImporter>.Instance.asset.GetModel(2));

            // load assets
            //LoadAssets();

            // setting for physics
            SettingForPhysics();
        }

        public void Destroy()
        {
#if SGD_DX12
            m_Gen2Layer.Destroy();
#endif
        }

        public void Dispose()
        {
            // wait for the GPU to be done with all resources
            WaitForPrevFrame();

            // destroy managers
            m_ResourceManager.Destroy();
            m_ShaderManager.Destroy();
        }

        public void Render()
        {
            // set the thread local
            Thread.H1ThreadGlobal.ThreadContext.RendererContext.CurrCommandList = m_CommandList;

            // record all the commands we need to render the scene into the command list
            //PopulateCommandLists();
            //PopulateCommandListsForSkeletalMesh();
            PopulateCommandListsForVisualDebugDrawing();

            // reset the current command list
            Thread.H1ThreadGlobal.ThreadContext.RendererContext.CurrCommandList = null;

            // execute the command list
            if (m_CommandList != null)
            {
                m_DeviceContext.MainCommandListPool.CommandQueue.ExecuteCommandList(m_CommandList.CommandList);
            }            

            // swap the back and front buffers
            m_SwapChainDX12.SwapChain.Present(1, 0);

            // wait and reset Everything
            WaitForPrevFrame();

            // reset visual debugger 
            H1Global<H1VisualDebugger>.Instance.ClearAllMeshBuilders();
        }

        public void SettingForPhysics()
        {
            //@TODO - temporary sampler
            StaticSamplerDescription pointClamp = new StaticSamplerDescription(ShaderVisibility.Pixel, 0, 0);
            pointClamp.Filter = Filter.ComparisonMinLinearMagMipPoint;
            pointClamp.AddressU = TextureAddressMode.Clamp;
            pointClamp.AddressV = TextureAddressMode.Clamp;
            pointClamp.AddressW = TextureAddressMode.Clamp;

            StaticSamplerDescription[] staticSamArray = new[] { pointClamp };

            // create an empty root signature
            var rootSignatureDesc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout,
                // root parameters
                new[]
                {
                    new RootParameter(ShaderVisibility.Vertex,
                    new DescriptorRange()
                    {
                        RangeType = DescriptorRangeType.ConstantBufferView,
                        BaseShaderRegister = 0,
                        OffsetInDescriptorsFromTableStart = 0,
                        DescriptorCount = 1
                    }),
                    new RootParameter(ShaderVisibility.Pixel,
                    new DescriptorRange()
                    {
                        RangeType = DescriptorRangeType.ShaderResourceView,
                        BaseShaderRegister = 0,
                        OffsetInDescriptorsFromTableStart = 0,
                        DescriptorCount = 1
                    })
                }
                , staticSamArray
                );

            m_RootSignature = Device.CreateRootSignature(rootSignatureDesc.Serialize());

            // create the pipeline state, which includes compiling and loading shader
            H1VertexFactoryType vertexFactoryType = ShaderManager.GetVertexFactoryType("H1LocalVertexFactory");
            //H1VertexFactoryType vertexFactoryType = ShaderManager.GetVertexFactoryType("H1GpuSkinVertexFactory");
#if DEBUG
            //var vertexShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shader.hlsl", "VSMain", "vs_5_1", SharpDX.D3DCompiler.ShaderFlags.Debug));            
            var vertexShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassVertexShader", vertexFactoryType).ShaderByteCode;
#else
            var vertexShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassVertexShader", vertexFactoryType).ShaderByteCode;
#endif

#if DEBUG
            //var pixelShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shader.hlsl", "PSMain", "ps_5_1", SharpDX.D3DCompiler.ShaderFlags.Debug));
            var pixelShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassPixelShader", vertexFactoryType).ShaderByteCode;
#else
            var pixelShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassPixelShader", vertexFactoryType).ShaderByteCode;
#endif

            // set the seperate rasterizerstatedesc
            RasterizerStateDescription rasterizerStateDesc = RasterizerStateDescription.Default();
            rasterizerStateDesc.FillMode = FillMode.Wireframe;
            rasterizerStateDesc.CullMode = CullMode.None;
            //rasterizerStateDesc.CullMode = CullMode.Front; //@TODO - what the fuck?!... need to solve this urgently~******
            //rasterizerStateDesc.FillMode = FillMode.Solid;

            Vector3 position = new Vector3();
            Vector3 size = new Vector3(1, 1, 1);

            H1RenderUtils.H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            {
                meshBuilder.AddLine(new Vector4(position + size * new Vector3(1, 0, 0), 1), new Vector4(position - size * new Vector3(1, 0, 0), 1), new Vector4(1));
                meshBuilder.AddLine(new Vector4(position + size * new Vector3(0, 1, 0), 1), new Vector4(position - size * new Vector3(0, 1, 0), 1), new Vector4(1));
                meshBuilder.AddLine(new Vector4(position + size * new Vector3(0, 0, 1), 1), new Vector4(position - size * new Vector3(0, 0, 1), 1), new Vector4(1));
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            // describe and create the graphics pipeline state object (PSO)
            var psoDesc = new GraphicsPipelineStateDescription()
            {
                InputLayout = meshBuilder.VertexFactory.VertexDeclaration.InputLayout.Description,
                RootSignature = m_RootSignature,
                VertexShader = vertexShader,
                PixelShader = pixelShader,
                RasterizerState = rasterizerStateDesc,
                BlendState = BlendStateDescription.Default(),
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                DepthStencilState = DepthStencilStateDescription.Default(),
                //DepthStencilState = new DepthStencilStateDescription() { IsDepthEnabled = false, IsStencilEnabled = false },
                SampleMask = int.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RenderTargetCount = 1,
                Flags = PipelineStateFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                StreamOutput = new StreamOutputDescription()
            };

            psoDesc.RenderTargetFormats[0] = SharpDX.DXGI.Format.R8G8B8A8_UNorm;

            m_PipelineState = Device.CreateGraphicsPipelineState(psoDesc);

            // create the command list
            m_CommandList = new Direct3D12.H1CommandList(m_DeviceContext.Dx12Device, m_DeviceContext.MainCommandListPool);
            m_CommandList.Initialize();

            // create the vertex buffer
            float aspectRatio = m_Viewport.Width / m_Viewport.Height;

            m_ConstantBuffer = Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer(1024 * 64), ResourceStates.GenericRead);

            var cbvDesc = new ConstantBufferViewDescription
            {
                BufferLocation = m_ConstantBuffer.GPUVirtualAddress,
                SizeInBytes = 256 * 256//(Utilities.SizeOf<TransformationCB>() + 255) & ~255
            };
            Device.CreateConstantBufferView(cbvDesc, m_ConstantBufferViewHeap.CPUDescriptorHandleForHeapStart);
        }

        public void LoadAssets()
        {
            //@TODO - temporary sampler
            StaticSamplerDescription pointClamp = new StaticSamplerDescription(ShaderVisibility.Pixel, 0, 0);
            pointClamp.Filter = Filter.ComparisonMinLinearMagMipPoint;
            pointClamp.AddressU = TextureAddressMode.Clamp;
            pointClamp.AddressV = TextureAddressMode.Clamp;
            pointClamp.AddressW = TextureAddressMode.Clamp;

            StaticSamplerDescription[] staticSamArray = new[] { pointClamp };

            // create an empty root signature
            var rootSignatureDesc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout,
                // root parameters
                new[]
                {
                    new RootParameter(ShaderVisibility.Vertex,
                    new DescriptorRange()
                    {
                        RangeType = DescriptorRangeType.ConstantBufferView,
                        BaseShaderRegister = 0,
                        OffsetInDescriptorsFromTableStart = 0,
                        DescriptorCount = 1
                    }),
                    new RootParameter(ShaderVisibility.Pixel,
                    new DescriptorRange()
                    {
                        RangeType = DescriptorRangeType.ShaderResourceView,
                        BaseShaderRegister = 0,
                        OffsetInDescriptorsFromTableStart = 0,
                        DescriptorCount = 1
                    })
                }
                , staticSamArray
                );

            m_RootSignature = Device.CreateRootSignature(rootSignatureDesc.Serialize());

            // create the pipeline state, which includes compiling and loading shader
            //H1VertexFactoryType vertexFactoryType = ShaderManager.GetVertexFactoryType("H1LocalVertexFactory");
            H1VertexFactoryType vertexFactoryType = ShaderManager.GetVertexFactoryType("H1GpuSkinVertexFactory");
#if DEBUG
            //var vertexShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shader.hlsl", "VSMain", "vs_5_1", SharpDX.D3DCompiler.ShaderFlags.Debug));            
            var vertexShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassVertexShader", vertexFactoryType).ShaderByteCode;
#else
            var vertexShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassVertexShader", vertexFactoryType).ShaderByteCode;
#endif

#if DEBUG
            //var pixelShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shader.hlsl", "PSMain", "ps_5_1", SharpDX.D3DCompiler.ShaderFlags.Debug));
            var pixelShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassPixelShader", vertexFactoryType).ShaderByteCode;
#else
            var pixelShader = H1Material.DefaultMaterial.MaterialResource.GetShader("H1BasePassPixelShader", vertexFactoryType).ShaderByteCode;
#endif

            // set the seperate rasterizerstatedesc
            RasterizerStateDescription rasterizerStateDesc = RasterizerStateDescription.Default();
            //rasterizerStateDesc.FillMode = FillMode.Wireframe;
            //rasterizerStateDesc.CullMode = CullMode.None;
            rasterizerStateDesc.CullMode = CullMode.Front; //@TODO - what the fuck?!... need to solve this urgently~******
            rasterizerStateDesc.FillMode = FillMode.Solid;

            //H1StaticMeshLODResource resource = m_TempStaticMesh.StaticMeshData.GetLODResource(0);
            //H1StaticMeshLODResource resource = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1StaticMeshComponent>().StaticMesh.StaticMeshData.GetLODResource(1);
            H1SkeletalMeshObjectGPUSkin skeletalMeshObject = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1SkeletalMeshComponent>().SkeletalMeshObjectGPUSkin;

            // describe and create the graphics pipeline state object (PSO)
            var psoDesc = new GraphicsPipelineStateDescription()
            {
                //InputLayout = resource.LocalVertexFactory.VertexDeclaration.InputLayout,
                InputLayout = ((Gen2Layer.H1InputLayout)skeletalMeshObject.GetSkeletalMeshObjectLODByIndex(0).GPUSkinVertexFactories.VertexFactories[0].VertexDeclaration.InputLayout).Description, 
                RootSignature = m_RootSignature,
                VertexShader = vertexShader,                
                PixelShader = pixelShader,
                RasterizerState = rasterizerStateDesc,
                BlendState = BlendStateDescription.Default(),
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                DepthStencilState = DepthStencilStateDescription.Default(),
                //DepthStencilState = new DepthStencilStateDescription() { IsDepthEnabled = false, IsStencilEnabled = false },
                SampleMask = int.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RenderTargetCount = 1,
                Flags = PipelineStateFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                StreamOutput = new StreamOutputDescription()
            };

            psoDesc.RenderTargetFormats[0] = SharpDX.DXGI.Format.R8G8B8A8_UNorm;

            m_PipelineState = Device.CreateGraphicsPipelineState(psoDesc);

            // create the command list
            m_CommandList = new Direct3D12.H1CommandList(m_DeviceContext.Dx12Device, m_DeviceContext.MainCommandListPool);
            m_CommandList.Initialize();

            // create the vertex buffer
            float aspectRatio = m_Viewport.Width / m_Viewport.Height;

            m_ConstantBuffer = Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer(1024 * 64), ResourceStates.GenericRead);

            var cbvDesc = new ConstantBufferViewDescription
            {
                BufferLocation = m_ConstantBuffer.GPUVirtualAddress,
                SizeInBytes = 256 * 256//(Utilities.SizeOf<TransformationCB>() + 255) & ~255
            };
            Device.CreateConstantBufferView(cbvDesc, m_ConstantBufferViewHeap.CPUDescriptorHandleForHeapStart);

            m_TransformationCBPointer = m_ConstantBuffer.Map(0);
            //Utilities.Write(m_TransformationCBPointer, ref m_TransformationCB);
            List<Matrix> dataToCopy = new List<Matrix>();
            dataToCopy.Add(m_TransformationCB.viewProjectionMatrix);
            foreach (Matrix mtx in m_TransformationCB.BoneMatrices)
                dataToCopy.Add(mtx);
            Utilities.Write(m_TransformationCBPointer, dataToCopy.ToArray(), 0, 101);
            m_ConstantBuffer.Unmap(0);

            // @TODO - temporary so need to delete
            String path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets\\");
            m_tempImageLoader = new H1ImageWrapper(path + "alduin.JPG");

            //CpuDescriptorHandle hDescriptor = m_srvDescriptorHeap.CPUDescriptorHandleForHeapStart;
            CpuDescriptorHandle hDescriptor = m_ConstantBufferViewHeap.CPUDescriptorHandleForHeapStart;
            hDescriptor += m_ConstantBufferDescriptorSize;
            //Int32 sizeInBytes = (Utilities.SizeOf<TransformationCB>() + 255) & ~255;
            //hDescriptor += sizeInBytes;

            ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription()
            {
                Shader4ComponentMapping = 5768, //@TODO - temporary!
                Format = m_tempImageLoader.m_tempTextureObject.Resource.Description.Format,
                Dimension = ShaderResourceViewDimension.Texture2D
            };
            srvDesc.Texture2D.MostDetailedMip = 0;
            srvDesc.Texture2D.MipLevels = 1;// m_tempImageLoader.m_tempTextureObject.Resource.Description.MipLevels;
            srvDesc.Texture2D.ResourceMinLODClamp = 0.0f;

            Device.CreateShaderResourceView(m_tempImageLoader.m_tempTextureObject.Resource, srvDesc, hDescriptor);
        }

        private void PopulateCommandLists()
        {
            m_CommandList.CommandAllocator.Reset();
            //m_DeviceContext.MainCommandListPool

            m_CommandList.CommandList.Reset(m_CommandList.CommandAllocator, m_PipelineState);

            // set viewport and sissors
            m_CommandList.CommandList.SetGraphicsRootSignature(m_RootSignature);

            // @TODO - redesign this section
            m_TransformationCBPointer = m_ConstantBuffer.Map(0);
            //Utilities.Write(m_TransformationCBPointer, ref m_TransformationCB);
            List<Matrix> dataToCopy = new List<Matrix>();
            dataToCopy.Add(m_TransformationCB.viewProjectionMatrix);
            foreach (Matrix mtx in m_TransformationCB.BoneMatrices)
                dataToCopy.Add(mtx);
            Utilities.Write(m_TransformationCBPointer, dataToCopy.ToArray(), 0, 101);
            m_ConstantBuffer.Unmap(0);

            m_CommandList.CommandList.SetDescriptorHeaps(1, new DescriptorHeap[] { m_ConstantBufferViewHeap });
            //m_CommandList.SetDescriptorHeaps(2, new DescriptorHeap[] { m_ConstantBufferViewHeap, m_srvDescriptorHeap });

            GpuDescriptorHandle hDescriptor = m_ConstantBufferViewHeap.GPUDescriptorHandleForHeapStart;            
            m_CommandList.CommandList.SetGraphicsRootDescriptorTable(0, hDescriptor);

            hDescriptor += m_ConstantBufferDescriptorSize;
            //Int32 sizeInBytes = (Utilities.SizeOf<TransformationCB>() + 255) & ~255;
            //hDescriptor += sizeInBytes;

            m_CommandList.CommandList.SetGraphicsRootDescriptorTable(1, hDescriptor);

            m_CommandList.CommandList.SetViewport(m_Viewport);
            m_CommandList.CommandList.SetScissorRectangles(m_SissorRect);

            // use barrier to notify we are using the m_RenderTarget
            Resource rtvBackBuffer = m_SwapChainDX12.GetBackBuffer(m_FrameIndex);
            m_CommandList.CommandList.ResourceBarrierTransition(rtvBackBuffer, ResourceStates.Present, ResourceStates.RenderTarget);

            //CpuDescriptorHandle rtvHandle = m_RenderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            //rtvHandle += m_FrameIndex * m_RTVDescriptorSize;
            //CpuDescriptorHandle dsvHandle = m_DepthStencilViewHeap.CPUDescriptorHandleForHeapStart;

            CpuDescriptorHandle rtvHandle = m_DeviceContext.Dx12Device.RenderTargetDescriptorCache.GetCpuAddressByOffset(m_FrameIndex);
            // @TODO - need to set the depth value consistent with frame counts!
            CpuDescriptorHandle dsvHandle = m_DeviceContext.Dx12Device.DepthStencilDescriptorCache.GetCpuAddressByOffset(0);

            m_CommandList.CommandList.SetRenderTargets(rtvHandle, dsvHandle);

            // clear the render target & depth stencil
            m_CommandList.CommandList.ClearRenderTargetView(rtvHandle, new Color4(0, 0.2f, 0.4f, 1), 0, null);
            m_CommandList.CommandList.ClearDepthStencilView(dsvHandle, ClearFlags.FlagsDepth, 1.0f, 0, 0, null);

            // record commands
            m_CommandList.CommandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            //m_CommandList.SetVertexBuffer(0, m_VertexBufferView);
            // @TODO - I need to wrap command list separately!
            //H1StaticMeshLODResource resource = m_TempStaticMesh.StaticMeshData.GetLODResource(0);
            H1StaticMeshLODResource resource = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1StaticMeshComponent>().StaticMesh.StaticMeshData.GetLODResource(1);
            resource.LocalVertexFactory.setVertexBuffers(m_CommandList.CommandList);

            //m_CommandList.DrawInstanced(3, 1, 0, 0);
            //m_CommandList.SetIndexBuffer(m_IndexBufferView);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)resource.IndexBuffer.View).View;
            m_CommandList.CommandList.SetIndexBuffer(rawIBV);

            int instanceCount = Convert.ToInt32(resource.IndexBuffer.Count);
            m_CommandList.CommandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);

            // use barrier to notify that we are going to present the render target            
            m_CommandList.CommandList.ResourceBarrierTransition(rtvBackBuffer, ResourceStates.RenderTarget, ResourceStates.Present);

            // execute the command
            m_CommandList.CommandList.Close();
        }

        private void PopulateCommandListsForSkeletalMesh()
        {
            m_CommandList.CommandAllocator.Reset();
            //m_DeviceContext.MainCommandListPool

            m_CommandList.CommandList.Reset(m_CommandList.CommandAllocator, m_PipelineState);

            // set viewport and sissors
            m_CommandList.CommandList.SetGraphicsRootSignature(m_RootSignature);

            // @TODO - redesign this section
            // extract and update bone matrix
            H1SkeletalMeshComponent skeletalMeshComponent = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1SkeletalMeshComponent>();
            skeletalMeshComponent.SkeletalMesh.SkeletonDebugDrawing(m_CommandList.CommandList, false);

            m_TransformationCBPointer = m_ConstantBuffer.Map(0);
            //Utilities.Write(m_TransformationCBPointer, ref m_TransformationCB);
            List<Matrix> dataToCopy = new List<Matrix>();
            dataToCopy.Add(m_TransformationCB.viewProjectionMatrix);
            foreach (Matrix mtx in m_TransformationCB.BoneMatrices)
                dataToCopy.Add(mtx);
            Utilities.Write(m_TransformationCBPointer, dataToCopy.ToArray(), 0, 101);
            m_ConstantBuffer.Unmap(0);

            m_CommandList.CommandList.SetDescriptorHeaps(1, new DescriptorHeap[] { m_ConstantBufferViewHeap });
            //m_CommandList.SetDescriptorHeaps(2, new DescriptorHeap[] { m_ConstantBufferViewHeap, m_srvDescriptorHeap });

            GpuDescriptorHandle hDescriptor = m_ConstantBufferViewHeap.GPUDescriptorHandleForHeapStart;
            m_CommandList.CommandList.SetGraphicsRootDescriptorTable(0, hDescriptor);

            hDescriptor += m_ConstantBufferDescriptorSize;
            //Int32 sizeInBytes = (Utilities.SizeOf<TransformationCB>() + 255) & ~255;
            //hDescriptor += sizeInBytes;

            m_CommandList.CommandList.SetGraphicsRootDescriptorTable(1, hDescriptor);

            m_CommandList.CommandList.SetViewport(m_Viewport);
            m_CommandList.CommandList.SetScissorRectangles(m_SissorRect);

            // use barrier to notify we are using the m_RenderTarget
            Resource rtvBackBuffer = m_SwapChainDX12.GetBackBuffer(m_FrameIndex);
            m_CommandList.CommandList.ResourceBarrierTransition(rtvBackBuffer, ResourceStates.Present, ResourceStates.RenderTarget);

            //CpuDescriptorHandle rtvHandle = m_RenderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            //rtvHandle += m_FrameIndex * m_RTVDescriptorSize;
            //CpuDescriptorHandle dsvHandle = m_DepthStencilViewHeap.CPUDescriptorHandleForHeapStart;

            CpuDescriptorHandle rtvHandle = m_DeviceContext.Dx12Device.RenderTargetDescriptorCache.GetCpuAddressByOffset(m_FrameIndex);
            // @TODO - need to set the depth value consistent with frame counts!
            CpuDescriptorHandle dsvHandle = m_DeviceContext.Dx12Device.DepthStencilDescriptorCache.GetCpuAddressByOffset(0);

            m_CommandList.CommandList.SetRenderTargets(rtvHandle, dsvHandle);

            // clear the render target & depth stencil
            m_CommandList.CommandList.ClearRenderTargetView(rtvHandle, new Color4(0, 0.2f, 0.4f, 1), 0, null);
            m_CommandList.CommandList.ClearDepthStencilView(dsvHandle, ClearFlags.FlagsDepth, 1.0f, 0, 0, null);

            // record commands
            m_CommandList.CommandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            H1SkeletalMeshObjectGPUSkin skeletalMeshObject = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1SkeletalMeshComponent>().SkeletalMeshObjectGPUSkin;
            H1SkeletalMeshObjectGPUSkin.H1SkeletalMeshObjectLOD skeletalMeshObjectLOD_0 = skeletalMeshObject.GetSkeletalMeshObjectLODByIndex(0);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)skeletalMeshObjectLOD_0.IndexBuffer.View).View;
            m_CommandList.CommandList.SetIndexBuffer(rawIBV);

            H1SkeletalMesh skeletalMesh = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1SkeletalMeshComponent>().SkeletalMesh;
            List<H1SkelMeshSection> skelSections = skeletalMesh.SkeletalMeshResource.GetLODModel(0).Sections;

            Int32 sectionIndex = 0;
            Int32 currIndexBufferOffset = 0;
            Int32 totalCount = skeletalMeshObjectLOD_0.GPUSkinVertexFactories.VertexFactories.Count;
            for (Int32 vertexfactoryIndex = 0; vertexfactoryIndex < totalCount; ++vertexfactoryIndex)
            {
                var vertexfactory = skeletalMeshObjectLOD_0.GPUSkinVertexFactories.VertexFactories[vertexfactoryIndex];
                vertexfactory.setVertexBuffers(m_CommandList.CommandList);

                H1SkelMeshSection currSkelMeshSection = skelSections[sectionIndex];
                Int32 indexCount = Convert.ToInt32(currSkelMeshSection.NumTriangles) * 3;
                
                m_CommandList.CommandList.DrawIndexedInstanced(indexCount, 1, currIndexBufferOffset, 0, 0);

                currIndexBufferOffset += indexCount;
                sectionIndex++;
            }

            // use barrier to notify that we are going to present the render target            
            m_CommandList.CommandList.ResourceBarrierTransition(rtvBackBuffer, ResourceStates.RenderTarget, ResourceStates.Present);

            // execute the command
            m_CommandList.CommandList.Close();
        }

        private void PopulateCommandListsForVisualDebugDrawing()
        {
            m_CommandList.CommandAllocator.Reset();

            m_CommandList.CommandList.Reset(m_CommandList.CommandAllocator, m_PipelineState);

            // set viewport and scissors
            m_CommandList.CommandList.SetGraphicsRootSignature(m_RootSignature);

            // @TODO - redesign this section
            m_TransformationCBPointer = m_ConstantBuffer.Map(0);
            //Utilities.Write(m_TransformationCBPointer, ref m_TransformationCB);
            List<Matrix> dataToCopy = new List<Matrix>();
            dataToCopy.Add(m_TransformationCB.viewProjectionMatrix);
            foreach (Matrix mtx in m_TransformationCB.BoneMatrices)
                dataToCopy.Add(mtx);
            Utilities.Write(m_TransformationCBPointer, dataToCopy.ToArray(), 0, dataToCopy.Count);
            m_ConstantBuffer.Unmap(0);

            m_CommandList.CommandList.SetDescriptorHeaps(1, new DescriptorHeap[] { m_ConstantBufferViewHeap });
            //m_CommandList.SetDescriptorHeaps(2, new DescriptorHeap[] { m_ConstantBufferViewHeap, m_srvDescriptorHeap });

            GpuDescriptorHandle hDescriptor = m_ConstantBufferViewHeap.GPUDescriptorHandleForHeapStart;
            m_CommandList.CommandList.SetGraphicsRootDescriptorTable(0, hDescriptor);

            hDescriptor += m_ConstantBufferDescriptorSize;
            //Int32 sizeInBytes = (Utilities.SizeOf<TransformationCB>() + 255) & ~255;
            //hDescriptor += sizeInBytes;

            m_CommandList.CommandList.SetGraphicsRootDescriptorTable(1, hDescriptor);

            m_CommandList.CommandList.SetViewport(m_Viewport);
            m_CommandList.CommandList.SetScissorRectangles(m_SissorRect);

            // use barrier to notify we are using the m_RenderTarget
            Resource rtvBackBuffer = m_SwapChainDX12.GetBackBuffer(m_FrameIndex);
            m_CommandList.CommandList.ResourceBarrierTransition(rtvBackBuffer, ResourceStates.Present, ResourceStates.RenderTarget);

            //CpuDescriptorHandle rtvHandle = m_RenderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            //rtvHandle += m_FrameIndex * m_RTVDescriptorSize;
            //CpuDescriptorHandle dsvHandle = m_DepthStencilViewHeap.CPUDescriptorHandleForHeapStart;

            CpuDescriptorHandle rtvHandle = m_DeviceContext.Dx12Device.RenderTargetDescriptorCache.GetCpuAddressByOffset(m_FrameIndex);
            // @TODO - need to set the depth value consistent with frame counts!
            CpuDescriptorHandle dsvHandle = m_DeviceContext.Dx12Device.DepthStencilDescriptorCache.GetCpuAddressByOffset(0);

            m_CommandList.CommandList.SetRenderTargets(rtvHandle, dsvHandle);

            // clear the render target & depth stencil
            m_CommandList.CommandList.ClearRenderTargetView(rtvHandle, new Color4(0, 0.2f, 0.4f, 1), 0, null);
            m_CommandList.CommandList.ClearDepthStencilView(dsvHandle, ClearFlags.FlagsDepth, 1.0f, 0, 0, null);

            // record commands
            //m_CommandList.CommandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            //m_CommandList.SetVertexBuffer(0, m_VertexBufferView);
            // @TODO - I need to wrap command list separately!
            //H1StaticMeshLODResource resource = m_TempStaticMesh.StaticMeshData.GetLODResource(0);
            //H1StaticMeshLODResource resource = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1StaticMeshComponent>().StaticMesh.StaticMeshData.GetLODResource(1);
            //resource.LocalVertexFactory.setVertexBuffers(m_CommandList.CommandList);

            //m_CommandList.DrawInstanced(3, 1, 0, 0);
            //m_CommandList.SetIndexBuffer(m_IndexBufferView);
            //m_CommandList.CommandList.SetIndexBuffer(resource.IndexBuffer.View);

            //int instanceCount = Convert.ToInt32(resource.IndexBuffer.Count);
            //m_CommandList.CommandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);

            //H1RenderUtils.DrawBox(m_CommandList.CommandList, new H1Transform());
            //H1RenderUtils.DrawPlane10x10(m_CommandList.CommandList, new Vector2(0, 0), new Vector2(1, 1));
            //H1RenderUtils.DrawSphere(m_CommandList.CommandList, new Vector3(0, 0, 0), new Vector3(1), 8, 6);
            //AngleSingle angle1 = new AngleSingle(10.0f, AngleType.Degree);
            //H1RenderUtils.DrawCone(m_CommandList.CommandList, angle1.Radians, angle1.Radians, 24, false, new Vector4(1));
            //H1RenderUtils.DrawCylinder(m_CommandList.CommandList, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), 1, 3, 10);
            //H1RenderUtils.DrawCapsule(m_CommandList.CommandList, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), 1, 3, 10);
            //H1RenderUtils.DrawDisc(m_CommandList.CommandList, new Vector3(0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 1, 10);
            //H1RenderUtils.DrawFlatArrow(m_CommandList.CommandList, new Vector3(0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 10, 1, 0.1f);
            //H1RenderUtils.DrawWireBox(m_CommandList.CommandList, Matrix.Identity, new Vector3(-1, -1, -1), new Vector3(1), 1, 0, 0);
            //H1RenderUtils.DrawWireSphere(m_CommandList.CommandList, new Vector3(0), 2.0f, 24);
            //H1RenderUtils.DrawWireCylinder(m_CommandList.CommandList, new Vector3(0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), 1.0f, 3, 10);
            //H1RenderUtils.DrawWireCapsule(m_CommandList.CommandList, new Vector3(0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), 1.0f, 3, 10);
            //H1RenderUtils.DrawWireCone(m_CommandList.CommandList, Matrix.Identity, 1.0f, 15.0f, 10);
            //H1RenderUtils.DrawWireSphereCappedCone(m_CommandList.CommandList, Matrix.Identity, 1.0f, 15.0f, 16, 4, 10);
            //H1RenderUtils.DrawOrientedWireBox(m_CommandList.CommandList, new Vector3(0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(2));
            //H1RenderUtils.DrawCoordinateSystem(m_CommandList.CommandList, new Vector3(0), Matrix.Identity, 2.0f);
            //H1RenderUtils.DrawDirectionalArrow(m_CommandList.CommandList, Matrix.Identity, 2.0f, 0.2f);
            //H1RenderUtils.DrawWireStar(m_CommandList.CommandList, new Vector3(0), 1.0f);
            //H1RenderUtils.DrawDashedLine(m_CommandList.CommandList, new Vector3(-1), new Vector3(1), 0.2f);
            //H1RenderUtils.DrawWireDiamond(m_CommandList.CommandList, Matrix.Identity, 0.2f);

            //H1SkeletalMeshComponent skeletalMeshComponent = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1SkeletalMeshComponent>();
            //skeletalMeshComponent.SkeletalMesh.SkeletonDebugDrawing(m_CommandList.CommandList);

            // execute commands
            H1Global<Commands.H1CommandManager>.Instance.ExecuteCommands();

            // use barrier to notify that we are going to present the render target            
            m_CommandList.CommandList.ResourceBarrierTransition(rtvBackBuffer, ResourceStates.RenderTarget, ResourceStates.Present);

            // execute the command
            m_CommandList.CommandList.Close();
        }

        private void WaitForPrevFrame()
        {
            // waiting for the frame to complete before continuing is not best practice
            // this is code implemented as such for simplicity
            Direct3D12.H1CommandListFenceSet cmdFenceSet = m_DeviceContext.CommandListFenceSet;
            Int32 id = Convert.ToInt32(Direct3D12.H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS);
            Int64 currFenceValue = cmdFenceSet.GetCurrentValue(id);
            Fence currFence = cmdFenceSet.GetFence(id);

            // get command queue to signal currFenceValue
            m_DeviceContext.MainCommandListPool.CommandQueue.Signal(currFence, currFenceValue);

            if (currFence.CompletedValue < currFenceValue)
            {
                cmdFenceSet.WaitForFence(currFenceValue, id);
            }

            // update fence value
            cmdFenceSet.SetCurrentValue(currFenceValue + 1, id);         
            // update back buffer index
            m_FrameIndex = m_SwapChainDX12.SwapChain.CurrentBackBufferIndex;
        }

        // @TODO - I need to move this part to new created RHIDynamicLayer class (in the future)
        #region resource managements
        public H1VertexBuffer CreateVertexBuffer(uint bufferSize, uint strideSize)
        {
            int index = m_ResourceManager.CreateVertexBuffer(bufferSize);
            if (index == -1) // if it failed to create the vertex buffer, return null
                return null;
            return new H1VertexBuffer(m_ResourceManager, Convert.ToUInt32(index), bufferSize, strideSize); // return the created vertex buffer
        }

        public H1IndexBuffer CreateIndexBuffer(uint bufferSize)
        {
            int index = m_ResourceManager.CreateIndexBuffer(bufferSize);
            if (index == -1) // if it failed to create the index buffer, return null
                return null;
            return new H1IndexBuffer(m_ResourceManager, Convert.ToUInt32(index), bufferSize); // return the created index buffer
        }

        public H1GeneralBuffer CreateGeneralBuffer(uint bufferSize)
        {
            int index = m_ResourceManager.CreateGeneralBuffer(bufferSize);
            if (index == -1)
                return null;
            return new H1GeneralBuffer(m_ResourceManager, Convert.ToUInt32(index), bufferSize);
        }

        public H1Texture2D CreateTexture2D(H1PixelFormat elementType, int width, int height, Vector4 clearValue, H1SubresourceData[] initialData)
        {
            int index = m_ResourceManager.CreateTexture2D(elementType, width, height, clearValue, initialData);
            if (index == -1) // if it failed to create the texture2D, return null
                return null;
            return new H1Texture2D(m_ResourceManager, Convert.ToUInt32(index), elementType, width, height);
        }

        #endregion

        //---------------------------------------------------------------------
        // Gen2Layer

        private Gen2Layer.H1Gen2Layer m_Gen2Layer = null;

        private Gen2Layer.H1CommandListManager m_CommandListManager = new Gen2Layer.H1CommandListManager();
    }
}

