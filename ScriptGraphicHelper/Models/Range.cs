using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptGraphicHelper.Models
{
    public class Range
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public int Mode_1 { get; set; }
        public int Mode_2 { get; set; }

        public Range(double left, double top, double right, double bottom, int mode_1 = -1, int mode_2 = -1)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Mode_1 = mode_1;
            Mode_2 = mode_2;
        }
        public string ToStr(int mode = 0)
        {

            if (mode == 1)
            {
                double width = Right - Left;
                double height = Bottom - Top;
                return string.Format("{0},{1},{2},{3}", Left.ToString(), Top.ToString(), width.ToString(), height.ToString());
            }
            else if (mode == 2)
            {
                string mode_1 = Mode_1 == 0 ? "left" : Mode_1 == 1 ? "center" : Mode_1 == 2 ? "right" : "normal";
                string mode_2 = Mode_2 == 0 ? "left" : Mode_2 == 1 ? "center" : Mode_2 == 2 ? "right" : "normal";

                if (Mode_1 == Mode_2)
                {
                    return string.Format("{0},{1},{2},{3},{4}", Left.ToString(), Top.ToString(), Right.ToString(), Bottom.ToString(), mode_1);
                }
                else
                {
                    return string.Format("{0},{1},{2},{3},{4},{5}", Left.ToString(), Top.ToString(), mode_1, Right.ToString(), Bottom.ToString(), mode_2);
                }
            }
            else
            {
                return string.Format("{0},{1},{2},{3}", Left.ToString(), Top.ToString(), Right.ToString(), Bottom.ToString());
            }
        }
    }
}
