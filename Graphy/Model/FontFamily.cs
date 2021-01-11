using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Graphy.Model
{
    public class FontFamily : System.Windows.Media.FontFamily
    {
        public FontFamily(string familyName) : base(familyName)
        {
            
        }

        private KerningPair[] _kerningPairs;

        public KerningPair[] KerningPairs { get => _kerningPairs; set => _kerningPairs = value; }


        private Typeface GetTypeface(bool isBold, bool isItalic)
        {
            Typeface selectedTypeface = GetTypefaces().First();

            if (isBold || isItalic)
            {

                foreach (Typeface typeface in GetTypefaces())
                {
                    if (((!isBold && typeface.Weight == FontWeights.Normal) || (isBold && typeface.Weight == FontWeights.Bold)) &&
                       ((!isItalic && typeface.Style == FontStyles.Normal) || (isItalic && typeface.Style == FontStyles.Italic)))
                    {
                        selectedTypeface = typeface;
                    }
                }
            }

            return selectedTypeface;
        }


        public PathGeometry GetCharacterGeometry(char c, double toleranceValue, bool isBold, bool isItalic)
        {
            int unicodeValue = Convert.ToUInt16(c);

            Typeface selectedTypeface = GetTypeface(isBold, isItalic);

            if (selectedTypeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
                {
                    return glyphTypeFace.GetGlyphOutline(glyphIndex, 16, 1).GetFlattenedPathGeometry(toleranceValue, ToleranceType.Relative);
                }
                else
                    return null;
            }
            else
                return null;
        }


        public double GetRightSideBearing(char c, double refHeight, bool isBold, bool isItalic)
        {
            int unicodeValue = Convert.ToUInt16(c);

            Typeface selectedTypeface = GetTypeface(isBold, isItalic);

            if (selectedTypeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
                {
                    double ratio = GetRatio(refHeight, isBold, isItalic);
                    return glyphTypeFace.RightSideBearings[glyphIndex] * ratio;
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public double GetLeftSideBearing(char c, double refHeight, bool isBold, bool isItalic)
        {
            int unicodeValue = Convert.ToUInt16(c);

            Typeface selectedTypeface = GetTypeface(isBold, isItalic);

            if (selectedTypeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
                {
                    double ratio = GetRatio(refHeight, isBold, isItalic);
                    return glyphTypeFace.LeftSideBearings[glyphIndex] * ratio;
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public (double position, double thickness) GetUnderline(double refHeight, bool isBold, bool isItalic)
        {
            Typeface selectedTypeFace = GetTypeface(isBold, isItalic);

            if (selectedTypeFace.TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                double ratio = GetRatio(refHeight, isBold, isItalic);
                return (position: glyphTypeFace.UnderlinePosition * ratio, thickness: glyphTypeFace.UnderlineThickness * ratio);
            }
            else
                return (position: 0, thickness: 0);
        }

        public (double position, double thickness) GetStrikeThrough(double refHeight, bool isBold, bool isItalic)
        {
            Typeface selectedTypeFace = GetTypeface(isBold, isItalic);

            if (selectedTypeFace.TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                double ratio = GetRatio(refHeight, isBold, isItalic);
                return (position: glyphTypeFace.StrikethroughPosition * ratio, thickness: glyphTypeFace.StrikethroughThickness * ratio);
            }
            else
                return (position: 0, thickness: 0);
        }


        /// <summary>
        /// Return the ratio between the reference height and the em relative height of the 'M' character.
        /// </summary>
        /// <param name="refHeight">Reference height.</param>
        /// <param name="isBold"></param>
        /// <param name="isItalic"></param>
        /// <returns></returns>
        private double GetRatio(double refHeight, bool isBold, bool isItalic)
        {
            int unicodeValue = Convert.ToUInt16('M');

            Typeface selectedTypeface = GetTypeface(isBold, isItalic);

            if (selectedTypeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
                {
                    return refHeight / (glyphTypeFace.Height - glyphTypeFace.TopSideBearings[glyphIndex] - glyphTypeFace.BottomSideBearings[glyphIndex]);
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        #region KERNING PAIR AREA

        // KERNING PAIR STRUCTURE
        [StructLayout(LayoutKind.Sequential)]
        public struct KerningPair
        {
            public Int16 wFirst;
            public Int16 wSecond;
            public int iKernAmount;
        }

        #region DLL IMPORTS FOR KERNING PAIR COMPUTATION
        [DllImport("Gdi32.dll", EntryPoint = "GetKerningPairs", SetLastError = true)]
        static extern int GetKerningPairsW(int hdc, int nNumPairs, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] KerningPair[] kerningPairs);

        [DllImport("Gdi32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("Gdi32.dll", CharSet = CharSet.Unicode)]
        static extern bool DeleteObject(IntPtr hdc);
        #endregion

        public void ComputeKerningPairs()
        {
            // NOTE: We put the font size to 1000 because kerning value are expressed in 1000/em.
            // The kerning value retrieved from the function is an integer so with a font size smaller than 1000, it will be rounded.
            System.Drawing.Font font = new System.Drawing.Font(Source, 1000);

            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            g.PageUnit = System.Drawing.GraphicsUnit.Pixel;

            // Select the HFONT into the HDC.
            IntPtr hDC = g.GetHdc();
            System.Drawing.Font fontClone = (System.Drawing.Font)font.Clone();
            IntPtr hFont = fontClone.ToHfont();
            SelectObject(hDC, hFont);

            // Find out how many pairs there are and allocate them.
            int numKerningPairs = GetKerningPairsW(hDC.ToInt32(), 0, null);
            KerningPair[] kerningPairs = new KerningPair[numKerningPairs];

            // Get the pairs.
            GetKerningPairsW(hDC.ToInt32(), kerningPairs.Length, kerningPairs);

            DeleteObject(hFont);
            g.ReleaseHdc();

            KerningPairs = kerningPairs;
        }
        #endregion
    }
}
