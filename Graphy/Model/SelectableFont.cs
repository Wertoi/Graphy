using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public PathGeometry GetCharacterGeometry(char c, double toleranceValue)
        {
            int unicodeValue = Convert.ToUInt16(c);

            if (FontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
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


        public double GetWidth(char c)
        {
            int unicodeValue = Convert.ToUInt16(c);

            if (FontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
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
