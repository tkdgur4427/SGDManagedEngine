using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using WPFKeys = System.Windows.Input.Key;

using SharpDX;
using System.Windows.Input;
using System.Diagnostics;

namespace SGDManagedEngine.SGD
{
    /// <summary>
    /// @TODO - temporary stay in this file, I need to move this class seperately!
    /// this is just context (as input to initialize input manager)
    /// </summary>
    /// <typeparam name="H1_T"></typeparam>
    public class H1InteractionContext<H1_T>
    {
        public H1_T Control
        {
            get;
            internal set; // code within the assembly can read(get) and write the property, but other code can only read (interal set)
        }

        public H1InteractionContext(H1_T control)
        {
            Control = control;
        }
    }

    public abstract class H1InputManagerBase
    {
        internal static readonly Dictionary<WPFKeys, H1Keys> m_MapKeys = new Dictionary<WPFKeys, H1Keys>();
        internal List<KeyBoardInputEvent> m_KeyBoardInputEvents = new List<KeyBoardInputEvent>();

        private readonly Dictionary<H1Keys, bool> m_ActiveKeys = new Dictionary<H1Keys, bool>();
        private readonly List<H1Keys> m_DownKeyList = new List<H1Keys>();
        private readonly HashSet<H1Keys> m_PressedKeySet = new HashSet<H1Keys>();
        private readonly HashSet<H1Keys> m_ReleasedKeySet = new HashSet<H1Keys>();

        internal List<MouseInputEvent> m_MouseInputEvents = new List<MouseInputEvent>();

        internal Vector2 m_CurrentMousePosition = new Vector2(0.0f);
        internal Vector2 m_CurrentMouseDelta = new Vector2(0.0f);

        private const int m_NumberOfMouseButtons = 5;
        internal readonly bool[] m_MouseButtons = new bool[m_NumberOfMouseButtons];
        internal readonly bool[] m_MouseButtonsPrevious = new bool[m_NumberOfMouseButtons];
        internal readonly bool[] m_MouseButtonCurrentlyDown = new bool[m_NumberOfMouseButtons];

        internal readonly Dictionary<int, MouseButtonInfo> m_MouseButtonInfos = new Dictionary<int, MouseButtonInfo>();
        private readonly List<H1MouseButtonMovingEvent> m_MouseButtonMovingEvents = new List<H1MouseButtonMovingEvent>();

        private float m_ControlWidth;
        private float m_ControlHeight;
        private float m_ScreenAspectRatio;

        private Vector2 m_MousePosition;
        private bool m_LostFocus;
        
        internal float ControlWidth
        {
            get { return m_ControlWidth; }
            set
            {
                m_ControlWidth = Math.Max(0, value);
                if (m_ControlHeight > 0)
                    m_ScreenAspectRatio = m_ControlWidth / m_ControlHeight;
            }
        }

        internal float ControlHeight
        {
            get { return m_ControlHeight; }
            set
            {
                m_ControlHeight = Math.Max(0, value);
                if (m_ControlHeight > 0)
                    m_ScreenAspectRatio = m_ControlWidth / m_ControlHeight;
            }
        }

        internal float ScreenAspectRatio
        {
            get { return m_ScreenAspectRatio; }
            private set
            {
                m_ScreenAspectRatio = value;
            }
        }

        public Vector2 MousePosition
        {
            get { return m_MousePosition; }            
        }

        public Vector2 MouseDelta { get; private set; }

        public float MouseWheelDelta { get; private set; }

        internal struct KeyBoardInputEvent
        {
            public H1Keys Key;
            public InputEventType Type;
            public bool OutOfFocus;
        }

        internal struct MouseInputEvent
        {
            public H1MouseButton MouseButton;
            public InputEventType Type;
            public float Value;
        }

        internal enum InputEventType
        {
            Up,
            Down,
            Wheel,
        }

        internal class MouseButtonInfo
        {
            public readonly Stopwatch MouseButtonClock = new Stopwatch();
            public Vector2 LastPosition; 
        }

