using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock
{
    static class FormApi
    {
        public static void SetImage(Form form, Bitmap bitmap)
        {
            var screenDC = User32.GetDC(IntPtr.Zero);
            var memDC = User32.CreateCompatibleDC(screenDC);

            IntPtr bitmapHandle = IntPtr.Zero;
            IntPtr oldBitmapHandle = IntPtr.Zero;
            try
            {
                bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBitmapHandle = User32.SelectObject(memDC, bitmapHandle);

                var size = new User32.Size { cx = bitmap.Width, cy = bitmap.Height };
                var poinSource = new User32.Point { x = 0, y = 0 };
                var topPos = new User32.Point { x = form.Left, y = form.Top };
                var blend = new User32.BLENDFUNCTION
                {
                    blendOp = User32.AC_SRC_ALPHA,
                    blendFlags = 0,
                    sourceConstantAlpha = 255,
                    alphaFormat = User32.AC_SRC_ALPHA
                };
                User32.UpdateLayeredWindow(form.Handle, screenDC, ref topPos, ref size, memDC, ref poinSource, 0, ref blend, User32.ULW_ALPHA);
            }
            finally
            {
                User32.ReleaseDC(IntPtr.Zero, screenDC);
                if (bitmapHandle != IntPtr.Zero)
                {
                    User32.SelectObject(memDC, oldBitmapHandle);
                    User32.DeleteObject(bitmapHandle);
                }
                User32.DeleteDC(memDC);
            }
        }
    }
}
