using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    class H1ManagedEngine
    {
        public H1ManagedEngine()
        {
            
        }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            m_WindowHandle = windowHandle;
         
            //@TODO - re-ordering this correctly!
                                
            // renderer
            m_Renderer = H1Global<H1ManagedRenderer>.Instance;
            m_Renderer.Initialize(width, height, windowHandle);

            // asset importer (Assimp)
            m_AssimpImporter = H1Global<H1AssimpImporter>.Instance;
            m_AssimpImporter.Initialize();

            // world system
            m_World = H1Global<H1World>.Instance;
            Int32 lvIndex = m_World.AddLevel(new H1Level());
            // set persistent level
            H1Level persistentLevel = m_World.GetLevel(lvIndex);
            m_World.PersistentLevel = persistentLevel;

            H1AssetContext AssetContext = H1Global<H1AssimpImporter>.Instance.asset;
            if (AssetContext != null)
            {
                H1ModelContext ModelContext = H1Global<H1AssimpImporter>.Instance.asset.GetModel(0);

                // create temporary actor
                H1Actor testActor = new H1Actor();
                H1StaticMeshComponent staticMeshComponent = new H1StaticMeshComponent();
                staticMeshComponent.StaticMesh = new H1StaticMesh(ModelContext);
                testActor.AddActorComponent<H1StaticMeshComponent>(staticMeshComponent);

                // create temporary skeletal mesh component
                H1SkeletalMeshComponent skeletalMeshComponent = new H1SkeletalMeshComponent();
                skeletalMeshComponent.SkeletalMesh = new H1SkeletalMesh();
                H1StaticLODModel staticLODModelRef = skeletalMeshComponent.SkeletalMesh.PrepareProcessAssetContext(ModelContext.SkeletalContexts[0]);
                skeletalMeshComponent.SkeletalMesh.ProcessAssetContext(staticLODModelRef, ModelContext.Meshes.ToArray(), ModelContext.SkeletalContexts[0]);
                skeletalMeshComponent.AnimScriptInstance.ProcessAnimationContext(ModelContext.AnimationContext);

                // generate skeletalmeshobject
                skeletalMeshComponent.GenerateSkeleltalMeshObjectGpuSkin();

                testActor.AddActorComponent<H1SkeletalMeshComponent>(skeletalMeshComponent);

                // add the actor to the world
                m_World.PersistentLevel.AddActor(testActor);

                //@TODO - temporary force to order to working
                // after load assets
                m_Renderer.LoadAssets();
            }

            // @TODO - make the global accessor to access the current running app class!
            m_WPFInputManager = H1Global<H1InputManagerWpf>.Instance;

            H1InteractionContext<Window> context = new H1InteractionContext<Window>(Application.Current.MainWindow);
            m_WPFInputManager.Initialize(context);

            // initialize the camera
            m_Camera = new H1Camera();

            // set the camera properties
            Vector3 eye = new Vector3(0.0f, 0.0f, -10.0f);
            Vector3 lookAtPoint = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 upVector = new Vector3(0.0f, 1.0f, 0.0f);
            float fov = Convert.ToSingle((Math.PI / 180.0) * 45.0);
            float nearZ = 1.0f;
            float farZ = 10000.0f;
            float focusRadius = 1.0f;
            float aspectRatio = m_Renderer.Width / m_Renderer.Height;

            // set the camera state
            m_Camera.SetState(H1ViewTypes.Perspective, eye, lookAtPoint, upVector, fov, aspectRatio, nearZ, farZ, focusRadius);

            // camera controller
            m_CameraController = new H1CameraController();
            m_CameraController.Camera = m_Camera;

            m_VisualDebugger = H1Global<H1VisualDebugger>.Instance;

            // task scheduler (fiber-based) c++
            SGDManagedEngineWrapper.H1ManagedTaskSchedulerLayerWrapper.InitializeTaskScheduler();
            SGDManagedEngineWrapper.H1ManagedTaskSchedulerLayerWrapper.StartTaskScheduler();
        }

        public void Destroy()
        {
            m_Renderer.Destroy();
        }

        public void Render()
        {            
            m_Renderer.Render();       
        }

        // task for signaling quit all threads
        public void TaskSchedulerSignalQuitAll(SGDManagedEngineWrapper.H1ManagedTaskData taskData)
        {
            SGDManagedEngineWrapper.H1ManagedTaskSchedulerLayerWrapper.SignalQuitAll();
        }

        public void Exit()
        {
            SGDManagedEngineWrapper.H1ManagedTaskDeclaration task = new SGDManagedEngineWrapper.H1ManagedTaskDeclaration();
            task.TaskData = null;
            task.TaskEntryPoint = TaskSchedulerSignalQuitAll;

            SGDManagedEngineWrapper.H1ManagedTaskCounterWrapper taskCounter = new SGDManagedEngineWrapper.H1ManagedTaskCounterWrapper();            

            SGDManagedEngineWrapper.H1ManagedTaskSchedulerLayerWrapper.RunTask(task, taskCounter);

            SGDManagedEngineWrapper.H1ManagedTaskSchedulerLayerWrapper.WaitTaskScheduler();
            SGDManagedEngineWrapper.H1ManagedTaskSchedulerLayerWrapper.DestroyTaskScheduler();

            // @TODO - temporary call destroy methods in here (need to add new event for processing destroy)
            Destroy();
        }

        public void Update()
        {
            m_WPFInputManager.Update();
            m_CameraController.Update(0.0f);

            // update world
            m_World.Tick(0.0f);

            // @TODO - view projection matrix update
            m_Renderer.ViewProjectionMatrix = Matrix.Multiply(m_Camera.ViewMatrix, m_Camera.ProjectionMatrix);

            // update physics engine
            H1Global<Physics.Test.TestPhysicsSimulator>.Instance.Update();
        }

        private IntPtr m_WindowHandle;
        private H1ManagedRenderer m_Renderer;
        private H1InputManagerWpf m_WPFInputManager;

        // @TODO - separate the camera to the object system
        private H1Camera m_Camera;
        private H1CameraController m_CameraController;

        private H1AssimpImporter m_AssimpImporter;

        // object system
        // @TODO - really simple object system
        private H1World m_World;

        // visual debugger
        private H1VisualDebugger m_VisualDebugger;

        // wrapper c++/cli class     
    }
}
