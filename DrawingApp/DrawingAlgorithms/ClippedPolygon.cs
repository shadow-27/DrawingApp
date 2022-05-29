﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winforms_image_processor
{
    [Serializable]
    class ClippedPolygon : Polygon
    {
        Rectangle boundingRect = new Rectangle(Color.Transparent, 0);

        public ClippedPolygon(Color col, int thicc) : base(col, thicc) { }

        public ClippedPolygon(Polygon P) : base(P.shapeColor, P.thickness)
        {
            shapeType = DrawingShape.CPOLY;
            supportsAA = true;
            points = P.points;
        }

        public override List<ColorPoint> GetPixels(params object[] param)
        {
            var pixels = new List<ColorPoint>();

            Clipping clipper = new Clipping();
            clipper.SetBoundingRectangle(boundingRect);

            if (filler != null)
                pixels.AddRange(filler.FillPoints(boundingRect));

            Color ofLine = shapeColor;
            if (param.Length == 2 && param[1] is Color)
                ofLine = Color.FromArgb(((Color)param[1]).ToArgb() ^ 0xffffff);

            for (int i = 0; i <= points.Count - 2; i++)
                pixels.AddRange(new MidPointLine(ofLine, thickness + 3, points[i], points[i + 1], clipper).GetPixels());

            if (param.Length > 0 || false)
                pixels.AddRange(boundingRect.GetPixels());

            return pixels;
        }

        public override List<ColorPoint> GetPixelsAA(Bitmap bmp)
        {
            var pixels = new List<ColorPoint>();

            Clipping clipper = new Clipping();
            clipper.SetBoundingRectangle(boundingRect);

            if (filler != null)
                pixels.AddRange(filler.FillPoints(boundingRect));

            for (int i = 0; i <= points.Count - 2; i++)
                pixels.AddRange(new MidPointLine(shapeColor, thickness + 3, points[i], points[i + 1], clipper).GetPixelsAA(bmp));

            return pixels;
        }

        public void SetBoundingRect(Rectangle rect)
        {
            boundingRect = rect;

        }

        public override string howToDraw()
        {
            return "Click each point and click on first to finish.";
        }

        public override string ToString()
        {
            return "Clipped Polygon";
        }
    }
}
