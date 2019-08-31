using MonoMenu.Engine.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static MonoMenu.Engine.NodeProperties;
using Microsoft.Xna.Framework.Content;
using System.IO;
using MonoMenu.Engine.Effects;

namespace MonoMenu.Engine
{
    public class MonoMenu : IDisposable
    {
        private static Random random = new Random();

        public static Exception InvalidColor = new Exception("String could not be parsed into a valid color");
        public static SpriteFont defaultFont;
        public static int defaultFontSize = 48;
        private string filePath;
        private GraphicsDevice graphicsDevice;
        private ContentManager contentManager;
        LogicalTree.LogicalNode root;
        private Dictionary<string, LogicalTree.LogicalNode> nodes;
        private List<Style> styles;

        public List<Style> Styles
        {
            get
            {
                return styles;
            }

            set
            {
                styles = value;
            }
        }

        public ContentManager ContentManager { get => contentManager; set => contentManager = value; }

        public LogicalTree.LogicalNode this[string key]
        {
            get
            {
                return nodes[key];
            }
            set
            {
                nodes[key] = value;
            }
        }

        public MonoMenu(GraphicsDevice device, ContentManager contentManager)
        {
            this.graphicsDevice = device;
            this.contentManager = contentManager;
            nodes = new Dictionary<string, LogicalTree.LogicalNode>();
            Styles = new List<Style>();
            MouseInput.LeftMouseButtonClick += LeftMouseButtonClick;
            MouseInput.LeftMouseButtonDoubleClick += LeftMouseButtonDoubleClick;
        }

        public void LoadXml(string xml)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            ParseXmlDoc(doc);
        }

        public void LoadXaml(string xaml)
        {
            System.Xml.XmlDocument doc = new XmlDocument();
            doc.LoadXml(xaml);
            ParseXamlDoc(doc);
        }

