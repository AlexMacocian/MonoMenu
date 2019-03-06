using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMenu.Engine.Events.MenuEvent;

namespace MonoMenu.Engine.Animations
{
    class BaseAnimation
    {
        public EventHandler Finished;
        protected string sfrom, sto;
        protected int iteration, totalIterations;
        protected TimeSpan duration;
        protected bool enabled;
        protected LogicalTree.LogicalNode node;
        protected Target targetProperty;
        protected double currMillis;
        protected string name;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public BaseAnimation(string name, string from, string to, TimeSpan duration, LogicalTree.LogicalNode node, Target targetProperty)
        {
            this.sfrom = from;
            this.sto = to;
            this.duration = duration;
            this.node = node;
            this.targetProperty = targetProperty;
            this.name = name;
        }

        public virtual void Enable()
        {
            enabled = true;
        }

        public virtual void Tick(GameTime gameTime) {
        }
    }
}
