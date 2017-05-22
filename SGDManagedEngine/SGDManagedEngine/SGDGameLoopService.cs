using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// additional system references
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

// atf references
using Sce.Atf;
using LevelEditorCore;

namespace SGDManagedEngine
{
    /// <summary>
    /// Export(typeof(type)) - specifies that a type, property, field or method provides a particular export
    /// PartCreationPolicy - specifies when and how a part will be instantiated
    ///     - shared(default = any) : a single shared instance of the associated ComposablePart will be created by the CompositionContainer and shared by all requestors
    ///     - nonshared : a new non-shared instance of the associated ComposablePart will be created by the CompositionContainer for every requestor
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IGameLoop))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SGDGameLoopService : IGameLoop, IInitializable
    {
        #region SGDIGameLoop Members
        public UpdateType UpdateType
        {
            get;
            set;
        }

        public void Render()
        {
            // set upper limit of rendering frequency to 1 / UpdateStep
            var startTime = Timing.GetHiResCurrentTime();
            var rdt = startTime - m_lastRenderTime;

            // if not enough time passed, skip rendering
            if (rdt < UpdateStep) return;

            m_lastRenderTime = startTime;

            // rendering by the game engine
            m_gameEngine.Render();

            // @TODO - add design views (for sub-control rendering)
            //foreach (var view in m_designView.Views)
            //    view.Render();
        }

        public void Update()
        {
            double lag = (Timing.GetHiResCurrentTime() - m_lastUpdateTime) + m_updateLagRemainder;

            if (lag < UpdateStep) return; // ealy return

            if (UpdateType == UpdateType.Paused)
            {
                m_lastUpdateTime = Timing.GetHiResCurrentTime();
                FrameTime fr = new FrameTime(m_simulationTime, 0.0f);
                m_gameEngine.Update(fr, UpdateType);
                m_updateLagRemainder = 0.0;
            }
            else
            {
                // set upper limit of update calls
                const int MaxUpdates = 3;
                int updateCount = 0;

                while (lag >= UpdateStep && updateCount < MaxUpdates)
                {
                    m_lastUpdateTime = Timing.GetHiResCurrentTime();
                    FrameTime fr = new FrameTime(m_simulationTime, (float)UpdateStep);
                    m_gameEngine.Update(fr, UpdateType);
                    m_simulationTime += UpdateStep;
                    lag -= UpdateStep;
                    updateCount++;
                }
            }
        }
        #endregion

        #region IInitalizable Members
        void IInitializable.Initialize()
        {
            ComponentDispatcher.ThreadIdle += Application_Idle;
        }
        #endregion

        private void Application_Idle(object sender, EventArgs e)
        {
            while (IsIdle())
            {
                Update();
                Render();
            }
        }   
        
        private bool IsIdle()
        {
            return PeekMessage(out m_msg, IntPtr.Zero, 0, 0, 0) == 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Message
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern int PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
        private Message m_msg;

        [Import(AllowDefault = false)]
        private IGameEngineProxy m_gameEngine;

        //[Import(AllowDefault = false)]
        //private IDesignView m_designView;

        private double m_simulationTime;
        private double m_lastRenderTime;
        private double m_lastUpdateTime;
        private double m_updateLagRemainder;
        private const double UpdateStep = 1.0 / 60.0;
    }
}
