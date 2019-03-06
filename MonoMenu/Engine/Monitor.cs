using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoMenu.Engine
{
    class Monitor
    {
        private static double frames, millis, updates;
        private static Stopwatch stopWatch;

        /// <summary>
        /// Number of frames per second
        /// </summary>
        public static double FrameRate
        {
            get
            {
                if (Seconds > 0)
                {
                    return frames / Seconds;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Number of updates per second
        /// </summary>
        public static double UpdateRate
        {
            get
            {
                if (Seconds > 0)
                {
                    return updates / Seconds;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Number of bytes allocated by current process
        /// </summary>
        public static double MemoryUsage
        {
            get
            {
                return Process.GetCurrentProcess().WorkingSet64;
            }
        }

        /// <summary>
        /// Milliseconds since initialization
        /// </summary>
        public static double Milliseconds
        {
            get
            {
                return stopWatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Seconds since initialization
        /// </summary>
        public static double Seconds
        {
            get
            {
                return stopWatch.ElapsedMilliseconds / 1000;
            }
        }
        /// <summary>
        /// Minutes since initialization
        /// </summary>
        public static double Minutes
        {
            get
            {
                return Seconds / 60;
            }
        }

        /// <summary>
        /// Call this method on draw to gather information about drawing method
        /// </summary>
        public static void DrawCalled()
        {
            frames++;
        }
        /// <summary>
        /// Call this method on update to gather information about update method
        /// </summary>
        public static void UpdateCalled()
        {
            updates++;
        }

        public static void Initialize()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        private static void Timer_CallBack(Object stateinfo)
        {
            millis += 100;
        }
    }
}
