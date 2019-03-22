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
        private string name;
        public BasicEffect(string name)
        {
            this.name = name;
        }

        public bool Running { get => running;}
        public string Name { get => name;}

        public virtual void ApplyEffect(RenderTarget2D renderTarget, SpriteBatch spriteBatch)
        {
            running = false;
        }
    }
}
