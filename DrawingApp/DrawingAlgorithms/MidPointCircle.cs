using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winforms_image_processor
{

    [Serializable]
     class MidPointCircle : Shape
    {
        Point? center = null;
        int? radius = null;
        Color backColor;

        public MidPointCircle(Color color) : base(color)
        {
            shapeType = DrawingShape.CIRCLE;
            supportsAA = true;
        }

        public MidPointCircle(Color color, Point center, int radius) : base(color)
        {
            shapeType = DrawingShape.CIRCLE;
            this.center = center;
            this.radius = radius;
            supportsAA = true;
        }

        public override string ToString()
        {
            return "Circle";
        }

        public override int AddPoint(Point point)
        {
            if (center == null)
                center = point;
            else if (radius == null)
            {
                radius = (int)Math.Sqrt(Math.Pow(point.X - center.Value.X, 2) + Math.Pow(point.Y - center.Value.Y, 2));
                return 1;
            }
            return 0;
        }
        
        public override List<ColorPoint> SuperGetPixels(params object[] param)
        // https://www.geeksforgeeks.org/mid-point-circle-drawing-algorithm/
        {
            if (!center.HasValue || !radius.HasValue)
                throw new MissingMemberException();

            if (radius.Value == 0)
                return new List<ColorPoint>() { new ColorPoint(shapeColor, center.Value) };

            var points = new List<ColorPoint>();

            int x = 2*radius.Value, y = 0;
            int P = 1 - x;

            while (x > y)
            {

                y++;

                if (P <= 0)
                    P = P + 2 * y + 1;
                else
                {
                    x--;
                    P = P + 2 * y - 2 * x + 1;
                }

                if (x < y)
                    break;

                points.Add(new ColorPoint(shapeColor, new Point(2*(x + center.Value.X), 2*(y + center.Value.Y))));
                points.Add(new ColorPoint(shapeColor, new Point(2*(-x + center.Value.X), 2*(y + center.Value.Y))));
                points.Add(new ColorPoint(shapeColor, new Point(2 * (x + center.Value.X), 2 * (-y + center.Value.Y))));
                points.Add(new ColorPoint(shapeColor, new Point(2 * (-x + center.Value.X), 2*(-y + center.Value.Y))));

                // If the generated point is on the  
                // line x = y then the perimeter points 
                // have already been printed 
                if (x != y)
                {
                    points.Add(new ColorPoint(shapeColor, new Point(2 * (y + center.Value.X), 2 * (x + center.Value.Y))));
                    points.Add(new ColorPoint(shapeColor, new Point(2 * (-y + center.Value.X), 2 * (x + center.Value.Y))));
                    points.Add(new ColorPoint(shapeColor, new Point(2 * (y + center.Value.X), 2 * (-x + center.Value.Y))));
                    points.Add(new ColorPoint(shapeColor, new Point(2 * (-y + center.Value.X), 2 * (-x + center.Value.Y))));
                }
            }

            return points;




        }




        public override List<ColorPoint> GetPixels(params object[] param)
        // https://www.geeksforgeeks.org/mid-point-circle-drawing-algorithm/
        {
             if (!center.HasValue || !radius.HasValue)
                 throw new MissingMemberException();

             if (radius.Value == 0)
                 return new List<ColorPoint>() { new ColorPoint(shapeColor, center.Value) };

             var points = new List<ColorPoint>();

             int x = radius.Value, y = 0;
             int P = 1 - x;

             while (x > y)
             {

                 y++;

                 if (P <= 0)
                     P = P + 2 * y + 1;
                 else
                 {
                     x--;
                     P = P + 2 * y - 2 * x + 1;
                 }

                 if (x < y)
                     break;

                 points.Add(new ColorPoint(shapeColor, new Point(x + center.Value.X, y + center.Value.Y)));
                 points.Add(new ColorPoint(shapeColor, new Point(-x + center.Value.X, y + center.Value.Y)));
                 points.Add(new ColorPoint(shapeColor, new Point(x + center.Value.X, -y + center.Value.Y)));
                 points.Add(new ColorPoint(shapeColor, new Point(-x + center.Value.X, -y + center.Value.Y)));

                 // If the generated point is on the  
                 // line x = y then the perimeter points 
                 // have already been printed 
                 if (x != y)
                 {
                     points.Add(new ColorPoint(shapeColor, new Point(y + center.Value.X, x + center.Value.Y)));
                     points.Add(new ColorPoint(shapeColor, new Point(-y + center.Value.X, x + center.Value.Y)));
                     points.Add(new ColorPoint(shapeColor, new Point(y + center.Value.X, -x + center.Value.Y)));
                     points.Add(new ColorPoint(shapeColor, new Point(-y + center.Value.X, -x + center.Value.Y)));
                 }
             }

             return points;
            
        

            
        }

        List<ColorPoint> circleXiaolinWu(Bitmap bmp, Point center, double radius)
        {
            List<ColorPoint> points = new List<ColorPoint>();

            Color L = shapeColor; /*Line color*/
            Color B = backColor; /*Background Color*/
            int x = (int)radius;
            int y = 0;

            points.Add(new ColorPoint(shapeColor, new Point(x + center.X, y + center.Y)));
            points.Add(new ColorPoint(shapeColor, new Point(-x + center.X, -y + center.Y)));
            points.Add(new ColorPoint(shapeColor, new Point(x + center.X, -y + center.Y)));
            points.Add(new ColorPoint(shapeColor, new Point(-x + center.X, y + center.Y)));
            while ((int)radius > y)
            {
                y++;

                x = (int)Math.Ceiling(Math.Sqrt((radius * radius - (double)(y * y))));
                double T = distance(radius, y);
                Color c2 = Color.FromArgb(
                    L.A,
                    (int)(L.R * (1 - T) + B.R * T),
                    (int)(L.G * (1 - T) + B.G * T),
                    (int)(L.B * (1 - T) + B.B * T)
                );
                Color c1 = Color.FromArgb(
                    L.A,
                    (int)(L.R * T + B.R * (1 - T)),
                    (int)(L.G * T + B.G * (1 - T)),
                    (int)(L.B * T + B.B * (1 - T))
                );

                points.Add(new ColorPoint(c2, new Point(x + center.X, y + center.Y))); // point A
                points.Add(new ColorPoint(c2, new Point(-x + center.X, -y + center.Y))); // A symmetric to circle center
                points.Add(new ColorPoint(c2, new Point(x + center.X, -y + center.Y))); // point B: A symetric to axis x=center.X
                points.Add(new ColorPoint(c2, new Point(-x + center.X, y + center.Y))); // B symmetric to axis x=center.X 

                points.Add(new ColorPoint(c1, new Point(x - 1 + center.X, y + center.Y)));
                points.Add(new ColorPoint(c1, new Point(-x + 1 + center.X, -y + center.Y)));
                points.Add(new ColorPoint(c1, new Point(x - 1 + center.X, -y + center.Y)));
                points.Add(new ColorPoint(c1, new Point(-x + 1 + center.X, y + center.Y)));
            }

            return points;
        }

        double distance(double r, int y)
        {
            return Math.Ceiling(Math.Sqrt(r * r - y * y)) - Math.Sqrt(r * r - y * y);
        }

        public override void MovePoints(Point displacement)
        {
            center = center.Value + (Size)displacement;
        }

        public override string howToDraw()
        {
            return "Click center and radius";
        }

        public override List<ColorPoint> GetPixelsAA(Bitmap bmp)
        {
            return circleXiaolinWu(bmp, this.center.Value, this.radius.Value);
        }
    }
}