        public void Load(string filePath)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(filePath);
            if (Path.GetExtension(filePath).ToLower() == ".xml")
            {
                ParseXmlDoc(doc);
            }
            else if(Path.GetExtension(filePath).ToLower() == ".xaml")
            {
                ParseXamlDoc(doc);
            }
        }

        public void Update(GameTime gameTime)
        {
            MouseInput.Poll(gameTime);
            PropagateMouse();
            Queue queue = new Queue();
            queue.Enqueue(root);
            while(queue.Count > 0)
            {
                LogicalTree.LogicalNode ln = (LogicalTree.LogicalNode)queue.Dequeue();
                ln.Update(gameTime);
                foreach(LogicalTree.LogicalNode child in ln.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, null);
        }

        public void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            root.VisualNode.RecursiveDraw(spriteBatch);
            graphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
            spriteBatch.Draw(root.VisualNode.RenderTarget, new Rectangle(root.AbsolutePosition.X, root.AbsolutePosition.Y, (int)root.DesiredWidth, (int)root.DesiredHeight), Color.White);
            spriteBatch.End();
        }

        public void OnTextInput(TextInputEventArgs e)
        {
            foreach(KeyValuePair<string, LogicalTree.LogicalNode> kp in nodes)
            {
                if (kp.Value.Focused)
                {
                    kp.Value.OnTextChange(e);
                }
            }
        }

        public LogicalTree.LogicalNode FindNode(string name)
        {
            Queue q = new Queue();
            q.Enqueue(root);

            while(q.Count > 0)
            {
                LogicalTree.LogicalNode node = (LogicalTree.LogicalNode)q.Dequeue();
                if(node.Name == name)
                {
                    return node;
                }
                foreach(LogicalTree.LogicalNode child in node.Children)
                {
                    q.Enqueue(child);
                }
            }
            return null;
        }

        public void Resize(double width, double height)
        {
            root.Resize(width, height);
        }

        public void Resize()
        {
            Resize(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        }

        private void ParseXmlDoc(XmlDocument doc)
        {
            root = new LogicalTree.LogicalNode(graphicsDevice, MonoMenu.GenerateString(12), this);
            root.DesiredWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            root.DesiredHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {                 
                if (node.Name == "Style")
                    ParseStyleXml(node);
                else
                    ParseNodeXml(node, root);

            }
        }

        private void ParseXamlDoc(XmlDocument doc)
        {
            root = new LogicalTree.LogicalNode(graphicsDevice, MonoMenu.GenerateString(12), this);
            root.DesiredWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            root.DesiredHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {                    
                if (node.Name == "Style")
                    ParseStyleXaml(node);
                else
                    ParseNodeXaml(node, root);

            }
        }

        private void ParseNodeXaml(XmlNode node, LogicalTree.LogicalNode parent)
        {
            List<XmlNode> ChildrenNodes = new List<XmlNode>();

            string primitive = node.Name;
            double rx = 0, ry = 0, width = 0, height = 0, radius = 0;
            int fontSize = 12, borderSize = 0;
            string name = null;
            string text = "", source = "";
            Dictionary<string, MenuEvent> events = new Dictionary<string, MenuEvent>();
            Dictionary<string, Effects.BasicEffect> effects = new Dictionary<string, Effects.BasicEffect>();
            Color background = new Color();
            Color foreground = new Color();
            Color borderColor = new Color();
            Style style = null;
            SpriteFont font = null;
            bool percentageX = false, percentageY = false, percentageWidth = false, percentageHeight = false, autoArrange = false,
                setBackgroundColor = false, setForegroundColor = false, setBorderColor = false,
                setFont = false, setBorderSize = false, setFontSize = false, setHorizontalAlignment = false, setVerticalAlignment = false,
                setHorizontalTextAlignment = false, setVerticalTextAlignment = false, textWrapping = false;
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, verticalTextAlignment = VerticalAlignment.Center;
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, horizontalTextAlignment = HorizontalAlignment.Center;
            LogicalTree.LogicalNode.NodeOrientation orientation = LogicalTree.LogicalNode.NodeOrientation.Vertical;
            Visibility visibility = Visibility.Visible;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.Name == "RelativeX")
                {
                    string s = attribute.Value;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageX = true;
                        s = s.Remove(s.Length - 1);
                    }
                    rx = double.Parse(s);
                }
                else if (attribute.Name == "RelativeY")
                {
                    string s = attribute.Value;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageY = true;
                        s = s.Remove(s.Length - 1);
                    }
                    ry = double.Parse(s);
                }
                else if (attribute.Name == "Name")
                {
                    name = attribute.Value;
                }
                else if (attribute.Name == "Width")
                {
                    string s = attribute.Value;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageWidth = true;
                        s = s.Remove(s.Length - 1);
                    }
                    width = double.Parse(s);
                }
                else if (attribute.Name == "Height")
                {
                    string s = attribute.Value;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageHeight = true;
                        s = s.Remove(s.Length - 1);
                    }
                    height = double.Parse(s);
                }
                else if (attribute.Name == "Radius")
                {
                    string s = attribute.Value;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageHeight = true;
                        percentageWidth = true;
                        s = s.Remove(s.Length - 1);
                    }
                    radius = double.Parse(s);
                }
                else if (attribute.Name == "Style")
                {
                    style = styles.Find(s => s.Name == attribute.Value);
                }
                else if (attribute.Name == "Font")
                {
                    font = contentManager.Load<SpriteFont>(attribute.Value);
                    setFont = true;
                }
                else if (attribute.Name == "Background")
                {
                    background = MonoMenu.ColorFromString(attribute.Value);
                    setBackgroundColor = true;
                }
                else if (attribute.Name == "Foreground")
                {
                    foreground = MonoMenu.ColorFromString(attribute.Value);
                    setForegroundColor = true;
                }
                else if (attribute.Name == "VerticalAlignment")
                {
                    verticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), attribute.Value);
                    setVerticalAlignment = true;
                }
                else if (attribute.Name == "HorizontalAlignment")
                {
                    horizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), attribute.Value);
                    setHorizontalAlignment = true;
                }
                else if (attribute.Name == "VerticalTextAlignment")
                {
                    verticalTextAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), attribute.Value);
                    setVerticalTextAlignment = true;
                }
                else if (attribute.Name == "HorizontalTextAlignment")
                {
                    horizontalTextAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), attribute.Value);
                    setHorizontalTextAlignment = true;
                }
                else if (attribute.Name == "Text")
                {
                    text = attribute.Value;
                }
                else if (attribute.Name == "Source")
                {
                    source = attribute.Value;
                }
                else if (attribute.Name == "FontSize")
                {
                    fontSize = int.Parse(attribute.Value);
                    setFontSize = true;
                }
                else if (attribute.Name == "BorderSize")
                {
                    borderSize = int.Parse(attribute.Value);
                    setBorderSize = true;
                }
                else if (attribute.Name == "BorderColor")
                {
                    borderColor = MonoMenu.ColorFromString(attribute.Value);
                    setBorderColor = true;
                }
                else if (attribute.Name == "Visibility")
                {
                    visibility = (Visibility)Enum.Parse(typeof(Visibility), attribute.Value);
                }
                else if (attribute.Name == "AutoArrange")
                {
                    autoArrange = bool.Parse(attribute.Value);
                }
                else if (attribute.Name == "TextWrapping")
                {
                    textWrapping = bool.Parse(attribute.Value);
                }
                else if (attribute.Name == "Orientation")
                {
                    orientation = (LogicalTree.LogicalNode.NodeOrientation)Enum.Parse(typeof(LogicalTree.LogicalNode.NodeOrientation), attribute.Value);
                }
            }

            foreach(XmlNode innerNode in node.ChildNodes)
            {
                if (innerNode.Name == "Events")
                {
                    foreach (XmlNode eventNode in innerNode.ChildNodes)
                    {
                        if (eventNode.Name == "BasicEvent")
                        {
                            MenuEvent.Type evType = MenuEvent.Type.Click;
                            MenuEvent.Target evTarget = MenuEvent.Target.Background;
                            string value = string.Empty;
                            string evName = string.Empty;
                            string trigName = string.Empty;
                            foreach (XmlAttribute eventAttribute in eventNode.Attributes)
                            {
                                if (eventAttribute.Name == "Name")
                                {
                                    evName = eventAttribute.Value;
                                }
                                if (eventAttribute.Name == "Type")
                                {
                                    evType = (MenuEvent.Type)Enum.Parse(typeof(MenuEvent.Type), eventAttribute.Value);
                                }
                                else if (eventAttribute.Name == "Target")
                                {
                                    evTarget = (MenuEvent.Target)Enum.Parse(typeof(MenuEvent.Target), eventAttribute.Value);
                                }
                                else if (eventAttribute.Name == "Value")
                                {
                                    value = eventAttribute.Value;
                                }
                                else if (eventAttribute.Name == "Trigger")
                                {
                                    trigName = eventAttribute.Value;
                                }
                            }
                            if (string.IsNullOrEmpty(evName))
                            {
                                evName = MonoMenu.GenerateString(8);
                            }
                            events[evName] = (new BasicEvent(evName, evType, value, trigName, evTarget));
                        }
                        else if (eventNode.Name == "AnimatedEvent")
                        {
                            MenuEvent.Type evType = MenuEvent.Type.Click;
                            MenuEvent.Target evTarget = MenuEvent.Target.Background;
                            string from = string.Empty, to = string.Empty, duration = string.Empty, evName = string.Empty;
                            string trigName = string.Empty;
                            foreach (XmlAttribute eventAttribute in eventNode.Attributes)
                            {
                                if (eventAttribute.Name == "Name")
                                {
                                    evName = eventAttribute.Value;
                                }
                                if (eventAttribute.Name == "Type")
                                {
                                    evType = (MenuEvent.Type)Enum.Parse(typeof(MenuEvent.Type), eventAttribute.Value);
                                }
                                else if (eventAttribute.Name == "Target")
                                {
                                    evTarget = (MenuEvent.Target)Enum.Parse(typeof(MenuEvent.Target), eventAttribute.Value);
                                }
                                else if (eventAttribute.Name == "From")
                                {
                                    from = eventAttribute.Value;
                                }
                                else if (eventAttribute.Name == "To")
                                {
                                    to = eventAttribute.Value;
                                }
                                else if (eventAttribute.Name == "Duration")
                                {
                                    duration = eventAttribute.Value;
                                }
                                else if (eventAttribute.Name == "Trigger")
                                {
                                    trigName = eventAttribute.Value;
                                }
                            }
                            if (string.IsNullOrEmpty(evName))
                            {
                                evName = MonoMenu.GenerateString(8);
                            }
                            events[evName] = (new AnimatedEvent(evName, evType, evTarget, from, to, duration, trigName));
                        }
                    }
                }
                else if (innerNode.Name == "Children")
                {
                    foreach (XmlNode childNode in innerNode)
                    {
                        ChildrenNodes.Add(childNode);
                    }
                }
                else if (innerNode.Name == "Effects")
                {
                    foreach(XmlNode effectNode in innerNode.ChildNodes)
                    {
                        if(effectNode.Name == "Blur")
                        {
                            int bradius = 0;
                            BlurEffect.Kernel kernel = BlurEffect.Kernel.Gaussian;
                            bool hardwareAccelerated = false;
                            string efname = GenerateString(8);
                            foreach(XmlAttribute effectAttribute in effectNode.Attributes)
                            {
                                if(effectAttribute.Name == "Radius")
                                {
                                    bradius = int.Parse(effectAttribute.Value); 
                                }
                                else if(effectAttribute.Name == "Kernel")
                                {
                                    kernel = (BlurEffect.Kernel)Enum.Parse(typeof(BlurEffect.Kernel), effectAttribute.Value);
                                }
                                else if(effectAttribute.Name == "HardwareAccelerated")
                                {
                                    hardwareAccelerated = bool.Parse(effectAttribute.Value);
                                }
                                else if(effectAttribute.Name == "Name")
                                {
                                    efname = effectAttribute.Value;
                                }
                            }
                            BlurEffect blurEffect = new BlurEffect(efname, graphicsDevice);
                            blurEffect.Radius = bradius;
                            blurEffect.HardwareAccelerated = hardwareAccelerated;
                            blurEffect.KernelType = kernel;
                            effects[efname] = (blurEffect);
                        }
                    }
                }
            }


            if (name == null)
            {
                name = MonoMenu.GenerateString(12);
            }
            LogicalTree.LogicalNode lnode = null;
            if (primitive == "Rectangle")
            {
                lnode = new LogicalTree.RectangleNode(graphicsDevice, name, this);
            }
            else if (primitive == "Ellipse")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, name, this);
            }
            else if (primitive == "Circle")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, name, this);
                height = radius * 2;
                width = radius * 2;
            }
            else if (primitive == "Image")
            {
                lnode = new LogicalTree.ImageNode(graphicsDevice, this, name, contentManager.Load<Texture2D>(source));
            }
            else if (primitive == "Textbox")
            {
                lnode = new LogicalTree.TextboxNode(graphicsDevice, name, this);
            }
            if (style != null)
            {
                lnode.Style = style;
            }
            lnode.PercentageHeight = percentageHeight;
            lnode.PercentageWidth = percentageWidth;
            lnode.PercentageX = percentageX;
            lnode.PercentageY = percentageY;
            lnode.Parent = parent;
            lnode.DesiredHeight = height;
            lnode.DesiredWidth = width;
            lnode.DesiredRelativePosition = new Point((int)Math.Round(rx), (int)Math.Round(ry));
            if (setBackgroundColor)
            {
                lnode.Background = background;
            }
            if (setForegroundColor)
            {
                lnode.Foreground = foreground;
            }
            if (setBorderColor)
            {
                lnode.BorderColor = borderColor;
            }
            if (setFont)
            {
                lnode.Font = font;
            }
            if (setFontSize)
            {
                lnode.FontSize = fontSize;
            }
            if (setBorderSize)
            {
                lnode.BorderSize = borderSize;
            }
            if (setHorizontalAlignment)
            {
                lnode.HorizontalAlignment = horizontalAlignment;
            }
            if (setHorizontalTextAlignment)
            {
                lnode.HorizontalTextAlignment = horizontalTextAlignment;
            }
            if (setVerticalAlignment)
            {
                lnode.VerticalAlignment = verticalAlignment;
            }
            if (setVerticalTextAlignment)
            {
                lnode.VerticalTextAlignment = verticalTextAlignment;
            }
            lnode.Events = events;
            lnode.Effects = effects;
            lnode.AutoArrangeChildren = autoArrange;
            lnode.TextWrapping = textWrapping;
            lnode.Orientation = orientation;
            lnode.InvalidLayout = autoArrange;
            lnode.OnFocusChange += NodeFocused;
            lnode.Text = text;
            if (parent != null)
            {
                parent.Children.Add(lnode);
            }
            nodes[lnode.Name] = lnode;
            foreach (XmlNode childNode in ChildrenNodes)
            {
                ParseNodeXaml(childNode, lnode);
            }
        }

        private void ParseNodeXml(XmlNode node, LogicalTree.LogicalNode parent)
        {
            List<XmlNode> ChildrenNodes = new List<XmlNode>();

            string primitive = node.Name;
            double rx = 0, ry = 0, width = 0, height = 0, radius = 0;
            int fontSize = 12, borderSize = 0;
            string name = null;
            string text = "", source = "";
            Dictionary<string, MenuEvent> events = new Dictionary<string, MenuEvent>();
            Dictionary<string, Effects.BasicEffect> effects = new Dictionary<string, Effects.BasicEffect>();
            Color background = new Color();
            Color foreground = new Color();
            Color borderColor = new Color();
            Style style = null;
            SpriteFont font = null;
            bool percentageX = false, percentageY = false, percentageWidth = false, percentageHeight = false, autoArrange = false,
                setBackgroundColor = false, setForegroundColor = false, setBorderColor = false,
                setFont = false, setBorderSize = false, setFontSize = false, setHorizontalAlignment = false, setVerticalAlignment = false,
                setHorizontalTextAlignment = false, setVerticalTextAlignment = false, textWrapping = false;
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, verticalTextAlignment = VerticalAlignment.Center;
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, horizontalTextAlignment = HorizontalAlignment.Center;
            LogicalTree.LogicalNode.NodeOrientation orientation = LogicalTree.LogicalNode.NodeOrientation.Vertical;
            Visibility visibility = Visibility.Visible;
            foreach (XmlNode innerNode in node.ChildNodes)
            {
                if (innerNode.Name == "RelativeX")
                {
                    string s = innerNode.InnerText;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageX = true;
                        s = s.Remove(s.Length - 1);
                    }
                    rx = double.Parse(s);
                }
                else if (innerNode.Name == "RelativeY")
                {
                    string s = innerNode.InnerText;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageY = true;
                        s = s.Remove(s.Length - 1);
                    }
                    ry = double.Parse(s);
                }
                else if (innerNode.Name == "Name")
                {
                    name = innerNode.InnerText;
                }
                else if (innerNode.Name == "Width")
                {
                    string s = innerNode.InnerText;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageWidth = true;
                        s = s.Remove(s.Length - 1);
                    }
                    width = double.Parse(s);
                }
                else if (innerNode.Name == "Height")
                {
                    string s = innerNode.InnerText;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageHeight = true;
                        s = s.Remove(s.Length - 1);
                    }
                    height = double.Parse(s);
                }
                else if (innerNode.Name == "Radius")
                {
                    string s = innerNode.InnerText;
                    s = s.Replace(" ", "");
                    if (s[s.Length - 1] == '%')
                    {
                        percentageHeight = true;
                        percentageWidth = true;
                        s = s.Remove(s.Length - 1);
                    }
                    radius = double.Parse(s);
                }
                else if (innerNode.Name == "Style")
                {
                    style = styles.Find(s => s.Name == innerNode.InnerText);
                }
                else if (innerNode.Name == "Font")
                {
                    font = contentManager.Load<SpriteFont>(innerNode.InnerText);
                    setFont = true;
                }
                else if (innerNode.Name == "Background")
                {
                    background = MonoMenu.ColorFromString(innerNode.InnerText);
                    setBackgroundColor = true;
                }
                else if (innerNode.Name == "Foreground")
                {
                    foreground = MonoMenu.ColorFromString(innerNode.InnerText);
                    setForegroundColor = true;
                }
                else if (innerNode.Name == "VerticalAlignment")
                {
                    verticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), innerNode.InnerText);
                    setVerticalAlignment = true;
                }
                else if (innerNode.Name == "HorizontalAlignment")
                {
                    horizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), innerNode.InnerText);
                    setHorizontalAlignment = true;
                }
                else if (innerNode.Name == "VerticalTextAlignment")
                {
                    verticalTextAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), innerNode.InnerText);
                    setVerticalTextAlignment = true;
                }
                else if (innerNode.Name == "HorizontalTextAlignment")
                {
                    horizontalTextAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), innerNode.InnerText);
                    setHorizontalTextAlignment = true;
                }
                else if (innerNode.Name == "Text")
                {
                    text = innerNode.InnerText;
                }
                else if (innerNode.Name == "Source")
                {
                    source = innerNode.InnerText;
                }
                else if (innerNode.Name == "FontSize")
                {
                    fontSize = int.Parse(innerNode.InnerText);
                    setFontSize = true;
                }
                else if (innerNode.Name == "BorderSize")
                {
                    borderSize = int.Parse(innerNode.InnerText);
                    setBorderSize = true;
                }
                else if (innerNode.Name == "BorderColor")
                {
                    borderColor = MonoMenu.ColorFromString(innerNode.InnerText);
                    setBorderColor = true;
                }
                else if (innerNode.Name == "Visibility")
                {
                    visibility = (Visibility)Enum.Parse(typeof(Visibility), innerNode.InnerText);
                }
                else if (innerNode.Name == "AutoArrange")
                {
                    autoArrange = bool.Parse(innerNode.InnerText);
                }
                else if (innerNode.Name == "TextWrapping")
                {
                    textWrapping = bool.Parse(innerNode.InnerText);
                }
                else if (innerNode.Name == "Orientation")
                {
                    orientation = (LogicalTree.LogicalNode.NodeOrientation)Enum.Parse(typeof(LogicalTree.LogicalNode.NodeOrientation), innerNode.InnerText);
                }
                else if (innerNode.Name == "Events")
                {
                    foreach (XmlNode eventNode in innerNode.ChildNodes)
                    {
                        if (eventNode.Name == "BasicEvent")
                        {
                            MenuEvent.Type evType = MenuEvent.Type.Click;
                            MenuEvent.Target evTarget = MenuEvent.Target.Background;
                            string value = string.Empty;
                            string evName = string.Empty;
                            string trigName = string.Empty;
                            foreach (XmlNode innerEventNode in eventNode.ChildNodes)
                            {
                                if (innerEventNode.Name == "Name")
                                {
                                    evName = innerEventNode.InnerText;
                                }
                                if (innerEventNode.Name == "Type")
                                {
                                    evType = (MenuEvent.Type)Enum.Parse(typeof(MenuEvent.Type), innerEventNode.InnerText);
                                }
                                else if (innerEventNode.Name == "Target")
                                {
                                    evTarget = (MenuEvent.Target)Enum.Parse(typeof(MenuEvent.Target), innerEventNode.InnerText);
                                }
                                else if (innerEventNode.Name == "Value")
                                {
                                    value = innerEventNode.InnerText;
                                }
                                else if (innerEventNode.Name == "Trigger")
                                {
                                    trigName = innerEventNode.InnerText;
                                }
                            }
                            if (string.IsNullOrEmpty(evName))
                            {
                                evName = MonoMenu.GenerateString(8);
                            }
                            events[evName] = (new BasicEvent(evName, evType, value, trigName, evTarget));
                        }
                        else if (eventNode.Name == "AnimatedEvent")
                        {
                            MenuEvent.Type evType = MenuEvent.Type.Click;
                            MenuEvent.Target evTarget = MenuEvent.Target.Background;
                            string from = string.Empty, to = string.Empty, duration = string.Empty, evName = string.Empty;
                            string trigName = string.Empty;
                            foreach (XmlNode innerEventNode in eventNode.ChildNodes)
                            {
                                if (innerEventNode.Name == "Name")
                                {
                                    evName = innerEventNode.InnerText;
                                }
                                if (innerEventNode.Name == "Type")
                                {
                                    evType = (MenuEvent.Type)Enum.Parse(typeof(MenuEvent.Type), innerEventNode.InnerText);
                                }
                                else if (innerEventNode.Name == "Target")
                                {
                                    evTarget = (MenuEvent.Target)Enum.Parse(typeof(MenuEvent.Target), innerEventNode.InnerText);
                                }
                                else if (innerEventNode.Name == "From")
                                {
                                    from = innerEventNode.InnerText;
                                }
                                else if (innerEventNode.Name == "To")
                                {
                                    to = innerEventNode.InnerText;
                                }
                                else if (innerEventNode.Name == "Duration")
                                {
                                    duration = innerEventNode.InnerText;
                                }
                                else if (innerEventNode.Name == "Trigger")
                                {
                                    trigName = innerEventNode.InnerText;
                                }
                            }
                            if (string.IsNullOrEmpty(evName))
                            {
                                evName = MonoMenu.GenerateString(8);
                            }
                            events[evName] = (new AnimatedEvent(evName, evType, evTarget, from, to, duration, trigName));
                        }
                    }
                }
                else if (innerNode.Name == "Children")
                {
                    foreach (XmlNode childNode in innerNode)
                    {
                        ChildrenNodes.Add(childNode);
                    }
                }
                else if (innerNode.Name == "Effects")
                {
                    foreach(XmlNode effectNode in innerNode.ChildNodes)
                    {
                        if(effectNode.Name == "Blur")
                        {
                            int bradius = 0;
                            BlurEffect.Kernel kernel = BlurEffect.Kernel.Gaussian;
                            bool hardwareAccelerated = false;
                            string efname = GenerateString(8);
                            foreach(XmlNode innerEffectNode in effectNode.ChildNodes)
                            {
                                if(innerEffectNode.Name == "Radius")
                                {
                                    bradius = int.Parse(innerEffectNode.InnerText);
                                }
                                else if(innerEffectNode.Name == "Kernel")
                                {
                                    kernel = (BlurEffect.Kernel)Enum.Parse(typeof(BlurEffect.Kernel) ,innerEffectNode.InnerText);
                                }
                                else if(innerEffectNode.Name == "HardwareAccelerated")
                                {
                                    hardwareAccelerated = bool.Parse(innerEffectNode.InnerText);
                                }
                                else if(innerEffectNode.Name == "Name")
                                {
                                    efname = innerEffectNode.InnerText;
                                }
                            }
                            BlurEffect effect = new BlurEffect(efname, graphicsDevice);
                            effect.Radius = bradius;
                            effect.HardwareAccelerated = hardwareAccelerated;
                            effect.KernelType = kernel;
                            effects[efname] = (effect);
                        }
                    }
                }
            }
            if (name == null)
            {
                name = MonoMenu.GenerateString(12);
            }
            LogicalTree.LogicalNode lnode = null;
            if (primitive == "Rectangle")
            {
                lnode = new LogicalTree.RectangleNode(graphicsDevice, name, this);
            }
            else if (primitive == "Ellipse")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, name, this);
            }
            else if (primitive == "Circle")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, name, this);
                height = radius * 2;
                width = radius * 2;
            }
            else if (primitive == "Image")
            {
                lnode = new LogicalTree.ImageNode(graphicsDevice, this, name, contentManager.Load<Texture2D>(source));
            }
            else if (primitive == "Textbox")
            {
                lnode = new LogicalTree.TextboxNode(graphicsDevice, name, this);
            }
            if (style != null)
            {
                lnode.Style = style;
            }
            lnode.PercentageHeight = percentageHeight;
            lnode.PercentageWidth = percentageWidth;
            lnode.PercentageX = percentageX;
            lnode.PercentageY = percentageY;
            lnode.Parent = parent;
            lnode.DesiredHeight = height;
            lnode.DesiredWidth = width;
            lnode.DesiredRelativePosition = new Point((int)Math.Round(rx), (int)Math.Round(ry));
            if (setBackgroundColor)
            {
                lnode.Background = background;
            }
            if (setForegroundColor)
            {
                lnode.Foreground = foreground;
            }
            if (setBorderColor)
            {
                lnode.BorderColor = borderColor;
            }
            if (setFont)
            {
                lnode.Font = font;
            }
            if (setFontSize)
            {
                lnode.FontSize = fontSize;
            }
            if (setBorderSize)
            {
                lnode.BorderSize = borderSize;
            }
            if (setHorizontalAlignment)
            {
                lnode.HorizontalAlignment = horizontalAlignment;
            }
            if (setHorizontalTextAlignment)
            {
                lnode.HorizontalTextAlignment = horizontalTextAlignment;
            }
            if (setVerticalAlignment)
            {
                lnode.VerticalAlignment = verticalAlignment;
            }
            if (setVerticalTextAlignment)
            {
                lnode.VerticalTextAlignment = verticalTextAlignment;
            }
            lnode.Events = events;
            lnode.Effects = effects;
            lnode.AutoArrangeChildren = autoArrange;
            lnode.Orientation = orientation;
            lnode.InvalidLayout = autoArrange;
            lnode.TextWrapping = textWrapping;
            lnode.OnFocusChange += NodeFocused;
            lnode.Text = text;
            if (parent != null)
            {
                parent.Children.Add(lnode);
            }
            nodes[lnode.Name] = lnode;
            foreach (XmlNode childNode in ChildrenNodes)
            {
                ParseNodeXml(childNode, lnode);
            }
        }

        private void ParseStyleXaml(XmlNode node)
        {
            string name = string.Empty;
            SpriteFont font = null;
            Color borderColor = Color.Transparent, background = Color.Transparent, foreground = Color.Transparent;
            int borderSize = 0, fontSize = 0;
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, verticalTextAlignment = VerticalAlignment.Center;
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, horizontalTextAlignment = HorizontalAlignment.Center;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.Name == "Name")
                {
                    name = attribute.Value;
                }
                else if (attribute.Name == "BorderColor")
                {
                    borderColor = ColorFromString(attribute.Value);
                }
                else if (attribute.Name == "BorderSize")
                {
                    borderSize = int.Parse(attribute.Value);
                }
                else if (attribute.Name == "FontSize")
                {
                    fontSize = int.Parse(attribute.Value);
                }
                else if (attribute.Name == "Foreground")
                {
                    foreground = ColorFromString(attribute.Value);
                }
                else if (attribute.Name == "Background")
                {
                    background = ColorFromString(attribute.Value);
                }
                else if(attribute.Name == "Font")
                {
                    font = ContentManager.Load<SpriteFont>(attribute.Value);
                }
                else if(attribute.Name == "VerticalAlignment")
                {
                    verticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), attribute.Value);
                }
                else if(attribute.Name == "VerticalTextAlignment")
                {
                    verticalTextAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), attribute.Value);
                }
                else if(attribute.Name == "HorizontalAlignment")
                {
                    horizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), attribute.Value);
                }
                else if(attribute.Name == "HorizontalTextAlignment")
                {
                    horizontalTextAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), attribute.Value);
                }
            }
            Style style;
            if (string.IsNullOrEmpty(name))
            {
                style = new Style(GenerateString(8));
            }
            else
            {
                style = new Style(name);
            }
            style.BorderColor = borderColor;
            style.Foreground = foreground;
            style.Background = background;
            style.BorderSize = borderSize;
            style.FontSize = fontSize;
            style.Font = font;
            style.VerticalAlignment = verticalAlignment;
            style.VerticalTextAlignment = verticalTextAlignment;
            style.HorizontalAlignment = horizontalAlignment;
            style.HorizontalTextAlignment = horizontalTextAlignment;
            styles.Add(style);
        }

        private void ParseStyleXml(XmlNode node)
        {
            string name = string.Empty;
            SpriteFont font = null;
            Color borderColor = Color.Transparent, background = Color.Transparent, foreground = Color.Transparent;
            int borderSize = 0, fontSize = 0;
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, verticalTextAlignment = VerticalAlignment.Center;
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, horizontalTextAlignment = HorizontalAlignment.Center;
            foreach (XmlNode innerNode in node.ChildNodes)
            {
                if (innerNode.Name == "Name")
                {
                    name = innerNode.InnerText;
                }
                else if (innerNode.Name == "BorderColor")
                {
                    borderColor = ColorFromString(innerNode.InnerText);
                }
                else if (innerNode.Name == "BorderSize")
                {
                    borderSize = int.Parse(innerNode.InnerText);
                }
                else if (innerNode.Name == "FontSize")
                {
                    fontSize = int.Parse(innerNode.InnerText);
                }
                else if (innerNode.Name == "Foreground")
                {
                    foreground = ColorFromString(innerNode.InnerText);
                }
                else if (innerNode.Name == "Background")
                {
                    background = ColorFromString(innerNode.InnerText);
                }
                else if (innerNode.Name == "Font")
                {
                    font = ContentManager.Load<SpriteFont>(innerNode.InnerText);
                }
                else if (innerNode.Name == "VerticalAlignment")
                {
                    verticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), innerNode.InnerText);
                }
                else if (innerNode.Name == "VerticalTextAlignment")
                {
                    verticalTextAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), innerNode.InnerText);
                }
                else if (innerNode.Name == "HorizontalAlignment")
                {
                    horizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), innerNode.InnerText);
                }
                else if (innerNode.Name == "HorizontalTextAlignment")
                {
                    horizontalTextAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), innerNode.InnerText);
                }
            }
            Style style;
            if (string.IsNullOrEmpty(name))
            {
                style = new Style(GenerateString(8));
            }
            else
            {
                style = new Style(name);
            }
            style.BorderColor = borderColor;
            style.Foreground = foreground;
            style.Background = background;
            style.BorderSize = borderSize;
            style.FontSize = fontSize;
            style.Font = font;
            style.VerticalAlignment = verticalAlignment;
            style.VerticalTextAlignment = verticalTextAlignment;
            style.HorizontalAlignment = horizontalAlignment;
            style.HorizontalTextAlignment = horizontalTextAlignment;
            styles.Add(style);
        }

        private void LeftMouseButtonClick(object sender, object args)
        {
            root.PropagateClick(MouseInput.MousePosition);
        }

        private void LeftMouseButtonDoubleClick(object sender, object args)
        {
            root.PropagateDoubleClick(MouseInput.MousePosition);
        }

        private void PropagateMouse()
        {
            root.PropagateMouse(MouseInput.MousePosition);
        }

        private void NodeFocused(object sender, FocusChange focusChangeArg)
        {
            if (focusChangeArg == FocusChange.GainedFocus)
            {
                LogicalTree.LogicalNode focusedNode = sender as LogicalTree.LogicalNode;
                System.Diagnostics.Debug.WriteLine(focusedNode.Name);
                foreach (KeyValuePair<string, LogicalTree.LogicalNode> kp in nodes)
                {
                    LogicalTree.LogicalNode node = kp.Value;
                    if (node != focusedNode)
                    {
                        node.Focused = false;
                    }
                }
            }
        }

        public static Color ColorFromString(string colorcode)
        {
            if (colorcode[0] == '#')
            {
                return ColorFromHex(colorcode);
            }
            else
            {
                Type t = typeof(Color);
                try
                {
                    Color c = (Color)t.GetProperty(colorcode, BindingFlags.Static | BindingFlags.Public).GetValue(null);
                    return c;
                }
                catch
                {
                    throw InvalidColor;
                }
            }
        }

        public static Color ColorFromHex(string colorcode)
        {
            if (colorcode[0] == '#')
            {
                colorcode = colorcode.Remove(0, 1);
            }
            if (colorcode.Length == 8)
            {
                return Color.FromNonPremultiplied(int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(6, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber));
            }
            else
            {
                return Color.FromNonPremultiplied(
                int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber), 255);
            }
        }

        public static string GenerateString(int length)
        {

            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }
            return result.ToString();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach(KeyValuePair<string, LogicalTree.LogicalNode> kp in nodes)
                    {
                        kp.Value.Dispose();
                    }
                }
                nodes.Clear();
                root = null;
                MouseInput.LeftMouseButtonClick -= LeftMouseButtonClick;
                MouseInput.LeftMouseButtonDoubleClick -= LeftMouseButtonDoubleClick;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MonoMenu() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
