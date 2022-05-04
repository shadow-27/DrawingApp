﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace winforms_image_processor
{
    public enum DrawingShape { EMPTY, LINE, CIRCLE, POLY, CAPS, RECT, CPOLY, FILL };
   

    public partial class DrawForm : Form
    {

        public DrawForm()
        {
            InitializeComponent();

            listBox1.DataSource = shapes;
        }

        BindingList<Shape> shapes = new BindingList<Shape>();

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            shapes.Clear();

            pictureBox1.Image = NewBitmap();

            toolsToolStripMenuItem.Enabled = true;
        }

        Color backColor = Color.White;
        Bitmap NewBitmap()
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            Graphics g = Graphics.FromImage(bmp);
            g.Clear(backColor);

            return bmp;
        }

        private void DrawForm_ResizeEnd(object sender, EventArgs e)
        {
            if (pictureBox1.Width <= 0 || pictureBox1.Height <= 0)
                return;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            RefreshShapes();
        }

        void UpdateLabel()
        {
            StringBuilder sb;
            if (currentShape == null)
                sb = new StringBuilder();
            else
            {
                sb = new StringBuilder("Currently drawing: ");
                sb.AppendLine(currentShape.ToString());
                sb.Append(currentShape.howToDraw());
            }

            label1.Text = sb.ToString();
        }

        void RefreshShapes()
        {
            Bitmap bmp = NewBitmap();
            foreach (var shape in shapes)
            {
                DrawShape(bmp, shape);
            }

            pictureBox1.Image = bmp;
        }

        void SuperRefreshShapes()
        {
            Bitmap bmp = new Bitmap(2*pictureBox1.Width, 2*pictureBox1.Height);

            Graphics g = Graphics.FromImage(bmp);
            g.Clear(backColor);

            foreach (var shape in shapes)
            {
                SuperDrawShape(bmp, shape);
            }

            pictureBox1.Image = bmp;
        }

        Bitmap DrawShape(Bitmap bmp, Shape shape)
        {
            if (!antialiasingToolStripMenuItem.Checked || !shape.supportsAA)
                foreach (var point in shape.GetPixels(colorDialog2.Color, bmp))
                {
                    if (point.Point.X >= pictureBox1.Width || point.Point.Y >= pictureBox1.Height || point.Point.X <= 0 || point.Point.Y <= 0)
                        continue;

                    bmp.SetPixelFast(point.Point.X, point.Point.Y, point.Color);
                }
            else
            {
                foreach (var point in shape.GetPixelsAA(bmp))
                {
                    if (point.Point.X >= pictureBox1.Width || point.Point.Y >= pictureBox1.Height || point.Point.X <= 0 || point.Point.Y <= 0)
                        continue;

                    bmp.SetPixelFast(point.Point.X, point.Point.Y, point.Color);
                }
            }

            return bmp;
        }


        Bitmap SuperDrawShape(Bitmap bmp, Shape shape)
        {
            if (!antialiasingToolStripMenuItem.Checked || !shape.supportsAA)
                foreach (var point in shape.SuperGetPixels(colorDialog2.Color, bmp))
                {
                    if (point.Point.X >= 2*pictureBox1.Width || point.Point.Y >= 2*pictureBox1.Height || point.Point.X <= 0 || point.Point.Y <= 0)
                        continue;

                    bmp.SetPixelFast(point.Point.X, point.Point.Y, point.Color);
                }
            else
            {
                foreach (var point in shape.GetPixelsAA(bmp))
                {
                    if (point.Point.X >= pictureBox1.Width || point.Point.Y >= pictureBox1.Height || point.Point.X <= 0 || point.Point.Y <= 0)
                        continue;

                    bmp.SetPixelFast(point.Point.X, point.Point.Y, point.Color);
                }
            }

            return bmp;
        }


        Shape currentShape = null;
        DrawingShape currentDrawingShape = DrawingShape.EMPTY;

       // bool canRefresh = true;

        bool drawing = false;
        bool moving = false;
       // bool clipping = false;
        int index;

        void drawMode(
            bool status,
            Shape shape = null,
            int modify_index = -1
            )
        {
            if (!status && index == -1)
            {
                shapes.Add(currentShape);
                
                RefreshShapes();
            }
            else if (!status)
            {
                shapes[index] = currentShape;
                RefreshShapes();
            }

            splitContainer2.Panel1.Enabled = !status;

            index = modify_index;
            drawing = status;
            currentDrawingShape = shape == null ? DrawingShape.EMPTY : shape.shapeType;
            currentShape = shape;
            UpdateLabel();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                if (currentShape.shapeType == DrawingShape.POLY)
                {
                    if (1 == ((Polygon)currentShape).AddPoint(e.Location, out MidPointLine line))
                        drawMode(false);

                    if (line != null)
                        pictureBox1.Image = DrawShape((Bitmap)pictureBox1.Image, line);

                    return;
                }

                if (1 == currentShape.AddPoint(e.Location))
                    drawMode(false);
            }

            
        }

        private void midpointCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawMode(true, new MidPointCircle(colorDialog1.Color));
        }

        private void midpointLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawMode(true, new MidPointLine(colorDialog1.Color, (int)numericUpDown1.Value));
        }

       






        

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button1.BackColor = colorDialog1.Color;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (shapes.Count == 0)
                return;

            switch (shapes[listBox1.SelectedIndex].shapeType)
            {
                case DrawingShape.CIRCLE:
                    drawMode(true, new MidPointCircle(colorDialog1.Color), listBox1.SelectedIndex);
                    break;
                case DrawingShape.LINE:
                    drawMode(true, new MidPointLine(colorDialog1.Color, (int)numericUpDown1.Value), listBox1.SelectedIndex);
                    break;
                case DrawingShape.POLY:
                case DrawingShape.CPOLY:
                    drawMode(true, new Polygon(colorDialog1.Color, (int)numericUpDown1.Value), listBox1.SelectedIndex);
                    break;

                default:
                case DrawingShape.EMPTY:
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (shapes.Count == 0)
                return;

            shapes.RemoveAt(listBox1.SelectedIndex);

          

            RefreshShapes();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Vector shapes (*.cg2022)|*.cg2022";
            saveFileDialog1.Title = "Save the drawing image";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                ShapeSerializer.Save(saveFileDialog1.FileName, shapes);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
               
                openFileDialog.Filter = "Vector shapes (*.cg2022)|*.cg2022";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    shapes.Clear();
                    foreach (var shape in ShapeSerializer.Load<Shape>(openFileDialog.FileName))
                        shapes.Add(shape);
                }
                else
                    return;
            }

            RefreshShapes();
        }

        private void antialiasingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                return;
            RefreshShapes();
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog2.ShowDialog() == DialogResult.OK)
            {
                backColor = colorDialog2.Color;
                RefreshShapes();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            moving = true;
        }

        System.Drawing.Point movestart;
        System.Drawing.Point moveend;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!moving)
                return;

            movestart = e.Location;
            splitContainer2.Panel1.Enabled = false;
            checkBox1.Checked = false;

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!moving)
                return;

            moveend = e.Location;

            moving = false;
            checkBox1.Checked = false;

            splitContainer2.Panel1.Enabled = true;

            ((Shape)listBox1.SelectedItem).MovePoints(moveend - (System.Drawing.Size)movestart);

            RefreshShapes();
        }

        private void showClipBorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshShapes();
        }

       

        private void selectColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog3.ShowDialog();
        }

        private void polygonToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            drawMode(true, new Polygon(colorDialog1.Color, (int)numericUpDown1.Value));
        }

        private void superSamplingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            Size size = new Size(2*this.Width, 2*this.Height);
            this.Size = size;
            this.Refresh();
            SuperRefreshShapes();




        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (shapes.Count == 0)
                return;

            shapes.Clear();

            RefreshShapes();
        }
    }
}
