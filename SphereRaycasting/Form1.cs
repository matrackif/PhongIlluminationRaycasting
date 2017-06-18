using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
namespace SphereRaycasting
{
    public class Ray
    {
        public Point3D From { get; set; }
        public Point3D To { get; set; }
        public Ray(double fromX, double fromY, double fromZ, double toX, double toY, double toZ)
        {
            From = new Point3D(fromX, fromY, fromZ);
            To = new Point3D(toX, toY, toZ);
        }
        public Ray(Point3D from, Point3D to)
        {
            From = new Point3D(from.X, from.Y, from.Z);
            to = new Point3D(to.X, to.Y, to.Z);
        }
        public Vector3D ToVector3D()
        {
            Vector3D vec = new Vector3D();
            if (From != null && To != null)
            {
                vec.X = To.X - From.X;
                vec.Y = To.Y - From.Y;
                vec.Z = To.Z - From.Z;
            }
            return vec;
        }
    }
    public class Sphere
    {
        public int centerX { get; set; }
        public int centerY { get; set; }
        public int centerZ { get; set; }
        public int radius { get; set; }
        public Sphere(int centX, int centY, int centZ, int r)
        {
            centerX = centX;
            centerY = centY;
            centerZ = centZ;
            radius = r;
        }
    }
    public partial class PhongForm : Form
    {

