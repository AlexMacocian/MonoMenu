using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine.VisualPrimitives
{
    class PrimitiveHandler
    {
        private static Texture2D rectangle, ellipse;
        public static Texture2D GetRectangle(GraphicsDevice device)
        {
            if(rectangle == null)
            {
                rectangle = new Texture2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);
                Color[] colorData = new Color[device.PresentationParameters.BackBufferWidth * device.PresentationParameters.BackBufferHeight];

                for (int x = 0; x < device.PresentationParameters.BackBufferWidth; x++)
                {
                    for (int y = 0; y < device.PresentationParameters.BackBufferHeight; y++)
                    {
                        colorData[y * device.PresentationParameters.BackBufferWidth + x] = Color.White;
                    }
                }
                rectangle.SetData(colorData);
            }
            return rectangle;
        }

        public static Texture2D GetEllipse(GraphicsDevice device)
        {
            if(ellipse == null)
            {
                ellipse = new Texture2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);
                Color[] colorData = new Color[device.PresentationParameters.BackBufferWidth * device.PresentationParameters.BackBufferHeight];

                int width = device.PresentationParameters.BackBufferWidth;
                int height = device.PresentationParameters.BackBufferHeight;

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
                ellipse.SetData(colorData);
            }
            return ellipse;
        }
    }
}
