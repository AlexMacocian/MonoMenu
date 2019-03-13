using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMenu.Engine
{
    public class Style
    {
        public EventHandler StyleChanged;

        private string name;
        private SpriteFont font;
        private Color borderColor = Color.Transparent, foreground = Color.Transparent, background = Color.Transparent;
        private int borderSize = -1, fontSize = -1;

        public Style(string name)
        {
            this.name = name;
        }
        public Color Background
        {
            get
            {
                return background;
            }

            set
            {
                background = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public Color BorderColor
        {
            get
            {
                return borderColor;
            }

            set
            {
                borderColor = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public int BorderSize
        {
            get
            {
                return borderSize;
            }

            set
            {
                borderSize = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public int FontSize
        {
            get
            {
                return fontSize;
            }

            set
            {
                fontSize = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public Color Foreground
        {
            get
            {
                return foreground;
            }

            set
            {
                foreground = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
        public SpriteFont Font
        {
            get
            {
                return font;
            }

            set
            {
                font = value;
                StyleChanged?.Invoke(this, null);
            }
        }
    }
}