        protected internal void BindWPFInputKeyBoard(Window WPFControl)
        {
            EnsureMapKeys();
            WPFControl.KeyDown += DeviceOnKeyBoardInput;
            WPFControl.KeyUp += DeviceOnKeyBoardInput;
        }

        private void DeviceOnKeyBoardInput(object sender, KeyEventArgs e)
        {
            var key = H1Keys.None;
            
            var virtualKey = e.Key;
            var keyState = e.KeyStates;

            if (Convert.ToInt32(virtualKey) == 255)
                return; // discard 'fake keys' which are part of an escaped sequeunce
            
            if (key == H1Keys.None)
            {
                m_MapKeys.TryGetValue(virtualKey, out key);
            }

            if (key != H1Keys.None)
            {
                bool isKeyUp = e.IsUp;
                if(isKeyUp)
                {
                    m_KeyBoardInputEvents.Add(new KeyBoardInputEvent { Key = key, Type = InputEventType.Up });
                }
                else
                {
                    m_KeyBoardInputEvents.Add(new KeyBoardInputEvent { Key = key, Type = InputEventType.Down });
                }
            }
        }

        private void EnsureMapKeys()
        {
            if (m_MapKeys.Count > 0)
                return;

            AddKeys(WPFKeys.None, H1Keys.None);
            AddKeys(WPFKeys.Cancel, H1Keys.Cancel);
            AddKeys(WPFKeys.Back, H1Keys.Back);
            AddKeys(WPFKeys.Tab, H1Keys.Tab);
            AddKeys(WPFKeys.LineFeed, H1Keys.LineFeed);
            AddKeys(WPFKeys.Clear, H1Keys.Clear);
            AddKeys(WPFKeys.Enter, H1Keys.Enter);
            AddKeys(WPFKeys.Return, H1Keys.Return);
            AddKeys(WPFKeys.Pause, H1Keys.Pause);
            AddKeys(WPFKeys.Capital, H1Keys.Capital);
            AddKeys(WPFKeys.CapsLock, H1Keys.CapsLock);
            AddKeys(WPFKeys.HangulMode, H1Keys.HangulMode);
            AddKeys(WPFKeys.KanaMode, H1Keys.KanaMode);
            AddKeys(WPFKeys.JunjaMode, H1Keys.JunjaMode);
            AddKeys(WPFKeys.FinalMode, H1Keys.FinalMode);
            AddKeys(WPFKeys.HanjaMode, H1Keys.HanjaMode);
            AddKeys(WPFKeys.KanjiMode, H1Keys.KanjiMode);
            AddKeys(WPFKeys.Escape, H1Keys.Escape);
            AddKeys(WPFKeys.Space, H1Keys.Space);
            AddKeys(WPFKeys.PageUp, H1Keys.PageUp);
            AddKeys(WPFKeys.Prior, H1Keys.Prior);
            AddKeys(WPFKeys.Next, H1Keys.Next);
            AddKeys(WPFKeys.PageDown, H1Keys.PageDown);
            AddKeys(WPFKeys.End, H1Keys.End);
            AddKeys(WPFKeys.Home, H1Keys.Home);
            AddKeys(WPFKeys.Left, H1Keys.Left);
            AddKeys(WPFKeys.Up, H1Keys.Up);
            AddKeys(WPFKeys.Right, H1Keys.Right);
            AddKeys(WPFKeys.Down, H1Keys.Down);
            AddKeys(WPFKeys.Select, H1Keys.Select);
            AddKeys(WPFKeys.Print, H1Keys.Print);
            AddKeys(WPFKeys.Execute, H1Keys.Execute);
            AddKeys(WPFKeys.PrintScreen, H1Keys.PrintScreen);
            AddKeys(WPFKeys.Snapshot, H1Keys.Snapshot);
            AddKeys(WPFKeys.Insert, H1Keys.Insert);
            AddKeys(WPFKeys.Delete, H1Keys.Delete);
            AddKeys(WPFKeys.Help, H1Keys.Help);
            AddKeys(WPFKeys.D0, H1Keys.D0);
            AddKeys(WPFKeys.D1, H1Keys.D1);
            AddKeys(WPFKeys.D2, H1Keys.D2);
            AddKeys(WPFKeys.D3, H1Keys.D3);
            AddKeys(WPFKeys.D4, H1Keys.D4);
            AddKeys(WPFKeys.D5, H1Keys.D5);
            AddKeys(WPFKeys.D6, H1Keys.D6);
            AddKeys(WPFKeys.D7, H1Keys.D7);
            AddKeys(WPFKeys.D8, H1Keys.D8);
            AddKeys(WPFKeys.D9, H1Keys.D9);
            AddKeys(WPFKeys.A, H1Keys.A);
            AddKeys(WPFKeys.B, H1Keys.B);
            AddKeys(WPFKeys.C, H1Keys.C);
            AddKeys(WPFKeys.D, H1Keys.D);
            AddKeys(WPFKeys.E, H1Keys.E);
            AddKeys(WPFKeys.F, H1Keys.F);
            AddKeys(WPFKeys.G, H1Keys.G);
            AddKeys(WPFKeys.H, H1Keys.H);
            AddKeys(WPFKeys.I, H1Keys.I);
            AddKeys(WPFKeys.J, H1Keys.J);
            AddKeys(WPFKeys.K, H1Keys.K);
            AddKeys(WPFKeys.L, H1Keys.L);
            AddKeys(WPFKeys.M, H1Keys.M);
            AddKeys(WPFKeys.N, H1Keys.N);
            AddKeys(WPFKeys.O, H1Keys.O);
            AddKeys(WPFKeys.P, H1Keys.P);
            AddKeys(WPFKeys.Q, H1Keys.Q);
            AddKeys(WPFKeys.R, H1Keys.R);
            AddKeys(WPFKeys.S, H1Keys.S);
            AddKeys(WPFKeys.T, H1Keys.T);
            AddKeys(WPFKeys.U, H1Keys.U);
            AddKeys(WPFKeys.V, H1Keys.V);
            AddKeys(WPFKeys.W, H1Keys.W);
            AddKeys(WPFKeys.X, H1Keys.X);
            AddKeys(WPFKeys.Y, H1Keys.Y);
            AddKeys(WPFKeys.Z, H1Keys.Z);
            AddKeys(WPFKeys.LWin, H1Keys.LeftWin);
            AddKeys(WPFKeys.RWin, H1Keys.RightWin);
            AddKeys(WPFKeys.Apps, H1Keys.Apps);
            AddKeys(WPFKeys.Sleep, H1Keys.Sleep);
            AddKeys(WPFKeys.NumPad0, H1Keys.NumPad0);
            AddKeys(WPFKeys.NumPad1, H1Keys.NumPad1);
            AddKeys(WPFKeys.NumPad2, H1Keys.NumPad2);
            AddKeys(WPFKeys.NumPad3, H1Keys.NumPad3);
            AddKeys(WPFKeys.NumPad4, H1Keys.NumPad4);
            AddKeys(WPFKeys.NumPad5, H1Keys.NumPad5);
            AddKeys(WPFKeys.NumPad6, H1Keys.NumPad6);
            AddKeys(WPFKeys.NumPad7, H1Keys.NumPad7);
            AddKeys(WPFKeys.NumPad8, H1Keys.NumPad8);
            AddKeys(WPFKeys.NumPad9, H1Keys.NumPad9);
            AddKeys(WPFKeys.Multiply, H1Keys.Multiply);
            AddKeys(WPFKeys.Add, H1Keys.Add);
            AddKeys(WPFKeys.Separator, H1Keys.Separator);
            AddKeys(WPFKeys.Subtract, H1Keys.Subtract);
            AddKeys(WPFKeys.Decimal, H1Keys.Decimal);
            AddKeys(WPFKeys.Divide, H1Keys.Divide);
            AddKeys(WPFKeys.F1, H1Keys.F1);
            AddKeys(WPFKeys.F2, H1Keys.F2);
            AddKeys(WPFKeys.F3, H1Keys.F3);
            AddKeys(WPFKeys.F4, H1Keys.F4);
            AddKeys(WPFKeys.F5, H1Keys.F5);
            AddKeys(WPFKeys.F6, H1Keys.F6);
            AddKeys(WPFKeys.F7, H1Keys.F7);
            AddKeys(WPFKeys.F8, H1Keys.F8);
            AddKeys(WPFKeys.F9, H1Keys.F9);
            AddKeys(WPFKeys.F10, H1Keys.F10);
            AddKeys(WPFKeys.F11, H1Keys.F11);
            AddKeys(WPFKeys.F12, H1Keys.F12);
            AddKeys(WPFKeys.F13, H1Keys.F13);
            AddKeys(WPFKeys.F14, H1Keys.F14);
            AddKeys(WPFKeys.F15, H1Keys.F15);
            AddKeys(WPFKeys.F16, H1Keys.F16);
            AddKeys(WPFKeys.F17, H1Keys.F17);
            AddKeys(WPFKeys.F18, H1Keys.F18);
            AddKeys(WPFKeys.F19, H1Keys.F19);
            AddKeys(WPFKeys.F20, H1Keys.F20);
            AddKeys(WPFKeys.F21, H1Keys.F21);
            AddKeys(WPFKeys.F22, H1Keys.F22);
            AddKeys(WPFKeys.F23, H1Keys.F23);
            AddKeys(WPFKeys.F24, H1Keys.F24);
            AddKeys(WPFKeys.NumLock, H1Keys.NumLock);
            AddKeys(WPFKeys.Scroll, H1Keys.Scroll);
            AddKeys(WPFKeys.BrowserBack, H1Keys.BrowserBack);
            AddKeys(WPFKeys.BrowserForward, H1Keys.BrowserForward);
            AddKeys(WPFKeys.BrowserRefresh, H1Keys.BrowserRefresh);
            AddKeys(WPFKeys.BrowserStop, H1Keys.BrowserStop);
            AddKeys(WPFKeys.BrowserSearch, H1Keys.BrowserSearch);
            AddKeys(WPFKeys.BrowserFavorites, H1Keys.BrowserFavorites);
            AddKeys(WPFKeys.BrowserHome, H1Keys.BrowserHome);
            AddKeys(WPFKeys.VolumeMute, H1Keys.VolumeMute);
            AddKeys(WPFKeys.VolumeDown, H1Keys.VolumeDown);
            AddKeys(WPFKeys.VolumeUp, H1Keys.VolumeUp);
            AddKeys(WPFKeys.MediaNextTrack, H1Keys.MediaNextTrack);
            AddKeys(WPFKeys.MediaPreviousTrack, H1Keys.MediaPreviousTrack);
            AddKeys(WPFKeys.MediaStop, H1Keys.MediaStop);
            AddKeys(WPFKeys.MediaPlayPause, H1Keys.MediaPlayPause);
            AddKeys(WPFKeys.LaunchMail, H1Keys.LaunchMail);
            AddKeys(WPFKeys.SelectMedia, H1Keys.SelectMedia);
            AddKeys(WPFKeys.LaunchApplication1, H1Keys.LaunchApplication1);
            AddKeys(WPFKeys.LaunchApplication2, H1Keys.LaunchApplication2);
            AddKeys(WPFKeys.Oem1, H1Keys.Oem1);
            AddKeys(WPFKeys.OemSemicolon, H1Keys.OemSemicolon);
            AddKeys(WPFKeys.OemMinus, H1Keys.OemMinus);
            AddKeys(WPFKeys.OemPeriod, H1Keys.OemPeriod);
            AddKeys(WPFKeys.Oem2, H1Keys.Oem2);
            AddKeys(WPFKeys.OemQuestion, H1Keys.OemQuestion);
            AddKeys(WPFKeys.Oem3, H1Keys.Oem3);
            AddKeys(WPFKeys.Oem4, H1Keys.Oem4);
            AddKeys(WPFKeys.OemOpenBrackets, H1Keys.OemOpenBrackets);
            AddKeys(WPFKeys.Oem5, H1Keys.Oem5);
            AddKeys(WPFKeys.OemPipe, H1Keys.OemPipe);
            AddKeys(WPFKeys.Oem6, H1Keys.Oem6);
            AddKeys(WPFKeys.OemCloseBrackets, H1Keys.OemCloseBrackets);
            AddKeys(WPFKeys.Oem7, H1Keys.Oem7);
            AddKeys(WPFKeys.OemQuotes, H1Keys.OemQuotes);
            AddKeys(WPFKeys.Oem8, H1Keys.Oem8);
            AddKeys(WPFKeys.Oem102, H1Keys.Oem102);
            AddKeys(WPFKeys.OemBackslash, H1Keys.OemBackslash);
            AddKeys(WPFKeys.Attn, H1Keys.Attn);
            AddKeys(WPFKeys.EraseEof, H1Keys.EraseEof);
            AddKeys(WPFKeys.Play, H1Keys.Play);
            AddKeys(WPFKeys.Zoom, H1Keys.Zoom);
            AddKeys(WPFKeys.NoName, H1Keys.NoName);
            AddKeys(WPFKeys.Pa1, H1Keys.Pa1);
            AddKeys(WPFKeys.OemClear, H1Keys.OemClear);
        }

