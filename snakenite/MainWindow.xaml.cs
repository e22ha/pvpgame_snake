using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace snakenite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double workHeight = SystemParameters.WorkArea.Height;
        double workWidth = SystemParameters.WorkArea.Width;

        static Walls walls;
        Snake snake;
        Client c = new Client();
        
        static  Game.Point food { get;  set; }

        public MainWindow()
        {
            InitializeComponent();
            c.main = Main;
            Line.X2 = Pole.Width + 5;

            Game.UpDraw += Game_UpDraw;

            walls = new Walls(43, 43, '#');
            chekall();
            //Field.SetTop(new Rectangle { Width = 15, Height = 15, Fill = Brushes.Black }, Left);
        }

        private void chekall()
        {
            snake = new Snake(4, 4, 3);
            food = (10 * 10, 10 * 10, '@');
            food.Draw();
        }

        private void Game_UpDraw(object sender, EventArgs e)
        {
            string[] Data = sender.ToString().Split(',');

            int x = int.Parse(Data[0]);
            int y = int.Parse(Data[1]);

            Field.Children.Add(Quad.getQ(x, y, Data[2]));
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Lead_Click(object sender, RoutedEventArgs e)
        {
            _Info.Visibility = Visibility.Hidden;
            _Info.IsEnabled = false;
            _Set.Visibility = Visibility.Hidden;
            _Set.IsEnabled = false;
            _Multi.Visibility = Visibility.Hidden;
            _Multi.IsEnabled = false;

            _Lead.IsEnabled = true;
            _Lead.Visibility = Visibility.Visible;
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            _Info.Visibility = Visibility.Hidden;
            _Info.IsEnabled = false;
            _Lead.Visibility = Visibility.Hidden;
            _Lead.IsEnabled = false;
            _Multi.Visibility = Visibility.Hidden;
            _Multi.IsEnabled = false;

            _Set.IsEnabled = true;
            _Set.Visibility = Visibility.Visible;
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            _Lead.Visibility = Visibility.Hidden;
            _Lead.IsEnabled = false;
            _Set.Visibility = Visibility.Hidden;
            _Set.IsEnabled = false;
            _Multi.Visibility = Visibility.Hidden;
            _Multi.IsEnabled = false;

            _Info.IsEnabled = true;
            _Info.Visibility = Visibility.Visible;
        }

        private void Small_Click(object sender, RoutedEventArgs e)
        {
            Small.Content = "[Small]";
            Medium.Content = "Medium";
            Big.Content = "Big";
            Pole.Width = Pole.Height = 450;
            Line.X2 = Pole.Width + 5;

            CenterPosition();
        }

        private void Medium_Click(object sender, RoutedEventArgs e)
        {
            Small.Content = "Small";
            Medium.Content = "[Medium]";
            Big.Content = "Big";
            Pole.Width = 700;
            Pole.Height = 500;
            Line.X2 = Pole.Width + 5;

            CenterPosition();
        }

        private void Big_Click(object sender, RoutedEventArgs e)
        {
            Small.Content = "Small";
            Medium.Content = "Medium";
            Big.Content = "[Big]";

            Pole.Width = 1200;
            Pole.Height = 800;
            Line.X2 = Pole.Width + 5;
            
            CenterPosition();
        }

        private void CenterPosition()
        {
            this.Top = (workHeight - this.Height) / 2;
            this.Left = (workWidth - this.Width) / 2;
        }

        private void Main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Multi_Click(object sender, RoutedEventArgs e)
        {
            _Lead.Visibility = Visibility.Hidden;
            _Lead.IsEnabled = false;
            _Set.Visibility = Visibility.Hidden;
            _Set.IsEnabled = false;
            _Info.Visibility = Visibility.Hidden;
            _Info.IsEnabled = false;

            _Multi.IsEnabled = true;
            _Multi.Visibility = Visibility.Visible;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
