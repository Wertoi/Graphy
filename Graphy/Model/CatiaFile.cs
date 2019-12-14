using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Graphy.Model.CatiaDocument;

namespace Graphy.Model
{
    public class CatiaFile : ObservableObject
    {
        public CatiaFile(string fullPath)
        {
            FullPath = fullPath;
        }

        private const string PART_DOCUMENT_FILE_EXTENSION = ".CATPart";
        private const string PRODUCT_DOCUMENT_FILE_EXTENSION = ".CATProduct";
        private const string DRAWING_DOCUMENT_FILE_EXTENSION = ".CATDrawing";

        private string _fullPath;
        private string _name;
        private bool _fileExist = false;
        private bool _isSelected = true;

        public string FullPath
        {
            get => _fullPath;
            set
            {
                Set(() => FullPath, ref _fullPath, value);

                Name = Path.GetFileNameWithoutExtension(FullPath);

                FileExist = Path.GetFileName(FullPath) == FullPath ? false : true;
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                Set(() => IsSelected, ref _isSelected, value);
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public bool FileExist
        {
            get => _fileExist;
            set
            {
                Set(() => FileExist, ref _fileExist, value);
            }
        }

        public CatiaGenericDocument.CatiaDocumentFormat GetDocumentFormat()
        {
            switch (System.IO.Path.GetExtension(FullPath))
            {
                case PART_DOCUMENT_FILE_EXTENSION:
                    return CatiaGenericDocument.CatiaDocumentFormat.CATPart;

                case PRODUCT_DOCUMENT_FILE_EXTENSION:
                    return CatiaGenericDocument.CatiaDocumentFormat.CATProduct;

                case DRAWING_DOCUMENT_FILE_EXTENSION:
                    return CatiaGenericDocument.CatiaDocumentFormat.Drawing;

                default:
                    return CatiaGenericDocument.CatiaDocumentFormat.Unidentified;
            }
        }
    }
}