        private static void AddKeys(WPFKeys fromKey, H1Keys toKey)
        {
            if (!m_MapKeys.ContainsKey(fromKey))
            {
                m_MapKeys.Add(fromKey, toKey);
            }
        }

        public bool IsKeyDown(H1Keys key)
        {
            bool pressed;
            // tip1 - trygetvalue(hash lookup) is faster than containKey
            // containkey need to compute the hash code from a string key (which is penality). it is why it is slow!
            // tip2 - don't use dictionary with ++ operator (it is hash table so, it is slow!)
            m_ActiveKeys.TryGetValue(key, out pressed); 
            return pressed;
        }

        public bool IsKeyPressed(H1Keys key)
        {
            return m_PressedKeySet.Contains(key);
        }

        public bool IsKeyReleased(H1Keys key)
        {
            return m_ReleasedKeySet.Contains(key);
        }

        public bool IsMouseButtonDown(H1MouseButton mouseButton)
        {
            return m_MouseButtons[(int)mouseButton];
        }

        // pressed is trigger!!!!
        public bool IsMouseButtonPressed(H1MouseButton mouseButton)
        {
            return !m_MouseButtonsPrevious[(int)mouseButton] && m_MouseButtons[(int)mouseButton];
        }

        public bool IsMouseButtonReleased(H1MouseButton mouseButton)
        {
            return m_MouseButtonsPrevious[(int)mouseButton] && !m_MouseButtons[(int)mouseButton];
        }

