using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoMenu.Engine.LogicalTree
{
    class ImageNode : LogicalNode
    {
        public ImageNode(GraphicsDevice device, string name, double rx, double ry, double width, double height, Texture2D image) : base(device, name, rx, ry, width, height)
        {
            this.visualNode.Primitive = image;
            this.Background = Color.White;
        }

        public ImageNode(GraphicsDevice device, string name, double rx, double ry, double width, double height, LogicalNode parent, Color background, Color foreground, Color borderColor, Texture2D image, NodeProperties.VerticalAlignment verticalAlignment = NodeProperties.VerticalAlignment.Center, NodeProperties.HorizontalAlignment horizontalAlignment = NodeProperties.HorizontalAlignment.Center, NodeProperties.VerticalAlignment verticalTextAlignment = NodeProperties.VerticalAlignment.Center, NodeProperties.HorizontalAlignment horizontalTextAlignment = NodeProperties.HorizontalAlignment.Center, int fontSize = 0, int borderSize = 0, bool percentageWidth = false, bool percentageHeight = false, bool percentageX = false, bool percentageY = false, string text = "") : base(device, name, rx, ry, width, height, parent, background, foreground, borderColor, verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight, percentageX, percentageY, text)
        {
            this.visualNode.Primitive = image;
            this.Background = Color.White;
        }
    }
}
