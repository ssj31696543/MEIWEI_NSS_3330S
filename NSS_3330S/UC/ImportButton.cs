using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace NSK_8000S.UC
{
    [ToolboxItem(true)]
    [DesignerCategory("code")]
    [Designer(typeof(CustomDesigner))]
    public class ImportButton : Button
    {
        private Color m_FocusColor = Color.LightBlue;
        private bool m_DrawFocusCue = false;

        public ImportButton() 
        {
            //this.Cursor = Cursors.Hand;
            //this.Image = new Bitmap(Properties.Resources.[SomeImage]);
            this.FlatAppearance.BorderSize = 0;
            //this.FlatAppearance.MouseDownBackColor = Color.SteelBlue;
            //this.FlatAppearance.MouseOverBackColor = Color.Transparent;
            this.FlatStyle = FlatStyle.Flat;
        }

        protected override bool ShowFocusCues 
        {
            get
            {
                m_DrawFocusCue = !ClientRectangle.Contains(PointToClient(MousePosition));
                return !IsHandleCreated;
            }
        }

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(false);
        }

        // Make it public if this value should be customizable
        private int FocusBorderSize { get; set; }

        protected override void OnPaint(PaintEventArgs e) 
        {
            base.OnPaint(e);
        
            if (Focused && m_DrawFocusCue)
            {
                var rect = ClientRectangle;
                rect.Inflate(-FocusBorderSize, -FocusBorderSize);
                using (var pen = new Pen(FlatAppearance.MouseDownBackColor, FocusBorderSize))
                {
                    e.Graphics.DrawLine(pen, 0, rect.Bottom, rect.Right, rect.Bottom);
                }
            }
        }

        protected override void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                this.Image.Dispose();
            }
            base.Dispose(disposing);
        }

        public class CustomDesigner : System.Windows.Forms.Design.ControlDesigner
        {
            //private static string[] RemovedProperties = new[] 
            //{
            //    "AutoEllipsis", "AutoSize", "AutoSizeMode",
            //    "BackColor", "Cursor", "FlatAppearance", "FlatStyle",
            //    "ForeColor", "Text", "TextAlign", "TextImageRelation"
            //};
            private static string[] RemovedProperties = new[] 
            {
                "AutoEllipsis", "AutoSize", "AutoSizeMode",
                "BackColor", "Cursor", "FlatAppearance", "FlatStyle"
            };

            public CustomDesigner() { }

            protected override void PreFilterProperties(IDictionary properties) 
            {
                foreach (string prop in RemovedProperties) 
                {
                    properties.Remove(prop);
                }
                base.PreFilterProperties(properties);
            }
        }
    }
}
