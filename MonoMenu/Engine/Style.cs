using Microsoft.Xna.Framework;
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
        bool setBorderColor, setBorderSize, setForeground, setBackground, setFontSize;
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
                setBackground = true;
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
                setBorderColor = true;
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
                setBorderSize = true;
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
                setFontSize = true;
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
                setForeground = true;
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
        public bool SetBorderColor
        {
            get
            {
                return setBorderColor;
            }
        }
        public bool SetBorderSize
        {
            get
            {
                return setBorderSize;
            }
        }
        public bool SetForeground
        {
            get
            {
                return setForeground;
            }
        }
        public bool SetBackground
        {
            get
            {
                return setBackground;
            }
        }
        public bool SetFontSize
        {
            get
            {
                return setFontSize;
            }
        }
    }
}
