using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using System;

namespace Graphy.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            // To remove
            //System.Windows.MessageBox.Show("Initialisation ViewModelLocator.");
            try
            {
                ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

                // Pour info : "true" permet de contrôler l'ordre d'initialisation des view models
                SimpleIoc.Default.Register<StatusViewModel>(true);
                SimpleIoc.Default.Register<InputDataViewModel>(true);
                SimpleIoc.Default.Register<FontViewModel>(true);
                SimpleIoc.Default.Register<SettingViewModel>(true);
                SimpleIoc.Default.Register<CatiaViewModel>(true);
                SimpleIoc.Default.Register<DesignTableViewModel>(true);       
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("ViewModelLocator :\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.Source + "\r\n\r\n" + ex.StackTrace + "\r\n\r\n" + ex.TargetSite);
            }
            

            // To remove
            //System.Windows.MessageBox.Show("Fin initialisation ViewModelLocator.");
        }

        public FontViewModel FontViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FontViewModel>();
            }
        }

        public CatiaViewModel CatiaViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CatiaViewModel>();
            }
        }

        public StatusViewModel StatusViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StatusViewModel>();
            }
        }

        public InputDataViewModel InputDataViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<InputDataViewModel>();
            }
        }

        public SettingViewModel SettingViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingViewModel>();
            }
        }

        public DesignTableViewModel DesignTableViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DesignTableViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
