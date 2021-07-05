using System.Collections.Generic;
using System.Drawing;

namespace SplitImg
{
    public static class ImgHelper
    {
        /// <summary>
        /// 对图像pic进行图块分割，分割为一个个的矩形子图块区域
        /// 分割原理： 相邻的连续区域构成一个图块，透明区域为分割点
        /// </summary>
        public static Rectangle[] GetRects(Bitmap pic)
        {
            List<Rectangle> rects = new List<Rectangle>();
            //获取图像对应的非透明像素点
            bool[][] colors = GetColors(pic);

            for (int i = 0; i < pic.Height; i++)
            for (int j = 0; j < pic.Width; j++)
            {
                if (Exist(colors, i, j))
                {
                    Rectangle rect = GetRect(colors, i, j);

                    if (rect.Width > 10 && rect.Height > 10) //剔除尺寸小于10x10的子图区域
                    {
                        rects.Add(rect);
                    }
                }
            }

            return rects.ToArray();
        }

        //获取图像buildPic的所有子图区域的图像
        public static Bitmap[] GetSubPics(Image buildPic, Rectangle[] buildRects)
        {
            Bitmap[] buildTiles = new Bitmap[buildRects.Length];
            for (int i = 0; i < buildRects.Length; i++)
            {
                buildTiles[i] = GetRect(buildPic, buildRects[i]); 
            }

            return buildTiles;
        }

        //判断所有像素点是否存在非透明像素
        private static bool[][] GetColors(Bitmap pic)
        {
            bool[][] has = new bool[pic.Height][];

            for (int i = 0; i < pic.Height; i++)
            {
                has[i] = new bool[pic.Width];
                for (int j = 0; j < pic.Width; j++)
                {
                    var color = pic.GetPixel(j, i);
                    //统计RGB值近似为0的数目
                    int count = 0;
                    if (color.R < 4)
                    {
                        count++;
                    }

                    if (color.G < 4)
                    {
                        count++;
                    }

                    if (color.B < 4)
                    {
                        count++;
                    }

                    //若透明度近似为0，视为透明像素。或RGB中有两个值近似为0且透明度很小，也视为透明像素。
                    if (color.A < 3 || count >= 2 && color.A < 30)
                    {
                        has[i][j] = false;
                    }
                    else
                    {
                        has[i][j] = true;
                    }
                }
            }

            return has;
        }

        //判断坐标处是否存在非透明像素值
        private static bool Exist(bool[][] colors, int x, int y)
        {
            if (x < 0 || y < 0 || x >= colors.Length || y >= colors[0].Length)
            {
                return false;
            }

            return colors[x][y];
        }

        //判定区域Rect右侧是否存在像素点
        private static bool R_Exist(bool[][] colors, Rectangle rect)
        {
            if (rect.Right >= colors[0].Length || rect.Left < 0)
            {
                return false;
            }

            for (int i = 0; i < rect.Height; i++)
            {
                if (Exist(colors, rect.Top + i, rect.Right + 1))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool D_Exist(bool[][] colors, Rectangle rect)
        {
            if (rect.Bottom >= colors.Length || rect.Top < 0)
            {
                return false;
            }

            for (int i = 0; i < rect.Width; i++)
            {
                if (Exist(colors, rect.Bottom + 1, rect.Left + i))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool L_Exist(bool[][] colors, Rectangle rect)
        {
            if (rect.Right >= colors[0].Length || rect.Left < 0)
            {
                return false;
            }

            for (int i = 0; i < rect.Height; i++)
            {
                if (Exist(colors, rect.Top + i, rect.Left - 1))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool U_Exist(bool[][] colors, Rectangle rect)
        {
            if (rect.Bottom >= colors.Length || rect.Top < 0)
            {
                return false;
            }

            for (int i = 0; i < rect.Width; i++)
            {
                if (Exist(colors, rect.Top - 1, rect.Left + i))
                {
                    return true;
                }
            }

            return false;
        }

        //获取坐标所在图块的区域范围
        private static Rectangle GetRect(bool[][] colors, int x, int y)
        {
            Rectangle rect = new Rectangle(new Point(y, x), new Size(1, 1));

            bool flag;
            do
            {
                flag = false;

                while (R_Exist(colors, rect))
                {
                    rect.Width++;
                    flag = true;
                }

                while (D_Exist(colors, rect))
                {
                    rect.Height++;
                    flag = true;
                }

                while (L_Exist(colors, rect))
                {
                    rect.Width++;
                    rect.X--;
                    flag = true;
                }

                while (U_Exist(colors, rect))
                {
                    rect.Height++;
                    rect.Y--;
                    flag = true;
                }
            } while (flag);

            ClearRect(colors, rect);
            rect.Width++;
            rect.Height++;

            return rect;
        }

        //清空区域内的像素非透明标记
        private static void ClearRect(bool[][] colors, Rectangle rect)
        {
            for (int i = rect.Top; i <= rect.Bottom; i++)
            {
                for (int j = rect.Left; j <= rect.Right; j++)
                {
                    colors[i][j] = false;
                }
            }
        }

        /// <summary>
        /// 从图像pic中截取区域Rect构建新的图像
        /// </summary>
        private static Bitmap GetRect(Image pic, Rectangle rect)
        {
            Rectangle drawRect = new Rectangle(0, 0, rect.Width, rect.Height);
            Bitmap tmp = new Bitmap(drawRect.Width, drawRect.Height);

            Graphics g = Graphics.FromImage(tmp);
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.DrawImage(pic, drawRect, rect, GraphicsUnit.Pixel);

            return tmp;
        }
    }
}