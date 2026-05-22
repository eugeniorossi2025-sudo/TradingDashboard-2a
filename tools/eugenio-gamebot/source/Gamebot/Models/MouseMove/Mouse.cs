using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Gamebot.Models.MouseMove
{
    internal class Mouse
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        public void Click()
        {
            Mouse.mouse_event(Mouse.DOWN | Mouse.MOUSEEVENTF_ABSOLUTE, 800, 800, 0U, 0);
            Thread.Sleep(100);
            Mouse.mouse_event(Mouse.UP | Mouse.MOUSEEVENTF_ABSOLUTE, 800, 800, 0U, 0);
        }

        public void MoveTo(Point toGo)
        {
            Point newPosition = toGo;
            int steps = 10;
            Point start = Cursor.Position;
            PointF iterPoint = start;
            PointF slope = new PointF((float)(newPosition.X - start.X), (float)(newPosition.Y - start.Y));
            slope.X /= (float)steps;
            slope.Y /= (float)steps;
            for (int i = 0; i < steps; i++)
            {
                iterPoint = new PointF(iterPoint.X + slope.X, iterPoint.Y + slope.Y);
                int num = Convert.ToInt32(iterPoint.X + slope.X);
                int y = Convert.ToInt32(iterPoint.Y + slope.Y);
                Cursor.Position = new Point(num + 30, y - 30);
                Thread.Sleep(5);
            }
            Cursor.Position = newPosition;
        }

        public void MoveToSlow(Point toGo)
        {
            Point newPosition = toGo;
            int steps = 20;
            Point start = Cursor.Position;
            PointF iterPoint = start;
            PointF slope = new PointF((float)(newPosition.X - start.X), (float)(newPosition.Y - start.Y));
            slope.X /= (float)steps;
            slope.Y /= (float)steps;
            for (int i = 0; i < steps; i++)
            {
                iterPoint = new PointF(iterPoint.X + slope.X, iterPoint.Y + slope.Y);
                int num = Convert.ToInt32(iterPoint.X + slope.X);
                int y = Convert.ToInt32(iterPoint.Y + slope.Y);
                Cursor.Position = new Point(num + 30, y - 30);
                Thread.Sleep(5);
            }
            Cursor.Position = newPosition;
        }

        private static uint DOWN = 2U;

        private static uint UP = 4U;

        private static uint MOUSEEVENTF_ABSOLUTE = 32768U;

        private static uint MOUSEEVENTF_MOVE = 1U;
    }
}
