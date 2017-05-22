using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.ComponentModel.Composition;

using SGDManagedEngine.SGD;

using Sce.Atf;
using Sce.Atf.Wpf.Interop;
using LevelEditorCore;

namespace SGDManagedEngine
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IGameEngineProxy))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SGDGameEngineProxy : IGameEngineProxy, IInitializable
    {
        public EngineInfo Info
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void SetGameWorld(IGame game)
        {
            throw new NotImplementedException();
        }

        public void Update(FrameTime time, UpdateType updateType)
        {
            m_ManagedEngine.Update();
        }

        public void Render()
        {
            m_ManagedEngine.Render();
        }

        public void Exit(object sender, ExitEventArgs e)
        {
            m_ManagedEngine.Exit();
        }

        public void WaitForPendingResources()
        {
            throw new NotImplementedException();
        }

        void IInitializable.Initialize()
        {  
            // create managed engine and initialize the engine
            m_ManagedEngine = new H1ManagedEngine();

            var mainWindow = Application.Current.MainWindow;
            var windowHandle = new WindowInteropHelper(mainWindow).EnsureHandle();
            int width = Convert.ToInt32(mainWindow.Width);
            int height = Convert.ToInt32(mainWindow.Height);

            m_ManagedEngine.Initialize(windowHandle, width, height);

            // add exit handler (add exit handler)
            Application.Current.Exit += Exit;
        }     

        private H1ManagedEngine m_ManagedEngine;
    }
}
