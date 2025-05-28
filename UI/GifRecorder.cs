using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace UI
{
    public sealed class GifRecorder : IDisposable
    {
        private readonly List<BitmapFrame> frames = new();
        private readonly int delayMs;

        public GifRecorder(int delayMs = 100) => this.delayMs = delayMs;

        public void AddFrame(WriteableBitmap bmp) =>
            frames.Add(BitmapFrame.Create(bmp));

        public void Save(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            var encoder = new GifBitmapEncoder();

            foreach (var frame in frames)
            {
                var meta = new BitmapMetadata("gif");
                meta.SetQuery("/grctlext/Delay", (ushort)(delayMs / 10));
                var framed = BitmapFrame.Create(frame, null, meta, null);
                encoder.Frames.Add(framed);
            }

            encoder.Save(stream);
        }

        public void Dispose() => frames.Clear();
    }

}
