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
        public ImageNode(GraphicsDevice device, MonoMenu menu, string name, Texture2D image) : base(device, name, menu)
        {
            this.visualNode.Primitive = image;
            this.Background = Color.White;
        }

        public string Source
        {
            get
            {
                return visualNode.Primitive.Name;
            }
            set
            {
                menu.ContentManager.Load<Texture2D>(value);
            }
        }
    }
}
