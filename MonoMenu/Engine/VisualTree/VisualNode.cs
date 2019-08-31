using MonoMenu.Engine.LogicalTree;
using MonoMenu.Engine.VisualPrimitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMenu.Engine.NodeProperties;

namespace MonoMenu.Engine.VisualTree
{
    public class VisualNode
    {
        private RenderTarget2D renderTarget;
        private GraphicsDevice graphicsDevice;
        private double width, height;
        private Texture2D primitive;
        private LogicalNode LogicalNode;
        private Color backgroundColor, foregroundColor, borderColor;
        private string text = string.Empty;
        private bool modified = true, invalidRenderTarget = false, textWrapping = false;
        private int fontSize, borderSize;
        private HorizontalAlignment horizontalTextAlignment;
        private VerticalAlignment verticalTextAlignment;
        private Visibility visibility;
        private SpriteFont font;
        private Dictionary<string, Effects.BasicEffect> effects;

        public bool Modified
        {
            set
            {
                modified = value;
                if (value == true)
                {
                    if (LogicalNode.Parent != null)
                    {
                        if(!LogicalNode.Parent.VisualNode.Modified)
                            LogicalNode.Parent.VisualNode.Modified = true;
                    }
                }
            }
            get
            {
                return modified;
            }
        }

        public bool TextWrapping
        {
            get
            {
                return textWrapping;
            }
            set
            {
                if(value != textWrapping)
                {
                    Modified = true;
                }
                textWrapping = value;
            }
        }

        public Texture2D Primitive
        {
            get
            {
                return primitive;
            }

            set
            {
                primitive = value;
            }
        }

        public RenderTarget2D RenderTarget
        {
            get
            {
                return renderTarget;
            }
        }

        public double Width
        {
            get
            {
                return width;
            }

            set
            {
                if (value != width)
                {
                    Modified = true;
                }
                width = value;
                invalidRenderTarget = true;
            }
        }

        public double Height
        {
            get
            {
                return height;
            }

            set
            {
                if (value != height)
                {
                    Modified = true;
                }
                height = value;
                invalidRenderTarget = true;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return backgroundColor;
            }

            set
            {
                backgroundColor = value;
                Modified = true;
            }
        }

        public Color ForegroundColor
        {
            get
            {
                return foregroundColor;
            }

            set
            {
                foregroundColor = value;
                Modified = true;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                text = value;
                Modified = true;
            }
        }

        public int FontSize
        {
            get
            {
                return fontSize;
            }

            set
            {
                if(fontSize != value)
                {
                    Modified = true;
                }
                fontSize = value;     
            }
        }

        public HorizontalAlignment HorizontalTextAlignment
        {
            get
            {
                return horizontalTextAlignment;
            }

            set
            {
                if(value != horizontalTextAlignment)
                {
                    Modified = true;
                }
                horizontalTextAlignment = value;
            }
        }

        public VerticalAlignment VerticalTextAlignment
        {
            get
            {
                return verticalTextAlignment;
            }

            set
            {
                if(value != verticalTextAlignment)
                {
                    Modified = true;
                }
                verticalTextAlignment = value;
            }
        }

        public int BorderSize
        {
            get
            {
                return borderSize;
            }

            set
            {
                if(borderSize != value)
                {
                    Modified = true;
                }
                borderSize = value;
            }
        }

        public Color BorderColor
        {
            get
            {
                return borderColor;
            }

            set
            {
                borderColor = value;
                Modified = true;
            }
        }

        public Visibility Visibility
        {
            get
            {
                return visibility;
            }

            set
            {
                visibility = value;
                if (visibility == Visibility.Visible)
                {
                    Modified = true;
                }
            }
        }

        public SpriteFont Font
        {
            get
            {
                return font;
            }

            set
            {
                font = value;
                Modified = true;
            }
        }

        public Dictionary<string, Effects.BasicEffect> Effects
        {
            get
            {
                return effects;
            }
            set
            {
                effects = value;
                Modified = true;
            }
        }

        public VisualNode(GraphicsDevice device, LogicalNode lnode)
        {
            this.LogicalNode = lnode;
            this.graphicsDevice = device;
            this.BackgroundColor = Color.Transparent;
            this.ForegroundColor = Color.Transparent;
            this.BorderColor = Color.Transparent;
            this.fontSize = 0;
            this.borderSize = 0;
            this.text = "";
            this.VerticalTextAlignment = VerticalAlignment.Top;
            this.HorizontalTextAlignment = HorizontalAlignment.Left;
            this.Primitive = PrimitiveHandler.GetRectangle(device);
        }

