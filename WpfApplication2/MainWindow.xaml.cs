using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Text.RegularExpressions;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Visualizer V;
        
        public MainWindow()
        {
            InitializeComponent();
            
            /* ============ */
            /*  Visualizer  */

            V = new Visualizer(canvas);

            //var task = V.insert(5);
            //task.GetAwaiter().OnCompleted(async () =>
            //{
            //    await V.insert(2);
            //    await V.insert(11);
            //    await V.insert(7);
            //    await V.insert(9);
            //    await V.insert(4);
            //    await V.insert(0);
            //    await V.insert(15);
            //    await V.insert(3);
            //    await V.insert(1);
            //    await V.insert(-1);
            //    await V.insert(6);
            //    await V.insert(8);
            //    await V.insert(10);

            //    await V.insert(14);
            //});
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            V.root.redraw();
        }

        private void ToggleInput(bool isEnable)
        {
            DeleteButton.IsEnabled = DeleteInputBox.IsEnabled =
            InsertButton.IsEnabled = InsertInputBox.IsEnabled =
            QueryButton.IsEnabled = QueryInputBox.IsEnabled = isEnable;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int number = int.Parse(DeleteInputBox.Text);
            ToggleInput(false);
            await V.delete(number);
            ToggleInput(true);
            DeleteInputBox.Focus();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("-?[0-9]+");
            e.Handled = !re.IsMatch(e.Text);
        }

        private async void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            int number = int.Parse(InsertInputBox.Text);
            ToggleInput(false);
            await V.insert(number);
            ToggleInput(true);
            InsertInputBox.Focus();
        }

        private void InsertKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                InsertButton_Click(null, null);
            //e.Handled = true;
        }

        private void DeleteKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                DeleteButton_Click(null, null);
            //e.Handled = true;
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            int number = int.Parse(QueryInputBox.Text);
            ToggleInput(false);
            await V.query(number);
            ToggleInput(true);
            QueryInputBox.Focus();
        }

        private void QueryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                QueryButton_Click(null, null);
        }
    }
}
