using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine
{
    public class NodeProperties
    {
        public enum HorizontalAlignment
        {
            Left,
            Right,
            Center,
            Stretch
        }

        public enum VerticalAlignment
        {
            Top,
            Bottom,
            Center,
            Stretch
        }

        public enum Visibility
        {
            Visible,
            Hidden
        }

        public enum FocusChange
        {
            GainedFocus,
            LostFocus
        }

    }
}
