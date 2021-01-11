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
            SimpleIoc.Default.Register<SimpleMarkingViewModel>(true);
            SimpleIoc.Default.Register<ComplexMarkingViewModel>(true);
            SimpleIoc.Default.Register<IconViewModel>(true);
            SimpleIoc.Default.Register<SettingViewModel>(true);
            SimpleIoc.Default.Register<CatiaViewModel>(true);
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

        public SettingViewModel SettingViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingViewModel>();
            }
        }


        public IconViewModel IconViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IconViewModel>();
            }
        }

        public SimpleMarkingViewModel SimpleMarkingViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SimpleMarkingViewModel>();
            }
        }

        public ComplexMarkingViewModel ComplexMarkingViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ComplexMarkingViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
