using Common.Entities;
using Common.Configs;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Render.Rendering
{
    internal static class SliceRenderer
    {
        public static WriteableBitmap Render(SoilSlice slice, ColorRamp ramp)
        {
            int CP = RenderConfig.CellPx;
            int W = slice.Width * CP;
            int H = slice.Height * CP;

            var bmp = BitmapFactory.New(W, H);
            bmp.Clear(RenderConfig.BackColor);

            for (int gx = 0; gx <= slice.Width; gx++)
                bmp.DrawLine(gx * CP, 0, gx * CP, H - 1, RenderConfig.GridClr);

            for (int gy = 0; gy <= slice.Height; gy++)
                bmp.DrawLine(0, gy * CP, W - 1, gy * CP, RenderConfig.GridClr);

            for (int y = 0; y < slice.Height; y++)
            {
                int yy = y * CP + 1;
                for (int x = 0; x < slice.Width; x++)
                {
                    byte m = slice.Moisture(x, y);
                    if (m == 0) continue;

                    int xx = x * CP + 1;
                    Color clr = ramp[m];

                    bmp.FillRectangle(
                        xx, yy,
                        xx + CP - 2, yy + CP - 2,
                        clr);
                }
            }

            foreach (int cx in slice.CrackCells)
            {
                int xLeft = cx * CP;
                int xRight = xLeft + CP - 1;

                byte m0 = slice.Moisture(cx, 0);
                if (m0 > 0)
                {
                    Color clr0 = ramp[m0];
                    bmp.FillRectangle(xLeft + 1, 1,
                                      xRight - 1, CP - 1,
                                      clr0);
                }

                bmp.DrawLine(xLeft, 0, xLeft, CP - 1, RenderConfig.CrackRod);
                bmp.DrawLine(xRight, 0, xRight, CP - 1, RenderConfig.CrackRod);
            }

            return bmp;
        }
    }
}
