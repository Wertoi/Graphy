using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Graphy.Model.CatiaDocument;
using INFITF;
using KnowledgewareTypeLib;

namespace Graphy.Model
{
    public class CatiaEnv : ObservableObject
    {
        // CONSTRUCTOR
        public CatiaEnv()
        {
            Initialize();
        }


        // ATTRIBUTS
        private Application _application;
        private bool _isApplicationOpen = false;
        private string _errorLog = "";
        private string _lengthUnit;

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

        public string LengthUnit
        {
            get => _lengthUnit;
            set
            {
                Set(() => LengthUnit, ref _lengthUnit, value);
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
                Application = (Application)Marshal.GetActiveObject("Catia.Application");
            }
            catch (Exception ex)
            {
                // We don't really care about exceptions, we just store the message
                ErrorLog = ex.Message;
            }

            if (Application != null)
            {
                IsApplicationOpen = true;
                LengthUnit = GetLengthUnit();
            }
            else
                IsApplicationOpen = false;
        }



        /// <summary>
        /// Returns Catia current length unit name.
        /// </summary>
        /// <returns></returns>
        private string GetLengthUnit()
        {
            UnitsSheetSettingAtt unitSetting = (UnitsSheetSettingAtt)Application.SettingControllers.Item("CATLieUnitsSheetSettingCtrl");
            string unitName = "";
            double decimalWrite;
            double decimalRead;
            unitSetting.GetMagnitudeValues("LENGTH", ref unitName, out decimalWrite, out decimalRead);

            return unitName;
        }


        /// <summary>
        /// Create a new document of the specified type.
        /// </summary>
        /// <param name="catiaDocumentFormat"></param>
        /// <returns></returns>
        public CatiaGenericDocument AddNewDocument(CatiaGenericDocument.CatiaDocumentFormat catiaDocumentFormat)
        {
            CatiaGenericDocument newCatiaDocument = new CatiaGenericDocument(this);
            bool isDocumentCollectionEmpty = Application.Documents.Count == 0 ? true : false;

            if (isDocumentCollectionEmpty)
            {
                newCatiaDocument.Document = Application.Documents.Add(CatiaGenericDocument.GetDocumentFormatName(catiaDocumentFormat));

                do
                {
                    newCatiaDocument.Document.Activate();
                }
                while (Application.Documents.Count == 0);
            }
            else
            {
                Document previousActiveDocument = Application.ActiveDocument;
                newCatiaDocument.Document = Application.Documents.Add(CatiaGenericDocument.GetDocumentFormatName(catiaDocumentFormat));

                do
                {
                    newCatiaDocument.Document.Activate();
                }
                while (Application.ActiveDocument == previousActiveDocument);
            }

            return newCatiaDocument;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public CatiaGenericDocument OpenDocument(CatiaFile file)
        {
            CatiaGenericDocument newCatiaDocument = new CatiaGenericDocument(this);
            bool isDocumentCollectionEmpty = Application.Documents.Count == 0 ? true : false;

            // If there is no document already open
            if (isDocumentCollectionEmpty)
            {
                newCatiaDocument.Document = Application.Documents.Open(file.FullPath);

                do
                {
                    newCatiaDocument.Document.Activate();
                }
                while (Application.Documents.Count == 0);
            }

            // Otherwise
            else
            {
                Document previousActiveDocument = Application.ActiveDocument;
                newCatiaDocument.Document = Application.Documents.Open(file.FullPath);

                do
                {
                    newCatiaDocument.Document.Activate();
                }
                while (Application.ActiveDocument == previousActiveDocument);
            }

            return newCatiaDocument;
        }
    }
}
