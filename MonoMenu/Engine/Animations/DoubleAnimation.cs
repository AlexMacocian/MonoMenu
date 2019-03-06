using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static MonoMenu.Engine.Events.MenuEvent;

namespace MonoMenu.Engine.Animations
{
    public class DoubleAnimation : BaseAnimation
    {
        private double from, to, step, current;
        public DoubleAnimation(string name, string from, string to, TimeSpan duration, LogicalTree.LogicalNode node, Target targetProperty) : base(name, from, to, duration, node, targetProperty)
        {
            this.from = double.Parse(from);
            this.to = double.Parse(to);
        }

        public override void Enable()
        {
            current = from;
            double dif = to - from;
            totalIterations = (int)duration.TotalMilliseconds / 16;
            iteration = 0;
            step = dif / totalIterations;
            base.Enable();
        }

        public override void Tick(GameTime gameTime)
        {
            if (enabled)
            {
                currMillis += gameTime.ElapsedGameTime.Milliseconds;
                if(currMillis < 16)
                {
                    return;
                }
                if(iteration < totalIterations)
                {
                    iteration++;
                    switch (targetProperty)
                    {
                        case Target.Height:
                            current += step;
                            node.DesiredHeight = current;
                            break;
                        case Target.Width:
                            current += step;
                            node.DesiredWidth = current;
                            break;
                        case Target.BorderSize:
                            current += step;
                            node.BorderSize = (int)current;
                            break;
                        case Target.FontSize:
                            current += step;
                            node.FontSize = (int)current;
                            break;
                        case Target.RelativeX:
                            current += step;
                            node.DesiredRelativePosition = new Point((int)current, node.DesiredRelativePosition.Y);
                            break;
                        case Target.RelativeY:
                            current += step;
                            node.DesiredRelativePosition = new Point(node.DesiredRelativePosition.X, (int)current);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    enabled = false;
                    Finished?.Invoke(this, null);
                }
            }
        }
    }
}
