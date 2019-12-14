﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Graphy.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using INFITF;
using MECMOD;
using Graphy.Model.CatiaDocument;

namespace Graphy.ViewModel
{
    public class CatiaViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public CatiaViewModel()
        {
            CatiaEnv = new CatiaEnv();
            PartDocumentCollection = new ObservableCollection<CatiaPartDocument>();

            // MESSENGER REGISTRATION
            MessengerInstance.Register<CatiaPartDocument>(this, Enum.CatiaToken.Refresh, (doc) => RefreshCatiaApplicationCommandAction(doc));

            // COMMANDS INITIALIZATION
            RefreshCatiaApplicationCommand = new RelayCommand<CatiaPartDocument>((doc) => RefreshCatiaApplicationCommandAction(doc));
            OpenDocumentCommand = new RelayCommand(OpenDocumentCommandAction);

            RefreshCatiaApplicationCommandAction();
        }


        // ATTRIBUTS
        private CatiaEnv _catiaEnv;
        private CatiaPartDocument _selectedPartDocument;
        private ObservableCollection<CatiaPartDocument> _partDocumentCollection;
        private bool _isPartDocumentCollectionEmpty;

        public CatiaEnv CatiaEnv
        {
            get => _catiaEnv;
            set
            {
                Set(() => CatiaEnv, ref _catiaEnv, value);
            }
        }

        public CatiaPartDocument SelectedPartDocument
        {
            get => _selectedPartDocument;
            set
            {
                Set(() => SelectedPartDocument, ref _selectedPartDocument, value);

                if(SelectedPartDocument != null)
                {
                    // We send the selected part document
                    MessengerInstance.Send<CatiaPartDocument>(SelectedPartDocument, Enum.InputDataToken.WorkingPartDocumentChanged);

                    SelectedPartDocument.Document.Activate();
                } 
            }
        }

        public ObservableCollection<CatiaPartDocument> PartDocumentCollection
        {
            get => _partDocumentCollection;
            set
            {
                Set(() => PartDocumentCollection, ref _partDocumentCollection, value);
            }
        }

        public bool IsPartDocumentCollectionEmpty
        {
            get => _isPartDocumentCollectionEmpty;
            set
            {
                Set(() => IsPartDocumentCollectionEmpty, ref _isPartDocumentCollectionEmpty, value);
            }
        }


        // COMMANDS

        // REFRESH THE CATIA APPLICATION
        private RelayCommand<CatiaPartDocument> _refreshCatiaApplicationCommand;
        public RelayCommand<CatiaPartDocument> RefreshCatiaApplicationCommand { get => _refreshCatiaApplicationCommand; set => _refreshCatiaApplicationCommand = value; }

        private void RefreshCatiaApplicationCommandAction(CatiaPartDocument previousSelectedPartDocument = null)
        {
            CatiaEnv.Initialize();

            if (CatiaEnv.IsApplicationOpen)
            {
                // WE SEND APPLICATION
                MessengerInstance.Send(CatiaEnv, Enum.CatiaToken.Open);

                // CLEAR PARTDOCUMENT COLLECTION
                PartDocumentCollection.Clear();

                // CHECK IF LENGTH UNIT IS MILLIMETER
                if (CatiaEnv.LengthUnit.Length < 6 || (CatiaEnv.LengthUnit.Length > 6 && CatiaEnv.LengthUnit.Substring(0, 6) != "Millim"))
                {
                    MessengerInstance.Send(CatiaEnv.LengthUnit, Enum.CatiaToken.IncorrectLengthUnit);
                }

                foreach (Document document in CatiaEnv.Application.Documents)
                {
                    CatiaGenericDocument tempGenericDocument = new CatiaGenericDocument(CatiaEnv)
                    {
                        Document = document
                    };

                    if (tempGenericDocument.DocumentFormat == CatiaGenericDocument.CatiaDocumentFormat.CATPart)
                    {
                        PartDocumentCollection.Add(new CatiaPartDocument(tempGenericDocument));
                    }
                }

                // IF THERE IS NOT PART DOCUMENT, WE SEND "NULL" TO AVOID WORKING ON PART WHICH DOES NOT EXIST
                if (PartDocumentCollection.Count == 0)
                {
                    MessengerInstance.Send<PartDocument>(null, Enum.InputDataToken.WorkingPartDocumentChanged);
                    IsPartDocumentCollectionEmpty = true;
                }
                else
                {
                    IsPartDocumentCollectionEmpty = false;

                    // IF DOCUMENT PASSED BY PARAMETER IS NON NULL AND IF IT IS FOUND WE SELECT IT
                    if (previousSelectedPartDocument != null && PartDocumentCollection.Contains(previousSelectedPartDocument))
                    {
                        SelectedPartDocument = previousSelectedPartDocument;
                    }
                    else
                    {
                        CatiaGenericDocument activeDocument = new CatiaGenericDocument(CatiaEnv)
                        {
                            Document = CatiaEnv.Application.ActiveDocument
                        };

                        // IF ACTIVE DOCUMENT IS A CATPART, WE SELECT ACTIVE DOCUMENT
                        if(activeDocument.DocumentFormat == CatiaGenericDocument.CatiaDocumentFormat.CATPart)
                        {
                            foreach(CatiaPartDocument partDocument in PartDocumentCollection)
                            {
                                if(partDocument.Name == activeDocument.Name)
                                {
                                    SelectedPartDocument = partDocument;
                                    break;
                                }
                            }
                        }
                        else // BY DEFAULT WE SELECT FIRST DOCUMENT IN THE LIST
                        {
                            SelectedPartDocument = PartDocumentCollection.First();
                        }
                    }
                }
            }
            else
            {
                MessengerInstance.Send(CatiaEnv.ErrorLog, Enum.CatiaToken.Close);
            }
        }


        // OPEN A DOCUMENT
        private RelayCommand _openDocumentCommand;
        public RelayCommand OpenDocumentCommand { get => _openDocumentCommand; set => _openDocumentCommand = value; }

        private void OpenDocumentCommandAction()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false,
                Filter = "CATPart files (*.CATPart)|*.CATPart"
            };

            if ((bool)openFileDialog.ShowDialog())
            {
                SelectedPartDocument = new CatiaPartDocument(CatiaEnv.OpenDocument(new CatiaFile(openFileDialog.FileName)));
            }

            RefreshCatiaApplicationCommandAction();
        }
    }
}