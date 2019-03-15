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
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            spriteBatch.Draw(root.VisualNode.RenderTarget, new Rectangle(root.AbsolutePosition.X, root.AbsolutePosition.Y, (int)root.DesiredWidth, (int)root.DesiredHeight), Color.White);
            spriteBatch.End();
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
            root = new LogicalTree.LogicalNode(graphicsDevice, this, MonoMenu.GenerateString(12), 0, 0, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                if (node.Name == "Rectangle" ||
                    node.Name == "Ellipse")
                    ParseNodeXml(node, root);
                else if (node.Name == "Style")
                    ParseStyleXml(node);

            }
        }

        private void ParseXamlDoc(XmlDocument doc)
        {
            root = new LogicalTree.LogicalNode(graphicsDevice, this, MonoMenu.GenerateString(12), 0, 0, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                if (node.Name == "Rectangle" ||
                    node.Name == "Ellipse")
                    ParseNodeXaml(node, root);
                else if (node.Name == "Style")
                    ParseStyleXaml(node);

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
            List<MenuEvent> events = new List<MenuEvent>();
            Color background = new Color();
            Color foreground = new Color();
            Color borderColor = new Color();
            Style style = null;
            SpriteFont font = null;
            bool percentageX = false, percentageY = false, percentageWidth = false, percentageHeight = false, autoArrange = false;
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
                }
                else if (attribute.Name == "Background")
                {
                    background = MonoMenu.ColorFromString(attribute.Value);
                }
                else if (attribute.Name == "Foreground")
                {
                    foreground = MonoMenu.ColorFromString(attribute.Value);
                }
                else if (attribute.Name == "VerticalAlignment")
                {
                    verticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), attribute.Value);
                }
                else if (attribute.Name == "HorizontalAlignment")
                {
                    horizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), attribute.Value);
                }
                else if (attribute.Name == "VerticalTextAlignment")
                {
                    verticalTextAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), attribute.Value);
                }
                else if (attribute.Name == "HorizontalTextAlignment")
                {
                    horizontalTextAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), attribute.Value);
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
                }
                else if (attribute.Name == "BorderSize")
                {
                    borderSize = int.Parse(attribute.Value);
                }
                else if (attribute.Name == "BorderColor")
                {
                    borderColor = MonoMenu.ColorFromString(attribute.Value);
                }
                else if (attribute.Name == "Visibility")
                {
                    visibility = (Visibility)Enum.Parse(typeof(Visibility), attribute.Value);
                }
                else if (attribute.Name == "AutoArrange")
                {
                    autoArrange = bool.Parse(attribute.Value);
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
                            events.Add(new BasicEvent(evName, evType, value, trigName, evTarget));
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
                            events.Add(new AnimatedEvent(evName, evType, evTarget, from, to, duration, trigName));
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
            }


            if (name == null)
            {
                name = MonoMenu.GenerateString(12);
            }
            LogicalTree.LogicalNode lnode = null;
            if (primitive == "Rectangle")
            {
                lnode = new LogicalTree.RectangleNode(graphicsDevice, this, name, rx, ry, width, height, parent, background, foreground, borderColor,
                verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight,
                percentageX, percentageY, text, font);
            }
            else if (primitive == "Ellipse")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, this, name, rx, ry, width, height, parent, background, foreground, borderColor,
                verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight,
                percentageX, percentageY, text, font);
            }
            else if (primitive == "Circle")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, this, name, rx, ry, radius * 2, radius * 2, parent, background, foreground, borderColor,
                verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight,
                percentageX, percentageY, text, font);
            }
            else if (primitive == "Image")
            {
                lnode = new LogicalTree.ImageNode(graphicsDevice, this, name, rx, ry, width, height, parent, background, foreground, borderColor,
                    contentManager.Load<Texture2D>(source), verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment,
                    fontSize, borderSize, percentageWidth, percentageHeight, percentageX, percentageY, text, font);
            }
            if (style != null)
            {
                lnode.Style = style;
            }
            lnode.Events = events;
            lnode.AutoArrangeChildren = autoArrange;
            lnode.Orientation = orientation;
            lnode.InvalidLayout = autoArrange;
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
            List<MenuEvent> events = new List<MenuEvent>();
            Color background = new Color();
            Color foreground = new Color();
            Color borderColor = new Color();
            Style style = null;
            SpriteFont font = null;
            bool percentageX = false, percentageY = false, percentageWidth = false, percentageHeight = false, autoArrange = false;
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
                }
                else if (innerNode.Name == "Background")
                {
                    background = MonoMenu.ColorFromString(innerNode.InnerText);
                }
                else if (innerNode.Name == "Foreground")
                {
                    foreground = MonoMenu.ColorFromString(innerNode.InnerText);
                }
                else if (innerNode.Name == "VerticalAlignment")
                {
                    verticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), innerNode.InnerText);
                }
                else if (innerNode.Name == "HorizontalAlignment")
                {
                    horizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), innerNode.InnerText);
                }
                else if (innerNode.Name == "VerticalTextAlignment")
                {
                    verticalTextAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), innerNode.InnerText);
                }
                else if (innerNode.Name == "HorizontalTextAlignment")
                {
                    horizontalTextAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), innerNode.InnerText);
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
                }
                else if (innerNode.Name == "BorderSize")
                {
                    borderSize = int.Parse(innerNode.InnerText);
                }
                else if (innerNode.Name == "BorderColor")
                {
                    borderColor = MonoMenu.ColorFromString(innerNode.InnerText);
                }
                else if (innerNode.Name == "Visibility")
                {
                    visibility = (Visibility)Enum.Parse(typeof(Visibility), innerNode.InnerText);
                }
                else if (innerNode.Name == "AutoArrange")
                {
                    autoArrange = bool.Parse(innerNode.InnerText);
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
                            events.Add(new BasicEvent(evName, evType, value, trigName, evTarget));
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
                            events.Add(new AnimatedEvent(evName, evType, evTarget, from, to, duration, trigName));
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
            }
            if (name == null)
            {
                name = MonoMenu.GenerateString(12);
            }
            LogicalTree.LogicalNode lnode = null;
            if (primitive == "Rectangle")
            {
                lnode = new LogicalTree.RectangleNode(graphicsDevice, this, name, rx, ry, width, height, parent, background, foreground, borderColor,
                verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight,
                percentageX, percentageY, text, font);
            }
            else if(primitive == "Ellipse")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, this, name, rx, ry, width, height, parent, background, foreground, borderColor,
                verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight,
                percentageX, percentageY, text, font);
            }
            else if(primitive == "Circle")
            {
                lnode = new LogicalTree.EllipseNode(graphicsDevice, this, name, rx, ry, radius * 2, radius * 2, parent, background, foreground, borderColor,
                verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight,
                percentageX, percentageY, text, font);
            }
            else if(primitive == "Image")
            {
                lnode = new LogicalTree.ImageNode(graphicsDevice, this, name, rx, ry, width, height, parent, background, foreground, borderColor, 
                    contentManager.Load<Texture2D>(source), verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, 
                    fontSize, borderSize, percentageWidth, percentageHeight, percentageX, percentageY, text, font);
            }
            if (style != null)
            {
                lnode.Style = style;
            }
            lnode.Events = events;
            lnode.AutoArrangeChildren = autoArrange;
            lnode.Orientation = orientation;
            lnode.InvalidLayout = autoArrange;
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
            Color borderColor = Color.Transparent, background = Color.Transparent, foreground = Color.Transparent;
            int borderSize = -1, fontSize = -1;
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
            if (borderColor != Color.Transparent)
            {
                style.BorderColor = borderColor;
            }
            if (foreground != Color.Transparent)
            {
                style.Foreground = foreground;
            }
            if (background != Color.Transparent)
            {
                style.Background = background;
            }
            if (borderSize != -1)
            {
                style.BorderSize = borderSize;
            }
            if (fontSize != -1)
            {
                style.FontSize = fontSize;
            }
            styles.Add(style);
        }

        private void ParseStyleXml(XmlNode node)
        {
            string name = string.Empty;
            Color borderColor = Color.Transparent, background = Color.Transparent, foreground = Color.Transparent;
            int borderSize = -1, fontSize = -1;
            foreach (XmlNode innerNode in node)
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
            if (borderColor != Color.Transparent)
            {
                style.BorderColor = borderColor;
            }
            if (foreground != Color.Transparent)
            {
                style.Foreground = foreground;
            }
            if (background != Color.Transparent)
            {
                style.Background = background;
            }
            if (borderSize != -1)
            {
                style.BorderSize = borderSize;
            }
            if (fontSize != -1)
            {
                style.FontSize = fontSize;
            }
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
