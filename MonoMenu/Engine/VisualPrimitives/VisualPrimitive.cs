using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine.VisualPrimitives
{
    public class VisualPrimitive
    {
        protected Texture2D texture;
        protected GraphicsDevice graphicsDevice;
        protected bool dirty = false;
        public VisualPrimitive(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public Texture2D Texture
        {
            get
            {
                if(texture == null || dirty)
                {
                    MakeTexture();
                }
                return texture;
            }
        }

        protected virtual void MakeTexture()
        {
            if(texture != null) {
                texture.Dispose();
            }
            texture = new Texture2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        }
    }
}
