using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine.Effects
{
    class BlurEffect : BasicEffect
    {
        public enum Kernel
        {
            Gaussian,
            Box
        }

        private int radius;
        private bool hardwareAccelerated;
        private Effect effect;
        private GraphicsDevice graphicsDevice;
        private Vector2[] offsetsHoriz, offsetsVert;
        private Kernel kernelType;

        public bool HardwareAccelerated { get => hardwareAccelerated; set => hardwareAccelerated = value; }
        public Kernel KernelType { get => kernelType; set => kernelType = value; }
        public int Radius { get => radius; set => radius = value; }

        public BlurEffect(GraphicsDevice device)
        {
            this.graphicsDevice = device;
            Stream s = new FileStream(System.IO.Path.GetFullPath(@"..\..\..\..\Engine\Effects\Blur.mgfxo"), FileMode.Open);
            BinaryReader Reader = new BinaryReader(s);
            effect = new Effect(device, Reader.ReadBytes((int)Reader.BaseStream.Length));
        }

        public override void ApplyEffect(RenderTarget2D renderTarget, SpriteBatch spriteBatch)
        {
            running = true;
            if (HardwareAccelerated)
            {
                ApplyGPUEffect(renderTarget, spriteBatch);
            }
            else
            {
                ApplyCPUEffect(renderTarget);
            }
            base.ApplyEffect(renderTarget, spriteBatch);
        }

        private void ApplyGPUEffect(RenderTarget2D renderTarget, SpriteBatch spriteBatch)
        {
            float[] kernel = ComputeKernel(Radius, 3);
            ComputeOffsets(renderTarget.Width, renderTarget.Height);

            effect.CurrentTechnique = effect.Techniques["GaussianBlur"];
            effect.Parameters["weights"].SetValue(kernel);
            effect.Parameters["colorMapTexture"].SetValue(renderTarget);
            effect.Parameters["offsets"].SetValue(offsetsHoriz);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, effect, null);
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
            spriteBatch.End();

            effect.Parameters["colorMapTexture"].SetValue(renderTarget);
            effect.Parameters["offsets"].SetValue(offsetsVert);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, effect, null);
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
            spriteBatch.End();
        }

        private void ApplyCPUEffect(RenderTarget2D renderTarget)
        {
            running = true;
            if (kernelType == Kernel.Gaussian)
            {
                var rct = new Rectangle(0, 0, renderTarget.Width, renderTarget.Height);

                int width = renderTarget.Width;
                int height = renderTarget.Height;

                int[] alpha = new int[width * height];
                int[] red = new int[width * height];
                int[] green = new int[width * height];
                int[] blue = new int[width * height];

                Color[] source = new Color[width * height];
                renderTarget.GetData<Color>(source);

                for (int i = 0; i < alpha.Length; i++)
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

                GaussBlur(alpha, newAlpha, Radius, width, height);
                GaussBlur(red, newRed, Radius, width, height);
                GaussBlur(green, newGreen, Radius, width, height);
                GaussBlur(blue, newBlue, Radius, width, height);

                for (int i = 0; i < dest.Length; i++)
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
            }
            else if(kernelType == Kernel.Box)
            {
                var rct = new Rectangle(0, 0, renderTarget.Width, renderTarget.Height);

                int width = renderTarget.Width;
                int height = renderTarget.Height;

                int[] alpha = new int[width * height];
                int[] red = new int[width * height];
                int[] green = new int[width * height];
                int[] blue = new int[width * height];

                Color[] source = new Color[width * height];
                renderTarget.GetData<Color>(source);

                for (int i = 0; i < alpha.Length; i++)
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

                BoxBlur(alpha, newAlpha, width, height, radius);
                BoxBlur(red, newRed, width, height, radius);
                BoxBlur(green, newGreen, width, height, radius);
                BoxBlur(blue, newBlue, width, height, radius);

                for (int i = 0; i < dest.Length; i++)
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
            }
        }

        private void GaussBlur(int[] source, int[] dest, int r, int width, int height)
        {
            var bxs = BoxesForGauss(r, 3);
            BoxBlur(source, dest, width, height, ((int)bxs[0] - 1) / 2);
            BoxBlur(dest, source, width, height, ((int)bxs[1] - 1) / 2);
            BoxBlur(source, dest, width, height, ((int)bxs[2] - 1) / 2);
        }

        private void BoxBlur(int[] source, int[] dest, int w, int h, int r)
        {
            for (var i = 0; i < source.Length; i++) dest[i] = source[i];
            BoxBlurH(dest, source, w, h, r);
            BoxBlurT(source, dest, w, h, r);
        }

        private void BoxBlurH(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for (int i = 0; i < h; i++)
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

        private void BoxBlurT(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for (int i = 0; i < w; i++)
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

        private float[] ComputeKernel(int sigma, int n)
        {
            float[] kernel = new float[n * 2 + 1];
            float total = 0.0f;

            if (kernelType == Kernel.Gaussian)
            {
                sigma = n;

                float twoSigmaSquare = 2.0f * sigma * sigma;
                float sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
                float distance = 0.0f;
                int index = 0;

                for (int i = -n; i <= n; ++i)
                {
                    distance = i * i;
                    index = i + n;
                    kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                    total += kernel[index];
                }

                for (int i = 0; i < kernel.Length; ++i)
                    kernel[i] /= total;
            }
            else
            {
                for (int i = 0; i < kernel.Length; ++i)
                {
                    kernel[i] = 1;
                    total++;
                }

                for (int i = 0; i < kernel.Length; ++i)
                    kernel[i] /= total;
            }
            return kernel;
        }

        private float[] BoxesForGauss(float sigma, int n)  // standard deviation, number of boxes
        {
            var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);  // Ideal averaging filter width 
            var wl = Math.Floor(wIdeal); if (wl % 2 == 0) wl--;
            var wu = wl + 2;

            var mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = Math.Round(mIdeal);
            // var sigmaActual = Math.sqrt( (m*wl*wl + (n-m)*wu*wu - n)/12 );

            float[] sizes = new float[n]; for (var i = 0; i < n; i++) sizes[i] = (float)(i < m ? wl : wu);
            return sizes;
        }

        private void ComputeOffsets(float textureWidth, float textureHeight)
        {
            offsetsHoriz = new Vector2[Radius * 2 + 1];

            offsetsVert = new Vector2[Radius * 2 + 1];

            int index = 0;
            float xOffset = 1.0f / textureWidth;
            float yOffset = 1.0f / textureHeight;

            for (int i = -Radius; i <= Radius; ++i)
            {
                index = i + Radius;
                offsetsHoriz[index] = new Vector2(i * xOffset, 0.0f);
                offsetsVert[index] = new Vector2(0.0f, i * yOffset);
            }
        }
    }
}
