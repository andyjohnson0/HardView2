using System;
using System.Collections.Generic;
using System.Drawing;

namespace uk.andyjohnson.HardView2
{
    public static class Ext
    {
        public static (SizeF, float) MeasureStrings(
            this Graphics grp,
            string[] strings,
            Font fnt,
            float lineSpacing)
        {
            var measuredSize = new SizeF(0F, 0F);
            var maxLineHeight = 0F;
            foreach (var str in strings)
            {
                var s = grp.MeasureString(str, fnt);
                measuredSize.Width = Math.Max(s.Width, measuredSize.Width);
                measuredSize.Height += s.Height;
                maxLineHeight = Math.Max(s.Height, maxLineHeight);
            }
            measuredSize.Height += Math.Max((strings.Length - 1) * lineSpacing, 0F);
            return (measuredSize, maxLineHeight);
        }
    }
}
