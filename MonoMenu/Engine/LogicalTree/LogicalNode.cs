using MonoMenu.Engine.Animations;
using MonoMenu.Engine.Events;
using MonoMenu.Engine.VisualTree;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoMenu.Engine.NodeProperties;

namespace MonoMenu.Engine.LogicalTree
{
    public class LogicalNode
    {
        public enum NodeOrientation
        {
            Horizontal,
            Vertical
        }
        protected EventHandler MouseOver, Click, DoubleClick, Dragging, MouseEnter, MouseLeave, LeftMouseDown, LeftMouseUp,
            RightMouseDown, RightMouseUp, OnResize;
        protected double desiredWidth, desiredHeight;
        protected VisualNode visualNode;
        protected LogicalNode parent;
        protected Point relativePosition, previousAbsolutePosition, desiredRelativePosition;
        protected bool recalculateAbsolutePosition = true, mouseOver, leftMousePressed, rightMousePressed,
            percentageX = false, percentageY = false, percentageWidth = false, percentageHeight = false, invalidLayout = true, 
            autoArrangeChildren = false;
        protected VerticalAlignment verticalAlignment;
        protected HorizontalAlignment horizontalAlignment;
        protected List<MenuEvent> events, eventsToTrigger;
        protected string name;
        protected Style style;
        protected List<BaseAnimation> animations, animationsToRemove;
        protected List<LogicalNode> children;
        protected NodeOrientation orientation= NodeOrientation.Vertical;
        protected MonoMenu menu;

        public LogicalNode Parent
        {
            get
            {
                return parent;
            }

            set
            {
                parent = value;
            }
        }
        public Point RelativePosition
        {
            get
            {
                return relativePosition;
            }
        }
        public Point DesiredRelativePosition
        {
            get
            {
                return desiredRelativePosition;
            }
            set
            {
                desiredRelativePosition = value;
                int rx = desiredRelativePosition.X, ry = desiredRelativePosition.Y;
                if (percentageX)
                {
                    rx = desiredRelativePosition.X * (int)this.parent.Width / 100;
                }
                if (percentageY)
                {
                    ry = desiredRelativePosition.Y * (int)this.parent.Height / 100;
                }
                relativePosition = new Point(rx, ry);
            }
        }
        public Point AbsolutePosition
        {
            get
            {
                if (recalculateAbsolutePosition) 
                {
                    if(parent == null)
                    {
                        recalculateAbsolutePosition = false;
                        previousAbsolutePosition = relativePosition;
                        return previousAbsolutePosition;
                    }

                    double x, y;

                    if (VerticalAlignment == VerticalAlignment.Top || VerticalAlignment == VerticalAlignment.Stretch)
                    {
                        y = parent.AbsolutePosition.Y + relativePosition.Y;
                    }
                    else if (VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        y = parent.AbsolutePosition.Y + parent.Height - this.Height - relativePosition.Y;
                    }
                    else
                    {
                        y = parent.AbsolutePosition.Y + parent.Height / 2 - this.Height / 2 + relativePosition.Y;
                    }

                    if (HorizontalAlignment == HorizontalAlignment.Left || HorizontalAlignment == HorizontalAlignment.Stretch)
                    {
                        x = parent.AbsolutePosition.X + relativePosition.X;
                    }
                    else if (HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        x = parent.AbsolutePosition.X + parent.Width - this.Width - relativePosition.X;
                    }
                    else
                    {
                        x = parent.AbsolutePosition.X + parent.Width / 2 - this.Width / 2 + relativePosition.X;
                    }

                    previousAbsolutePosition.X = (int)x;
                    previousAbsolutePosition.Y = (int)y;
                    recalculateAbsolutePosition = false;
                }
                return previousAbsolutePosition;
            }
        }
        public double DesiredWidth
        {
            get
            {
                return desiredWidth;
            }
            set
            {
                desiredWidth = value;
                if (percentageWidth)
                {
                    visualNode.Width = this.Parent.Width * desiredWidth / 100;
                }
                else
                {
                    visualNode.Width = value;
                }
                if(VerticalAlignment != VerticalAlignment.Top)
                {
                    recalculateAbsolutePosition = true;
                }
                foreach(LogicalNode child in Children)
                {
                    child.recalculateAbsolutePosition = true;
                }
                OnResize?.Invoke(this, null);
            }
        }
        public double DesiredHeight
        {
            get
            {
                return desiredHeight;
            }
            set
            {
                desiredHeight = value;
                if (percentageHeight)
                {
                    visualNode.Height = this.Parent.Height * desiredHeight / 100;
                }
                else
                {
                    visualNode.Height = value;
                }
                if(HorizontalAlignment != HorizontalAlignment.Left)
                {
                    recalculateAbsolutePosition = true;
                }
                foreach (LogicalNode child in Children)
                {
                    child.recalculateAbsolutePosition = true;
                }
                OnResize?.Invoke(this, null);
            }
        }
        public double Width
        {
            get
            {
                return VisualNode.Width;
            }
        }
        public double Height
        {
            get
            {
                return VisualNode.Height;
            }
        }
        public VisualNode VisualNode
        {
            get
            {
                return visualNode;
            }

            set
            {
                visualNode = value;
            }
        }
        public bool RecalculateAbsolutePosition
        {
            get
            {
                return recalculateAbsolutePosition;
            }
        }
        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return verticalAlignment;
            }

