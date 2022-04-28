using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winforms_image_processor
{


    public struct Line
    {
        public Point Start;
        public Point End;

        public Line(Point s, Point e)
        {
            End = e;
            Start = s;
        }

        public override string ToString()
        {
            return $"{Start.ToString()} && {End.ToString()}";
        }
    }

    [Serializable]
    class MidPointLine : Shape
    {
        public Point? startPoint = null;
        public Point? endPoint = null;
        public int thickness;

        //Clipping clipper = null;

        public MidPointLine(Color color, int thicc) : base(color)
        {
            thickness = thicc - 1;
            shapeType = DrawingShape.LINE;
            supportsAA = true;
        }

        public MidPointLine(Color color, int thicc, Point start, Point end) : base(color)
        {
            thickness = thicc - 1;
            shapeType = DrawingShape.LINE;
            startPoint = start;
            endPoint = end;
        }


        public override string ToString()
        {
            return "Line";
        }

        public override int AddPoint(Point point)
        {
            if (startPoint == null)
                startPoint = point;
            else if (endPoint == null)
            {
                endPoint = point;
                return 1;
            }
            return 0;
        }

        public override List<ColorPoint> GetPixels(params object[] param)
        {
            Line line = new Line(startPoint.Value, endPoint.Value);

           
           return SymmetricMidpointLine(line.Start, line.End);
        }

        public override List<ColorPoint> GetPixelsAA(Bitmap bmp)
        {
            Line line = new Line(startPoint.Value, endPoint.Value);

            //if (clipper != null && !clipper.ClipLine(ref line))
               // return new List<ColorPoint>();

            return GuptaSproullAlgorithm(bmp, line.Start, line.End);
        }


        List<ColorPoint> SymmetricMidpointLine(Point p1, Point p2)
        {
            List<ColorPoint> points = new List<ColorPoint>();

            // TODO: simplify
            // TODO: optimize


            // division by zero is turned into Infinity; which here is correct
            int slope = Math.Abs((p2.Y - p1.Y) / (p2.X - p1.X));
            

            // slope > 1 means we are in the 2nd, 3rd, 6th, or 7th octant: flip the axes
            if (slope > 1)
            {




                if (p1.Y < p2.Y)
                {
                    p1 = p1;
                    p2 = p2;
                }
                else
                {
                    Point temp;
                    temp = p1;
                    p1 = p2;
                    p2 = temp;
                }

                int dx = p2.X - p1.X;
                int dy = p2.Y - p1.Y;
                Point delta = new Point(dx, dy);

                int dE = 2 * delta.X;
                int dNE = 2 * (delta.X - delta.Y);
                int dSE = 2 * (delta.X + delta.Y);
                Point f = p1;
                Point b = p2;

                // 3rd or 6th octant:
                if (p1.X < p2.X)
                {
                    Point fNE = new Point(1, 0);
                    var bNE = new Point(-fNE.X,-fNE.Y);

                    int d = 2 * delta.X - delta.Y;

                    do
                    {
                        points.Add(new ColorPoint(shapeColor, new Point(f.X,f.Y)));
                        points.Add(new ColorPoint(shapeColor, new Point(b.X, b.Y)));


                        f = new Point(f.X+0, f.Y+1);
                        b = new Point(b.X+0, b.Y-1);
                        if (d < 0)
                        {
                            d += dE;
                        }
                        else
                        {
                            d += dNE;
                            f = new Point(f.X + fNE.X, f.Y + fNE.Y);
                            b = new Point(b.X + bNE.X, b.Y +bNE.Y);
                        }
                    } while (f.Y <= f.Y);
                }
                else
                {
                    var fSE = new Point(-1, 0);
                    var bSE = new Point(-fSE.X, -fSE.Y);

                     int d = 2 * delta.X + delta.Y;

                    do
                    {
                        points.Add(new ColorPoint(shapeColor, new Point(f.X, f.Y)));
                        points.Add(new ColorPoint(shapeColor, new Point(b.X, b.Y)));

                        f = new Point(f.X + 0, f.Y + 1);
                        b = new Point(b.X+0, b.Y-1);
                        if (d < 0)
                        {
                            d += dSE;
                            f = new Point(f.X + fSE.X, f.Y + fSE.Y);
                            b = new Point(b.X + bSE.X, b.Y + bSE.Y);
                           
                        }
                        else
                        {
                            d += dE;
                        }
                    } while (f.Y <= b.Y);
                }
            }
            else
            {
                if (p1.X < p2.X)
                {
                    p1 = p1;
                    p2 = p2;
                }
                else
                {
                    Point temp;
                    temp=p1;
                    p1 = p2;
                    p2 = temp;
                }
                int dx = p2.X - p1.X;
                int dy = p2.Y - p1.Y;
                Point delta = new Point(dx, dy);

                //const delta = p2.sub(p1);
                int dE = 2 * delta.Y;
                int dNE = 2 * (delta.Y - delta.X);
                int dSE = 2 * (delta.Y + delta.X);
                Point f = p1;
                Point b = p2;

                // 1st or 5th octant:
                if (p1.Y < p2.Y)
                {
                    var fNE = new Point(0, 1);
                    var bNE = new Point(-fNE.X,-fNE.Y);

                    int d = 2 * delta.Y - delta.X;

                    do
                    {
                        points.Add(new ColorPoint(shapeColor, new Point(f.X, f.Y)));
                        points.Add(new ColorPoint(shapeColor, new Point(b.X, b.Y)));

                        f = new Point(f.X + 1, f.Y + 0);
                        b = new Point(b.X + -1, b.Y +0);

                        if (d < 0)
                        {
                            d += dE;
                        }
                        else
                        {
                            d += dNE;
                            f = new Point(f.X + fNE.X, f.Y + fNE.Y);
                            b = new Point(b.X + bNE.X, b.Y + bNE.Y);
                        }
                    } while (f.X <= b.X);
                }
                else
                {
                    var fSE = new Point(0, -1);
                    var bSE = new Point(-fSE.X, -fSE.Y);
                    

                    int d = 2 * delta.Y + delta.X;

                    do
                    {
                        points.Add(new ColorPoint(shapeColor, new Point(f.X, f.Y)));
                        points.Add(new ColorPoint(shapeColor, new Point(b.X, b.Y)));


                        f = new Point(f.X + 1, f.Y + 0);
                        b = new Point(b.X + -1, b.Y + 0);

                        
                        if (d < 0)
                        {
                            d += dSE;
                            f = new Point(f.X + fSE.X, f.Y + fSE.Y);
                            b = new Point(b.X + bSE.X, b.Y + bSE.Y);
                        }
                        else
                        {
                            d += dE;
                        }
                    } while (f.X <= b.X);
                }
            }

            return points;
        }
    


        private void plotLineHigh(List<ColorPoint> points, int x0, int y0, int x1, int y1)
        {
            int dx = x1 - x0 ;
            int dy = y1 - y0 ;
            int xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }

            int D = (2 * dx) - dy;
            int x = x0;

            for (int y = y0; y < y1; y1++)
            {
                points.Add(new ColorPoint(shapeColor, new Point(x, y)));
                if (D > 0)
                {
                    x = x + xi;
                    D = D + (2 * (dx - dy));
                }

                else
                {
                    D = D + 2 * dx;
                }
                
            }

           


        }

        private void plotLineLow(List<ColorPoint> points, int x0, int y0,int x1,int y1)
        {
            int dx = x1 - x0;
            int dy = y1 - y0;
            int yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }

            int D = (2 * dy) - dx;
            int y = y0;

            for (int x = x0; x < x1; x++)
            {
                points.Add(new ColorPoint(shapeColor,new Point(x,y)));
                if (D > 0)
                {
                    y = y + yi;
                    D += (2 * (dy - dx));
                }
                else
                {
                    D += 2 * dy;
                }
            }


        }

        private List<ColorPoint> plotLine(Point start, Point end)
        {
            List<ColorPoint> points = new List<ColorPoint>();
            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;
            if (Math.Abs((y1 - y0)) < Math.Abs((x1 - x0)))
            {
                if (x0 > x1)
                    plotLineLow(points, x1, y1, x0, y0);

                else
                    plotLineLow(points, x0, y0, x1, y1);

            }
            else
            {
                if (y0 > y1)
                    plotLineHigh(points, x1, y1, x0, y0);

                else
                    plotLineHigh(points, x0, y0, x1, y1);

            }

            return points;
        }
            
       



        List<ColorPoint> BresenhamMidPointAlgorithm(Point start, Point end)
        // https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
        {
            /* List<ColorPoint> points = new List<ColorPoint>();

             int x = start.X, y = start.Y;
             int x2 = end.X, y2 = end.Y;

             int w = x2 - x;
             int h = y2 - y;
             int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
             if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
             if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
             if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
             int longest = Math.Abs(w);
             int shortest = Math.Abs(h);
             if (!(longest > shortest))
             {
                 longest = Math.Abs(h);
                 shortest = Math.Abs(w);
                 if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                 dx2 = 0;
             }
             int numerator = longest >> 1;
             for (int i = 0; i <= longest; i++)
             {
                 points.Add(new ColorPoint(shapeColor, new Point(x, y)));
                 if (Math.Abs(h) > Math.Abs(w))
                     for (int j = 1; j < thickness; j++)
                     {
                         points.Add(new ColorPoint(shapeColor, new Point(x - j, y)));
                         points.Add(new ColorPoint(shapeColor, new Point(x + j, y)));
                     }
                 else if (Math.Abs(w) > Math.Abs(h))
                     for (int j = 1; j < thickness; j++)
                     {
                         points.Add(new ColorPoint(shapeColor, new Point(x - j, y)));
                         points.Add(new ColorPoint(shapeColor, new Point(x + j, y)));
                     }

                 numerator += shortest;
                 if (!(numerator < longest))
                 {
                     numerator -= longest;
                     x += dx1;
                     y += dy1;
                 }
                 else
                 {
                     x += dx2;
                     y += dy2;
                 }
             }

             return points;*/

            List<ColorPoint> points = new List<ColorPoint>();

            int x1 = start.X, y1 = start.Y;
            int x2 = end.X, y2 = end.Y;

            int dx = x2 - x1;
            int dy = y2 - y1;
           // int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            int d = 2 * dy - dx;
            int dE = 2 * dy;
            int dNE = 2 * (dy - dx);
            int xf = x1, yf = y1;
            int xb = x2, yb = y2;
            points.Add(new ColorPoint(shapeColor,new Point(xf,yf)));  //putPixel(xf, yf);
            points.Add(new ColorPoint(shapeColor, new Point(xb, yb)));//putPixel(xb, yb);
            while (xf < xb)
            {
                ++xf; --xb;
                if (d < 0)
                    d += dE;
                else
                {
                    d += dNE;
                    ++yf;
                        --yb;
                }
                points.Add(new ColorPoint(shapeColor, new Point(xf, yf)));//putPixel(xf, yf);
                points.Add(new ColorPoint(shapeColor, new Point(xb, yb)));//putPixel(xb, yb);
            }
            while (yf < yb)
            {
                ++yf; --yb;
                if (d < 0)
                    d += dE;
                else
                {
                    d -= dNE;
                    ++xb;
                    --xf;
                }
                points.Add(new ColorPoint(shapeColor, new Point(xf, yf)));//putPixel(xf, yf);
                points.Add(new ColorPoint(shapeColor, new Point(xb, yb)));//putPixel(xb, yb);
            }



            return points;
           



        }

        List<ColorPoint> GuptaSproullAlgorithm(Bitmap bmp, Point start, Point end)
        // http://elynxsdk.free.fr/ext-docs/Rasterization/Antialiasing/Gupta%20sproull%20antialiased%20lines.htm
        // https://jamesarich.weebly.com/uploads/1/4/0/3/14035069/480xprojectreport.pdf
        {
            var points = new List<ColorPoint>();

            int x1 = start.X, y1 = start.Y;
            int x2 = end.X, y2 = end.Y;

            int dx = x2 - x1;
            int dy = y2 - y1;

            int du, dv, u, x, y, ix, iy;

            // By switching to (u,v), we combine all eight octant
            int adx = dx < 0 ? -dx : dx;
            int ady = dy < 0 ? -dy : dy;
            x = x1;
            y = y1;
            if (adx > ady)
            {
                du = adx;
                dv = ady;
                u = x2;
                ix = dx < 0 ? -1 : 1;
                iy = dy < 0 ? -1 : 1;
            }
            else
            {
                du = ady;
                dv = adx;
                u = y2;
                ix = dx < 0 ? -1 : 1;
                iy = dy < 0 ? -1 : 1;
            }

            int uEnd = u + du;
            int d = (2 * dv) - du; // Initial value as in Bresenham's
            int incrS = 2 * dv; // Δd for straight increments
            int incrD = 2 * (dv - du); // Δd for diagonal increments
            int twovdu = 0; // Numerator of distance
            double invD = 1.0 / (2.0 * Math.Sqrt(du * du + dv * dv)); // Precomputed inverse denominator
            double invD2du = 2.0 * (du * invD); // Precomputed constant

            if (adx > ady)
            {
                do
                {
                    points.Add(newColorPixel(bmp, x, y, twovdu * invD));
                    points.Add(newColorPixel(bmp, x, y + iy, invD2du - twovdu * invD));
                    points.Add(newColorPixel(bmp, x, y - iy, invD2du + twovdu * invD));


                    if (d < 0)
                    {
                        // Choose straight
                        twovdu = d + du;
                        d += incrS;

                    }
                    else
                    {
                        // Choose diagonal
                        twovdu = d - du;
                        d += incrD;
                        y += iy;
                    }
                    u++;
                    x += ix;
                } while (u < uEnd);
            }
            else
            {
                do
                {
                    points.Add(newColorPixel(bmp, x, y, twovdu * invD));
                    points.Add(newColorPixel(bmp, x, y + iy, invD2du - twovdu * invD));
                    points.Add(newColorPixel(bmp, x, y - iy, invD2du + twovdu * invD));

                    if (d < 0)
                    {
                        // Choose straight
                        twovdu = d + du;
                        d += incrS;
                    }
                    else
                    {
                        // Choose diagonal
                        twovdu = d - du;
                        d += incrD;
                        x += ix;
                    }
                    u++;
                    y += iy;
                } while (u < uEnd);
            }

            return points;
        }

        ColorPoint newColorPixel(Bitmap bmp, int x, int y, double dist)
        {
            double value = 1 - Math.Pow((dist * 2 / 3), 2);

            Color old = bmp.GetPixelFast(x, y);
            Color col = ColorInterpolator.InterpolateBetween(old, shapeColor, value);

            return new ColorPoint(col, new Point(x, y));
        }

        public override void MovePoints(Point displacement)
        {
            startPoint = startPoint.Value + (Size)displacement;
            endPoint = endPoint.Value + (Size)displacement;
        }

        public override string howToDraw()
        {
            return "Click start and end points";
        }

        public override List<ColorPoint> SuperGetPixels(params object[] param)
        {
            Line line = new Line(startPoint.Value, endPoint.Value);
            return BresenhamMidPointAlgorithm(line.Start, line.End);


        }
    }
}
