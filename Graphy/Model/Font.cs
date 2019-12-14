using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Graphy.Model
{
    public class Font : ObservableObject
    {
        public Font()
        {

        }

        public Font(FontFamily fontFamily, string supportedCharacterList = "")
        {
            FontFamily = fontFamily;
            SupportedCharacterList = supportedCharacterList;
        }

        private string _name;
        private FontFamily _fontFamily;
        private bool _isCalculated = false;
        private string _supportedCharacterList;


        public string Name
        {
            get => _name;
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                Set(() => FontFamily, ref _fontFamily, value);
                FontFamily.FamilyNames.TryGetValue(System.Windows.Markup.XmlLanguage.GetLanguage("en-us"), out string familyName);
                Name = familyName;
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

                if (SupportedCharacterList != "")
                    IsCalculated = true;
                else
                    IsCalculated = false;
            }
        }


        // METHODS

        public static List<UnicodeCategory> GetUnicodeCategoryCollection()
        {
            List<UnicodeCategory> unicodeCategoryCollection = new List<UnicodeCategory>();

            // FILL UNICODE CATEGORY COLLECTION
            foreach (UnicodeCategory category in System.Enum.GetValues(typeof(UnicodeCategory)))
            {
                if (category != UnicodeCategory.PrivateUse)
                    unicodeCategoryCollection.Add(category);
            }

            return unicodeCategoryCollection;
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
            /*int unicodeValue = Convert.ToUInt16(c);

            var typefaces = FontFamily.GetTypefaces();
            if (typefaces.First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                glyphTypeFace.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex);
                return glyphTypeFace.AdvanceWidths[glyphIndex];
            }
            else
                return 0;*/

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


        // *************************************

        public override bool Equals(object obj)
        {
            // Is null?
            if (Object.ReferenceEquals(null, obj))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            // Is the same type?
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            Font font = (Font)obj;
            return (Name == font.Name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
