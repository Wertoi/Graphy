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
                if(!IsDefault || (IsDefault && value))
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

        // PUBLIC METHODS
        public void ComputeSupportedCharacterList()
        {
            if (!IsCalculated && FontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                for (ushort i = 0; i < ushort.MaxValue; i++)
                {
                    if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(i, out ushort glyphIndex))
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


        public double GetLeftSideBearing(char c)
        {
            int unicodeValue = Convert.ToUInt16(c);

            if (FontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
                {
                    return glyphTypeFace.LeftSideBearings[glyphIndex];
                }
                else
                    return 0;
            }
            else
                return 0;
        }


        public double GetRightSideBearing(char c)
        {
            int unicodeValue = Convert.ToUInt16(c);

            if (FontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
                {
                    return glyphTypeFace.RightSideBearings[glyphIndex];
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="isBold"></param>
        /// <param name="isItalic"></param>
        /// <returns></returns>
        public double GetKerning(CatiaCharacter c1, CatiaCharacter c2, bool isBold, bool isItalic, double characterHeight)
        {
            if (c1 != null)
            {
                Typeface selectedTypeface = GetTypeface(isBold, isItalic);
                double emSize = 11;
                double pixelPerDip = 96;

                /*FormattedText c1FormattedText = new FormattedText(c1.Value.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    selectedTypeface, emSize, Brushes.Black, pixelPerDip);

                FormattedText c2FormattedText = new FormattedText(c2.Value.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    selectedTypeface, emSize, Brushes.Black, pixelPerDip);*/


                FormattedText formattedText = new FormattedText(c1.Value.ToString() + c2.Value.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    selectedTypeface, emSize, Brushes.Black, pixelPerDip);

                Geometry c1Geometry = formattedText.BuildHighlightGeometry(new Point(0, 0), 0, 1);
                Geometry c2Geometry = formattedText.BuildHighlightGeometry(new Point(0, 0), 1, 1);
                Geometry formattedTextGeometry = formattedText.BuildHighlightGeometry(new Point(0, 0), 0, 2);

                double ratio1 = c1Geometry.Bounds.Width / c1.PathGeometry.Bounds.Width;

                FormattedText referenceFormattedText = new FormattedText("M", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    selectedTypeface, emSize, Brushes.Black, pixelPerDip);

                Geometry referenceFormattedTextGeometry = referenceFormattedText.BuildHighlightGeometry(new Point(0, 0), 0, 1);

                double ratio2 = characterHeight / referenceFormattedTextGeometry.Bounds.Height;



                return (formattedTextGeometry.Bounds.Width - (c1.PathGeometry.Bounds.Width + c2.PathGeometry.Bounds.Width) * ratio1 ) * ratio2;
            }
            else
                return 0;

        }


        public double GetWidth(char c, Typeface selectedTypeface)
        {
            int unicodeValue = Convert.ToUInt16(c);

            if (selectedTypeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
                {
                    return glyphTypeFace.AdvanceWidths[glyphIndex];
                }
                else
                    return 0;
            }
            else
                return 0;
        }


    }
}
