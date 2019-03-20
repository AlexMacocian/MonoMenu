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
        public RectangleNode(GraphicsDevice device, string name, MonoMenu menu) : base(device, name, menu)
        {
            this.visualNode.Primitive = VisualPrimitives.PrimitiveHandler.GetRectangle(device);
        }
    }
}
