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

           
            return XiaolinWu_Line(line.Start,line.End);
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








        List<ColorPoint> XiaolinWu_Line(Point point1, Point point2)
        {
            //Source: http://rosettacode.org/wiki/Xiaolin_Wu's_line_algorithm

            List<ColorPoint> points = new List<ColorPoint>();
            bool direction = Math.Abs(point2.Y - point1.Y) > Math.Abs(point2.X - point1.X);
            if (direction)//replace x and y
            {
                Point tmp = new Point(point1.X, point1.Y);
                Point tmp2 = new Point(point2.X, point2.Y);
                point1 = new Point(tmp.Y, tmp.X);
                point2 = new Point(tmp2.Y, tmp2.X);
            }
            if (point1.X > point2.X)//replace points
            {
                Point tmp = new Point(point1.X, point1.Y);
                Point tmp2 = new Point(point2.X, point2.Y);
                point1 = new Point(tmp2.X, tmp2.Y);
                point2 = new Point(tmp.X, tmp.Y);
            }
            double dx = point2.X - point1.X;
            double dy = point2.Y - point1.Y;
            double gradient = (double)((double)(dy) / (double)(dx));
            //first point
            int endX = round(point1.X);
            double endY = point1.Y + gradient * (endX - point1.X);
            double gapX = rfPart(point1.X + 0.5);
            int pxlX_1 = endX;
            int pxlY_1 = iPart(endY);
            if (direction)
            {
                Point a = new Point(pxlY_1, pxlX_1);
                ColorPoint b = new ColorPoint(shapeColor, new Point(pxlY_1, pxlX_1));
                points.Add(b);
                b = new ColorPoint(shapeColor, new Point(pxlY_1 + 1, pxlX_1));
                points.Add(b);
            }
            else
            {
                Point a = new Point(pxlX_1, pxlY_1);
                ColorPoint b = new ColorPoint(shapeColor, new Point(pxlX_1, pxlY_1));
                points.Add(b);
                b = new ColorPoint(shapeColor, new Point(pxlX_1, pxlY_1 + 1));
                points.Add(b);
            }
            double intery = endY + gradient;

            //second point
            endX = round(point2.X);
            endY = point2.Y + gradient * (endX - point2.X);
            gapX = fPart(point2.X + 0.5);
            int pxlX_2 = endX;
            int pxlY_2 = iPart(endY);
            if (direction)
            {
                Point a = new Point(pxlY_2, pxlX_2);
                ColorPoint b = new ColorPoint(shapeColor, new Point(pxlY_2, pxlX_2));
                points.Add(b);
                b = new ColorPoint(shapeColor, new Point(pxlY_2 + 1, pxlX_2));
                points.Add(b);
            }
            else
            {
                Point a = new Point(pxlX_2, pxlY_2);
                ColorPoint b = new ColorPoint(shapeColor, new Point(pxlX_2, pxlY_2));
                points.Add(b);
                b = new ColorPoint(shapeColor, new Point(pxlX_2, pxlY_2 + 1));
                points.Add(b);
            }

            //loop from all points
            for (double x = (pxlX_1 + 1); x <= (pxlX_2 - 1); x++)
            {
                if (direction)
                {
                    Point a = new Point(iPart(intery), (int)x);
                    ColorPoint b = new ColorPoint(shapeColor, new Point(iPart(intery), (int)x));
                    points.Add(b);
                    b = new ColorPoint(shapeColor, new Point(iPart(intery) + 1, (int)x));
                    points.Add(b);
                }
                else
                {
                    Point a = new Point((int)x, iPart(intery));
                    ColorPoint b = new ColorPoint(shapeColor, new Point((int)x, iPart(intery)));
                    points.Add(b);
                    b = new ColorPoint(shapeColor, new Point((int)x, iPart(intery) + 1));
                    points.Add(b);
                }
                intery += gradient;
            }

            return points;
        }

        //functions for calculating
        int iPart(double d)
        {
            return (int)d;
        }

        int round(double d)
        {
            return (int)(d + 0.50000);
        }

        double fPart(double d)
        {
            return (double)(d - (int)(d));
        }

        double rfPart(double d)
        {
            return (double)(1.00000 - (double)(d - (int)(d)));
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
            return SymmetricMidpointLine(line.Start, line.End);


        }
    }
}
