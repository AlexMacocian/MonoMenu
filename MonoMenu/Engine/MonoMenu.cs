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

namespace MonoMenu.Engine
{
    public class MonoMenu
    {
        private static Random random = new Random();

        public static Exception InvalidColor = new Exception("String could not be parsed into a valid color");
        public static SpriteFont defaultFont;
        public static int defaultFontSize = 48;
        private string filePath;
        private GraphicsDevice graphicsDevice;
        LogicalTree.LogicalNode root;
        public Dictionary<string, LogicalTree.LogicalNode> Nodes;
        public MonoMenu(GraphicsDevice device)
        {
            this.graphicsDevice = device;
            Nodes = new Dictionary<string, LogicalTree.LogicalNode>();
            MouseInput.LeftMouseButtonClick += LeftMouseButtonClick;
            MouseInput.LeftMouseButtonDoubleClick += LeftMouseButtonDoubleClick;
        }

        public void LoadXml(string xml)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            ParseDoc(doc);
        }

        public void Load(string filePath)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(filePath);
            ParseDoc(doc);
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

        private void ParseDoc(XmlDocument doc)
        {
            root = new LogicalTree.LogicalNode(graphicsDevice, MonoMenu.GenerateString(12), 0, 0, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
            foreach (XmlNode node in doc.ChildNodes)
            {
                ParseNode(node, root);
            }
        }

        private void ParseNode(XmlNode node, LogicalTree.LogicalNode parent)
        {
            List<XmlNode> ChildrenNodes = new List<XmlNode>();

            string primitive = node.Name;
            Assembly a = Assembly.GetExecutingAssembly();
            Type t = a.GetType("MonoMenu.Engine.VisualPrimitives." + primitive);
            if(t == null)
            {
                return;
            }
            object type = t.GetConstructor(new[] { typeof(GraphicsDevice) }).Invoke(new object[] { graphicsDevice });
            double rx = 0, ry = 0, width = 0, height = 0;
            int fontSize = 12, borderSize = 0;
            string name = null;
            string text = "";
            List<MenuEvent> events = new List<MenuEvent>();
            Color background = new Color();
            Color foreground = new Color();
            Color borderColor = new Color();
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
                else if(innerNode.Name == "HorizontalAlignment")
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
                else if(innerNode.Name == "Events")
                {
                    foreach(XmlNode eventNode in innerNode.ChildNodes)
                    {
                        if(eventNode.Name == "BasicEvent")
                        {
                            MenuEvent.Type evType = MenuEvent.Type.Click;
                            MenuEvent.Target evTarget = MenuEvent.Target.Background;
                            string value = string.Empty;
                            string evName = string.Empty;
                            string trigName = string.Empty;
                            foreach (XmlNode innerEventNode in eventNode.ChildNodes)
                            {
                                if(innerEventNode.Name == "Name")
                                {
                                    evName = innerEventNode.InnerText;
                                }
                                if(innerEventNode.Name == "Type")
                                {
                                    evType = (MenuEvent.Type)Enum.Parse(typeof(MenuEvent.Type), innerEventNode.InnerText);
                                }
                                else if(innerEventNode.Name == "Target")
                                {
                                    evTarget = (MenuEvent.Target)Enum.Parse(typeof(MenuEvent.Target), innerEventNode.InnerText);
                                }
                                else if(innerEventNode.Name == "Value")
                                {
                                    value = innerEventNode.InnerText;
                                }
                                else if(innerEventNode.Name == "Trigger")
                                {
                                    trigName = innerEventNode.InnerText;
                                }
                            }
                            if (string.IsNullOrEmpty(evName))
                            {
                                evName = MonoMenu.GenerateString(8);
                            }
                            events.Add(new BasicEvent(evName, evType, value, trigName,evTarget));
                        }
                        else if(eventNode.Name == "AnimatedEvent")
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
            if(name == null)
            {
                name = MonoMenu.GenerateString(12);
            }
            LogicalTree.LogicalNode lnode = new LogicalTree.LogicalNode(graphicsDevice, name, rx, ry, width, height, parent, background, foreground, borderColor, type as VisualPrimitives.VisualPrimitive, 
                verticalAlignment, horizontalAlignment, verticalTextAlignment, horizontalTextAlignment, fontSize, borderSize, percentageWidth, percentageHeight,
                percentageX, percentageY, text);
            lnode.Events = events;
            lnode.AutoArrangeChildren = autoArrange;
            lnode.Orientation = orientation;
            lnode.InvalidLayout = autoArrange;
            if (parent != null)
            {
                parent.Children.Add(lnode);
            }
            Nodes[lnode.Name] = lnode;
            foreach(XmlNode childNode in ChildrenNodes)
            {
                ParseNode(childNode, lnode);
            }
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
    }
}
