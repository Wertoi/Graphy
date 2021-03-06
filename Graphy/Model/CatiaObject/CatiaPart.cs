using GalaSoft.MvvmLight;
using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Graphy.Model.CatiaObject
{
    public class CatiaPart : ObservableObject
    {
        public CatiaPart()
        {

        }

        public CatiaPart(string fileFullPath)
        {
            FileFullPath = fileFullPath;
        }

        private const string PART_FILE_EXTENSION = ".CATPart";

        private CatiaEnv _catiaEnv;
        private PartDocument _partDocument;
        private bool _isDocumentOpen;
        private string _fileFullPath;
        private string _name;

        public CatiaEnv CatiaEnv { get => _catiaEnv; set => _catiaEnv = value; }
        public PartDocument PartDocument { get => _partDocument; set => _partDocument = value; }
        public bool IsDocumentOpen { get => _isDocumentOpen; set => _isDocumentOpen = value; }

        public string FileFullPath
        {
            get => _fileFullPath;
            set
            {
                Set(() => FileFullPath, ref _fileFullPath, value);

                _name = Path.GetFileNameWithoutExtension(FileFullPath);
                RaisePropertyChanged(() => Name);
            }
        }

        public string Name { get => _name; }

        public bool TryAssignDocument(CatiaEnv catiaEnv, Document document)
        {
            IsDocumentOpen = false;

            if (!IsFormatOK(document.FullName))
                return false;

            FileFullPath = document.FullName;
            CatiaEnv = catiaEnv;
            PartDocument = (PartDocument)document;
            IsDocumentOpen = true;

            return true;
        }

        public void CloseDocument(bool save)
        {
            if(IsDocumentOpen)
            {
                if (save && PartDocument.Saved)
                    PartDocument.Save();

                PartDocument.Close();
                IsDocumentOpen = false;
            }
        }

        public bool TryOpenDocument(CatiaEnv catiaEnv, string fileFullPath)
        {
            IsDocumentOpen = false;

            if (!System.IO.File.Exists(fileFullPath) || !IsFormatOK(fileFullPath))
                return false;

            Document tempDocument;
            bool isDocumentCollectionEmpty = catiaEnv.Application.Documents.Count == 0;

            // If there is no document already open
            if (isDocumentCollectionEmpty)
            {
                tempDocument = catiaEnv.Application.Documents.Open(fileFullPath);

                do
                {
                    tempDocument.Activate();
                }
                while (catiaEnv.Application.Documents.Count == 0);
            }

            // Otherwise
            else
            {
                Document previousActiveDocument = catiaEnv.Application.ActiveDocument;
                tempDocument = catiaEnv.Application.Documents.Open(fileFullPath);

                if (tempDocument != previousActiveDocument)
                {
                    do
                    {
                        tempDocument.Activate();
                    }
                    while (catiaEnv.Application.ActiveDocument == previousActiveDocument);
                }
            }

            FileFullPath = fileFullPath;
            CatiaEnv = catiaEnv;
            PartDocument = (PartDocument)tempDocument;
            IsDocumentOpen = true;

            return true;
        }

        public static bool IsFormatOK(string fileFullPath)
        {
            return Path.GetExtension(fileFullPath) == PART_FILE_EXTENSION;
        }

    }
}