        public bool HashDownMouseButtons()
        {
            for (int i = 0; i < m_MouseButtons.Length; ++i)
                if (IsMouseButtonDown((H1MouseButton)i))
                    return true;
            return false;
        }

        public bool HasReleasedMouseButtons()
        {
            for (int i = 0; i < m_MouseButtons.Length; ++i)
                if (IsMouseButtonReleased((H1MouseButton)i))
                    return true;
            return false;
        }

        public bool HasPressedMouseButtons()
        {
            for (int i = 0; i < m_MouseButtons.Length; ++i)
                if (IsMouseButtonPressed((H1MouseButton)i))
                    return true;
            return false;
        }

        internal void HandleMouseMovingEvents(int mouseButtonId, Vector2 newPosition, H1MouseState state)
        {
            if (!m_MouseButtonInfos.ContainsKey(mouseButtonId))
                m_MouseButtonInfos[mouseButtonId] = new MouseButtonInfo();

            var mouseButtonInfo = m_MouseButtonInfos[mouseButtonId];

            if (state == H1MouseState.Down)
            {
                mouseButtonInfo.LastPosition = newPosition;
                mouseButtonInfo.MouseButtonClock.Restart();
            }

            var mouseButtonEvent = H1MouseButtonMovingEvent.GetOrCreateMouseButtonEvent();

            mouseButtonEvent.MouseButtonId = mouseButtonId;
            mouseButtonEvent.Position = newPosition;
            mouseButtonEvent.DeltaPosition = newPosition - mouseButtonInfo.LastPosition;
            mouseButtonEvent.State = state;
            mouseButtonEvent.DeltaTime = mouseButtonInfo.MouseButtonClock.Elapsed;

            m_MouseButtonMovingEvents.Add(mouseButtonEvent);

            mouseButtonInfo.LastPosition = newPosition;
            mouseButtonInfo.MouseButtonClock.Restart();
        }

