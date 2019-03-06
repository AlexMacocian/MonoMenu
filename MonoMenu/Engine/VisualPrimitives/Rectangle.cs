using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MonoMenu.Engine.VisualPrimitives
{
    class Rectangle : VisualPrimitive
    {
        public Rectangle(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        protected override void MakeTexture()
        {
            base.MakeTexture();
            Color[] colorData = new Color[graphicsDevice.PresentationParameters.BackBufferWidth * graphicsDevice.PresentationParameters.BackBufferHeight];

            for (int x = 0; x < graphicsDevice.PresentationParameters.BackBufferWidth; x++)
            {
                for (int y = 0; y < graphicsDevice.PresentationParameters.BackBufferHeight; y++)
                {
                    colorData[y * graphicsDevice.PresentationParameters.BackBufferWidth + x] = Color.White;
                }
            }
            texture.SetData(colorData);
        }
    }
}
