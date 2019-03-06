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
    class VisualNode
    {
        private RenderTarget2D renderTarget;
        private GraphicsDevice graphicsDevice;
        private double width, height;
        private VisualPrimitive type;
        private LogicalNode LogicalNode;
        private Color backgroundColor, foregroundColor, borderColor;
        private string text = string.Empty;
        private bool modified = true;
        private int fontSize, borderSize;
        private HorizontalAlignment horizontalTextAlignment;
        private VerticalAlignment verticalTextAlignment;
        private Visibility visibility;

        public bool Modified
        {
            set
            {
                modified = value;
                if (value == true)
                {
                    if (LogicalNode.Parent != null)
                    {
                        LogicalNode.Parent.VisualNode.Modified = true;
                    }
                }
            }
            get
            {
                return modified;
            }
        }

        public VisualPrimitive Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
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
                if (renderTarget != null)
                {
                    renderTarget.Dispose();
                }
                if (width > 0 && height > 0)
                {
                    renderTarget = new RenderTarget2D(graphicsDevice, (int)Math.Ceiling(width), (int)Math.Ceiling(height), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                }
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
                if (renderTarget != null)
                {
                    renderTarget.Dispose();
                }
                if (width > 0 && height > 0)
                {
                    renderTarget = new RenderTarget2D(graphicsDevice, (int)Math.Ceiling(width), (int)Math.Ceiling(height), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                }
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
                modified = true;
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

        public VisualNode(GraphicsDevice device, LogicalNode lnode)
        {
            this.LogicalNode = lnode;
            this.graphicsDevice = device;
            this.BackgroundColor = Color.White;
            this.ForegroundColor = Color.Transparent;
            this.BorderColor = Color.Transparent;
            this.fontSize = 0;
            this.borderSize = 0;
            this.text = "";
            this.VerticalTextAlignment = VerticalAlignment.Top;
            this.HorizontalTextAlignment = HorizontalAlignment.Left;
            this.Type = new VisualPrimitives.Rectangle(device);
        }

        public VisualNode(GraphicsDevice device, double width, double height, LogicalTree.LogicalNode lnode, Color backgroundColor, Color foregroundColor, Color borderColor, 
            string text = "", int fontSize = 12, int borderSize = 0, VerticalAlignment verticalTextAlignment = VerticalAlignment.Center, 
            HorizontalAlignment horizontalTextAlignment = HorizontalAlignment.Center)
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
                modified = false;
            }
        }

        public void DrawToRenderTarget(SpriteBatch spriteBatch)
        {
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            spriteBatch.Draw(type.Texture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int)width, (int)height), borderColor);
            spriteBatch.Draw(type.Texture, new Microsoft.Xna.Framework.Rectangle(borderSize, borderSize, (int)width - borderSize * 2, (int)height - borderSize * 2), backgroundColor);
            if(!string.IsNullOrEmpty(text) && foregroundColor.A > 0)
            {
                float scale = (float)fontSize / Menu.defaultFontSize;
                Vector2 size = Menu.defaultFont.MeasureString(text);
                size = new Vector2(size.X * scale, size.Y * scale);
                spriteBatch.DrawString(Menu.defaultFont, text, new Vector2((int)(width/2 - size.X / 2), (int)(height / 2 - size.Y / 2)), foregroundColor, 0, new Vector2(0, 0), scale, SpriteEffects.None, 1);
            }
            spriteBatch.End();
        }
    }
}
