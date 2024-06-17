using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace NSS_3330S.UC
{
    public partial class Stripmap : UserControl
    {
        private String stripName = "";

        public String StripName
        {
            get { return stripName; }
            set { stripName = value; }
        }

        private String tooSmallString = "TOO SMALL";

        public String TooSmallString
        {
            get { return tooSmallString; }
            set { tooSmallString = value; }
        }

        private String noDataString = "NO DATA";

        public String NoDataString
        {
            get { return noDataString; }
            set { noDataString = value; }
        }



        private int translation_x = 0;

        public int TranslationX
        {
            get { return translation_x; }
            set { translation_x = value; }
        }

        private int translation_y = 0;

        public int TranslationY
        {
            get { return translation_y; }
            set { translation_y = value; }
        }

        private int rotation;

        public int Rotation
        {
            get { return rotation; }
            set
            {
                if (value % 90 == 0 && value >= 0 && value < 360)
                    rotation = value;
                else
                    throw new ArgumentException("Rotation has to be 0, 90, 180 or 270 degrees (Is " + value + ")");


            }
        }


        private float zoom;

        public float Zoom
        {
            get { return zoom; }
            set { zoom = value; }
        }
        private int orientationLocation = 0;

        public int Orientationlocation
        {
            get { return orientationLocation; }
            set
            {
                if (value % 90 == 0 && value >= 0 && value <= 270)
                    orientationLocation = value;
                else
                    throw new ArgumentException("Orientationlocation has to be 0, 90, 180 or 270 degrees (Is " + value + ")");
            }
        }

        private int[,] dataset;

        public int[,] Dataset
        {
            get { return dataset; }
            set { dataset = value; }
        }

        private Color[] colors;

        public Color[] Colors
        {
            get { return colors; }
            set { colors = value; }
        }



        public Stripmap()
        {
            zoom = 1f;
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            setupDefaultColors();
            registerEvents();
        }



        private void setupDefaultColors()
        {
            // Just some sample colors to get started
            colors = new Color[255];
            colors[0] = Color.DarkGray;
            colors[1] = Color.Red;
            colors[2] = Color.Yellow;
            colors[3] = Color.Blue;
            colors[4] = Color.Orange;
            colors[5] = Color.Magenta;
            colors[6] = Color.DarkBlue;
            colors[7] = Color.Pink;
            colors[50] = Color.Black;
        }

        private void Stripmap_Load(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;
        }

        private bool isScaled;

        public bool IsScaled
        {
            get { return isScaled; }

        }

        private int scaleFactor;

        public int ScaleFactor
        {
            get { return scaleFactor; }
        }


        // We need some globals to be available for calculations
        RectangleF boundingBox_;
        SizeF dieSize_;

        protected override void OnPaint(PaintEventArgs e)
        {
            // set rotation
            e.Graphics.RotateTransform((float)rotation);
            if (rotation != 0)
            {
                // When we rotate, we also have to translate
                switch (rotation)
                {
                    case 90:
                        e.Graphics.TranslateTransform(0, -boundingBox_.Width);
                        break;
                    case 180:
                        e.Graphics.TranslateTransform(-boundingBox_.Width, -boundingBox_.Height);
                        break;
                    case 270:
                        e.Graphics.TranslateTransform(-boundingBox_.Height, 0);
                        break;
                }
            }
            // set additional translation
            e.Graphics.TranslateTransform(translation_x, translation_y);

            // Use antialias
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Here comes everything that has to be calculated on each resize/redraw
            // Just do this calculations once
            // Let's find the best Size for the outline
            float w = this.Width * zoom;
            float h = this.Height * zoom;

            float fsizeX = w;// w < h ? h : w;
            float fsizeY = h;// h < w ? w : h;
            // Wafersize is size-2 because we're not drawing the first and the last pixels
            SizeF stripsize = new SizeF(fsizeX - 2, fsizeY - 2);
            PointF starting = new PointF((w - fsizeX) / 2f, (h - fsizeY) / 2f);



            RectangleF boundingBox = new RectangleF(starting, stripsize);
            boundingBox_ = boundingBox;

            // Create graphics path.
            GraphicsPath clipPath = new GraphicsPath();
            clipPath.AddEllipse(boundingBox);


            // SetClip호출하면 cliPath에 Ellipse로 등록된 내부영역으로 지정된다.
            // Set clipping region to path.
            //e.Graphics.SetClip(clipPath, CombineMode.Replace);


            //drawCircle(e.Graphics, boundingBox);



            //drawNotch(e.Graphics, boundingBox, notchLocation);

            // Let's calculate everything needed for drawing the dies
            if (dataset != null && dataset.Length > 0)
            {
                int maxX = dataset.GetLength(0);
                int maxY = dataset.GetLength(1);
                float sizeX = boundingBox.Width / (float)maxX;
                int sizeY = (int)Math.Truncate(boundingBox.Height / maxY);

                int every = 1;

                // If dieSizeX or dieSizeY is less then 2 pixels
                // take only every nth die
                while (sizeX <= 2 || sizeY <= 2)
                {
                    every = every * 2;
                    sizeX = boundingBox.Width / (float)(maxX / every);
                    sizeY = (int)Math.Truncate(boundingBox.Height / (float)(maxY / every));
                }
                SizeF dieSize = new SizeF(sizeX, sizeY);
                dieSize_ = dieSize;
                // If every != 1 we recalculate the input data
                // Otherwise we pass the original dataset
                // Caveat: We must not overwrite the original dataset ;)
                if (every > 1)
                {
                    // Create a new dataset
                    // Get the highest bin code in x/y to x/y + every as result for x/y
                    // First set the property
                    isScaled = true;
                    scaleFactor = every;
                    drawDies(e.Graphics, boundingBox, WafermapTools.scaleArray(dataset, every), dieSize);
                    // Print "Too small" message
                    FontFamily myFontFamily = new FontFamily("Arial");
                    Font myFont = new Font(myFontFamily,
                       10,
                       FontStyle.Bold,
                       GraphicsUnit.Pixel);

                    //e.Graphics.DrawString(tooSmallString, myFont, new SolidBrush(Color.Red), boundingBox.Location);
                }
                else
                {
                    // Properties
                    isScaled = false;
                    scaleFactor = 1;

                    // Simply draw the die
                    drawDies(e.Graphics, boundingBox, dataset, dieSize);

                    //FontFamily myFontFamily = new FontFamily("Arial");
                    //Font myFont = new Font(myFontFamily,
                    //   10,
                    //   FontStyle.Bold,
                    //   GraphicsUnit.Pixel);
                    //e.Graphics.DrawString(tooSmallString, myFont, new SolidBrush(Color.Red), boundingBox.Location);
                }
            }
            else
            {
                // Display "No Data" message
                FontFamily myFontFamily = new FontFamily("Arial");
                Font myFont = new Font(myFontFamily,
                   10,
                   FontStyle.Bold,
                   GraphicsUnit.Pixel);

                e.Graphics.DrawString(noDataString, myFont, new SolidBrush(Color.Red), boundingBox.Location);
            }
            
            
            //drawOrientation(e.Graphics, boundingBox, orientationLocation);
            
            
            FontFamily ftFamily = new FontFamily("Arial");
            Font ft = new Font(ftFamily, 20, FontStyle.Bold, GraphicsUnit.Pixel);
            e.Graphics.DrawString(stripName, ft, new SolidBrush(Color.Red), new PointF(5f, 5f));
        }

        // Try to reuse - only instantiated once
        SolidBrush waferFillbrush = new SolidBrush(Color.Silver);
        Pen blackPen = new Pen(Color.Black);
        SolidBrush notchFillBrush = new SolidBrush(Color.Black);
        private void drawCircle(Graphics g, RectangleF boundingBox)
        {
            g.FillEllipse(waferFillbrush, boundingBox);
            g.DrawEllipse(blackPen, boundingBox);
        }

        private void drawOrientation(Graphics g, RectangleF boundingBox, int location)
        {
            // Draw the notch (Phyical property on the wafer for alignment. Can be at 0, 90, 180, 270 degrees
            // starting from 0° at the bottom CCW)
            // The Shape is  fixed to a cut circle
            float size = boundingBox.Width < boundingBox.Height ? boundingBox.Width : boundingBox.Height;
            size = size * 0.05f;
            // Calculate the location of the notch
            // 180°
            float x = boundingBox.X + (boundingBox.Width / 2f) - (size / 2f);
            float y = boundingBox.Y - (size / 2f);
            int start = 0;
            int end = 180;
            switch (location)
            {
                case 0:
                    y = boundingBox.Y + boundingBox.Height - (size / 2f);
                    end = -180;
                    break;
                case 90:
                    x = boundingBox.X - (size / 2f);
                    y = boundingBox.Y + (boundingBox.Height / 2f) - (size / 2f);
                    start = 90;
                    end = -180;
                    break;
                case 270:
                    x = boundingBox.X + boundingBox.Width - (size / 2f);
                    y = boundingBox.Y + (boundingBox.Height / 2f) - (size / 2f);
                    start = 90;
                    end = 180;
                    break;
            }


            g.FillPie(notchFillBrush, x, y, size, size, start, end);
        }
        Pen dieOutlinePen = new Pen(Color.Black);
        private void drawDies(Graphics g, RectangleF boundingBox, int[,] data, SizeF dieSize)
        {
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    //data[x,y] 가 0이면 black으로 채운다.
                    //Color fill = Color.FromArgb(data[x, y] == 0 ? 210 : 120, data[x, y] == 0 ? Color.Black : colors[data[x, y]]);
                    //Color fill = Color.FromArgb(data[x, y] == 0 ? 210 : 120, data[x, y] == 0 ? Color.DarkGray : colors[data[x, y]]);

                    Color fill = Color.FromArgb(255, colors[data[x, y]]);


                    PointF position = new PointF(boundingBox.X + (float)x * dieSize.Width, boundingBox.Y + (float)y * dieSize.Height);
                    RectangleF die = new RectangleF(position, dieSize);
                    g.FillRectangle(new SolidBrush(fill), die);
                    g.DrawRectangle(dieOutlinePen, die.X, die.Y, die.Width, die.Height);

                    FontFamily myFontFamily = new FontFamily("Arial");
                    Font myFont = new Font(myFontFamily,
                       10,
                       FontStyle.Bold,
                       GraphicsUnit.Pixel);

                    g.DrawString((y + 1).ToString(), myFont, new SolidBrush(Color.Black), position);

                }
            }
        }

        private bool interactive = false;

        public bool Interactive
        {
            get { return interactive; }
            set
            {
                interactive = value;
                registerEvents();
            }
        }

        private void registerEvents()
        {
            // Event to be registered
            if (interactive)
            {
                this.MouseClick += Stripmap_MouseClick;
                this.MouseMove += Stripmap_MouseMove;
            }
        }

        void Stripmap_MouseMove(object sender, MouseEventArgs e)
        {
            // This one is going to be tricky
            // We need to calculate the die coordinates from screen coordinates
            // We have global vars boundingBox_ and dieSize_
            float x_coord = ((float)e.X - boundingBox_.X) / dieSize_.Width;
            float y_coord = ((float)e.Y - boundingBox_.Y) / dieSize_.Height;
            int x = (int)Math.Floor(x_coord);
            int y = (int)Math.Floor(y_coord);
            try
            {
                dieEntered(x, y, dataset[x, y]);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Not good move..");
            }

        }

        void Stripmap_MouseClick(object sender, MouseEventArgs e)
        {
            // Basically the same as MouseMove, just a few other infos passed
            float x_coord = ((float)e.X - boundingBox_.X) / dieSize_.Width;
            float y_coord = ((float)e.Y - boundingBox_.Y) / dieSize_.Height;
            int x = (int)Math.Floor(x_coord);
            int y = (int)Math.Floor(y_coord);

            int nArrLen1 = dataset.GetLength(0);
            int nArrLen2 = dataset.GetLength(1);
            if (x >= nArrLen1) return;
            if (y >= nArrLen2) return;
            try
            {
                dieClicked(x, y, dataset[x, y], e.Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Not good click..");
                throw ex;
            }

        }

        // This method will get called if the mousepointer enters a die
        public virtual void dieEntered(int x, int y, int bincode)
        {
        }

        // This method should get overridden if you want to reakt on clicks on a die
        public virtual void dieClicked(int x, int y, int bincode, MouseButtons btn) { }

        // This method should be used to change die coloring of a bin directly
        // This is needed to avoid redraws when not neccessary
        // The updated bins are filled with higher alpha to highlight them
        public void updateDie(int x, int y, int bincode)
        {
            Color fill = Color.FromArgb(255, colors[bincode]);

            PointF position = new PointF(boundingBox_.X + (float)x * dieSize_.Width, boundingBox_.Y + (float)y * dieSize_.Height);
            RectangleF die = new RectangleF(position, dieSize_);
            Graphics g = this.CreateGraphics();
            // update clipping
            // Create graphics path.
            GraphicsPath clipPath = new GraphicsPath();
            clipPath.AddEllipse(boundingBox_);
            // Set clipping region to path.
            //g.SetClip(clipPath, CombineMode.Replace);
            // Draw
            g.FillRectangle(new SolidBrush(fill), die);
            g.DrawRectangle(dieOutlinePen, die.X, die.Y, die.Width, die.Height);

            Invalidate();
            
        }
    }
}
