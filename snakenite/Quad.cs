using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace snakenite
{
    internal class Quad
    {

        private readonly static int height = 8;
        private readonly static int width = 8;
        public Quad()
        {

        }

        public static UIElement getQ(int x, int y, string type) 
        {
            Point p1 = new Point(x, y);
            Point p2 = new Point(x+width, y);
            Point p3 = new Point(x+width, y+height);
            Point p4 = new Point(x, y+height);

            Polygon p = new Polygon();
            p.Stroke = Brushes.Black;
            p.StrokeThickness = 1;


            switch (type)
            {
                case "*":
                    p.Fill = Brushes.Green;
                    break;
                case "@":
                    p.Fill = Brushes.Red;
                    break;
                case "#":
                    p.Fill = Brushes.Black;
                    break;
                case " ":
                    p.Fill = Brushes.Transparent;
                    break;
                default:
                    break;
            }

            p.Points.Add(p1);
            p.Points.Add(p2);
            p.Points.Add(p3);
            p.Points.Add(p4);

            return p;

        }
    }
}
