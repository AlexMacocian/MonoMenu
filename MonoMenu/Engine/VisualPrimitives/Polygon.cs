using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MonoMenu.Engine.VisualPrimitives
{
    class Polygon : VisualPrimitive
    {
        List<Point> points;
        public Polygon(GraphicsDevice graphicsDevice, List<Point> points) : base(graphicsDevice)
        {
        }

        protected override void MakeTexture()
        {
            base.MakeTexture();
            Color[] colorData = new Color[graphicsDevice.PresentationParameters.BackBufferWidth * graphicsDevice.PresentationParameters.BackBufferHeight];

            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;

            for (int i = 0; i < width; i++)
            {
                int dx = i - width / 2;
                int x = width / 2 + dx;

                int h = (int)Math.Round(height * Math.Sqrt(width * width / 4.0 - dx * dx) / width);
                for (int dy = 1; dy < h; dy++)
                {
                    colorData[x + (height / 2 + dy) * width] = Color.White;
                    colorData[x + (height / 2 - dy) * width] = Color.White;
                }

                if (h >= 0)
                {
                    colorData[x + (height / 2) * width] = Color.White;
                }
            }

            texture.SetData(colorData);
        }
    }
}
