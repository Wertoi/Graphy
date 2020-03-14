using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Graphy.Model.Generator
{
    public class SupportedCharGenerator
    {

        public SupportedCharGenerator()
        {

        }

        // PUBLIC METHODS
        public string ComputeSupportedCharacterList(FontFamily fontFamily)
        {
            string supportedCharacterList = "";

            if (fontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyphTypeFace))
            {
                for (ushort i = 0; i < ushort.MaxValue; i++)
                {
                    if (glyphTypeFace.CharacterToGlyphMap.TryGetValue(i, out ushort glyphIndex))
                        supportedCharacterList += Convert.ToChar(i);
                }
            }

            return supportedCharacterList;
        }

        public string GenerateSupportedCharacters(SelectableFont font)
        {
            string supportedCharacterList = "";

            // test

            for(ushort i = 0; i < ushort.MaxValue; i++)
            {
                if (IsCharacterSupported(Convert.ToChar(i), font.FontFamily))
                    supportedCharacterList += Convert.ToChar(i);
            }

            /*font.FontFamily.GetTypefaces().First().TryGetGlyphTypeface(out System.Windows.Media.GlyphTypeface glyph);
            foreach(KeyValuePair<int, ushort> character in glyph.CharacterToGlyphMap)
            {
                supportedCharacterList += Convert.ToChar(Convert.ToUInt16(character.Key));
            }*/

            // fin test

            /*foreach (UnicodeCategory unicodeCategory in Font.GetUnicodeCategoryCollection())
            {
                foreach (char c in GetCharsByUnicodeCategory(unicodeCategory))
                {
                    if (IsCharacterSupported(c, font.FontFamily))
                        supportedCharacterList += c;
                }

                ProgressRate += (1d / (double)Font.GetUnicodeCategoryCollection().Count());
            }*/

            return supportedCharacterList.ToString();
        }

        // PRIVATE METHODS
        private bool IsCharacterSupported(char characterToCheck, FontFamily fontFamily)
        {
            int unicodeValue = Convert.ToUInt16(characterToCheck);

            fontFamily.GetTypefaces().First().TryGetGlyphTypeface(out System.Windows.Media.GlyphTypeface glyph);
            if (glyph != null && glyph.CharacterToGlyphMap.TryGetValue(unicodeValue, out ushort glyphIndex))
            {
                return true;
            }

            return false;
        }

        private string GetCharsByUnicodeCategory(UnicodeCategory category)
        {
            string charList = "";

            for (ushort codePoint = 0; codePoint < ushort.MaxValue; codePoint++)
            {
                Char ch = Convert.ToChar(codePoint);

                if (CharUnicodeInfo.GetUnicodeCategory(ch) == category)
                {
                    charList += ch.ToString();
                }
            }

            return charList;
        }
    }
}