        Point3D viewerPoint;
        Point3D lightSource;
        private Color defaultBackgroundColor = Color.Gray;
        private Color defaultSphereColor = Color.Green;
        private List<Sphere> spheres = new List<Sphere>();
        public PhongForm()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Sphere s1 = new Sphere(pictureBox1.Width / 2, pictureBox1.Height / 2, 150, 200);
            spheres.Add(s1);
            //sphereCenterX = pictureBox1.Width / 2;
            //sphereCenterY = pictureBox1.Height / 2;
            //sphereCenterZ = 150;
            //sphereRadius = 200;
            viewerPoint = new Point3D(0, 0, -1000);
            lightSource = new Point3D(pictureBox1.Width / 2 - 150, pictureBox1.Height / 2, -100);
            foreach(Sphere s in spheres)
            {
                DrawIlluminatedSphere(s);
            }
        }
        public void DrawIlluminatedSphere(Sphere sphere)
        {
            int sphereCenterX = sphere.centerX;
            int sphereCenterY = sphere.centerY;
            int sphereCenterZ = sphere.centerZ;
            int sphereRadius = sphere.radius;

            Bitmap bmp = (Bitmap)pictureBox1.Image;
            int viewerX = (int)viewerPoint.X;
            int viewerY = (int)viewerPoint.Y;
            int viewerZ = (int)viewerPoint.Z;
            for (int i = 0; i < bmp.Width; i++)
            {
                for(int j = 0; j < bmp.Height; j++)
                {
                    Ray rayFromCameraToPixel = new Ray(viewerX, viewerY, viewerZ, i, j, 0);
                    List<double> intersectionXYZ = FindIntersectionWithSphere(rayFromCameraToPixel, sphere);
                    if(intersectionXYZ.Count > 0)
                    {
                        bmp.SetPixel(i, j, GetPhongIlluminationColor((int)intersectionXYZ[0], (int)intersectionXYZ[1], (int)intersectionXYZ[2], false, sphere));
                    }
                    else
                    {
                        bmp.SetPixel(i, j, GetPhongIlluminationColor(i, j, 0, true, sphere));
                    }
                }
            }
        }
        ///RAYCASTING
        ///Intersection of ray (x0, y0, z0) to (x1,y1,z1) with sphere
        ///TODO should accept a ray argument instead of enpoints of ray
        public List<double> FindIntersectionWithSphere(Ray ray, Sphere sphere)
        {
            double x0 = ray.From.X, y0 = ray.From.Y, z0 = ray.From.Z;
            double x1 = ray.To.X, y1 = ray.To.Y, z1 = ray.To.Z;
            double cx = sphere.centerX, cy = sphere.centerY, cz = sphere.centerZ, r = sphere.radius;
            double dx = x1 - x0;
            double dy = y1 - y0;
            double dz = z1 - z0;
            double a = dx * dx + dy * dy + dz * dz;
            double b = 2 * dx * (x0 - cx) + 2 * dy * (y0 - cy) + 2 * dz * (z0 - cz);
            double c = cx * cx + cy * cy + cz * cz + x0 * x0 + y0 * y0 + z0 * z0 + -2 * (cx * x0 + cy * y0 + cz * z0) - (r * r);
            List<double> intersectionXYZ = new List<double>();
            double discriminant = (b * b) - (4 * a * c);
            if (discriminant < 0)
            {
                // no intersection
            }
            if(discriminant == 0)
            {
                // ray is tangent to sphere
                // add x y and z of intersection
                double t = (-b - Math.Sqrt(discriminant)) / (2 * a);
                intersectionXYZ.Add(x0 + t * dx);
                intersectionXYZ.Add(y0 + t * dy);
                intersectionXYZ.Add(z0 + t * dz);
            }
            if(discriminant > 0)
            { 
                // ray intersects sphere in two points
                double t = (-b - Math.Sqrt(discriminant)) / (2 * a);
                // add x y and z of intersection
                intersectionXYZ.Add(x0 + t * dx);
                intersectionXYZ.Add(y0 + t * dy);
                intersectionXYZ.Add(z0 + t * dz);
            }
            return intersectionXYZ;
        }
        // x and y and z are points on the sphere
        private Color GetPhongIlluminationColor(int x, int y, int z, bool isBackgroundPixel, Sphere sphere)
        {
            Color pixelColor = defaultBackgroundColor;
            double cx = sphere.centerX, cy = sphere.centerY, cz = sphere.centerZ, r = sphere.radius;
            int lightX = (int)lightSource.X;
            int lightY = (int)lightSource.Y;
            int lightZ = (int)lightSource.Z;
            int viewX = (int)viewerPoint.X, viewY = (int)viewerPoint.Y, viewZ = (int)viewerPoint.Z;

            if (!isBackgroundPixel)
            {
                pixelColor = defaultSphereColor; // default sphereColor
                double ka = 0.2; //ambient coefficient
                double kd = 0.8; //diffuse coefficient
                double ks = 0.5; //specular coefficient
                Vector3D normalVec = new Vector3D((x - cx) / r, (y - cy) / r, (z - cz) / r);
                // TODO could use Ray class here
                Ray fromSphereToLight = new Ray(x, y, z, lightX, lightY, lightZ);
                //Vector3D vecFromSphereToLight = new Vector3D(lightX - x, lightY - y, lightZ - z);
                Vector3D vecFromSphereToLight = fromSphereToLight.ToVector3D();
                Ray fromSphereToViewPoint = new Ray(x, y, z, viewX, viewY, viewZ);
                Vector3D vecFromSphereToViewPoint = fromSphereToViewPoint.ToVector3D();
                Vector3D halfwayVector = vecFromSphereToLight + vecFromSphereToViewPoint;
                if (halfwayVector.X != 0 && halfwayVector.Y != 0 && halfwayVector.Z != 0)
                {
                    halfwayVector.Normalize();
                }
                
                if (vecFromSphereToLight.X != 0 && vecFromSphereToLight.Y != 0 && vecFromSphereToLight.Z != 0)
                {
                    vecFromSphereToLight.Normalize();
                }
                double fctr = Vector3D.DotProduct(normalVec, vecFromSphereToLight);
                int n = 2;
                double highlightFactor = Vector3D.DotProduct(halfwayVector, normalVec);
                highlightFactor = Math.Pow(highlightFactor, n);
                int sR = pixelColor.R;
                int sG = pixelColor.G;
                int sB = pixelColor.B;
                // ADDING DIFFUSE SHADING, AMBIENT LIGHT AND SPECULAR LIGHT
                int newR = Math.Abs((int)((ka * sR) + (kd * fctr * sR) + (ks * highlightFactor)));
                int newG = Math.Abs((int)((ka * sG) + (kd * fctr * sG) + (ks * highlightFactor)));
                int newB = Math.Abs((int)((ka * sB) + (kd * fctr * sB) + (ks * highlightFactor)));
                newR = (newR > 255 ? 255 : newR);
                newG = (newG > 255 ? 255 : newG);
                newB = (newB > 255 ? 255 : newB);
                          
                pixelColor = Color.FromArgb(newR, newG, newB);
            }
            else
            {
                // We find the shadows
                Ray fromBackgroundPixelToLight = new Ray(x, y, z, lightX, lightY, lightZ);
                Vector3D vecFromPixelToLight = fromBackgroundPixelToLight.ToVector3D();
                // TODO FindIntersectionWithSphere should accept Ray argument
                List<double> intersectionXYZ = FindIntersectionWithSphere(fromBackgroundPixelToLight, sphere);
                if(intersectionXYZ.Count > 0)
                {
                    // ray intersects
                    pixelColor = Color.FromArgb(pixelColor.R / 2, pixelColor.G / 2, pixelColor.B / 2);
                }
                else
                {
                    // ray does not intersect
                }
            }
            
            
            return pixelColor;
        }
    }
}
