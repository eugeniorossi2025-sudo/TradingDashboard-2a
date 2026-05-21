using Gamebot.Helpers;
using Gamebot.Models.Objects;
using System.Drawing;
using System.Windows.Forms;

namespace Gamebot.Models.UI
{
    internal class TaskScreenshot
    {
        internal class TakeScreenshot : Form
        {
            public TakeScreenshot(string keyAreaScreenshot)
            {
                base.FormBorderStyle = FormBorderStyle.None;
                Rectangle a = Screen.GetBounds(Point.Empty);
                this.ratio = (float)Config.zoom / 100f;
                int newW = (int)((float)a.Width * this.ratio);
                int newH = (int)((float)a.Height * this.ratio);
                base.Bounds = new Rectangle(0, 0, newW, newH);
                Log.PrintInfo(string.Format("SCREEN GET BOUNDS | X: {0} | Y: {1} | W: {2} | H: {3} | Zoom: {4}", new object[] { a.X, a.Y, a.Width, a.Height, this.ratio }));
                Log.PrintInfo(string.Format("PRIMARY SCREEN | W: {0}", Screen.PrimaryScreen.Bounds.Width));
                base.StartPosition = FormStartPosition.Manual;
                base.ShowInTaskbar = false;
                this.BackColor = Color.Black;
                base.Opacity = 0.3;
                base.MouseDown += this.MainForm_MouseDown;
                base.MouseMove += this.MainForm_MouseMove;
                base.MouseUp += this.MainForm_MouseUp;
                this.areaScreenshot = keyAreaScreenshot;
            }

            public TakeScreenshot(string tag, int x)
            {
                base.FormBorderStyle = FormBorderStyle.None;
                Rectangle a = Screen.GetBounds(Point.Empty);
                this.ratio = (float)Config.zoom / 100f;
                int newW = (int)((float)a.Width * this.ratio);
                int newH = (int)((float)a.Height * this.ratio);
                base.Bounds = new Rectangle(0, 0, newW, newH);
                Log.PrintInfo(string.Format("SCREEN CUSTOM GET BOUNDS | X: {0} | Y: {1} | W: {2} | H: {3} | Zoom: {4}", new object[] { a.X, a.Y, a.Width, a.Height, this.ratio }));
                Log.PrintInfo(string.Format("PRIMARY SCREEN CUSTOM | W: {0}", Screen.PrimaryScreen.Bounds.Width));
                base.StartPosition = FormStartPosition.Manual;
                base.ShowInTaskbar = false;
                this.BackColor = Color.Black;
                base.Opacity = 0.3;
                base.MouseDown += this.MainForm_MouseDown;
                base.MouseMove += this.MainForm_MouseMove;
                base.MouseUp += this.MainForm_MouseUpCustom;
                this.areaScreenshot = tag;
            }

            private void MainForm_MouseDown(object sender, MouseEventArgs e)
            {
                this.selectionRectangle.Location = e.Location;
                this.isDragging = true;
                this.initButton = Cursor.Position;
            }

            private void MainForm_MouseMove(object sender, MouseEventArgs e)
            {
                if (this.isDragging)
                {
                    this.selectionRectangle.Size = new Size(e.X - this.selectionRectangle.X, e.Y - this.selectionRectangle.Y);
                    this.endButton = Cursor.Position;
                    base.Invalidate();
                }
            }

            private void MainForm_MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left && this.selectionRectangle.Size != Size.Empty)
                {
                    base.Hide();
                    AreaElement areaSelected;
                    if (this.areaScreenshot == "AREA_VINCITA" || this.areaScreenshot == "AREA_MAZZO" || this.areaScreenshot == "AREA_PUNTARE")
                    {
                        areaSelected = new AreaElement
                        {
                            startX = ((this.initButton.X < this.endButton.X) ? this.initButton.X : this.endButton.X),
                            startY = ((this.initButton.Y < this.endButton.Y) ? this.initButton.Y : this.endButton.Y),
                            endX = ((this.initButton.X < this.endButton.X) ? this.endButton.X : this.initButton.X),
                            endY = ((this.initButton.Y < this.endButton.Y) ? this.endButton.Y : this.initButton.Y)
                        };
                    }
                    else
                    {
                        areaSelected = new AreaElement
                        {
                            startX = ((this.initButton.X < this.endButton.X) ? ((int)((float)this.initButton.X / this.ratio)) : ((int)((float)this.endButton.X / this.ratio))),
                            startY = ((this.initButton.Y < this.endButton.Y) ? ((int)((float)this.initButton.Y / this.ratio)) : ((int)((float)this.endButton.Y / this.ratio))),
                            endX = ((this.initButton.X < this.endButton.X) ? ((int)((float)this.endButton.X / this.ratio)) : ((int)((float)this.initButton.X / this.ratio))),
                            endY = ((this.initButton.Y < this.endButton.Y) ? ((int)((float)this.endButton.Y / this.ratio)) : ((int)((float)this.initButton.Y / this.ratio)))
                        };
                    }
                    ListAreaElement.Instance.AddArea(this.areaScreenshot, areaSelected);
                    base.Close();
                    return;
                }
                this.selectionRectangle = default(Rectangle);
                this.isDragging = false;
                base.Invalidate();
            }

            private void MainForm_MouseUpCustom(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left && this.selectionRectangle.Size != Size.Empty)
                {
                    base.Hide();
                    AreaElement areaSelected = new AreaElement
                    {
                        startX = ((this.initButton.X < this.endButton.X) ? ((int)((float)this.initButton.X / this.ratio)) : ((int)((float)this.endButton.X / this.ratio))),
                        startY = ((this.initButton.Y < this.endButton.Y) ? ((int)((float)this.initButton.Y / this.ratio)) : ((int)((float)this.endButton.Y / this.ratio))),
                        endX = ((this.initButton.X < this.endButton.X) ? ((int)((float)this.endButton.X / this.ratio)) : ((int)((float)this.initButton.X / this.ratio))),
                        endY = ((this.initButton.Y < this.endButton.Y) ? ((int)((float)this.endButton.Y / this.ratio)) : ((int)((float)this.initButton.Y / this.ratio)))
                    };
                    CustomFicheWidgetsContainer.modEntry(this.areaScreenshot, areaSelected);
                    base.Close();
                    return;
                }
                this.selectionRectangle = default(Rectangle);
                this.isDragging = false;
                base.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.DrawRectangle(Pens.Red, this.selectionRectangle);
            }

            private Rectangle selectionRectangle;

            private bool isDragging;

            private Point initButton;

            private Point endButton;

            private string areaScreenshot;

            private float ratio;
        }
    }
}
