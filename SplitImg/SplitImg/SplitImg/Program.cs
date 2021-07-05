using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SplitImg
{
    internal class Program
    {
        public static void Main(string[] args)
        {
           if (args.Length == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(args[0]))
            {
                return;
            }

            args = args[0].Replace("\"", string.Empty).Split(';');
            for (int i = 0; i < args.Length; i++)
            {
                string imgPath = args[i];
                if (string.IsNullOrEmpty(imgPath))
                {
                    continue;
                }

                if (!imgPath.EndsWith(".png") && !imgPath.EndsWith(".PNG"))
                {
                    continue;
                }

                string dirPath = Path.GetDirectoryName(imgPath);
                string imgName = Path.GetFileNameWithoutExtension(imgPath);
                Bitmap bitmap = new Bitmap(imgPath);
                Image img = Image.FromFile(imgPath);
                Rectangle[] rectangles = ImgHelper.GetRects(bitmap);
                Bitmap[] bitmaps = ImgHelper.GetSubPics(img, rectangles);
                bitmap.Dispose();
                img.Dispose();
                for (int j = 0; j < bitmaps.Length; j++)
                {
                    string saveName = bitmaps.Length == 1 ? imgName : imgName + j;
                    string savePath = Path.Combine(dirPath, saveName + ".png");
                    Console.WriteLine("savePath:" + savePath);
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }

                    bitmaps[j].Save(savePath, ImageFormat.Png);
                }
            }
        }
    }
}