        private void UpdateMouse()
        {
            MouseWheelDelta = 0;

            for (int i = 0; i < m_MouseButtons.Length; ++i)
            {
                m_MouseButtonsPrevious[i] = m_MouseButtons[i];
            }

            foreach (MouseInputEvent mouseInputEvent in m_MouseInputEvents)
            {
                var mouseButton = (int)mouseInputEvent.MouseButton;
                if (mouseButton < 0 || mouseButton >= m_MouseButtons.Length)
                    continue;

                switch (mouseInputEvent.Type)
                {
                    case InputEventType.Down:
                        m_MouseButtons[mouseButton] = true;
                        break;
                    case InputEventType.Up:
                        m_MouseButtons[mouseButton] = false;
                        break;
                    case InputEventType.Wheel:
                        if (mouseInputEvent.MouseButton != H1MouseButton.Middle)
                        {
                            throw new NotImplementedException();
                        }
                        MouseWheelDelta += mouseInputEvent.Value;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            m_MouseInputEvents.Clear();

            m_MousePosition = m_CurrentMousePosition;
            MouseDelta = m_CurrentMouseDelta;
            m_CurrentMouseDelta = Vector2.Zero;

            if (m_LostFocus)
            {
                for (int i = 0; i < m_MouseButtons.Length; ++i)
                    m_MouseButtons[i] = false;
            }
        }

        private void UpdateKeyBoard()
        {
            m_PressedKeySet.Clear();
            m_ReleasedKeySet.Clear();
            
            foreach (KeyBoardInputEvent keyboardInputEvent in m_KeyBoardInputEvents)
            {
                var key = keyboardInputEvent.Key;

                if (key == H1Keys.None)
                    continue;

                switch (keyboardInputEvent.Type)
                {
                    case InputEventType.Down:                        
                        if (!IsKeyDown(key)) // prevent from several inconsistent pressed key due to OS repeat key
                        {
                            m_ActiveKeys[key] = true;
                            if (!keyboardInputEvent.OutOfFocus)
                            {
                                m_PressedKeySet.Add(key);                                
                            }
                            m_DownKeyList.Add(key);
                        }
                        break;
                    case InputEventType.Up:
                        m_ActiveKeys[key] = false;
                        m_ReleasedKeySet.Add(key);
                        m_DownKeyList.Remove(key);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            m_KeyBoardInputEvents.Clear();

            if (m_LostFocus)
            {
                m_ActiveKeys.Clear();
                m_DownKeyList.Clear();
            }
        }

        public void Update()
        {
            // udpate key board state!
            UpdateKeyBoard();
            UpdateMouse();
            m_LostFocus = false;
        }
    }

    internal abstract class H1InputManager<H1_T> : H1InputManagerBase
    {
        protected H1_T Control;
        
        public abstract void Initialize(H1InteractionContext<H1_T> context);
    }

    internal abstract class H1InputManagerWindows<H1_T> : H1InputManager<H1_T>
    {
       
    }

    /// <summary>
    /// @TODO - I need to make this input manager more scalarable (like any other input manager ex. Winform Manager....)
    /// </summary>
    internal class H1InputManagerWpf : H1InputManagerWindows<Window>
    {
        public override void Initialize(H1InteractionContext<Window> context)
        {
            Control = context.Control;
            
            // @TODO
            ControlWidth = Convert.ToSingle(context.Control.Width);
            ControlHeight = Convert.ToSingle(context.Control.Height);

            // @TODO - custom key binding (for scalarability)

            // Private Note - a one form of lamda function (_ : object), (e : any arguments) => (triggered function name)
            Control.LostFocus += (_, e) => OnUIControlLostFocus();
            Control.Deactivated += (_, e) => OnUIControlLostFocus();
            Control.MouseMove += (_, e) => OnMouseMoveEvent(PointToVector2(e.GetPosition(Control)));
            Control.MouseDown += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(Control)), ConvertMouseButton(e.ChangedButton), InputEventType.Down);
            Control.MouseUp += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(Control)), ConvertMouseButton(e.ChangedButton), InputEventType.Up);
            Control.MouseWheel += (_, e) => OnMouseInputEvent(PointToVector2(e.GetPosition(Control)), H1MouseButton.Middle, InputEventType.Wheel, e.Delta);

            // binding keyboard events
            BindWPFInputKeyBoard(Control);
        }

