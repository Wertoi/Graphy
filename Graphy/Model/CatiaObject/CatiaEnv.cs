using System;
using System.Runtime.InteropServices;
using GalaSoft.MvvmLight;
using INFITF;
using KnowledgewareTypeLib;

namespace Graphy.Model.CatiaObject
{
    public class CatiaEnv : ObservableObject
    {
        // CONSTRUCTOR
        public CatiaEnv()
        {
            
        }


        // ATTRIBUTS
        private Application _application;
        private bool _isApplicationOpen = false;
        private string _errorLog = "";
        private string _lengthUnitSymbol;
        private string _fullVersion;

        public Application Application
        {
            get => _application;
            set
            {
                Set(() => Application, ref _application, value);
            }
        }

        public bool IsApplicationOpen
        {
            get => _isApplicationOpen;
            set
            {
                Set(() => IsApplicationOpen, ref _isApplicationOpen, value);
            }
        }

        public string ErrorLog
        {
            get => _errorLog;
            set
            {
                Set(() => ErrorLog, ref _errorLog, value);
            }
        }

        public string LengthUnitSymbol
        {
            get => _lengthUnitSymbol;
            set
            {
                Set(() => LengthUnitSymbol, ref _lengthUnitSymbol, value);
            }
        }

        public string FullVersion
        {
            get => _fullVersion;
            set
            {
                Set(() => FullVersion, ref _fullVersion, value);
            }
        }


        // METHODS

        /// <summary>
        /// Initialize or reset the instance
        /// </summary>
        public void Initialize()
        {
            Application = null;

            try
            {
                Application = (Application)Marshal.GetActiveObject("CATIA.Application");
            }
            catch (Exception ex)
            {
                // We don't really care about exceptions, we just store the message
                ErrorLog = ex.Message;
            }

            if (Application != null)
            {
                IsApplicationOpen = true;
                LengthUnitSymbol = GetLengthUnitSymbol();
                FullVersion = "V" + Application.SystemConfiguration.Version + "R" + Application.SystemConfiguration.Release;
            }
            else
            {
                IsApplicationOpen = false;
                LengthUnitSymbol = "No unit";
                FullVersion = "/";
            }
        }



        /// <summary>
        /// Returns Catia current length unit name.
        /// </summary>
        /// <returns></returns>
        private string GetLengthUnitSymbol()
        {
            UnitsSheetSettingAtt unitSetting = (UnitsSheetSettingAtt)Application.SettingControllers.Item("CATLieUnitsSheetSettingCtrl");

            string unitName = "";
            unitSetting.GetMagnitudeValues("LENGTH", ref unitName, out _, out _);
            switch (unitName)
            {
                case string str when str.Contains("Micro"):
                    return "μm";

                case string str when str.Contains("Millim"):
                    return "mm";

                case string str when str.Contains("Centim"):
                    return "cm";

                case "Mètre":
                case "Meter":
                    return "m";

                case string str when str.Contains("Kilom"):
                    return "km";

                case "Pouce":
                case "Inch":
                    return "in";

                case "Pied":
                case "Foot":
                    return "ft";

                default:
                    return unitName;
            }
        }

    }
}
