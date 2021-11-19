using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Floating_Horizon
{
    public partial class Form1 : Form
    {
        static Bitmap bmp = new Bitmap(800, 800);
        Graphics g = Graphics.FromImage(bmp);
        Pen myPen = new Pen(Color.Black);
        Polyhedron poly = new Polyhedron();

        Point3 moving_point = new Point3(0, 0, 0);
        Point3 moving_point_line = new Point3(0, 0, 0);
        Point3 centr;
        public Form1()
        {
            InitializeComponent();
            centr = new Point3(pictureBox1.Width / 4, pictureBox1.Height / 2, 0);
        }
        public class Point3
        {
            public double X;
            public double Y;
            public double Z;
            public int ID;

            public Point3() { X = 0; Y = 0; Z = 0; ID = 0; }

            public Point3(double x, double y, double z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }
            public Point3(double x, double y, double z, int id)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
                this.ID = id;
            }
        }

        public class Line
        {
            public Point3 p1;
            public Point3 p2;
            public int ID;

            public Line()
            {
                p1 = new Point3();
                p2 = new Point3();
            }

            public Line(Point3 p1, Point3 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }

        }

        public class Edge
        {
            public List<Point3> points;

            public Edge()
            {
                this.points = new List<Point3> { };
            }
            public Edge(List<Point3> p)
            {
                this.points = p;
            }
        }

        public class Polyhedron
        {
            public List<Edge> edges;

            public Polyhedron()
            {
                this.edges = new List<Edge> { };
            }
            public Polyhedron(List<Edge> e)
            {
                this.edges = e;
            }
        }
        public static double[,] MultiplyMatrix(double[,] m1, double[,] m2)
        {
            double[,] m = new double[1, 4];

            for (int i = 0; i < 4; i++)
            {
                var temp = 0.0;
                for (int j = 0; j < 4; j++)
                {
                    temp += m1[0, j] * m2[j, i];
                }
                m[0, i] = temp;
            }
            return m;
        }

        Point[] Position2d(Edge e)
        {
            List<Point> p2D = new List<Point> { };
            foreach (var p3 in e.points)
            {
                p2D.Add(new Point((int)p3.X + (int)centr.X, (int)p3.Y + (int)centr.Y));
            }
            return p2D.ToArray();
        }
        Point Position2d(Point3 p)
        {
            return new Point((int)p.X + (int)centr.X, (int)p.Y + (int)centr.Y);
        }
        public void DrawPol()
        {
            g.Clear(Color.White);
            foreach (var edge in poly.edges)
                g.DrawPolygon(myPen, Position2d(edge));
            pictureBox1.Image = bmp;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            poly = new Polyhedron();
            int scale = 60;
            //y = (1 / 5)sin x cos z – (3 / 2) cos(7α / 4) e^(-α), где α = (x - p)^2 + (z - p)^2
            Func<double, double, double> funcc = (double x, double z) =>
            {
                double alpha = Math.Pow((x / scale - Math.PI), 2) + Math.Pow((z / scale - Math.PI), 2);
                return scale * ((1 / 5.0) * Math.Sin(x / scale) * Math.Cos(z / scale) - (3 / 2.0) * Math.Cos(7 * alpha / 4) * Math.Pow(Math.E, -alpha));
            };

            if (comboBox1.Text == "sin(x)*cos(y)")
                funcc = (double x, double y) => { return scale * (Math.Sin(x / scale) * Math.Cos(y / scale)); };
            if (comboBox1.Text == "sin(x)^2*cos(y)^2")
                funcc = (double x, double y) => { return scale * (Math.Sin(x / scale) * Math.Sin(x / scale) * Math.Cos(y / scale) * Math.Cos(y / scale)); };
            if (comboBox1.Text == "sin(x)^2+cos(y)^2")
                funcc = (double x, double y) => { return scale * (Math.Sin(x / scale) * Math.Sin(x / scale) + Math.Cos(y / scale) * Math.Cos(y / scale)); };

            int index = 0;

            for (int z = int.Parse(textBoxZ0.Text); z <= int.Parse(textBoxZ1.Text); z+=3)
            {
                poly.edges.Add(new Edge(new List<Point3>()));
                for (int x = int.Parse(textBoxX0.Text); x <= int.Parse(textBoxX1.Text); x++)
                {
                    poly.edges[index].points.Add(new Point3(x, funcc(x,z), z));
                }
                index++;
            }
            DrawFunc();
        }

        public void DrawFunc()
        {
            g.Clear(Color.White);
            Dictionary<double, double> UpBound = new Dictionary<double, double>();
            Dictionary<double, double> DownBound = new Dictionary<double, double>();
            poly.edges.Sort((Edge first, Edge second) =>
            {
                return first.points[0].Z > second.points[0].Z ? 1 : -1;
            });

            foreach (var point in poly.edges[0].points)
            {
                UpBound[Math.Round(point.X, 0)] = point.Y;
                DownBound[Math.Round(point.X, 0)] = point.Y;
            }

            foreach (var edge in poly.edges)
            {
                bool is_last_visible = true;
                Point3 last_point = edge.points[0];
                foreach (var point in edge.points)
                {
                    double x = Math.Round(point.X, 0);
                    if (!UpBound.ContainsKey(x))
                    {
                        UpBound[x] = point.Y;
                        DownBound[x] = point.Y;
                        if (is_last_visible)
                            g.DrawLine(myPen, Position2d(last_point), Position2d(point));
                        last_point = point;
                        is_last_visible = true;
                    }
                    else
                    if (point.Y >= UpBound[x])
                    {
                        if (is_last_visible)
                            g.DrawLine(myPen, Position2d(last_point), Position2d(point));
                        UpBound[x] = point.Y;
                        last_point = point;
                        is_last_visible = true;
                    }
                    else
                    if (point.Y <= DownBound[x])
                    {
                        if (is_last_visible)
                            g.DrawLine(myPen, Position2d(last_point), Position2d(point));
                        DownBound[x] = point.Y;
                        last_point = point;
                        is_last_visible = true;
                    }
                    else
                        is_last_visible = false;
                }
            }
            pictureBox1.Image = bmp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            List<Edge> newEdges = new List<Edge>();
            foreach (var edge in poly.edges)
            {
                Edge newPoints = new Edge();
                foreach (var point in edge.points)
                {
                    double[,] m = new double[1, 4];
                    m[0, 0] = point.X - moving_point.X;
                    m[0, 1] = point.Y - moving_point.Y;
                    m[0, 2] = point.Z - moving_point.Z;
                    m[0, 3] = 1;

                    var angle = double.Parse(textBox6.Text) * Math.PI / 180;
                    double[,] matrx = new double[4, 4]
                {   { Math.Cos(angle), 0, Math.Sin(angle), 0},
                    { 0, 1, 0, 0 },
                    {-Math.Sin(angle), 0, Math.Cos(angle), 0 },
                    { 0, 0, 0, 1 } };

                    angle = double.Parse(textBox5.Text) * Math.PI / 180;
                    double[,] matry = new double[4, 4]
                    {  { 1, 0, 0, 0 },
                    { 0, Math.Cos(angle), -Math.Sin(angle), 0},
                    {0, Math.Sin(angle), Math.Cos(angle), 0 },
                    { 0, 0, 0, 1 } };

                    angle = double.Parse(textBox4.Text) * Math.PI / 180;
                    double[,] matrz = new double[4, 4]
                    {  { Math.Cos(angle), -Math.Sin(angle), 0, 0},
                    { Math.Sin(angle), Math.Cos(angle), 0, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 0, 1 } };

                    var final_matrix = MultiplyMatrix(m, matrx);
                    final_matrix = MultiplyMatrix(final_matrix, matry);
                    final_matrix = MultiplyMatrix(final_matrix, matrz);

                    newPoints.points.Add(new Point3(final_matrix[0, 0] + moving_point.X, final_matrix[0, 1] + moving_point.Y, final_matrix[0, 2] + moving_point.Z));
                }
                newEdges.Add(newPoints);
            }
            poly.edges = newEdges;
            //DrawPol();
            DrawFunc();
            pictureBox1.Image = bmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var posx = double.Parse(textBox1.Text);
            var posy = double.Parse(textBox2.Text);
            var posz = double.Parse(textBox3.Text);


            g.Clear(Color.White);
            moving_point.X += posx;
            moving_point.Y -= posy;
            moving_point.Z += posz;
            List<Edge> newEdges = new List<Edge>();
            foreach (var edge in poly.edges)
            {
                Edge newPoints = new Edge();
                foreach (var point in edge.points)
                {
                    double[,] m = new double[1, 4];
                    m[0, 0] = point.X;
                    m[0, 1] = point.Y;
                    m[0, 2] = point.Z;
                    m[0, 3] = 1;

                    double[,] matr = new double[4, 4]
                {   { 1, 0, 0, 0},
                    { 0, 1, 0, 0 },
                    {0, 0, 1, 0 },
                    { posx, -posy, posz, 1 } };

                    var final_matrix = MultiplyMatrix(m, matr);

                    newPoints.points.Add(new Point3(final_matrix[0, 0], final_matrix[0, 1], final_matrix[0, 2]));
                }
                newEdges.Add(newPoints);

            }
            poly.edges = newEdges;
            //DrawPol();
            DrawFunc();
            pictureBox1.Image = bmp;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            List<Edge> newEdges = new List<Edge>();
            var posx = double.Parse(textBox9.Text);
            var posy = double.Parse(textBox8.Text);
            var posz = double.Parse(textBox7.Text);
            foreach (var edge in poly.edges)
            {
                Edge newPoints = new Edge();
                foreach (var point in edge.points)
                {
                    double[,] m = new double[1, 4];
                    m[0, 0] = point.X - moving_point.X;
                    m[0, 1] = point.Y - moving_point.Y;
                    m[0, 2] = point.Z - moving_point.Z;
                    m[0, 3] = 1;

                    double[,] matr = new double[4, 4]
                {   { posx, 0, 0, 0 },
                    { 0, posy, 0, 0 },
                    { 0, 0, posz, 0 },
                    { 0, 0, 0, 1 } };

                    var final_matrix = MultiplyMatrix(m, matr);

                    newPoints.points.Add(new Point3(final_matrix[0, 0] + moving_point.X, final_matrix[0, 1] + moving_point.Y, final_matrix[0, 2] + moving_point.Z));
                }
                newEdges.Add(newPoints);

            }
            poly.edges = newEdges;
            //DrawPol();
            DrawFunc();
        }
    }
}
