using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using Graphy.Model.CatiaShape;
using System.Runtime.InteropServices;

namespace Graphy.Model
{
    public class SelectableFont : ObservableObject
    {
        public SelectableFont(FontFamily fontFamily, bool isDefault = false)
        {
            FontFamily = fontFamily;
            IsDefault = isDefault;

            if (IsDefault)
                IsSelected = true;
        }

        // ATTRIBUTS
        private FontFamily _fontFamily;
        private bool _isSelected;
        private bool _isCalculated;
        private string _supportedCharacterList;
        private bool _isDefault;
        private KerningPair[] _kerningPairs;


        public FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                Set(() => FontFamily, ref _fontFamily, value);
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!IsDefault || (IsDefault && value))
                {
                    Set(() => IsSelected, ref _isSelected, value);
                }
            }
        }

        public bool IsCalculated
        {
            get => _isCalculated;
            set
            {
                Set(() => IsCalculated, ref _isCalculated, value);
            }
        }

        public string SupportedCharacterList
        {
            get => _supportedCharacterList;
            set
            {
                Set(() => SupportedCharacterList, ref _supportedCharacterList, value);

                IsCalculated = SupportedCharacterList != "" ? true : false;
            }
        }

        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                Set(() => IsDefault, ref _isDefault, value);
            }
        }

        public KerningPair[] KerningPairs
        {
            get => _kerningPairs;
            set
            {
                Set(() => KerningPairs, ref _kerningPairs, value);
            }
        }


        // PUBLIC METHODS
        public void ComputeSupportedCharacterList()
        {
            if (!IsCalculated && FontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                for (ushort i = 0; i < ushort.MaxValue; i++)
                {
                    if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(i, out _))
                        SupportedCharacterList += Convert.ToChar(i);
                }
            }
        }


        public Typeface GetTypeface(bool isBold, bool isItalic)
        {
            Typeface selectedTypeface = FontFamily.GetTypefaces().First();

            if (isBold || isItalic)
            {

                foreach (Typeface typeface in FontFamily.GetTypefaces())
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
                    return glyphTypeFace.GetGlyphOutline(glyphIndex, 16, 1).GetFlattenedPathGeometry(toleranceValue, System.Windows.Media.ToleranceType.Relative);
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
                    return glyphTypeFace.RightSideBearings[glyphIndex] * refHeight / glyphTypeFace.Height;
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
                    return glyphTypeFace.LeftSideBearings[glyphIndex] * refHeight / glyphTypeFace.Height;
                }
                else
                    return 0;
            }
            else
                return 0;
        }


        // METHOD TO RETRIEVE KERNING PAIRS

        [StructLayout(LayoutKind.Sequential)]
        public struct KerningPair
        {
            public Int16 wFirst;
            public Int16 wSecond;
            public int iKernAmount;
        }


        [DllImport("Gdi32.dll", EntryPoint = "GetKerningPairs", SetLastError = true)]
        static extern int GetKerningPairsW(int hdc, int nNumPairs, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] KerningPair[] kerningPairs);

        [DllImport("Gdi32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("Gdi32.dll", CharSet = CharSet.Unicode)]
        static extern bool DeleteObject(IntPtr hdc);


        public void ComputeKerningPairs()
        {
            // NOTE: We put the font size to 1000 because kerning value are expressed in 1000/em.
            // The kerning value retrieved from the function is an integer so with a font size smaller than 1000, it will be rounded.
            System.Drawing.Font font = new System.Drawing.Font(FontFamily.ToString(), 1000);

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

    }
}
