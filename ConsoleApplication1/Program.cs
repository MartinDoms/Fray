using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fray;
using System.Windows.Media.Media3D;
using Microsoft.FSharp.Collections;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static Matrix3D mx1, mx2, mx3, mx4;
        static Fray.Material m1, m2, m3, m4;
        const int width = 800;
        const int height = 600;
        static object bmpLock = new object();

        static void Main(string[] args)
        {
            var cam = new Fray.Camera(new Point3D(3.5, -6, 4), new Point3D(0, 0.0, 0), new Vector3D(0, 0, 1), Math.PI/3.0);

            SetupMaterials();
            SetupMatrices();

            var sphere1 = Fray.Shape.NewSphere(m1, mx1);
            var sphere2 = Fray.Shape.NewSphere(m2, mx2);
            var sphere3 = Fray.Shape.NewSphere(m3, mx3);
            var plane = Fray.Shape.NewPlane(m4, mx4);
            
            var light = new Fray.Light(new Point3D(-3, 0, 2),      new Color(0.7, 0.7, 0.7));
            var light2 = new Fray.Light(new Point3D(-1, -5.5, 2.5), new Color(0.7, 0.7, 0.7));
            var light3 = new Fray.Light(new Point3D(1, 2.5, 2),    new Color(0.7, 0.7, 0.7));

            var scene = new Scene(cam, new[] { sphere1,sphere2,sphere3,plane }, new Color(0.2, 0.2, 0.2), new[] { light,light2,light3 });
            var ray = new Ray(cam.position, new Vector3D(3, 0.0, 0));

            var colors = Fray.Fray.RayTrace(width, height, 3, scene);

            var bmp = new System.Drawing.Bitmap(width, height);

            Parallel.ForEach(colors, tuple =>
            {
                var c = tuple.Item3;
                var color = System.Drawing.Color.FromArgb(255, (int)(255 * c.r), (int)(255 * c.g), (int)(255 * c.b));
                SetPixel(tuple.Item1, tuple.Item2, color, bmp);
            });

            bmp.Save(@"output.jpg");
        }

        private static void SetPixel(int x, int y, System.Drawing.Color color, System.Drawing.Bitmap bmp)
        {
            lock (bmpLock)
            {
                bmp.SetPixel(x, y, color);
            }
        }

        static void SetupMatrices()
        {
            mx1 = new Matrix3D();
            mx1.Translate(new Vector3D(0, -2.4, 0));
            mx2 = new Matrix3D();
            mx3 = new Matrix3D();
            mx3.Translate(new Vector3D(0, 2.4, 0));
            mx4 = new Matrix3D();
            mx4.Translate(new Vector3D(0, 0, -1));
        }

        static void SetupMaterials()
        {
            m1 = new Fray.Material(new Color(1,0.1,0.1), new Color(1,1,1), 80, 0.6);
            m2 = new Fray.Material(new Color(0.1,1,0.1), new Color(1,1,1), 80, 0.6);
            m3 = new Fray.Material(new Color(0.1,0.1,1), new Color(1,1,1), 80, 0.6);
            m4 = new Fray.Material(new Color(0.5,0.5,0.5), new Color(0.1,0.1,0.1), 300, 0.8);
        }

    }
}
