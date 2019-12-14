using DRAFTINGITF;
using GalaSoft.MvvmLight;
using INFITF;
using MECMOD;
using ProductStructureTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaDocument
{
    public class CatiaGenericDocument : ObservableObject
    {
        public enum CatiaDocumentFormat
        {
            Unidentified,
            CATPart,
            CATProduct,
            Drawing
        }

        public CatiaGenericDocument(CatiaEnv catiaEnv)
        {
            CatiaEnv = CatiaEnv;
        }

        private const string PART_DOCUMENT_FORMAT_NAME = "Part";
        private const string DRAWING_DOCUMENT_FORMAT_NAME = "Drawing";
        private const string PRODUCT_DOCUMENT_FORMAT_NAME = "Product";

        private CatiaEnv _catiaEnv;
        private string _name;
        protected Document _document;
        private CatiaDocumentFormat _documentFormat;
        private CatiaFile _file;

        public string Name
        {
            get => _name;
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public Document Document
        {
            get => _document;
            set
            {
                Set(() => Document, ref _document, value);

                if (File != null)
                    File.FullPath = Document.FullName;
                else
                    File = new CatiaFile(Document.FullName);

                DocumentFormat = File.GetDocumentFormat();
                Name = Document.get_Name();
            }
        }

        public CatiaDocumentFormat DocumentFormat
        {
            get => _documentFormat;
            set
            {
                Set(() => DocumentFormat, ref _documentFormat, value);
            }
        }

        public CatiaEnv CatiaEnv
        {
            get => _catiaEnv;
            set
            {
                Set(() => CatiaEnv, ref _catiaEnv, value);
            }
        }

        public CatiaFile File
        {
            get => _file;
            set
            {
                Set(() => File, ref _file, value);
            }
        }


        public static string GetDocumentFormatName(CatiaDocumentFormat format)
        {
            switch (format)
            {
                case CatiaDocumentFormat.CATPart:
                    return PART_DOCUMENT_FORMAT_NAME;

                case CatiaDocumentFormat.CATProduct:
                    return PRODUCT_DOCUMENT_FORMAT_NAME;

                case CatiaDocumentFormat.Drawing:
                    return DRAWING_DOCUMENT_FORMAT_NAME;

                default:
                    return "";
            }
        }
    }

    // 
    public class InvalidDocumentType : Exception
    {
        public InvalidDocumentType()
        {
        }

        public InvalidDocumentType(string message)
            : base(message)
        {
        }

        public InvalidDocumentType(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
