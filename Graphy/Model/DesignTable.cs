using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Graphy.Model.CatiaDocument;

namespace Graphy.Model
{
    public class DesignTable : ObservableObject
    {
        public DesignTable()
        {
            ParameterCollection = new ObservableCollection<DesignTableParameter>();
            PartCollection = new ObservableCollection<CatiaFile>();
        }

        private string _fullPath;
        private string _partFolderPath;
        private ObservableCollection<DesignTableParameter> _parameterCollection;
        private ObservableCollection<CatiaFile> _partCollection;

        public string FullPath
        {
            get => _fullPath;
            set
            {
                Set(() => FullPath, ref _fullPath, value);
            }
        }

        public string PartFolderPath
        {
            get => _partFolderPath;
            set
            {
                Set(() => PartFolderPath, ref _partFolderPath, value);

                LoadCatiaFileCollection(PartFolderPath);
            }
        }

        public ObservableCollection<DesignTableParameter> ParameterCollection
        {
            get => _parameterCollection;
            set
            {
                Set(() => ParameterCollection, ref _parameterCollection, value);
            }
        }

        public ObservableCollection<CatiaFile> PartCollection
        {
            get => _partCollection;
            set
            {
                Set(() => PartCollection, ref _partCollection, value);
            }
        }

        public void LoadCatiaFileCollection(string folderPath)
        {
            if(System.IO.Directory.Exists(folderPath))
            {
                foreach (string fileFullPath in System.IO.Directory.GetFiles(folderPath))
                {
                    CatiaFile tempCatiaFile = new CatiaFile(fileFullPath);

                    if (tempCatiaFile.GetDocumentFormat() == CatiaGenericDocument.CatiaDocumentFormat.CATPart)
                    {
                        PartCollection.Add(tempCatiaFile);
                    } 
                }
            }
        }
    }
}
