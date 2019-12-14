using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Graphy.Model
{
    public class GeneratedFont : Font
    {
        // CONSTRUCTION
        public GeneratedFont()
        {
            
        }

        // ATTRIBUTS
        private const string FONT_FILE_PREFIXE = "CATIA_FONT-";
        private const string CURRENT_FILE_VERSION = "2.0";


        private string _generatedFileFullPath;
        private Version _generatedFileVersion;
        private bool _isGeneratedFileOK;

        public string GeneratedFileFullPath
        {
            get => _generatedFileFullPath;
            set
            {
                if (IsGeneratedFileReadable(value))
                {
                    Set(() => GeneratedFileFullPath, ref _generatedFileFullPath, value);

                    // Get the generated file version
                    TryGetVersionFromName(GeneratedFileFullPath, out Version tempVersion);
                    GeneratedFileVersion = tempVersion;

                    // Get the font family associated
                    FontFamily = GetFontFamilyFromName(GeneratedFileFullPath);

                    // Say it's OK
                    IsGeneratedFileOK = true;
                }
                else
                    IsGeneratedFileOK = false;
            }
        }

        public bool IsGeneratedFileOK
        {
            get => _isGeneratedFileOK;
            set
            {
                Set(() => IsGeneratedFileOK, ref _isGeneratedFileOK, value);
            }
        }

        public Version GeneratedFileVersion
        {
            get => _generatedFileVersion;
            set
            {
                Set(() => GeneratedFileVersion, ref _generatedFileVersion, value);
            }
        }

       

        public static string GetFileNameFormat()
        {
            return FONT_FILE_PREFIXE + CURRENT_FILE_VERSION + "-";
        }

        // STATIC METHODS

        public static bool TryGetVersionFromName(string fileFullPath, out Version fileVersion)
        {
            string fontName = Path.GetFileNameWithoutExtension(fileFullPath);

            // CHECK THE NAME FORMAT
            if (fontName.Substring(0, FONT_FILE_PREFIXE.Length) == FONT_FILE_PREFIXE)
                return Version.TryParse(fontName.Substring(FONT_FILE_PREFIXE.Length, 3), out fileVersion);
            else
                fileVersion = null;
                return false;
        }

        public static bool IsGeneratedFileReadable(string fileFullPath)
        {
            bool versionOk = TryGetVersionFromName(fileFullPath, out Version fileVersion);

            if (versionOk && fileVersion >= new Version(CURRENT_FILE_VERSION))
                return true;
            else
                return false;
        }

        public static FontFamily GetFontFamilyFromName(string fileFullPath)
        {
            return new FontFamily(Path.GetFileNameWithoutExtension(fileFullPath).Substring((FONT_FILE_PREFIXE + CURRENT_FILE_VERSION + "-").Length));
        }
    }
}
