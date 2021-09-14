using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace WinCaffeine
{
    public class FancyLabel : Control
    {
        private Image _image;
        [Category("Appearance")]
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                Invalidate();
            }
        }

        private Color _barColorGradientStart;
        [Category("Appearance")]
        public Color BarColorGradientStart
        {
            get => _barColorGradientStart;
            set
            {
                _barColorGradientStart = value;
                Invalidate();
            }
        }

        private Color _barColorGradientMiddle;
        [Category("Appearance")]
        public Color BarColorGradientMiddle
        {
            get => _barColorGradientMiddle;
            set
            {
                _barColorGradientMiddle = value;
                Invalidate();
            }
        }

        private Color _barColorGradientEnd;
        [Category("Appearance")]
        public Color BarColorGradientEnd
        {
            get => _barColorGradientEnd;
            set
            {
                _barColorGradientEnd = value;
                Invalidate();
            }
        }


        private Color _backColorGradientStart = Color.Black;
        [Category("Appearance")]
        public Color BackColorGradientStart
        {
            get => _backColorGradientStart;
            set
            {
                _backColorGradientStart = value;
                Invalidate();
            }
        }

        private Color _backColorGradientMiddle = Color.Black;
        [Category("Appearance")]
        public Color BackColorGradientMiddle
        {
            get => _backColorGradientMiddle;
            set
            {
                _backColorGradientMiddle = value;
                Invalidate();
            }
        }

        private Color _backColorGradientEnd = Color.Black;
        [Category("Appearance")]
        public Color BackColorGradientEnd
        {
            get => _backColorGradientEnd;
            set
            {
                _backColorGradientEnd = value;
                Invalidate();
            }
        }

        private int _barSize;
        [Category("Appearance")]
        public int BarSize
        {
            get => _barSize;
            set
            {
                _barSize = value;
                Invalidate();
            }
        }

        private Color _shadowColor;
        [Category("Appearance")]
        public Color ShadowColor
        {
            get => _shadowColor;
            set
            {
                _shadowColor = value;
                Invalidate();
            }
        }

        private Point _shadowOffset;
        [Category("Appearance")]
        public Point ShadowOffset
        {
            get => _shadowOffset;
            set
            {
                _shadowOffset = value;
                Invalidate();
            }
        }

        public FancyLabel()
        {
            SetStyle(ControlStyles.Opaque, true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        { }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var b = MakeGradientBrush(new[] { _backColorGradientStart, _backColorGradientMiddle, _backColorGradientEnd }, ClientRectangle, 0f))
                e.Graphics.FillRectangle(b, ClientRectangle);

            var barRect = new Rectangle(0, Height - _barSize, Width, _barSize);
            using (var b = MakeGradientBrush(new[] { _barColorGradientStart, _barColorGradientMiddle, _barColorGradientEnd }, barRect, 0f))
                e.Graphics.FillRectangle(b, barRect);

            var textSize = e.Graphics.MeasureString(Text, Font);
            var textX = Padding.Left;
            if (Image != null)
            {
                textX = Padding.Left * 2 + Image.Width;

                var imgX = Padding.Left;
                var imgY = Height / 2 - Image.Height / 2;
                e.Graphics.DrawImage(Image, new Rectangle(imgX, imgY, Image.Width, Image.Height));
            }
            var textPoint = new Point(textX, (int)(Height / 2d - textSize.Height / 2d));
            var shadowPoint = textPoint;
            shadowPoint.X += _shadowOffset.X;
            shadowPoint.Y += _shadowOffset.Y;

            using (var b = new SolidBrush(_shadowColor))
                e.Graphics.DrawString(Text, Font, b, shadowPoint);

            using (var b = new SolidBrush(ForeColor))
                e.Graphics.DrawString(Text, Font, b, textPoint);
        }

        private LinearGradientBrush MakeGradientBrush(Color[] colors, Rectangle rect, float angle)
        {
            var br = new LinearGradientBrush(rect, Color.Black, Color.Black, angle);
            if (colors.Length < 1)
                return br;
            var cb = new ColorBlend();
            cb.Colors = colors;
            var offsets = new float[colors.Length];
            for (var i = 0; i < colors.Length; ++i)
                offsets[i] = (float)i / (colors.Length - 1);
            cb.Positions = offsets;
            br.InterpolationColors = cb;
            return br;
        }
    }
}
