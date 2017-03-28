using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.ComponentModel;

namespace WpfApplication2
{
    static class MyStyle
    {
        static public readonly Duration highlightDuration = TimeSpan.FromMilliseconds(100);

        static Dictionary<Control, Style> OldStyles = new Dictionary<Control, Style>();

        static public DropShadowEffect shadow = new DropShadowEffect()
        {
            BlurRadius = 3,
            ShadowDepth = 0,
            Opacity = 0.4
        };
        
        static public void SetStyle(Control e, string type)
        {
            if(!OldStyles.ContainsKey(e))
                OldStyles[e] = e.Style;
            e.Style = (Style)Application.Current.Resources[type];
        }

        static public void ResetStyle(Control e)
        {
            e.Style = OldStyles[e];
        }

        static public async Task highlight(Control e, string type = "Query")
        {
            SetStyle(e, type);
            await Task.Delay(highlightDuration.TimeSpan);
            ResetStyle(e);
        }
    }

    static class myAnimation
    {
        static public readonly int pauseDuration = 300;
        static public readonly Duration slideDuration = TimeSpan.FromMilliseconds(1000);
        static public readonly IEasingFunction EasingFunc = new SineEase() { EasingMode = EasingMode.EaseInOut };

        static public DoubleAnimation slideAnimationFactory(double to, Duration? duration = null)
        {
            return new DoubleAnimation()
            {
                Duration = duration != null ? (Duration)duration : slideDuration,
                EasingFunction = EasingFunc,
                To = to
            };
        }

        static public Task animateTo(UIElement e, DependencyProperty dp, double to)
        {
            var animation = slideAnimationFactory(to);
            var tcs = new TaskCompletionSource<object>();
            animation.Completed += (sender, args) => tcs.SetResult(null);
            e.BeginAnimation(dp, animation);
            return tcs.Task;
        }

        static public Storyboard fadeIn(UIElement e)
        {
            e.Opacity = 0;

            var fadeInAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(5),
                From = 0,
                To = 1,
                EasingFunction = EasingFunc
            };

            var sb = new Storyboard()
            {
                Duration = TimeSpan.FromSeconds(5),
            };

            sb.Children.Add(fadeInAnimation);

