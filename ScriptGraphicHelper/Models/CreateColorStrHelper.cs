using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using static System.Environment;

namespace ScriptGraphicHelper.Models
{
    public static class CreateColorStrHelper
    {
        public static bool IsAddRange { get; set; } = false;
        public static string Create(int index, ObservableCollection<ColorInfo> colorInfos, Range rect = null)
        {
            return index switch
            {
                0 => CompareStr(colorInfos),
                1 => DmFindStr(colorInfos, rect),
                2 => AjFindStr(colorInfos, rect),
                3 => AjCompareStr(colorInfos),
                4 => CdFindStr(colorInfos, rect),
                5 => CdCompareStr(colorInfos),
                6 => AutojsFindStr(colorInfos, rect),
                7 => EcFindStr(colorInfos, rect),
                8 => DiyStr(colorInfos, rect),
                9 => AnchorsCompareStr(colorInfos),
                10 => AnchorsCompareStrTest(colorInfos),
                _ => CompareStr(colorInfos),
            };
        }
        public static string DiyStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            DiyFormat diyFormat;
            try
            {
                StreamReader _sr = File.OpenText(CurrentDirectory + "\\diyFormat.json");
                string result = _sr.ReadToEnd();
                _sr.Close();
                diyFormat = JsonConvert.DeserializeObject<DiyFormat>(result);
                if (diyFormat.ColorMode > 1)
                    diyFormat.ColorMode = 0;
                if (diyFormat.Range > 2)
                    diyFormat.ColorMode = 0;
                if (diyFormat.Range > 2)
                    diyFormat.ColorMode = 2;
            }
            catch
            {
                MessageBox.Show("diyFormat.json文件不存在或自定义格式错误!");
                return string.Empty;
            }
            if (diyFormat.ColorMode == 0)
            {
                return DiyFindStr(colorInfos, rect, diyFormat);
            }
            else if (diyFormat.ColorMode == 1)
            {
                return DiyCompareStr(colorInfos, diyFormat);
            }
            else
            {
                return DiyFindStr(colorInfos, rect, diyFormat);
            }
        }
        public static string DiyCompareStr(ObservableCollection<ColorInfo> colorInfos, DiyFormat diyFormat)
        {
            string result = "\"";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.OffsetColor == "000000")
                        if (!diyFormat.BGR)
                            result += colorInfo.ThePoint.X.ToString() + diyFormat.ChildSplit + colorInfo.ThePoint.Y.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + diyFormat.ParentSplit;
                        else
                            result += colorInfo.ThePoint.X.ToString() + diyFormat.ChildSplit + colorInfo.ThePoint.Y.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + diyFormat.ParentSplit;
                    else
                    {
                        if (!diyFormat.BGR)
                            result += colorInfo.ThePoint.X.ToString() + diyFormat.ChildSplit + colorInfo.ThePoint.Y.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "-" + colorInfo.OffsetColor + diyFormat.ParentSplit;
                        else
                        {
                            string offsetColor = colorInfo.OffsetColor.Substring(4, 2) + colorInfo.OffsetColor.Substring(2, 2) + colorInfo.OffsetColor.Substring(0, 2);
                            result += colorInfo.ThePoint.X.ToString() + diyFormat.ChildSplit + colorInfo.ThePoint.Y.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + diyFormat.ParentSplit;
                        }
                    }
                }
            }
            result = result.Trim(diyFormat.ParentSplit.ToCharArray());
            result += "\"";
            if (diyFormat.Point == 0)
            {
                result = "x,y," + result;
            }
            else if (diyFormat.Point > 0)
            {
                result += ",x,y";
            }
            return diyFormat.Prefix + result + diyFormat.Suffix;
        }
        public static string DiyFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect, DiyFormat diyFormat)
        {
            bool isInit = false;
            Point startPoint = new Point();
            string result = "\"";

            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.ThePoint;
                        if (diyFormat.StartPoint)
                        {
                            result += "0" + diyFormat.ChildSplit + "0" + diyFormat.ChildSplit;
                        }
                        if (colorInfo.OffsetColor == "000000")
                        {
                            if (!diyFormat.BGR)
                                result += diyFormat.ColorPrefix + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "\"" + diyFormat.ParentSplit + "\"";
                            else
                                result += diyFormat.ColorPrefix + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + "\"" + diyFormat.ParentSplit + "\"";
                        }
                        else
                        {
                            if (!diyFormat.BGR)
                                result += diyFormat.ColorPrefix + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "-" + diyFormat.ColorPrefix + colorInfo.OffsetColor + "\"" + diyFormat.ParentSplit + "\"";
                            else
                            {
                                string offsetColor = colorInfo.OffsetColor.Substring(4, 2) + colorInfo.OffsetColor.Substring(2, 2) + colorInfo.OffsetColor.Substring(0, 2);
                                result += diyFormat.ColorPrefix + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + "-" + diyFormat.ColorPrefix + offsetColor + "\"" + diyFormat.ParentSplit + "\"";
                            }
                        }
                    }
                    else
                    {
                        double OffsetX = colorInfo.ThePoint.X - startPoint.X;
                        double OffsetY = colorInfo.ThePoint.Y - startPoint.Y;
                        if (colorInfo.OffsetColor == "000000")
                        {
                            if (!diyFormat.BGR)
                                result += OffsetX.ToString() + diyFormat.ChildSplit + OffsetY.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + diyFormat.ParentSplit;
                            else
                                result += OffsetX.ToString() + diyFormat.ChildSplit + OffsetY.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + diyFormat.ParentSplit;
                        }
                        else
                        {
                            if (!diyFormat.BGR)
                                result += OffsetX.ToString() + diyFormat.ChildSplit + OffsetY.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "-" + diyFormat.ColorPrefix + colorInfo.OffsetColor + diyFormat.ParentSplit;
                            else
                            {
                                string offsetColor = colorInfo.OffsetColor.Substring(4, 2) + colorInfo.OffsetColor.Substring(2, 2) + colorInfo.OffsetColor.Substring(0, 2);
                                result += OffsetX.ToString() + diyFormat.ChildSplit + OffsetY.ToString() + diyFormat.ChildSplit + diyFormat.ColorPrefix + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + "-" + diyFormat.ColorPrefix + offsetColor + diyFormat.ParentSplit;
                            }
                        }
                    }
                }
            }
            result = result.Trim(diyFormat.ParentSplit.ToCharArray());
            result += "\"";

            if (diyFormat.Range == 0)
                result = rect.ToStr() + "," + result;
            else if (diyFormat.Point == 0)
            {
                result = "x,y," + result;
            }
            if (diyFormat.Range == 1)
                result += "," + rect.ToStr();
            else if (diyFormat.Point == 1)
            {
                result += ",x,y";
            }
            if (diyFormat.Range == 2)
                result += "," + rect.ToStr();
            else if (diyFormat.Point == 2)
            {
                result += ",x,y";
            }
            return diyFormat.Prefix + result + diyFormat.Suffix;
        }
        public static string DmFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            if (IsAddRange)
            {
                result = rect.ToStr() + ",";
            }
            bool isInit = false;
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.ThePoint;
                        if (colorInfo.OffsetColor == "000000")
                            result += "\"" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "\",\"";
                        else
                            result += "\"" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "-" + colorInfo.OffsetColor + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.ThePoint.X - startPoint.X;
                        double OffsetY = colorInfo.ThePoint.Y - startPoint.Y;
                        if (colorInfo.OffsetColor == "000000")
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                            colorInfo.TheColor.B.ToString("x2") + ",";
                        else
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                            colorInfo.TheColor.B.ToString("x2") + "-" + colorInfo.OffsetColor + ",";
                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }
        public static string CdFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        startPoint = colorInfo.ThePoint;
                        if (colorInfo.OffsetColor == "000000")
                            result += "0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + ",\"";
                        else
                            result += "0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "|0x" + colorInfo.OffsetColor + ",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.ThePoint.X - startPoint.X;
                        double OffsetY = colorInfo.ThePoint.Y - startPoint.Y;
                        if (colorInfo.OffsetColor == "000000")
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                            colorInfo.TheColor.B.ToString("x2") + ",";
                        else
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                            colorInfo.TheColor.B.ToString("x2") + "|0x" + colorInfo.OffsetColor + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (IsAddRange)
            {
                result += "\",90," + rect.ToStr();
            }
            else
            {
                result += "\"";
            }
            return result;
        }
        public static string AjFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            if (IsAddRange)
            {
                result = rect.ToStr() + ",";
            }
            bool isInit = false;
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.ThePoint;
                        if (colorInfo.OffsetColor == "000000")
                            result += "\"" + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + "\",\"";
                        else
                        {
                            string offsetColor = colorInfo.OffsetColor.Substring(4, 2) + colorInfo.OffsetColor.Substring(2, 2) + colorInfo.OffsetColor.Substring(0, 2);
                            result += "\"" + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + "-" + offsetColor + "\",\"";
                        }
                    }
                    else
                    {
                        double OffsetX = colorInfo.ThePoint.X - startPoint.X;
                        double OffsetY = colorInfo.ThePoint.Y - startPoint.Y;
                        if (colorInfo.OffsetColor == "000000")
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|" + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                            colorInfo.TheColor.R.ToString("x2") + ",";
                        else
                        {
                            string offsetColor = colorInfo.OffsetColor.Substring(4, 2) + colorInfo.OffsetColor.Substring(2, 2) + colorInfo.OffsetColor.Substring(0, 2);
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|" + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + "-" + offsetColor + ",";
                        }
                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }
        public static string AutojsFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        startPoint = colorInfo.ThePoint;
                        result = "\"#" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "\",[";
                    }
                    else
                    {
                        double OffsetX = colorInfo.ThePoint.X - startPoint.X;
                        double OffsetY = colorInfo.ThePoint.Y - startPoint.Y;
                        result += "[" + OffsetX.ToString() + "," + OffsetY.ToString() + ",\"#" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                            colorInfo.TheColor.B.ToString("x2") + "\"],";
                    }
                }
            }
            result = result.Trim(',');
            if (IsAddRange)
            {
                result += "],{region:[" + rect.ToStr(1) + "],threshold:[26]}";
            }
            else
            {
                result += "]";
            }
            return result;

        }
        public static string EcFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        startPoint = colorInfo.ThePoint;
                        if (colorInfo.OffsetColor == "000000")
                            result += "\"0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "\",\"";
                        else
                            result += "\"0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "-0x" + colorInfo.OffsetColor + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.ThePoint.X - startPoint.X;
                        double OffsetY = colorInfo.ThePoint.Y - startPoint.Y;
                        if (colorInfo.OffsetColor == "000000")
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                            colorInfo.TheColor.B.ToString("x2") + ",";
                        else
                            result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") +
                           colorInfo.TheColor.B.ToString("x2") + "-0x" + colorInfo.OffsetColor + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (IsAddRange)
            {
                result += "\",0.9," + rect.ToStr();
            }
            else
            {
                result += "\"";
            }

            return result;
        }
        public static string CompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "\"";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.OffsetColor == "000000")
                        result += colorInfo.ThePoint.X.ToString() + "|" + colorInfo.ThePoint.Y.ToString() + "|" + colorInfo.TheColor.R.ToString("x2") +
                        colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + ",";
                    else
                        result += colorInfo.ThePoint.X.ToString() + "|" + colorInfo.ThePoint.Y.ToString() + "|" + colorInfo.TheColor.R.ToString("x2") +
                        colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "-" + colorInfo.OffsetColor + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }
        public static string AjCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "\"";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.OffsetColor == "000000")
                        result += colorInfo.ThePoint.X.ToString() + "|" + colorInfo.ThePoint.Y.ToString() + "|" + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + ",";
                    else
                    {
                        string offsetColor = colorInfo.OffsetColor.Substring(4, 2) + colorInfo.OffsetColor.Substring(2, 2) + colorInfo.OffsetColor.Substring(0, 2);
                        result += colorInfo.ThePoint.X.ToString() + "|" + colorInfo.ThePoint.Y.ToString() + "|" + colorInfo.TheColor.B.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.R.ToString("x2") + "-" + offsetColor + ",";
                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }
        public static string CdCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "{";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += "{" + colorInfo.ThePoint.X.ToString() + "," + colorInfo.ThePoint.Y.ToString() + ",0x" + colorInfo.TheColor.R.ToString("x2") + colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "},";
                }
            }
            result = result.Trim(',');
            result += "}";
            return result;
        }


        public static string AnchorsCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = colorInfos[0].Width.ToString() + "," + colorInfos[0].Height.ToString() + ",[";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.Anchors == "L")
                        result += "[left,";
                    if (colorInfo.Anchors == "C")
                        result += "[center,";
                    if (colorInfo.Anchors == "R")
                        result += "[right,";

                    if (colorInfo.OffsetColor == "000000")
                    {
                        result += colorInfo.ThePoint.X.ToString() + "," + colorInfo.ThePoint.Y.ToString() + ",0x" + colorInfo.TheColor.R.ToString("x2") +
                        colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + "],";
                    }
                    else
                    {
                        result += colorInfo.ThePoint.X.ToString() + "," + colorInfo.ThePoint.Y.ToString() + ",0x" + colorInfo.TheColor.R.ToString("x2") +
                        colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + ",0x" + colorInfo.OffsetColor + "],";
                    }
                }
            }
            result = result.Trim(',');
            result += "]";
            return result;
        }
        public static string AnchorsCompareStrTest(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "\"";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Anchors + "|" + colorInfo.ThePoint.X.ToString() + "|" + colorInfo.ThePoint.Y.ToString() + "|" + colorInfo.TheColor.R.ToString("x2") +
                        colorInfo.TheColor.G.ToString("x2") + colorInfo.TheColor.B.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }
    }
}
