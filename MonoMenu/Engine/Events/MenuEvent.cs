using MonoMenu.Engine.LogicalTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine.Events
{
    public class MenuEvent
    {
        private string name;
        protected Type eventType;
        protected string value;
        protected string triggerName;
        protected Target eventTarget;
        private Type type;

        public Type EventType
        {
            get
            {
                return eventType;
            }

            set
            {
                eventType = value;
            }
        }

        protected string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
        /// <summary>
        /// Name of the element that triggers this animation
        /// </summary>
        public string TriggerName
        {
            get
            {
                return triggerName;
            }
        }

        public enum Type
        {
            Click,
            DoubleClick,
            MouseEnter,
            MouseLeave,
            MouseDown,
            LeftMouseDown,
            RightMouseDown,
            MouseUp,
            LeftMouseUp,
            RightMouseUp,
            AnimationFinished
        }
        public enum Target
        {
            Background,
            Foreground,
            BorderColor,
            BorderSize,
            FontSize,
            Width,
            Height,
            RelativeX,
            RelativeY,
            VerticalAlignment,
            HorizontalAlignment,
            Visibility,
            None
        }
        public MenuEvent(string name, Type type, string value, string triggerName, Target target)
        {
            this.EventType = type;
            this.value = value;
            this.eventTarget = target;
            this.name = name;
            this.triggerName = triggerName;
        }

        public virtual void Trigger(LogicalNode node)
        {

        }
    }
}