            set
            {
                verticalAlignment = value;
                recalculateAbsolutePosition = true;
            }
        }
        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return horizontalAlignment;
            }

            set
            {
                horizontalAlignment = value;
                recalculateAbsolutePosition = true;
            }
        }
        public VerticalAlignment VerticalTextAlignment
        {
            get
            {
                return VisualNode.VerticalTextAlignment;
            }
            set
            {
                VisualNode.VerticalTextAlignment = value;
            }
        }
        public HorizontalAlignment HorizontalTextAlignment
        {
            get
            {
                return VisualNode.HorizontalTextAlignment;
            }
            set
            {
                VisualNode.HorizontalTextAlignment = value;
            }
        }
        public Visibility Visibility
        {
            get
            {
                return VisualNode.Visibility;
            }
            set
            {
                VisualNode.Visibility = value;
            }
        }
        public List<MenuEvent> Events
        {
            get
            {
                return events;
            }

            set
            {
                events = value;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
        public Color Background
        {
            get
            {
                return VisualNode.BackgroundColor;
            }
            set
            {
                VisualNode.BackgroundColor = value;
            }
        }
        public Color Foreground
        {
            get
            {
                return VisualNode.ForegroundColor;
            }
            set
            {
                VisualNode.ForegroundColor = value;
            }
        }
        public SpriteFont Font
        {
            get
            {
                return visualNode.Font;
            }
            set
            {
                visualNode.Font = value;
            }
        }
        public int FontSize
        {
            get
            {
                return VisualNode.FontSize;
            }
            set
            {
                VisualNode.FontSize = value;
            }
        }
        public string Text
        {
            get
            {
                return VisualNode.Text;
            }
            set
            {
                VisualNode.Text = value;
            }
        }
        public Color BorderColor
        {
            get
            {
                return VisualNode.BorderColor;
            }
            set
            {
                VisualNode.BorderColor = value;
            }
        }
        public int BorderSize
        {
            get
            {
                return VisualNode.BorderSize;
            }
            set
            {
                VisualNode.BorderSize = value;
            }
        }
        public bool PercentageX
        {
            get
            {
                return percentageX;
            }

            set
            {
                if(value != percentageX)
                {
                    
                }
                percentageX = value;
            }
        }
        public bool PercentageY
        {
            get
            {
                return percentageY;
            }

            set
            {
                percentageY = value;
            }
        }
        public bool PercentageWidth
        {
            get
            {
                return percentageWidth;
            }

            set
            {
                percentageWidth = value;
            }
        }
        public bool PercentageHeight
        {
            get
            {
                return percentageHeight;
            }

            set
            {
                percentageHeight = value;
            }
        }
        public List<LogicalNode> Children
        {
            get
            {
                return children;
            }

            set
            {
                children = value;
            }
        }
        public bool InvalidLayout
        {
            get
            {
                return invalidLayout;
            }

            set
            {
                invalidLayout = value;
            }
        }
        public bool AutoArrangeChildren
        {
            get
            {
                return autoArrangeChildren;
            }

            set
            {
                autoArrangeChildren = value;
            }
        }
        public NodeOrientation Orientation
        {
            get
            {
                return orientation;
            }

            set
            {
                orientation = value;
            }
        }
        public Style Style
        {
            get
            {
                return style;
            }

            set
            {
                if(style != null)
                {
                    style.StyleChanged -= StyleChanged;
                }
                style = value;
                style.StyleChanged += StyleChanged;
                StyleChanged(style, null);
            }
        }

        public LogicalNode(GraphicsDevice device, MonoMenu menu,string name, double rx, double ry, double width, double height)
        {
            Children = new List<LogicalNode>();
            this.menu = menu;
            Events = new List<MenuEvent>();
            animations = new List<BaseAnimation>();
            animationsToRemove = new List<BaseAnimation>();
            eventsToTrigger = new List<MenuEvent>();
            this.name = name;
            this.VisualNode = new VisualNode(device, this);
            this.DesiredWidth = width;
            this.DesiredHeight = height;
            this.DesiredRelativePosition = new Point((int)Math.Round(rx), (int)Math.Round(ry));
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
        }

        public LogicalNode(GraphicsDevice device, MonoMenu menu,string name, double rx, double ry, double width, double height,
            LogicalNode parent,
            Color background, Color foreground, Color borderColor, 
            VerticalAlignment verticalAlignment = VerticalAlignment.Center, 
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment verticalTextAlignment = VerticalAlignment.Center, 
            HorizontalAlignment horizontalTextAlignment = HorizontalAlignment.Center,
            int fontSize = 0, int borderSize = 0, 
            bool percentageWidth = false, bool percentageHeight = false, bool percentageX = false, bool percentageY = false, 
            string text = "", SpriteFont font = null)
        {
            Children = new List<LogicalNode>();
            Events = new List<MenuEvent>();
            animations = new List<BaseAnimation>();
            animationsToRemove = new List<BaseAnimation>();
            eventsToTrigger = new List<MenuEvent>();
            this.name = name;
            this.parent = parent;
            this.menu = menu;
            this.VisualNode = new VisualNode(device, this);
            this.VisualNode.VerticalTextAlignment = verticalTextAlignment;
            this.VisualNode.HorizontalTextAlignment = horizontalTextAlignment;
            this.VisualNode.ForegroundColor = foreground;
            this.VisualNode.BackgroundColor = background;
            this.VisualNode.BorderColor = borderColor;
            this.VisualNode.BorderSize = borderSize;
            this.VisualNode.FontSize = fontSize;
            this.VisualNode.Primitive = null;
            this.VisualNode.Text = text;
            if(font != null)
            {
                this.VisualNode.Font = font;
            }
            this.PercentageX = percentageX;
            this.PercentageY = percentageY;
            this.PercentageWidth = percentageWidth;
            this.PercentageHeight = percentageHeight;
            this.DesiredRelativePosition = new Point((int)Math.Round(rx), (int)Math.Round(ry));
            this.DesiredWidth = width;
            this.DesiredHeight = height;
            this.VerticalAlignment = verticalAlignment;
            this.HorizontalAlignment = horizontalAlignment;
        }

        public bool PropagateMouse(Point mousePosition)
        {
            if(Visibility == Visibility.Hidden)
            {
                return false;
            }
            if(mousePosition.X >= AbsolutePosition.X && mousePosition.X <= AbsolutePosition.X + Width &&
                mousePosition.Y >= AbsolutePosition.Y && mousePosition.Y <= AbsolutePosition.Y + Height)
            {
                if (Children.Count > 0)
                {
                    bool ret = false;
                    foreach (LogicalNode child in Children)
                    {
                        if (child.PropagateMouse(mousePosition))
                        {
                            ret = true;
                        }
                    }
                    if(ret == false)
                    {
                        bool mEnter = false, mPressed = false, mLPressed = false, mRPressed = false, mReleased = false, 
                            mLReleased = false, mRReleased = false;
                        MouseOver?.Invoke(this, null);
                        if (!mouseOver)
                        {
                            mouseOver = true;
                            MouseEnter?.Invoke(this, null);
                            mEnter = true;
                        }
                        if ((Mouse.GetState().LeftButton == ButtonState.Pressed &&
                            !leftMousePressed))
                        {
                            LeftMouseDown?.Invoke(this, null);
                            leftMousePressed = true;
                            mLPressed = true;
                        }
                        else if ((Mouse.GetState().LeftButton == ButtonState.Released &&
                            leftMousePressed))
                        {
                            LeftMouseUp?.Invoke(this, null);
                            leftMousePressed = false;
                            mLReleased = true;
                        }
                        if ((Mouse.GetState().RightButton == ButtonState.Pressed &&
                            !rightMousePressed))
                        {
                            RightMouseDown?.Invoke(this, null);
                            rightMousePressed = true;
                            mRPressed = true;
                        }
                        else if ((Mouse.GetState().RightButton == ButtonState.Released &&
                            rightMousePressed))
                        {
                            RightMouseUp?.Invoke(this, null);
                            rightMousePressed = false;
                            mRReleased = true;
                        }

                        foreach (MenuEvent ev in events)
                        {
                            if (ev.EventType == MenuEvent.Type.MouseEnter && mEnter)
                            {
                                ev.Trigger(this);
                            }
                            if (ev.EventType == MenuEvent.Type.MouseDown && mPressed)
                            {
                                ev.Trigger(this);
                            }
                            if (ev.EventType == MenuEvent.Type.MouseUp && mReleased)
                            {
                                ev.Trigger(this);
                            }
                            if (ev.EventType == MenuEvent.Type.LeftMouseDown && mLPressed)
                            {
                                ev.Trigger(this);
                            }
                            if (ev.EventType == MenuEvent.Type.LeftMouseUp && mLReleased)
                            {
                                ev.Trigger(this);
                            }
                            if (ev.EventType == MenuEvent.Type.RightMouseDown && mRPressed)
                            {
                                ev.Trigger(this);
                            }
                            if (ev.EventType == MenuEvent.Type.RightMouseUp && mRReleased)
                            {
                                ev.Trigger(this);
                            }
                        }
                        return true;
                    }
                    else
                    {
                        if (mouseOver)
                        {
                            mouseOver = false;
                            MouseLeave?.Invoke(this, null);
                            foreach (MenuEvent ev in events)
                            {
                                if (ev.EventType == MenuEvent.Type.MouseLeave)
                                {
                                    ev.Trigger(this);
                                }
                            }
                        }
                        return true;
                    }
                }
                else
                {
                    bool mEnter = false, mPressed = false, mLPressed = false, mRPressed = false, mReleased = false,
                        mLReleased = false, mRReleased = false;
                    MouseOver?.Invoke(this, null);
                    if (!mouseOver)
                    {
                        mouseOver = true;
                        MouseEnter?.Invoke(this, null);
                        mEnter = true;
                    }
                    if ((Mouse.GetState().LeftButton == ButtonState.Pressed &&
                        !leftMousePressed))
                    {
                        LeftMouseDown?.Invoke(this, null);
                        leftMousePressed = true;
                        mLPressed = true;
                    }
                    else if ((Mouse.GetState().LeftButton == ButtonState.Released &&
                        leftMousePressed))
                    {
                        LeftMouseUp?.Invoke(this, null);
                        leftMousePressed = false;
                        mLReleased = true;
                    }
                    if ((Mouse.GetState().RightButton == ButtonState.Pressed &&
                        !rightMousePressed))
                    {
                        RightMouseDown?.Invoke(this, null);
                        rightMousePressed = true;
                        mRPressed = true;
                    }
                    else if ((Mouse.GetState().RightButton == ButtonState.Released &&
                        rightMousePressed))
                    {
                        RightMouseUp?.Invoke(this, null);
                        rightMousePressed = false;
                        mRReleased = true;
                    }

                    foreach (MenuEvent ev in events)
                    {
                        if (ev.EventType == MenuEvent.Type.MouseEnter && mEnter)
                        {
                            ev.Trigger(this);
                        }
                        if (ev.EventType == MenuEvent.Type.MouseDown && mPressed)
                        {
                            ev.Trigger(this);
                        }
                        if (ev.EventType == MenuEvent.Type.MouseUp && mReleased)
                        {
                            ev.Trigger(this);
                        }
                        if (ev.EventType == MenuEvent.Type.LeftMouseDown && mLPressed)
                        {
                            ev.Trigger(this);
                        }
                        if (ev.EventType == MenuEvent.Type.LeftMouseUp && mLReleased)
                        {
                            ev.Trigger(this);
                        }
                        if (ev.EventType == MenuEvent.Type.RightMouseDown && mRPressed)
                        {
                            ev.Trigger(this);
                        }
                        if (ev.EventType == MenuEvent.Type.RightMouseUp && mRReleased)
                        {
                            ev.Trigger(this);
                        }
                    }
                    return true;
                }
            }
            else
            {
                if (mouseOver)
                {
                    mouseOver = false;
                    MouseLeave?.Invoke(this, null);
                    foreach (MenuEvent ev in events)
                    {
                        if (ev.EventType == MenuEvent.Type.MouseLeave)
                        {
                            ev.Trigger(this);
                        }
                    }
                }
                foreach(LogicalNode child in Children)
                {
                    child.PropagateMouse(mousePosition);
                }
                return false;
            }
        }

        public bool PropagateClick(Point mousePosition)
        {
            if(Visibility == Visibility.Hidden)
            {
                return false;
            }
            if (mousePosition.X >= AbsolutePosition.X && mousePosition.X <= AbsolutePosition.X + Width &&
                mousePosition.Y >= AbsolutePosition.Y && mousePosition.Y <= AbsolutePosition.Y + Height)
            {
                if (Children.Count > 0)
                {
                    bool ret = false;
                    foreach (LogicalNode child in Children)
                    {
                        if (child.PropagateClick(mousePosition))
                        {
                            ret = true;
                        }
                    }
                    if (ret == false)
                    {
                        Click?.Invoke(this, null);
                        foreach (MenuEvent ev in events)
                        {
                            if (ev.EventType == MenuEvent.Type.Click)
                            {
                                ev.Trigger(this);
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    Click?.Invoke(this, null);
                    foreach (MenuEvent ev in events)
                    {
                        if (ev.EventType == MenuEvent.Type.Click)
                        {
                            ev.Trigger(this);
                        }
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool PropagateDoubleClick(Point mousePosition)
        {
            if(Visibility == Visibility.Hidden)
            {
                return false;
            }
            if (mousePosition.X >= AbsolutePosition.X && mousePosition.X <= AbsolutePosition.X + Width &&
                mousePosition.Y >= AbsolutePosition.Y && mousePosition.Y <= AbsolutePosition.Y + Height)
            {
                if (Children.Count > 0)
                {
                    bool ret = false;
                    foreach (LogicalNode child in Children)
                    {
                        if (child.PropagateDoubleClick(mousePosition))
                        {
                            ret = true;
                        }
                    }
                    if (ret == false)
                    {
                        DoubleClick?.Invoke(this, null);
                        foreach (MenuEvent ev in events)
                        {
                            if (ev.EventType == MenuEvent.Type.DoubleClick)
                            {
                                ev.Trigger(this);
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    DoubleClick?.Invoke(this, null);
                    foreach (MenuEvent ev in events)
                    {
                        if (ev.EventType == MenuEvent.Type.DoubleClick)
                        {
                            ev.Trigger(this);
                        }
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public void Update(GameTime gameTime)
        {
            if(autoArrangeChildren && invalidLayout)
            {
                if(orientation == NodeOrientation.Vertical)
                {
                    double newHeight = this.Height / Children.Count;
                    for(int i = 0; i < Children.Count; i++)
                    {
                        Children[i].VerticalAlignment = VerticalAlignment.Top;
                        Children[i].HorizontalAlignment = HorizontalAlignment.Left;
                        Children[i].PercentageX = false;
                        Children[i].PercentageY = false;
                        Children[i].PercentageHeight = false;
                        Children[i].PercentageWidth = false;
                        Children[i].DesiredHeight = newHeight;
                        Children[i].DesiredRelativePosition = new Point(0, (int)Math.Round(i * newHeight));
                        Children[i].DesiredWidth = this.Width;
                    }
                }
                else
                {
                    double newWidth = this.Width / Children.Count;
                    for(int i = 0; i < Children.Count; i++)
                    {
                        Children[i].VerticalAlignment = VerticalAlignment.Top;
                        Children[i].HorizontalAlignment = HorizontalAlignment.Left;
                        Children[i].PercentageX = false;
                        Children[i].PercentageY = false;
                        Children[i].PercentageHeight = false;
                        Children[i].PercentageWidth = false;
                        Children[i].DesiredWidth = newWidth;
                        Children[i].DesiredRelativePosition = new Point((int)Math.Round(i * newWidth), 0);
                        Children[i].DesiredHeight = this.Height;
                    }
                }
                invalidLayout = false;
            }

            foreach(MenuEvent ev in eventsToTrigger)
            {
                ev.Trigger(this);
            }
            eventsToTrigger.Clear();
            foreach(BaseAnimation anim in animations)
            {
                if (anim.Enabled)
                {
                    if (anim is ColorAnimation)
                    {
                        ColorAnimation canim = anim as ColorAnimation;
                        canim.Tick(gameTime);
                    }
                    else if (anim is DoubleAnimation)
                    {
                        DoubleAnimation danim = anim as DoubleAnimation;
                        danim.Tick(gameTime);
                    }
                }
                else
                {
                    animationsToRemove.Add(anim);
                }
            }
            foreach(BaseAnimation anim in animationsToRemove)
            {
                animations.Remove(anim);
            }
            animationsToRemove.Clear();
        }

        public void Resize()
        {
            Resize(desiredWidth, desiredHeight);
        }

        public void Resize(double width, double height)
        {
            this.DesiredWidth = width;
            this.DesiredHeight = height;
            foreach(LogicalNode node in Children)
            {
                node.Resize();
                node.VisualNode.Modified = true;
            }
        }

        public BaseAnimation FindRunningAnimation(string name)
        {
            foreach(BaseAnimation animation in animations)
            {
                if(animation.Name == name)
                {
                    return animation;
                }
            }
            return null;
        }

        public void Dispose()
        {
            this.VisualNode.Dispose();
        }

        public void AddAnimation(BaseAnimation animation)
        {
            animations.Add(animation);
            animation.Finished += AnimationFinished;
        }

        public void AddChild(LogicalNode node)
        {
            menu[node.Name] = node;
            this.Children.Add(node);
        }

        protected void AnimationFinished(Object sender, EventArgs e)
        {
            BaseAnimation finishedAnim = sender as BaseAnimation;
            foreach(MenuEvent ev in events)
            {
                if(ev.EventType == MenuEvent.Type.AnimationFinished)
                {
                    if(string.IsNullOrEmpty(ev.TriggerName) || ev.TriggerName == finishedAnim.Name)
                    {
                        eventsToTrigger.Add(ev);
                    }
                }
            }
        }

        protected void StyleChanged(Object sender, EventArgs e)
        {
            this.Foreground = style.Foreground;
            this.Background = style.Background;
            this.BorderColor = style.BorderColor;
            this.BorderSize = style.BorderSize;
            this.FontSize = style.FontSize;
            if (style.Font != null)
                this.Font = style.Font;
        }
    }
}
