using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMenu.Engine.NodeProperties;

namespace MonoMenu.Engine
{
    public class Style
    {
        public EventHandler StyleChanged;

        private string name;
        private SpriteFont font;
        private Color borderColor = Color.Transparent, foreground = Color.Transparent, background = Color.Transparent;
        private int borderSize = -1, fontSize = -1;
        private HorizontalAlignment horizontalAlignment, horizontalTextAlignment;
        private VerticalAlignment verticalAlignment, verticalTextAlignment;

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
        public HorizontalAlignment HorizontalAlignment
        {
            get => horizontalAlignment;
            set
            {
                horizontalAlignment = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public HorizontalAlignment HorizontalTextAlignment
        {
            get => horizontalTextAlignment;
            set
            {
                horizontalTextAlignment = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public VerticalAlignment VerticalAlignment
        {
            get => verticalAlignment; set
            {
                verticalAlignment = value;
                StyleChanged?.Invoke(this, null);
            }
        }
        public VerticalAlignment VerticalTextAlignment
        {
            get => verticalTextAlignment;
            set
            {
                verticalTextAlignment = value;
                StyleChanged?.Invoke(this, null);
            }
        }
    }
}