            Storyboard.SetTarget(fadeInAnimation, e);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));

            sb.Completed += (o, args) =>
            {
                Debug.WriteLine("In Class Key: sb completed");
            };

            return sb;
        }
    }

    class Pointer : TextBox
    {
        public static readonly int BorderWidth = 2;
        public static readonly int DefaultHeight = 30, DefaultWidth = 15;

        private int index;
        private Page parentPage = null;

        private bool updating;

        public bool Updating
        {
            get { return updating; }
            set { updating = value; }
        }

        public Page ParentPage
        {
            get { return parentPage; }
            set { parentPage = value; }
        }

        public bool isUpToDate
        {
            get
            {
                return (Page)Parent == parentPage
                    && Canvas.GetLeft(this) == vLeft
                    && Canvas.GetTop(this) == vTop;
            }
        }

        public int Index
        {
            get { return index; }
            set
            {
                index = value;
            }
        }

        public double vLeft
        {
            get
            {
                return index * (this.Width + Key.DefaultWidth) - BorderWidth * 2 * index; ;
            }
        }

        public double vTop
        {
            get
            {
                return 0;
            }
        }

        public Task update(bool bAnimate = false)
        {
            var tcs = new TaskCompletionSource<object>();

            var task = tcs.Task;
            task.GetAwaiter().OnCompleted(() => {
                updating = false;
                this.SetValue(Panel.ZIndexProperty, DependencyProperty.UnsetValue);
            });

            if (isUpToDate || updating)
                tcs.SetResult(null);
            else
            {
                updating = true;
                Panel.SetZIndex(this, -1);
                var p = (Canvas)Parent;

                if (p != null)
                {
                    if (bAnimate)
                    {
                        var pos = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

                        if (double.IsNaN(pos.X))
                            pos.X = 0;
                        if (double.IsNaN(pos.Y))
                            pos.Y = 0;

                        var tranform = p.TransformToVisual(parentPage);
                        pos = tranform.Transform(pos);
                        p.Children.Remove(this);
                        parentPage.Children.Add(this);

                        Canvas.SetLeft(this, pos.X);
                        Canvas.SetTop(this, pos.Y);

                        var aniTop = myAnimation.slideAnimationFactory(vTop);
                        var aniLeft = myAnimation.slideAnimationFactory(vLeft);
                        aniTop.FillBehavior = aniLeft.FillBehavior = FillBehavior.Stop;
                        aniTop.Completed += (s, e) =>
                        {
                            Canvas.SetTop(this, vTop);
                            tcs.SetResult(null);
                        };
                        aniLeft.Completed += (s, e) =>
                        {
                            Canvas.SetLeft(this, vLeft);
                        };
                        BeginAnimation(Canvas.LeftProperty, aniLeft);
                        BeginAnimation(Canvas.TopProperty, aniTop);
                    }
                    else
                    {
                        Canvas.SetLeft(this, vLeft);
                        Canvas.SetTop(this, vTop);
                        tcs.SetResult(null);
                    }
                }
                else
                {
                    ParentPage.Children.Add(this);
                    Canvas.SetLeft(this, vLeft);
                    Canvas.SetTop(this, vTop);
                    if (bAnimate)
                    {
                        Task.Delay(myAnimation.slideDuration.TimeSpan)
                            .GetAwaiter()
                            .OnCompleted(() => tcs.SetResult(null));
                    }
                    else tcs.SetResult(null);
                }
            }

            return task;
        }

        public Pointer(int _index)
        {
            Focusable = false;
            Cursor = Cursors.Arrow;

            Width = DefaultWidth; Height = DefaultHeight;

            Style = (Style)Application.Current.Resources["TextBoxStyle"];

            index = _index;
            Text = "●";

            Canvas.SetLeft(this, index * (this.Width + Key.DefaultWidth) - BorderWidth * 2 * index);
        }

        public async Task slideIncrement(int inc, bool bAnimate = true)
        {
            index += inc;
            await update(true);
        }
    }

    class Key : TextBox
    {
        public static readonly int BorderWidth = 2;
        public static readonly int DefaultHeight = 30, DefaultWidth = 30;

        private int value, index;
        private Page parentPage;

        private bool updating;

        public bool Updating
        {
            get { return updating; }
            set { updating = value; }
        }

        public Page ParentPage
        {
            get { return parentPage; }
            set { parentPage = value; }
        }

        public bool isUpToDate
        {
            get
            {
                return (Page)Parent == parentPage
                    && Canvas.GetLeft(this) == vLeft
                    && Canvas.GetTop(this) == vTop;
            }
        }

        public int Value
        {
            get { return value; }
        }

        public int Index
        {
            get { return index; }
            set
            {
                index = value;
            }
        }

        public double vLeft
        {
            get
            {
                return index * Width + (index + 1) * Pointer.DefaultWidth - BorderWidth * (2 * index + 1);
            }
        }

        public double vTop
        {
            get
            {
                return 0;
            }
        }

        public Task update(bool bAnimate = false)
        {
            var tcs = new TaskCompletionSource<object>();

            var task = tcs.Task;
            task.GetAwaiter().OnCompleted(() => {
                updating = false;
                this.SetValue(Panel.ZIndexProperty, DependencyProperty.UnsetValue);
            });

            if (isUpToDate || updating) 
                tcs.SetResult(null);
            else
            {
                updating = true;
                Panel.SetZIndex(this, -1);
                var p = (Canvas)Parent;

                if(p != null)
                {
                    if(bAnimate)
                    {
                        var pos = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

                        if (double.IsNaN(pos.X))
                            pos.X = 0;
                        if (double.IsNaN(pos.Y))
                            pos.Y = 0;

                        var tranform = p.TransformToVisual(parentPage);
                        pos = tranform.Transform(pos);
                        p.Children.Remove(this);
                        parentPage.Children.Add(this);

                        Canvas.SetLeft(this, pos.X);
                        Canvas.SetTop(this, pos.Y);

                        var aniTop = myAnimation.slideAnimationFactory(vTop);
                        var aniLeft = myAnimation.slideAnimationFactory(vLeft);
                        aniTop.FillBehavior = aniLeft.FillBehavior = FillBehavior.Stop;
                        aniTop.Completed += (s, e) =>
                        {
                            Canvas.SetTop(this, vTop);
                            tcs.SetResult(null);
                        };
                        aniLeft.Completed += (s, e) =>
                        {
                            Canvas.SetLeft(this, vLeft);
                        };
                        BeginAnimation(Canvas.LeftProperty, aniLeft);
                        BeginAnimation(Canvas.TopProperty, aniTop);
                    }
                    else
                    {
                        Canvas.SetLeft(this, vLeft);
                        Canvas.SetTop(this, vTop);
                        tcs.SetResult(null);
                    }
                }
                else
                {
                    ParentPage.Children.Add(this);
                    Canvas.SetLeft(this, vLeft);
                    Canvas.SetTop(this, vTop);
                    if (bAnimate)
                    {
                        Task.Delay(myAnimation.slideDuration.TimeSpan)
                            .GetAwaiter()
                            .OnCompleted(() => tcs.SetResult(null));
                    }
                    else tcs.SetResult(null);
                }
            }
            
            return task;
        }

        public static readonly DependencyProperty IsHighlightProperty = DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(Key));
        public bool IsHighlight
        {
            get { return (bool)GetValue(IsHighlightProperty); }
            set { SetValue(IsHighlightProperty, value); }
        }

        public Key(int _value = 0, int _index = 0)
        {
            Cursor = Cursors.Arrow;
            Focusable = false;

            Width = DefaultWidth; Height = DefaultHeight;
            Style = (Style)Application.Current.Resources["TextBoxStyle"];
            value = _value; index = _index;
            Text = value.ToString();
            FontSize = 13;
            
            Canvas.SetLeft(this, index*this.Width + (index+1)*Pointer.DefaultWidth - BorderWidth*(2*index+1));

            MouseDown += (s, e) => { };
        }

        public async Task slideIncrement(int inc, bool bAnimate = true)
        {
            index += inc;
            await update(true);
        }
    }

    class PageChild
    {
        public Pointer pointer = null;
        public Line edge = null;
        public Page page = null;
        public Page parentPage = null;
        
        public PageChild(Page parent)
        {
            parentPage = parent;
        }

        public Page ParentPage
        {
            get { return parentPage; }
            set
            {
                parentPage = value;
                pointer.ParentPage = value;
            }
        }
    }

    class Page : Canvas
    {
        public static readonly double PageHeight = 75;
        public static readonly double PageSpaceWidth = 30;
        public List<Key> keys;
        public List<PageChild> children;
        public PageChild link = null;

        public bool IsPosUpToDate
        {
            get
            {
                bool ok = Canvas.GetLeft(this) == vLeft && Canvas.GetTop(this) == vTop;
                foreach (var child in children)
                    if (child.page != null)
                        ok = ok && child.page.IsPosUpToDate;
                return ok;
            }
        }

        public Page ParentPage
        {
            get
            {
                return link == null ? null : link.parentPage;
            }
        }

        public new int Height
        {
            get { return ParentPage == null ? 0 : ParentPage.Height + 1; }
        }

        public double vTop
        {
            get { return Height == 0 ? 0 : PageHeight; }
        }

        public int Index
        {
            get { return link == null ? 0 : link.pointer.Index; }
        }

        public double vLeft
        {
            get
            {
                if (ParentPage == null)
                    return (((Canvas)Parent).ActualWidth - vPageWidth) / 2;
                var p = ParentPage;
                double maxC = (double)p.maxChildWidth;
                double x0 = (p.vPageWidth - p.vPageTreeWidth) / 2 + Index * (maxC + PageSpaceWidth) + (maxC - vPageWidth) / 2;
                return x0;
            }
        }

        public double? maxChildWidth
        {
            get
            {
                if (children.Count() == 0 || children[0].page == null)
                    return null;
                double max = 0;
                foreach (var c in children)
                    max = Math.Max(max, c.page.vPageTreeWidth);
                return max;
            }
        }

        public double vPageTreeWidth
        {
            get
            {
                var maxC = maxChildWidth;
                return maxC == null ? vPageWidth : (double)maxC * children.Count + PageSpaceWidth * (children.Count - 1);
            }
        }

        public double vPageWidth
        {
            get
            {
                return children.Count * Pointer.DefaultWidth + keys.Count * Key.DefaultWidth;
            }
        }
        
        public Page(PageChild _link = null)
        {
            Effect = MyStyle.shadow;    
            //Loaded += LoadedHandler;

            keys = new List<Key>();
            children = new List<PageChild>();
            link = _link;
        }

        public void redraw()
        {
            ((Canvas)Parent).UpdateLayout();
            rejustPosition();
        }

        private void updateHierarchy()
        {
            if (Parent == null)
            {
                ParentPage.Children.Add(this);
                var transform = this.children[0].pointer.TransformToVisual(ParentPage);
                var pos = transform.Transform(new Point(0, 0));
                Canvas.SetTop(this, pos.X);
                Canvas.SetLeft(this, pos.Y);
            }
            foreach(var c in children)
            {
                Page page = c.page;
                if(page != null && page.Parent != this)
                {
                    if (page.ParentPage != this) throw new Exception();
                    var transform = page.children[0].pointer.TransformToVisual(this);
                    var pos = transform.Transform(new Point(0, 0));
                    Canvas.SetLeft(page, pos.X);
                    Canvas.SetTop(page, pos.Y);
                    if (page.Parent != null)
                        ((Page)page.Parent).Children.Remove(page);
                    this.Children.Add(page);
                }
                ((Canvas)Parent).UpdateLayout();
                /* === */
                if (page != null)
                    page.updateHierarchy();
                UpdateLayout();
            }
        }
        
        public Task rejustPosition(bool bAnimate = false)
        {
            var tcs = new TaskCompletionSource<object>();

            if(IsPosUpToDate)
            {
                tcs.SetResult(null);
                return tcs.Task;
            }

            Canvas p = (Canvas)Parent;

            if (children.Count() > 0)
            {
                if (!bAnimate)
                {
                    Canvas.SetTop(this, vTop);
                    Canvas.SetLeft(this, vLeft);
                    var tasks = new List<Task>();
                    foreach (var c in children)
                        if (c.page != null)
                            tasks.Add(c.page.rejustPosition());
                    Task.WhenAll(tasks).GetAwaiter().OnCompleted(() => tcs.SetResult(null));
                }
                else
                {
                    var tasks = new List<Task>();
                    foreach (var c in children)
                        if (c.page != null)
                            tasks.Add(c.page.rejustPosition(true));

                    var aniTop = myAnimation.slideAnimationFactory(vTop);
                    var aniLeft = myAnimation.slideAnimationFactory(vLeft);

                    if (double.IsNaN(Canvas.GetTop(this)))
                        aniTop.From = vTop;
                    if (double.IsNaN(Canvas.GetLeft(this)))
                        aniLeft.From = vLeft;

                    aniTop.FillBehavior = FillBehavior.Stop;
                    aniLeft.FillBehavior = FillBehavior.Stop;
                    aniTop.Completed += (s, a) =>
                    {
                        tcs.SetResult(null);
                        Canvas.SetTop(this, vTop);
                    };
                    aniLeft.Completed += (s, a) => Canvas.SetLeft(this, vLeft);
                    BeginAnimation(Canvas.TopProperty, aniTop);
                    BeginAnimation(Canvas.LeftProperty, aniLeft);
                }
            }
            else tcs.SetResult(null);
            return tcs.Task;
        }
        
        public async Task updateViewElements(bool bAnimate = false)
        {
            List<Task> tasks = new List<Task>();
            foreach (var key in keys)
                tasks.Add(key.update(true));
            foreach (var child in children)
                tasks.Add(child.pointer.update(true));
            await Task.WhenAll(tasks);
        }

        private void LoadedHandler(object o, EventArgs e)
        {
            //MessageBox.Show("loaded!");
            //redraw();
        }

        public async Task<Tuple<Page,Key>> query(int value)
        {
            if (keys.Count() == 0)
                return new Tuple<Page, Key>(this, null);

            int i;
            
            for (i = 0; i < keys.Count(); i++)
            {
                await MyStyle.highlight(keys[i]);
                if (keys[i].Value >= value)
                    break;
            }
                
            if (i < keys.Count() && keys[i].Value == value)
                return new Tuple<Page, Key>(this, keys[i]);
           
            if (children[i].page != null)
            {
                await MyStyle.highlight(children[i].pointer);
                Page child = children[i].page;
                return await child.query(value);
            }
            else
                return new Tuple<Page, Key>(this, null);
        }

        public async Task insert(int value)
        {
            var key = new Key(value);
            MyStyle.SetStyle(key, "Insert");

            await insert(key);
            await split();

            await MyStyle.highlight(key, "Insert");
        }

        public async Task insert(Key key, PageChild pre = null, PageChild nxt = null)
        {
            int value = key.Value;

            Pointer pleft, pright;

            if (pre == null)
            {
                pleft = new Pointer(0);
                pright = new Pointer(1);
                pre = new PageChild(this) { pointer = pleft };
                nxt = new PageChild(this) { pointer = pright };
            }
            else
            {
                pleft = pre.pointer;
                pright = nxt.pointer;
            }

            pre.parentPage = nxt.parentPage = key.ParentPage = pleft.ParentPage = pright.ParentPage = this;

            if (keys.Count() == 0)
            {
                foreach(var child in children)
                    Children.Remove(child.pointer);
                children.Clear();

                UpdateLayout();

                key.Index = 0;
                pleft.Index = 0;
                pright.Index = 1;

                keys.Add(key); 
                children.Add(pre);
                children.Add(nxt);
                
                await Task.WhenAll(rejustPosition(true), updateViewElements());
            }
            else
            {
                int i = 0;
                for (i = 0; i < keys.Count(); i++)
                    if (keys[i].Value >= value)
                        break;
                key.Index = i;
                pleft.Index = i;
                pright.Index = i + 1;

                if(i < keys.Count())
                {
                    if (keys[i].Value == value)
                        return;

                    Children.Remove(children[i].pointer);
                    children[i] = nxt;
                    pright.Index--; // to display at Index i then slide to i+1
                    await Task.WhenAll(pleft.update(), pright.update());

                    List<Task> tasks = new List<Task>();

                    for (int j = i; j < keys.Count(); j++)
                    {
                        tasks.Add(keys[j].slideIncrement(1));
                        tasks.Add(children[j].pointer.slideIncrement(1));
                    }
                    tasks.Add(children.Last().pointer.slideIncrement(1));
                    children.Insert(i, pre);

                    keys.Insert(i, key);
                    key.Index = i;

                    tasks.Add(rejustPosition(true));
                    tasks.Add(updateViewElements());

                    await Task.WhenAll(tasks);
                }
                else
                {
                    Children.Remove(children[i].pointer);
                    children[i] = pre;

                    children.Insert(i + 1, nxt);

                    keys.Insert(i, key);
                    key.Index = i;

                    updateHierarchy();
                    await Task.WhenAll(rejustPosition(true), updateViewElements());
                }
            }
        }

        public async Task split()
        {
            if (keys.Count() <= 2*Visualizer.D) return;

            // i short for Insert
            Page iPage = ParentPage == null ? this : ParentPage;
            PageChild lChild = new PageChild(iPage), rChild = new PageChild(iPage);
            Page lPage = new Page(lChild), rPage = new Page(rChild);

            int mid = keys.Count / 2;
            Key midKey = keys[mid];
            Pointer lPointer = new Pointer(mid) { ParentPage = iPage }, rPointer = new Pointer(mid+1) { ParentPage = iPage };
            await lPointer.update(); await rPointer.update();
            
            /* Step 1. Split into 2 parts */

            /* lPage */
            for(int i = 0; i < mid; i++)
            {
                lPage.keys.Add(keys[i]);
                keys[i].ParentPage = lPage;

                lPage.children.Add(children[i]);
                children[i].parentPage = lPage;
                children[i].pointer.ParentPage = lPage;
            }
            lPage.children.Add(children[mid]);
            children[mid].parentPage = lPage;
            children[mid].pointer.ParentPage = lPage;

            /* rPage */
            for(int i = mid+1; i < keys.Count; i++)
            {
                rPage.keys.Add(keys[i]);
                keys[i].ParentPage = rPage;

                rPage.children.Add(children[i]);
                children[i].parentPage = rPage;
                children[i].pointer.ParentPage = rPage;

                keys[i].Index = i - mid - 1;
                children[i].pointer.Index = i - mid - 1;
            }
            rPage.children.Add(children.Last());
            children.Last().parentPage = rPage;
            children.Last().pointer.ParentPage = rPage;
            children.Last().pointer.Index = children.Count - mid - 2;

            /* lrChild */
            lChild.page = lPage;
            rChild.page = rPage;

            rChild.page.MouseDown += (s, e) => { };

            lChild.pointer = lPointer;
            rChild.pointer = rPointer;

            /* Step 2. */
            /* insert midKey & lrChild to parentPage  */
            /* create a new key if parentPage == null */
            keys.Clear();
            children.Clear();
            
            if (ParentPage == null)
            {
                /* When this Page is root, iPage == this */
                /* Manually insert into iPage */

                midKey.Index = 0;
                iPage.keys.Add(midKey);
                lPointer.Index = 0;
                rPointer.Index = 1;
                iPage.children.Add(lChild);
                iPage.children.Add(rChild);
                
                iPage.updateHierarchy();
                await Task.WhenAll(iPage.updateViewElements(true),
                                   lPage.updateViewElements(true),
                                   rPage.updateViewElements(true),
                                   iPage.rejustPosition(true));
            }
            else
            {
                /* when parentPage exist, iPage == parentPage
                 * 1. insert key into iPage => lrPage attatched 
                 * 2. slide keys into lrPage
                 */ 

                await iPage.insert(midKey, lChild, rChild);
                iPage.UpdateLayout();
                iPage.updateHierarchy();
                await Task.WhenAll(iPage.rejustPosition(true),
                                   lPage.updateViewElements(true),
                                   rPage.updateViewElements(true));

                if (ParentPage != null)
                    ((Canvas)Parent).Children.Remove(this);
            }
            await iPage.split();
        }

        static public async Task delete(Key key, int? _index = null, bool inline = false)
        {
            Panel.SetZIndex(key, -1);

            Page page = key.ParentPage;
            var tasks = new List<Task>();

            /* Pass in _index because key.Index may be changed async. */
            int index = _index == null ? key.Index : (int)_index;

            if (page.children[0].page  == null || inline) /* leaf page */
            {
                /* delete key and the pointer *RIGHT* after it */
                var rChild = page.children[index + 1];
                bool bSlideAnimate = page.keys.Count != 1;
                for(int i = index + 1; i < page.keys.Count; i++)
                {
                    tasks.Add(page.keys[i].slideIncrement(-1, bSlideAnimate));
                    tasks.Add(page.children[i].pointer.slideIncrement(-1, bSlideAnimate));
                }
                tasks.Add(page.children.Last().pointer.slideIncrement(-1, bSlideAnimate));

                await Task.WhenAll(tasks);

                /* manually remove key and child(pointer) from data&view */
                page.keys.Remove(key);
                page.Children.Remove(key);

                page.Children.Remove(rChild.pointer);
                page.children.Remove(rChild);
                if(!inline)
                    await page.catenation();
            }
            else /* page is not leaf */
            {
                Key sKey = queryNext(page, index);

                if (sKey == null)
                    throw new Exception();

                Page sPage = sKey.ParentPage;

                tasks.Add(delete(sKey, 0, true));

                page.keys[index] = sKey;
                sKey.ParentPage = page;
                sKey.Index = index;

                tasks.Add(sKey.update(true));
                await Task.WhenAll(tasks);

                page.Children.Remove(key);
                if(!inline)
                    await sPage.catenation();
            }
        }

        private async Task catenation()
        {
            if (keys.Count >= Visualizer.D)
                return;

            if(ParentPage == null)
            {
                if (keys.Count > 0)
                    return;

                var parent = (Canvas)Parent;

                /* old root is empty */
                if (children.Count == 0 || children[0].page == null)
                {
                    Children.Clear();
                    children.Clear();
                    return;
                }

                var nRoot = children[0].page;
                var pos = nRoot.TransformToVisual(parent).Transform(new Point(0,0));
                Canvas.SetLeft(nRoot, pos.X);
                Canvas.SetTop(nRoot, pos.Y);
                parent.Children.Remove(this);
                this.Children.Remove(nRoot);
                nRoot.link = null;
                parent.Children.Add(nRoot);

                return;
            }

            int lIndex = link.pointer.Index, rIndex = lIndex + 1;
            if(rIndex == ParentPage.children.Count)
            {
                rIndex = lIndex;
                lIndex = rIndex - 1;
            }

            PageChild lChild = ParentPage.children[lIndex], rChild = ParentPage.children[rIndex];
            Key midKey = ParentPage.keys[lIndex];

            var tasks = new List<Task>();
            tasks.Add(delete(midKey, midKey.Index, true));

            midKey.Index = lChild.page.keys.Count;
            midKey.ParentPage = lChild.page;
            lChild.page.keys.Add(midKey);

            foreach (var key in rChild.page.keys)
            {
                key.Index += lChild.page.keys.Count;
                key.ParentPage = lChild.page;
            }
                
            foreach (var child in rChild.page.children)
            {
                child.pointer.Index += lChild.page.children.Count;
                child.ParentPage = lChild.page;
            }
                
            lChild.page.keys.AddRange(rChild.page.keys);
            lChild.page.children.AddRange(rChild.page.children);

            rChild.page.keys.Clear();
            rChild.page.children.Clear();

            ParentPage.updateHierarchy();
            tasks.Add(lChild.page.updateViewElements(true));
            await Task.WhenAll(tasks);

            ParentPage.children.Remove(rChild);
            ParentPage.Children.Remove(rChild.page);

            await lChild.page.split();
            await ParentPage.catenation();
        }

        private static Key queryNext(Page page, int index)
        {
            page = page.children[index + 1].page;
            while (page.children[0].page != null)
                page = page.children[0].page;
            return page.keys[0];
        }
    }
}
