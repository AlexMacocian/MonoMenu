using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine.Effects
{
    public class GaussianBlurEffect : BasicEffect
    {
        private int radius;

        public GaussianBlurEffect(int radius)
        {
            this.radius = radius;
        }

        public override void ApplyEffect(RenderTarget2D renderTarget)
        {
            running = true;
            var rct = new Rectangle(0, 0, renderTarget.Width, renderTarget.Height);

            int width = renderTarget.Width;
            int height = renderTarget.Height;

            int[] alpha = new int[width * height];
            int[] red = new int[width * height];
            int[] green = new int[width * height];
            int[] blue = new int[width * height];

            Color[] source = new Color[width * height];
            renderTarget.GetData<Color>(source);

            for(int i = 0; i < alpha.Length; i++)
            {
                alpha[i] = source[i].A;
                red[i] = source[i].R;
                green[i] = source[i].G;
                blue[i] = source[i].B;
            }

            var newAlpha = new int[width * height];
            var newRed = new int[width * height];
            var newGreen = new int[width * height];
            var newBlue = new int[width * height];
            var dest = new Color[width * height];

            gaussBlur_4(alpha, newAlpha, radius, width, height);
            gaussBlur_4(red, newRed, radius, width, height);
            gaussBlur_4(green, newGreen, radius, width, height);
            gaussBlur_4(blue, newBlue, radius, width, height);

            for(int i = 0; i < dest.Length; i++)
            {
                if (newAlpha[i] > 255) newAlpha[i] = 255;
                if (newRed[i] > 255) newRed[i] = 255;
                if (newGreen[i] > 255) newGreen[i] = 255;
                if (newBlue[i] > 255) newBlue[i] = 255;

                if (newAlpha[i] < 0) newAlpha[i] = 0;
                if (newRed[i] < 0) newRed[i] = 0;
                if (newGreen[i] < 0) newGreen[i] = 0;
                if (newBlue[i] < 0) newBlue[i] = 0;

                dest[i] = Color.FromNonPremultiplied(newRed[i], newGreen[i], newBlue[i], newAlpha[i]);
            }
            renderTarget.SetData(dest);
            base.ApplyEffect(renderTarget);
        }

        private void gaussBlur_4(int[] source, int[] dest, int r, int width, int height)
        {
            var bxs = boxesForGauss(r, 3);
            boxBlur_4(source, dest, width, height, (bxs[0] - 1) / 2);
            boxBlur_4(dest, source, width, height, (bxs[1] - 1) / 2);
            boxBlur_4(source, dest, width, height, (bxs[2] - 1) / 2);
        }

        private int[] boxesForGauss(int sigma, int n)
        {
            var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            var wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) wl--;
            var wu = wl + 2;

            var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = Math.Round(mIdeal);

            var sizes = new List<int>();
            for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
            return sizes.ToArray();
        }

        private void boxBlur_4(int[] source, int[] dest, int w, int h, int r)
        {
            for (var i = 0; i < source.Length; i++) dest[i] = source[i];
            boxBlurH_4(dest, source, w, h, r);
            boxBlurT_4(source, dest, w, h, r);
        }

        private void boxBlurH_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for(int i = 0; i < h; i++)
            {
                var ti = i * w;
                var li = ti;
                var ri = ti + r;
                var fv = source[ti];
                var lv = source[ti + w - 1];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri++] - fv;
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (var j = r + 1; j < w - r; j++)
                {
                    val += source[ri++] - dest[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (var j = w - r; j < w; j++)
                {
                    val += lv - source[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
            }
        }

        private void boxBlurT_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for(int i = 0; i < w; i++)
            {
                var ti = i;
                var li = ti;
                var ri = ti + r * w;
                var fv = source[ti];
                var lv = source[ti + w * (h - 1)];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j * w];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri] - fv;
                    dest[ti] = (int)Math.Round(val * iar);
                    ri += w;
                    ti += w;
                }
                for (var j = r + 1; j < h - r; j++)
                {
                    val += source[ri] - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ri += w;
                    ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ti += w;
                }
            }
        }
    }
}
