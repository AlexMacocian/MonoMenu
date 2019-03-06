using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMenu.Engine.LogicalTree;
using static MonoMenu.Engine.NodeProperties;
using Microsoft.Xna.Framework;
using MonoMenu.Engine.Animations;

namespace MonoMenu.Engine.Events
{
    public class AnimatedEvent : MenuEvent
    {
        private string from, to, duration;
        private bool started;
        private int iteration, totalIterations;
        private string animName;
        public AnimatedEvent(string name, Type type, Target target, string from, string to, string duration, string triggerName) : base(name, type, string.Empty, triggerName, target)
        {
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.animName = name;
        }

        public override void Trigger(LogicalNode node)
        {
            BaseAnimation anim;
            string from = string.Copy(this.from);
            string to = string.Copy(this.to);
            Target target = this.eventTarget;
            TimeSpan duration = TimeSpan.FromMilliseconds(double.Parse(this.duration));
            switch (eventTarget)
            {
                case Target.Height:
                    if (to.Contains('%'))
                    {
                        from = from.Replace(" ", "");
                        from = from.Replace("%", "");
                        to = to.Replace(" ", "");
                        to = to.Replace("%", "");
                        node.PercentageHeight = true;
                    }
                    else
                    {
                        node.PercentageHeight = false;
                    }
                    break;
                case Target.Width:
                    if (to.Contains('%'))
                    {
                        from = from.Replace(" ", "");
                        from = from.Replace("%", "");
                        to = to.Replace(" ", "");
                        to = to.Replace("%", "");
                        node.PercentageWidth = true;
                    }
                    else
                    {
                        node.PercentageWidth = false;
                    }
                    break;
                case Target.RelativeX:
                    if (to.Contains('%'))
                    {
                        from = from.Replace(" ", "");
                        from = from.Replace("%", "");
                        to = to.Replace(" ", "");
                        to = to.Replace("%", "");
                        node.PercentageX = true;
                    }
                    else
                    {
                        node.PercentageX = false;
                    }
                    break;
                case Target.RelativeY:
                    if (to.Contains('%'))
                    {
                        from = from.Replace(" ", "");
                        from = from.Replace("%", "");
                        to = to.Replace(" ", "");
                        to = to.Replace("%", "");
                        node.PercentageY = true;
                    }
                    else
                    {
                        node.PercentageY = false;
                    }
                    break;
                default:
                    break;
            }
            if (target == Target.Background || target == Target.Foreground || target == Target.BorderColor)
            {
                anim = new ColorAnimation(animName, from, to, duration, node, target);
            }
            else
            {
                anim = new DoubleAnimation(animName, from, to, duration, node, target);
            }
            anim.Enable();
            node.AddAnimation(anim);
        }
    }
}
