using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine.Effects
{
    public class BasicEffect
    {
        protected bool running = false;
        public BasicEffect()
        {
        }

        public bool Running { get => running;}

        public virtual void ApplyEffect(RenderTarget2D renderTarget, SpriteBatch spriteBatch)
        {
            running = false;
        }
    }
}
