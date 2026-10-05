using System.Drawing.Drawing2D;
using System.Drawing.Text;
using static Tuzat.App.Native.NativeMethods;

namespace Tuzat.App;

// Drawn at runtime so the repo has no binary icon files.
internal static class TrayIcons
{
    public static Icon Create(bool paused)
    {
        int size = SystemInformation.SmallIconSize.Width;
        using var bitmap = new Bitmap(size, size);
        using (var graphics = Graphics.FromImage(bitmap))
        using (var background = new SolidBrush(paused ? Color.Gray : Color.FromArgb(0x1E, 0x6F, 0xD9)))
        using (var font = new Font("Segoe UI", size * 0.6f, FontStyle.Bold, GraphicsUnit.Pixel))
        using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.FillEllipse(background, 0, 0, size - 1, size - 1);
            graphics.DrawString("T", font, Brushes.White, new RectangleF(0, 0, size, size), format);
        }

        nint handle = bitmap.GetHicon();
        try
        {
            using var temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }
}
