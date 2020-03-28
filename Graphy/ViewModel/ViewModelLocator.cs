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
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Pour info : "true" permet de contrôler l'ordre d'initialisation des view models
            SimpleIoc.Default.Register<ProcessViewModel>(true);
            SimpleIoc.Default.Register<InputDataViewModel>(true);
            SimpleIoc.Default.Register<FontViewModel>(true);
            SimpleIoc.Default.Register<SettingViewModel>(true);
            SimpleIoc.Default.Register<CatiaViewModel>(true);
            SimpleIoc.Default.Register<DesignTableViewModel>(true);
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

        public ProcessViewModel StatusViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProcessViewModel>();
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