        private void OnMouseInputEvent(Vector2 pixelPosition, H1MouseButton button, InputEventType type, float value = 0.0f)
        {
            // the mouse wheel event are still received when the mouse cursor is out of the control boundaries. discard the event in this case
            if (type == InputEventType.Wheel && !Control.IsMouseOver)
                return;
            
            // the mouse event series has been interrupted because out of the window
            if (type == InputEventType.Up && !m_MouseButtonCurrentlyDown[(int)button])
                return;

            m_CurrentMousePosition = NormalizeScreenPosition(pixelPosition);

            var mouseInputEvent = new MouseInputEvent { Type = type, MouseButton = button, Value = value };
            m_MouseInputEvents.Add(mouseInputEvent);

            if (type != InputEventType.Wheel)
            {
                var buttonId = (int)button;
                m_MouseButtonCurrentlyDown[buttonId] = type == InputEventType.Down;
                HandleMouseMovingEvents(buttonId, m_CurrentMousePosition, InputEventTypeToMouseState(type));
            }
        }        

        private void OnMouseMoveEvent(Vector2 pixelPosition)
        {
            var previousMousePosition = m_CurrentMousePosition;
            m_CurrentMousePosition = NormalizeScreenPosition(pixelPosition);

            m_CurrentMouseDelta += m_CurrentMousePosition - previousMousePosition;

            // trigger mouse move events
            foreach (H1MouseButton button in Enum.GetValues(typeof(H1MouseButton)))
            {
                var buttonId = Convert.ToInt32(button);
                if (m_MouseButtonCurrentlyDown[buttonId])
                {
                    HandleMouseMovingEvents(buttonId, m_CurrentMousePosition, H1MouseState.Move);
                }
            }            
        }        

