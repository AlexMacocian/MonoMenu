using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMenu.Engine.NodeProperties;

namespace MonoMenu.Engine.LogicalTree
{
    public class RectangleNode : LogicalNode
    {
        public RectangleNode(GraphicsDevice device, string name, double rx, double ry, double width, double height):base(device, name, rx, ry, width, height)
        {
            this.visualNode.Primitive = VisualPrimitives.PrimitiveHandler.GetRectangle(device);
        }

        public RectangleNode(GraphicsDevice device, string name, double rx, double ry, double width, double height,
            LogicalNode parent,
            Color background, Color foreground, Color borderColor,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment verticalTextAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalTextAlignment = HorizontalAlignment.Center,
            int fontSize = 0, int borderSize = 0,
            bool percentageWidth = false, bool percentageHeight = false, bool percentageX = false, bool percentageY = false,
            string text = "", SpriteFont font = null) : base(device, name, rx, ry, width, height, parent, background, foreground, borderColor, verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight, percentageX, percentageY, text, font)
        {
            this.visualNode.Primitive = VisualPrimitives.PrimitiveHandler.GetRectangle(device);
        }
    }
}
