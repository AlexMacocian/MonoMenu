using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMenu.Engine.Events.MenuEvent;

namespace MonoMenu.Engine.Animations
{
    class ColorAnimation : BaseAnimation
    {
        private Color from, to, step, current;
        private float stepA, stepR, stepG, stepB;
        public ColorAnimation(string name, string from, string to, TimeSpan duration, LogicalTree.LogicalNode node, Target targetProperty) : base(name, from, to, duration, node, targetProperty)
        {
            this.from = MonoMenu.ColorFromString(from);
            this.to = MonoMenu.ColorFromString(to);
        }

        public override void Enable()
        {
            current = from;
            float difA = to.A - from.A;
            float difR = to.R - from.R;
            float difG = to.G - from.G;
            float difB = to.B - from.B;
            totalIterations = (int)duration.TotalMilliseconds / 16;
            iteration = 0;
            stepA = difA / totalIterations;
            stepB = difB / totalIterations;
            stepR = difR / totalIterations;
            stepG = difG / totalIterations;
            step = new Color(stepR, stepG, stepB, stepA);
            base.Enable();
        }

        public override void Tick(GameTime gameTime)
        {
            if (enabled)
            {
                currMillis += gameTime.ElapsedGameTime.Milliseconds;
                if (currMillis < 16)
                {
                    return;
                }
                if (iteration < totalIterations)
                {
                    iteration++;
                    switch (targetProperty)
                    {
                        case Target.Background:
                            current = new Color(current.R + (int)stepR, current.G + (int)stepG, current.B + (int)stepB, current.A + (int)stepA);
                            node.Background = current;
                            break;
                        case Target.Foreground:
                            current = new Color(current.R + (int)stepR, current.G + (int)stepG, current.B + (int)stepB, current.A + (int)stepA);
                            node.Foreground = current;
                            break;
                        case Target.BorderColor:
                            current = new Color(current.R + (int)stepR, current.G + (int)stepG, current.B + (int)stepB, current.A + (int)stepA);
                            node.BorderColor = current;
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
