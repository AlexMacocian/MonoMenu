using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoMenu.Engine.LogicalTree
{
    class EllipseNode : LogicalNode
    {
        public EllipseNode(GraphicsDevice device, string name, MonoMenu menu) : base(device, name, menu)
        {
            this.VisualNode.Primitive = VisualPrimitives.PrimitiveHandler.GetEllipse(device);
        }
    }
}
