﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace winforms_image_processor
{
    [Serializable]
    public struct ActiveEdgeTableEntry
    {
        public double yMax;
        public double yMin;
        public double mInv;
        public double xOfMin;
        public double xOfMax;

        public ActiveEdgeTableEntry(Point start, Point end)
        {
            Point lower = start.Y > end.Y ? end : start;
            Point higher = start.Y > end.Y ? start : end;

            yMax = higher.Y;
            yMin = lower.Y;
            xOfMax = higher.X;
            xOfMin = lower.X;
            mInv = (xOfMax - xOfMin) / (yMax - yMin);
        }

        public ActiveEdgeTableEntry(ActiveEdgeTableEntry aete)
        {
            yMax = aete.yMax;
            yMin = aete.yMin;
            xOfMax = aete.xOfMax;
            xOfMin = aete.xOfMin + aete.mInv;
            mInv = aete.mInv;
        }

        public override string ToString()
        {
            return $"{yMin} && {xOfMin} && {yMax} && {xOfMax} && {mInv}";
        }
    }

    [Serializable]
    class Filler
    {
        List<Point> prePoints;
        List<ColorPoint> points = null;

        List<Point> vertices = new List<Point>();
        List<KeyValuePair<int, int>> indicies = new List<KeyValuePair<int, int>>();
        List<ActiveEdgeTableEntry> AET = new List<ActiveEdgeTableEntry>();

        Color? fillColor;
        Bitmap fillImage;

        Dictionary<Point, Color> imagePoints;

        public Filler(List<Point> pVertices, Color? fillColor = null, Bitmap fillImage = null)
        {
            var dict = new Dictionary<int, int>();

            for (int i = 0; i < pVertices.Count; i++)
            {
                vertices.Add(pVertices[i]);
                dict.Add(i, pVertices[i].Y);
            }

            indicies = dict.OrderBy(x => x.Value).ToList();

            this.fillColor = fillColor;
            if (fillImage != null)
            {
                this.fillImage = fillImage;
            }
        }

        public void UpdatePoints(List<Point> points)
        {
            vertices = new List<Point>(points);
            this.points = null;
        }

        public List<ColorPoint> FillPoints(Rectangle boundingRect = null)
        {
            if (this.points != null && boundingRect == null)
                return this.points;

            prePoints = new List<Point>();

            int k = 0;
            int i = indicies[k].Key;
            int y, ymin;
            y = ymin = vertices[indicies[0].Key].Y;
            int ymax = vertices[indicies[indicies.Count - 1].Key].Y;

            int len = vertices.Count;

            while (y < ymax)
            {
                while (vertices[i].Y == y)
                {
                    if (vertices[(i - 1 + len) % len].Y > vertices[i].Y)
                        AET.Add(new ActiveEdgeTableEntry(vertices[i], vertices[(i - 1 + len) % len]));

                    if (vertices[(i + 1) % len].Y > vertices[i].Y)
                        AET.Add(new ActiveEdgeTableEntry(vertices[i], vertices[(i + 1) % len]));

                    i = indicies[++k].Key;
                }

                AET.Sort(delegate (ActiveEdgeTableEntry e1, ActiveEdgeTableEntry e2)
                {
                    return e1.xOfMin.CompareTo(e2.xOfMin);
                });

                for (int t = 0; t < AET.Count; t += 2)
                    for (int x1 = (int)AET[t].xOfMin; x1 <= AET[(t + 1) % AET.Count].xOfMin; x1++)
                    {
                        if (boundingRect != null && (x1 > boundingRect.GetCorner(1).X || x1 < boundingRect.GetCorner(0).X || y > boundingRect.GetCorner(0).Y || y < boundingRect.GetCorner(2).Y))
                            continue;
                        prePoints.Add(new Point(x1, y));
                    }

                ++y;
                for (int t = 0; t < AET.Count; t++)
                {
                    AET[t] = new ActiveEdgeTableEntry(AET[t]);
                    if (AET[t].yMax == y)
                        AET.RemoveAt(t--);
                }

            }

            points = new List<ColorPoint>();

            if (fillColor.HasValue)
                foreach (var p in prePoints)
                    points.Add(new ColorPoint(fillColor.Value, p));

            else
                foreach (var kv in fillImage.GetPixels(prePoints, true))
                    points.Add(new ColorPoint(kv.Value, kv.Key));

            return points;
        }

        ColorPoint AddPoint(Point point)
        {
            if (fillColor.HasValue)
                return new ColorPoint(fillColor.Value, point);

            return new ColorPoint(fillImage.GetPixelFast(point.X % fillImage.Width, point.Y % fillImage.Height), point);
        }


    }

    public struct FillLog
    {
        public Color seedCol;
        public Point seedPoint;

        public FillLog(Color col, Point p)
        {
            seedCol = col;
            seedPoint = p;
        }
    }

    public static class FloodFiller
    {
        public static Bitmap FourWayFloodFill(Bitmap bmp, Color COLOR, Point q)
        // https://stackoverflow.com/questions/1257117/a-working-non-recursive-floodfill-algorithm-written-in-c/1257195
        {
            int h = bmp.Height;
            int w = bmp.Width;

            if (q.Y < 0 || q.Y > h - 1 || q.X < 0 || q.X > w - 1)
                return bmp;

            Color SEED_COLOR = bmp.GetPixelFast(q.X, q.Y);

            Stack<Point> stack = new Stack<Point>();
            stack.Push(q);
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int x = p.X;
                int y = p.Y;
                if (y < 0 || y > h - 1 || x < 0 || x > w - 1)
                    continue;
                Color val = bmp.GetPixelFast(x, y);
                if (val == SEED_COLOR)
                {
                    bmp.SetPixelFast(x, y, COLOR);
                    stack.Push(new Point(x + 1, y));
                    stack.Push(new Point(x - 1, y));
                    stack.Push(new Point(x, y + 1));
                    stack.Push(new Point(x, y - 1));
                }
            }
            return bmp;
        }


        public static Bitmap FourWayBoundryFill(Bitmap bmp,Color border_color, Color COLOR, Point q)
        {
            int h = bmp.Height;
            int w = bmp.Width;

            if (q.Y < 0 || q.Y > h - 1 || q.X < 0 || q.X > w - 1)
                return bmp;



            Stack<Point> stack = new Stack<Point>();
            stack.Push(q);
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int x = p.X;
                int y = p.Y;
                if (y < 0 || y > h - 1 || x < 0 || x > w - 1)
                    continue;
                Color val = bmp.GetPixelFast(x, y);
                if ((val.R != border_color.R) || (val.G != border_color.G) || (val.B != border_color.B)||(val.R!=COLOR.R) || (val.G != COLOR.G) ||  (val.B != COLOR.B))
                {
                    bmp.SetPixelFast(x, y, COLOR);
                    stack.Push(new Point(x + 1, y));
                    stack.Push(new Point(x - 1, y));
                    stack.Push(new Point(x, y + 1));
                    stack.Push(new Point(x, y - 1));
                }
            }
            return bmp;
        }



    }
}