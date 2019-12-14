using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Graphy.Model.Generator
{
    public class SupportedCharGenerator : IGenerator
    {
        public event EventHandler<ProgressRateChangedEventArgs> ProgressRateChanged;
        public event EventHandler<ProgressRateChangedEventArgs> StepProgressRateChanged;

        public SupportedCharGenerator()
        {

        }

        private double _progressRate;
        private int _stepProgressRate;

        public double ProgressRate
        {
            get => _progressRate;
            set
            {
                if (value != ProgressRate)
                {
                    _progressRate = value;
                    ProgressRateChanged?.Invoke(this, new ProgressRateChangedEventArgs(ProgressRate));
                }
            }
        }

        public int StepProgressRate
        {
            get => _stepProgressRate;
            set
            {
                if (value != StepProgressRate)
                {
                    _stepProgressRate = value;
                    StepProgressRateChanged?.Invoke(this, new ProgressRateChangedEventArgs(StepProgressRate));
                }
            }
        }

        // PUBLIC METHODS
        public string GenerateSupportedCharacters(Font font)
        {
            string supportedCharacterList = "";
            foreach (UnicodeCategory unicodeCategory in Font.GetUnicodeCategoryCollection())
            {
                foreach (char c in GetCharsByUnicodeCategory(unicodeCategory))
                {
                    if (IsCharacterSupported(c, font.FontFamily))
                        supportedCharacterList += c;
                }

                ProgressRate += (1d / (double)Font.GetUnicodeCategoryCollection().Count());
            }

            return supportedCharacterList.ToString();
        }

        // PRIVATE METHODS
        private bool IsCharacterSupported(char characterToCheck, FontFamily fontFamily)
        {
            int unicodeValue = Convert.ToUInt16(characterToCheck);

            fontFamily.GetTypefaces().First().TryGetGlyphTypeface(out GlyphTypeface glyph);
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