        public VisualNode(GraphicsDevice device, double width, double height, LogicalTree.LogicalNode lnode, Color backgroundColor, Color foregroundColor, Color borderColor, 
            string text = "", int fontSize = 12, int borderSize = 0, VerticalAlignment verticalTextAlignment = VerticalAlignment.Center, 
            HorizontalAlignment horizontalTextAlignment = HorizontalAlignment.Center, SpriteFont font = null)
        {
            graphicsDevice = device;
            this.LogicalNode = lnode;
            this.height = height;
            this.width = width;
            this.BackgroundColor = backgroundColor;
            this.ForegroundColor = foregroundColor;
            this.BorderColor = borderColor;
            this.text = text;
            this.fontSize = fontSize;
            this.borderSize = borderSize;
            this.verticalTextAlignment = verticalTextAlignment;
            this.horizontalTextAlignment = horizontalTextAlignment;
            renderTarget = new RenderTarget2D(device, (int)Math.Ceiling(width), (int)Math.Ceiling(height), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            if(font != null)
            {
                this.font = font;
            }
        }

        public void RecursiveDraw(SpriteBatch spriteBatch)
        {
            if (modified)
            {
                DrawToRenderTarget(spriteBatch);
                foreach (LogicalNode node in this.LogicalNode.Children) {
                    if(node.Visibility == Visibility.Hidden)
                    {
                        continue;
                    }
                    node.VisualNode.RecursiveDraw(spriteBatch);
                    graphicsDevice.SetRenderTarget(renderTarget);
                    spriteBatch.Begin();

                    double x, y;

                    if (node.VerticalAlignment == VerticalAlignment.Top || node.VerticalAlignment == VerticalAlignment.Stretch)
                    {
                        y = node.RelativePosition.Y;
                    }
                    else if (node.VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        y = Height - node.Height - node.RelativePosition.Y;
                    }
                    else
                    {
                        y = Height / 2 - node.Height / 2 + node.RelativePosition.Y;
                    }

                    if (node.HorizontalAlignment == HorizontalAlignment.Left || node.HorizontalAlignment == HorizontalAlignment.Stretch)
                    {
                        x = node.RelativePosition.X;
                    }
                    else if (node.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        x = Width - node.Width - node.RelativePosition.X;
                    }
                    else
                    {
                        x = Width / 2 - node.Width / 2 + node.RelativePosition.X;
                    }

                    spriteBatch.Draw(node.VisualNode.renderTarget, new Microsoft.Xna.Framework.Rectangle((int)x, (int)y, (int)node.Width, (int)node.Height), Color.White);
                    spriteBatch.End();
                }
                if (effects != null)
                {
                    foreach (Effects.BasicEffect effect in effects.Values)
                    {
                        if (!effect.Running)
                        {
                            effect.ApplyEffect(renderTarget, spriteBatch);
                        }
                    }
                }
                modified = false;
            }
        }

        public void DrawToRenderTarget(SpriteBatch spriteBatch)
        {
            if (invalidRenderTarget)
            {
                if(renderTarget != null)
                {
                    renderTarget.Dispose();
                }
                renderTarget = new RenderTarget2D(graphicsDevice, (int)Math.Ceiling(width), (int)Math.Ceiling(height), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            if (primitive != null)
            {
                if (borderSize > 0)
                {
                    if (this.LogicalNode is ImageNode)
                    {
                        spriteBatch.Draw(VisualPrimitives.PrimitiveHandler.GetRectangle(graphicsDevice), new Microsoft.Xna.Framework.Rectangle(0, 0, (int)width, (int)height), borderColor);
                    }
                    else
                    {
                        spriteBatch.Draw(primitive, new Microsoft.Xna.Framework.Rectangle(0, 0, (int)width, (int)height), borderColor);
                    }
                }
                spriteBatch.Draw(primitive, new Microsoft.Xna.Framework.Rectangle(borderSize, borderSize, (int)width - borderSize * 2, (int)height - borderSize * 2), backgroundColor);
            }
                if (!string.IsNullOrEmpty(text) && foregroundColor.A > 0 && font != null)
            {
                Vector2 fm = font.MeasureString("D");
                float scale = (float)fontSize / fm.X;
                fm.X *= scale;
                fm.Y *= scale;
                List<string> lines = GetLines(scale);
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    Vector2 size = font.MeasureString(line);
                    size = new Vector2(size.X * scale, size.Y * scale);
                    Vector2 pos = new Vector2();
                    if (horizontalTextAlignment == HorizontalAlignment.Right)
                    {
                        pos.X = (float)width - size.X - borderSize;
                    }
                    else if (horizontalTextAlignment == HorizontalAlignment.Center || horizontalTextAlignment == HorizontalAlignment.Stretch)
                    {
                        pos.X = (float)width / 2 - size.X / 2;
                    }
                    else
                    {
                        pos.X += borderSize;
                    }
                    if (verticalTextAlignment == VerticalAlignment.Bottom)
                    {
                        pos.Y = (float)height - size.Y;
                        pos.Y -= fm.Y * (lines.Count - i - 1) - borderSize;
                    }
                    else if (verticalTextAlignment == VerticalAlignment.Center || verticalTextAlignment == VerticalAlignment.Stretch)
                    {
                        pos.Y = (float)height / 2 - size.Y / 2;
                        pos.Y += fm.Y * i + fm.Y / 2 - fm.Y * lines.Count / 2;
                    }
                    else
                    {
                        pos.Y += fm.Y * i + borderSize;
                    }

                    if (pos.Y >= borderSize && pos.Y + size.Y <= height - borderSize)
                    {
                        spriteBatch.DrawString(font, line, pos, foregroundColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 1);
                    }
                }
            }
            spriteBatch.End();
        }

        public void Dispose()
        {
            renderTarget?.Dispose();
            if (LogicalNode is ImageNode)
            {
                primitive?.Dispose();
                primitive = null;
            }
            renderTarget = null;
            LogicalNode = null;
        }

        private List<string> GetLines(float scale)
        {
            List<string> lines = new List<string>();
            float px = 0;
            if (textWrapping)
            {
                StringBuilder sb = new StringBuilder();
                foreach(char c in text)
                {
                    if (c != '\n')
                    {
                        float charWidth = font.GetGlyphs()[c].WidthIncludingBearings * scale;
                        if (px + charWidth >= Width - borderSize * 2)
                        {
                            lines.Add(sb.ToString());
                            sb.Clear();
                            px = 0;
                        }
                        sb.Append(c);
                        px += charWidth;
                    }
                    else
                    {
                        lines.Add(sb.ToString());
                        sb.Clear();
                        px = 0;
                    }
                }
                lines.Add(sb.ToString());
            }
            else
            {
                lines.Add(text);
            }
            return lines;
        }
    }
}
