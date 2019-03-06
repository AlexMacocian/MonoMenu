using MonoMenu.Engine.LogicalTree;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMenu.Engine.NodeProperties;

namespace MonoMenu.Engine.Events
{
    public class BasicEvent : MenuEvent
    {
        public BasicEvent(string name, Type type, string value, string triggerName, Target target) : base(name, type, value, triggerName, target)
        {
        }

        public override void Trigger(LogicalNode node)
        {
            string v = string.Copy(value);
            switch(eventTarget)
            {
                case Target.BorderColor:
                    node.BorderColor = MonoMenu.ColorFromString(value);
                    break;
                case Target.Background:
                    node.Background = MonoMenu.ColorFromString(value);
                    break;
                case Target.Foreground:
                    node.Foreground = MonoMenu.ColorFromString(value);
                    break;
                case Target.Height:
                    if (value.Contains('%'))
                    {
                        v = v.Replace(" ", "");
                        v = v.Replace("%", "");
                        node.PercentageHeight = true;
                    }
                    else
                    {
                        node.PercentageHeight = false;
                    }
                    node.DesiredHeight = int.Parse(v);
                    break;
                case Target.Width:
                    if (value.Contains('%'))
                    {
                        v = v.Replace(" ", "");
                        v = v.Replace("%", "");
                        node.PercentageWidth = true;
                    }
                    else
                    {
                        node.PercentageWidth = false;
                    }
                    node.DesiredWidth = int.Parse(v);
                    break;
                case Target.HorizontalAlignment:
                    node.HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), value);
                    break;
                case Target.VerticalAlignment:
                    node.VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), value);
                    break;
                case Target.RelativeX:
                    Point xnewPos = node.DesiredRelativePosition;
                    if (value.Contains('%'))
                    {
                        v = v.Replace(" ", "");
                        v = v.Replace("%", "");
                        node.PercentageX = true;
                    }
                    else
                    {
                        node.PercentageX = false;
                    }
                    xnewPos.X = int.Parse(v);
                    node.DesiredRelativePosition = xnewPos;
                    break;
                case Target.RelativeY:
                    Point ynewPos = node.DesiredRelativePosition;
                    if (value.Contains('%'))
                    {
                        v = v.Replace(" ", "");
                        v = v.Replace("%", "");
                        node.PercentageY = true;
                    }
                    else
                    {
                        node.PercentageY = false;
                    }
                    ynewPos.Y = int.Parse(v);
                    node.DesiredRelativePosition = ynewPos;
                    break;
                case Target.BorderSize:
                    node.BorderSize = int.Parse(value);
                    break;
                case Target.FontSize:
                    node.FontSize = int.Parse(value);
                    break;
                case Target.Visibility:
                    node.Visibility = (Visibility)Enum.Parse(typeof(Visibility), value);
                    break;
                default:
                    break;
            }
        }
    }
}
