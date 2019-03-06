using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine
{
    public class MouseInput
    {    
        public static EventHandler LeftMouseButtonClick, LeftMouseButtonDoubleClick, 
            RightMouseButtonClick, RightMouseButtonDoubleClick, MiddleMouseButtonClick, MiddleMouseButtonDoubleClick;

        public static EventHandler<double> ScrollUp, ScrollDown;

        public static EventHandler<Point> Dragging;

        private enum MouseButtonState
        {
            InitialState,
            PressedOnce,
            ReleasedFirst,
            PressedTwice,
            Dragging
        }

        private static double doubleClickTimeout = 500, leftMouseButtonTimer, rightMouseButtonTimer, middleMouseButtonTimer,
            prevScrollWheelValue;
        private static MouseButtonState leftMouseButton, rightMouseButton, middleMouseButton;
        private static Point mousePosition;
        private static Point prevMousePosition;

        public static Point MousePosition
        {
            get
            {
                return mousePosition;
            }
        }

        public static void Poll(GameTime time)
        {
            MouseState m = Mouse.GetState();
            mousePosition = m.Position;
            leftMouseButtonTimer -= time.ElapsedGameTime.Milliseconds;
            rightMouseButtonTimer -= time.ElapsedGameTime.Milliseconds;
            middleMouseButtonTimer -= time.ElapsedGameTime.Milliseconds;
            #region LeftMouseButton
            if (leftMouseButton == MouseButtonState.InitialState)
            {
                if(m.LeftButton == ButtonState.Pressed)
                {
                    leftMouseButton = MouseButtonState.PressedOnce;
                    leftMouseButtonTimer = doubleClickTimeout;
                }
            }
            else if(leftMouseButton == MouseButtonState.PressedOnce)
            {
                if (leftMouseButtonTimer < 0)//CLICK TIMEOUT, BEGIN DRAGGING MODE
                {
                    leftMouseButton = MouseButtonState.Dragging;
                    Dragging?.Invoke(null, mousePosition - prevMousePosition);
                }
                else if(m.LeftButton == ButtonState.Released)//BUTTON RELEASED, WAIT FOR DOUBLE-CLICK
                {
                    leftMouseButton = MouseButtonState.ReleasedFirst;
                }
                else if((mousePosition - prevMousePosition).X != 0 || (mousePosition - prevMousePosition).Y != 0)
                {
                    Dragging?.Invoke(null, mousePosition - prevMousePosition);
                }
            }
            else if(leftMouseButton == MouseButtonState.Dragging)
            {
                if(m.LeftButton == ButtonState.Released)//FINISH DRAGGING
                {
                    leftMouseButton = MouseButtonState.InitialState;
                }
                else
                {
                    Dragging?.Invoke(null, mousePosition - prevMousePosition);
                }
            }
            else if(leftMouseButton == MouseButtonState.ReleasedFirst)
            {
                if(leftMouseButtonTimer < 0)//DOUBLE-CLICK TIMEOUT EXPIRED, COUNT AS SINGLE CLICK
                {
                    leftMouseButton = MouseButtonState.InitialState;
                    LeftMouseButtonClick?.Invoke(null, null);
                }
                else if(m.LeftButton == ButtonState.Pressed)//PRESSED SECOND TIME
                {
                    leftMouseButton = MouseButtonState.PressedTwice;
                }
            }
            else if(leftMouseButton == MouseButtonState.PressedTwice)
            {
                if(leftMouseButtonTimer < 0)//DOUBLE-CLICK TIMEOUT EXPIRED, COUNT AS DRAGGING
                {
                    leftMouseButton = MouseButtonState.Dragging;
                    Dragging?.Invoke(null, mousePosition - prevMousePosition);
                }
                else if(m.LeftButton == ButtonState.Released)//RELEASED SECOND TIME, COUNT AS DOUBLE-CLICK
                {
                    leftMouseButton = MouseButtonState.InitialState;
                    LeftMouseButtonDoubleClick?.Invoke(null, null);
                }
                else if ((mousePosition - prevMousePosition).X != 0 || (mousePosition - prevMousePosition).Y != 0)
                {
                    Dragging?.Invoke(null, mousePosition - prevMousePosition);
                }
            }
            #endregion
            #region RightMouseButton
            if (rightMouseButton == MouseButtonState.InitialState)
            {
                if (m.RightButton == ButtonState.Pressed)
                {
                    rightMouseButton = MouseButtonState.PressedOnce;
                    rightMouseButtonTimer = doubleClickTimeout;
                }
            }
            else if (rightMouseButton == MouseButtonState.PressedOnce)
            {
                if (rightMouseButtonTimer < 0)//CLICK TIMEOUT, BEGIN DRAGGING MODE
                {
                    rightMouseButton = MouseButtonState.Dragging;
                }
                else if (m.RightButton == ButtonState.Released)//BUTTON RELEASED, WAIT FOR DOUBLE-CLICK
                {
                    rightMouseButton = MouseButtonState.ReleasedFirst;
                }
            }
            else if (rightMouseButton == MouseButtonState.Dragging)
            {
                if (m.RightButton == ButtonState.Released)//FINISH DRAGGING
                {
                    rightMouseButton = MouseButtonState.InitialState;
                }
            }
            else if (rightMouseButton == MouseButtonState.ReleasedFirst)
            {
                if (rightMouseButtonTimer < 0)//DOUBLE-CLICK TIMEOUT EXPIRED, COUNT AS SINGLE CLICK
                {
                    rightMouseButton = MouseButtonState.InitialState;
                    RightMouseButtonClick?.Invoke(null, null);
                }
                else if (m.RightButton == ButtonState.Pressed)//PRESSED SECOND TIME
                {
                    rightMouseButton = MouseButtonState.PressedTwice;
                }
            }
            else if (rightMouseButton == MouseButtonState.PressedTwice)
            {
                if (rightMouseButtonTimer < 0)//DOUBLE-CLICK TIMEOUT EXPIRED, COUNT AS DRAGGING
                {
                    rightMouseButton = MouseButtonState.Dragging;
                }
                else if (m.RightButton == ButtonState.Released)//RELEASED SECOND TIME, COUNT AS DOUBLE-CLICK
                {
                    rightMouseButton = MouseButtonState.InitialState;
                    RightMouseButtonDoubleClick?.Invoke(null, null);
                }
            }
            #endregion
            #region MiddleMouseButton
            if (middleMouseButton == MouseButtonState.InitialState)
            {
                if (m.MiddleButton == ButtonState.Pressed)
                {
                    middleMouseButton = MouseButtonState.PressedOnce;
                    middleMouseButtonTimer = doubleClickTimeout;
                }
            }
            else if (middleMouseButton == MouseButtonState.PressedOnce)
            {
                if (middleMouseButtonTimer < 0)//CLICK TIMEOUT, BEGIN DRAGGING MODE
                {
                    middleMouseButton = MouseButtonState.Dragging;
                }
                else if (m.MiddleButton == ButtonState.Released)//BUTTON RELEASED, WAIT FOR DOUBLE-CLICK
                {
                    middleMouseButton = MouseButtonState.ReleasedFirst;
                }
            }
            else if (middleMouseButton == MouseButtonState.Dragging)
            {
                if (m.MiddleButton == ButtonState.Released)//FINISH DRAGGING
                {
                    middleMouseButton = MouseButtonState.InitialState;
                }
            }
            else if (middleMouseButton == MouseButtonState.ReleasedFirst)
            {
                if (middleMouseButtonTimer < 0)//DOUBLE-CLICK TIMEOUT EXPIRED, COUNT AS SINGLE CLICK
                {
                    middleMouseButton = MouseButtonState.InitialState;
                    MiddleMouseButtonClick?.Invoke(null, null);
                }
                else if (m.MiddleButton == ButtonState.Pressed)//PRESSED SECOND TIME
                {
                    middleMouseButton = MouseButtonState.PressedTwice;
                }
            }
            else if (middleMouseButton == MouseButtonState.PressedTwice)
            {
                if (middleMouseButtonTimer < 0)//DOUBLE-CLICK TIMEOUT EXPIRED, COUNT AS DRAGGING
                {
                    middleMouseButton = MouseButtonState.Dragging;
                }
                else if (m.RightButton == ButtonState.Released)//RELEASED SECOND TIME, COUNT AS DOUBLE-CLICK
                {
                    middleMouseButton = MouseButtonState.InitialState;
                    MiddleMouseButtonDoubleClick?.Invoke(null, null);
                }
            }
            #endregion
            #region MouseScroll
            double scrollWheelValue = m.ScrollWheelValue;
            if(scrollWheelValue < prevScrollWheelValue)
            {
                ScrollDown?.Invoke(null, scrollWheelValue - prevScrollWheelValue);
            }
            else if(scrollWheelValue > prevScrollWheelValue)
            {
                ScrollUp?.Invoke(null, scrollWheelValue - prevScrollWheelValue);
            }
            prevScrollWheelValue = scrollWheelValue;
            #endregion
            prevMousePosition = mousePosition;
        }
    }
}