        private void OnUIControlLostFocus()
        {
            //throw new NotImplementedException();
        }

        internal Vector2 NormalizeScreenPosition(Vector2 pixelPosition)
        {
            return new Vector2(pixelPosition.X / ControlWidth, pixelPosition.Y / ControlHeight);
        }

        private static Vector2 PointToVector2(System.Windows.Point point/*ambiguous problem with SharpDX*/)
        {
            return new Vector2(Convert.ToSingle(point.X), Convert.ToSingle(point.Y));
        }

        private static H1MouseButton ConvertMouseButton(System.Windows.Input.MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    return H1MouseButton.Left;
                case System.Windows.Input.MouseButton.Right:
                    return H1MouseButton.Right;
                case System.Windows.Input.MouseButton.Middle:
                    return H1MouseButton.Middle;
                case System.Windows.Input.MouseButton.XButton1:
                    return H1MouseButton.Extended1;
                case System.Windows.Input.MouseButton.XButton2:
                    return H1MouseButton.Extended2;
            }
            return (H1MouseButton)(-1);
        }

        private static H1MouseState InputEventTypeToMouseState(InputEventType type)
        {
            switch (type)
            {
                case InputEventType.Up:
                    return H1MouseState.Up;
                case InputEventType.Down:
                    return H1MouseState.Down;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }    
}
