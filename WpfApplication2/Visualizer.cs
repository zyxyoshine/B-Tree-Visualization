using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApplication2
{
    class Visualizer
    {
        static public readonly int TopMargin = 50;

        private Canvas canvas;
        public Page root;
        static private int d;

        static public int D { get { return d; } }

        public Visualizer(Canvas _canvas, int D = 1)
        {
            canvas = _canvas;
            root = new Page();
            canvas.Children.Add(root);
            d = D;
        }

        public async Task<string> query(int value)
        {
            Tuple<Page,Key> pin = await root.query(value);
            Key key = pin.Item2; Page page = pin.Item1;
            if (key != null)
                return string.Format("Value {0} found!", value);
            else
                return string.Format("Value {0} not found!", value);
        }

        public async Task<string> insert(int value)
        {
            Tuple<Page,Key> pin = await root.query(value);
            Key key = pin.Item2; Page page = pin.Item1;
            if (key != null)
                return string.Format("Value {0} already exists!", value);
            else
            {
                await page.insert(value);
                if (root.ParentPage != null)
                    root = root.ParentPage;
                await root.rejustPosition(true);
                return string.Format("Value {0} sucessfully inserted!", value);
            }
        }

        public async Task<string> delete(int value)
        {
            Tuple<Page, Key> pin = await root.query(value);
            Key key = pin.Item2; Page page = pin.Item1;
            if (key == null)
                return string.Format("Value {0} does not exist!", value);
            else
            {
                await Page.delete(key);
                if(root.keys.Count == 0)
                {
                    if (root.children.Count > 0 && root.children[0].page != null)
                        root = root.children[0].page;
                }
                await root.rejustPosition(true);
                return string.Format("Value {0} sucessfully deleted!", value);
            }
        }
    }
}
