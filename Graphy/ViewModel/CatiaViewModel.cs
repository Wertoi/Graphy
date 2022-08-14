using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Graphy.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using INFITF;
using MECMOD;
using System;
using System.Threading.Tasks;
using Graphy.Model.CatiaObject;

namespace Graphy.ViewModel
{
    public class CatiaViewModel : ViewModelBase
    {
        // CONSTRUCTOR
        public CatiaViewModel()
        {
            CatiaEnv = new CatiaEnv();
            PartCollection = new ObservableCollection<CatiaPart>();

            // MESSENGER REGISTRATION
            MessengerInstance.Register<CatiaPart>(this, Enum.CatiaToken.Refresh, (doc) => RefreshCatiaApplicationCommandAction(doc));

            // COMMANDS INITIALIZATION
            RefreshCatiaApplicationCommand = new RelayCommand<CatiaPart>((doc) => RefreshCatiaApplicationCommandAction(doc));
            OpenDocumentCommand = new RelayCommand(OpenDocumentCommandAction);

            RefreshCatiaApplicationCommandAction();
        }


        // ATTRIBUTS
        private CatiaEnv _catiaEnv;
        private CatiaPart _selectedPart;
        private ObservableCollection<CatiaPart> _partCollection;
        private DateTime _lastRefreshTime;

        public CatiaEnv CatiaEnv
        {
            get => _catiaEnv;
            set
            {
                Set(() => CatiaEnv, ref _catiaEnv, value);
            }
        }

        public CatiaPart SelectedPart
        {
            get => _selectedPart;
            set
            {
                Set(() => SelectedPart, ref _selectedPart, value);

                if (SelectedPart != null)
                {
                    // We send the selected part document
                    MessengerInstance.Send<CatiaPart>(SelectedPart, Enum.CatiaToken.SelectedPartChanged);

                    SelectedPart.PartDocument.Activate();
                }
            }
        }

        public ObservableCollection<CatiaPart> PartCollection
        {
            get => _partCollection;
            set
            {
                Set(() => PartCollection, ref _partCollection, value);
            }
        }


        public DateTime LastRefreshTime
        {
            get => _lastRefreshTime;
            set
            {
                Set(() => LastRefreshTime, ref _lastRefreshTime, value);
            }
        }


        // COMMANDS

        // REFRESH THE CATIA APPLICATION
        private RelayCommand<CatiaPart> _refreshCatiaApplicationCommand;
        public RelayCommand<CatiaPart> RefreshCatiaApplicationCommand { get => _refreshCatiaApplicationCommand; set => _refreshCatiaApplicationCommand = value; }

        private void RefreshCatiaApplicationCommandAction(CatiaPart previousSelectedPartDocument = null)
        {
            CatiaEnv.Initialize();
            MessengerInstance.Send<CatiaEnv>(CatiaEnv, Enum.CatiaToken.CatieEnvChanged);

            LastRefreshTime = DateTime.Now;

            // CLEAR PARTDOCUMENT COLLECTION
            PartCollection.Clear();

            if (CatiaEnv.IsApplicationOpen)
            {
                // CHECK ALL OPEN DOCUMENTS TO STORE ONLY CATPARTs
                foreach (Document document in CatiaEnv.Application.Documents)
                {
                    CatiaPart catiaPart = new CatiaPart();
                    if (catiaPart.TryAssignDocument(CatiaEnv, document))
                        PartCollection.Add(catiaPart);
                }

                // IF THERE IS NOT PART DOCUMENT, WE SEND "NULL" TO AVOID WORKING ON PART WHICH DOES NOT EXIST
                if (PartCollection.Count == 0)
                {
                    MessengerInstance.Send<CatiaPart>(null, Enum.CatiaToken.SelectedPartChanged);
                }
                else
                {
                    // IF DOCUMENT PASSED BY PARAMETER IS NON NULL AND IF IT IS FOUND WE SELECT IT
                    if (previousSelectedPartDocument != null && PartCollection.Contains(previousSelectedPartDocument))
                    {
                        SelectedPart = previousSelectedPartDocument;
                    }
                    else
                    {
                        // IF ACTIVE DOCUMENT IS A CATPART, WE SELECT ACTIVE DOCUMENT
                        CatiaPart activePart = new CatiaPart();
                        if(activePart.TryAssignDocument(CatiaEnv, CatiaEnv.Application.ActiveDocument))
                        {
                            foreach(CatiaPart part in PartCollection)
                            {
                                if(part.Name == activePart.Name)
                                {
                                    SelectedPart = part;
                                    break;
                                }
                            }
                        }
                        else // BY DEFAULT WE SELECT FIRST DOCUMENT IN THE LIST
                        {
                            SelectedPart = PartCollection.First();
                        }
                    }
                }
            }
            else
            {
                MessengerInstance.Send<string>(CatiaEnv.ErrorLog, Enum.ProcessToken.Failed);
            }
        }


        // OPEN A DOCUMENT
        private RelayCommand _openDocumentCommand;
        public RelayCommand OpenDocumentCommand { get => _openDocumentCommand; set => _openDocumentCommand = value; }

        private async void OpenDocumentCommandAction()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false,
                Filter = "CATPart files (*.CATPart)|*.CATPart"
            };

            if ((bool)openFileDialog.ShowDialog())
            {
                // Start the process
                MessengerInstance.Send<object>(null, Enum.ProcessToken.SimpleStarted);

                await Task.Run(() =>
                {
                    try
                    {
                        // Open the document
                        CatiaPart newPart = new CatiaPart();
                        if (newPart.TryOpenDocument(CatiaEnv, openFileDialog.FileName))
                            SelectedPart = newPart;

                        // Finish the process
                        MessengerInstance.Send<string>("CATPart " + newPart + "opening completed !", Enum.ProcessToken.Finished);
                    }
                    catch (Exception ex)
                    {
                        MessengerInstance.Send(ex.Message, Enum.ProcessToken.Failed);
                    }
                });
            }

            RefreshCatiaApplicationCommandAction();
        }
    }
}
