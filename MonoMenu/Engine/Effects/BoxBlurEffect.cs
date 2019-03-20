using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoMenu.Engine.Effects
{
    class BoxBlurEffect : BasicEffect
    {
        private int radius;
        public BoxBlurEffect(int radius)
        {
            this.radius = radius;
        }

        public override void ApplyEffect(RenderTarget2D renderTarget)
        {
            running = true;
            Color[] source = new Color[renderTarget.Width * renderTarget.Height];
            renderTarget.GetData<Color>(source);
            Color[] dest = new Color[renderTarget.Width * renderTarget.Height];
            for(int i = radius / 2; i < renderTarget.Width - radius / 2; i++)
            {
                for(int j = radius / 2; j < renderTarget.Height - radius / 2; j++)
                {
                    int a = 0, r = 0, g = 0, b = 0, div = 0;
                    for(int x = i - radius / 2; x < i + radius / 2; x++)
                    {
                        for(int y = j - radius / 2; y < j + radius / 2; y++)
                        {
                            Color t = GetColor(source, x, y, renderTarget.Width);
                            a += t.A;
                            r += t.R;
                            g += t.G;
                            b += t.B;
                            div++;
                        }
                    }
                    Color color = new Color(r / div, g / div, b / div, a / div);
                    SetColor(dest, color, i, j, renderTarget.Width);
                }
            }
            renderTarget.SetData(dest);
            base.ApplyEffect(renderTarget);
        }

        private Color GetColor(Color[] data, int x, int y, int width)
        {
            return data[y * width + x];
        }

        private void SetColor(Color[] data, Color value, int x, int y, int width)
        {
            data[y * width + x] = value;
        }
    }
}
