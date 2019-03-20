using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static MonoMenu.Engine.NodeProperties;

namespace MonoMenu.Engine.LogicalTree
{
    class TextboxNode : LogicalNode
    {
        public EventHandler TextChanged;
        private bool showingCaret = false;
        private double millisPassed = 0;

        public TextboxNode(GraphicsDevice device, string name, MonoMenu menu) : base(device, name, menu)
        {
            OnFocusChange += NodeFocusChange;
        }

        private void NodeFocusChange(object sender, FocusChange args)
        {
            if(args == FocusChange.LostFocus)
            {
                if (showingCaret)
                {
                    showingCaret = false;
                    Text = Text.Substring(0, Text.Length - 1);
                }
            }
            else
            {
                if (!showingCaret)
                {
                    showingCaret = true;
                    Text += "|";
                }
            }
        }

        public override void OnTextChange(TextInputEventArgs args)
        {
            base.OnTextChange(args);
            StringBuilder sb = new StringBuilder(Text);
            if (showingCaret)
            {
                sb.Remove(sb.Length - 1, 1);
                showingCaret = false;
            }
            if(args.Key == Microsoft.Xna.Framework.Input.Keys.Back)
            {
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
            }
            else if(args.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
            {
                sb.Append('\n');
            }
            else
            {
                sb.Append(args.Character);
            }
            millisPassed = 0;
            Text = sb.ToString();
            TextChanged?.Invoke(this, null);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Focused)
            {
                millisPassed += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (millisPassed > 1000)
                {
                    millisPassed = 0;
                    if (showingCaret)
                    {
                        Text = Text.Remove(Text.Length - 1, 1);
                    }
                    else
                    {
                        Text += "|";
                    }
                    showingCaret = !showingCaret;
                }
            }
        }
    }
}
