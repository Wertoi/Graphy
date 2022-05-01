using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graphy.CustomControl

{
    /// <summary>
    /// Suivez les étapes 1a ou 1b puis 2 pour utiliser ce contrôle personnalisé dans un fichier XAML.
    ///
    /// Étape 1a) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans le projet actif.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:CADSketcher"
    ///
    ///
    /// Étape 1b) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans un autre projet.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:CADSketcher;assembly=CADSketcher"
    ///
    /// Vous devrez également ajouter une référence du projet contenant le fichier XAML
    /// à ce projet et régénérer pour éviter des erreurs de compilation :
    ///
    ///     Cliquez avec le bouton droit sur le projet cible dans l'Explorateur de solutions, puis sur
    ///     "Ajouter une référence"->"Projets"->[Recherchez et sélectionnez ce projet]
    ///
    ///
    /// Étape 2)
    /// Utilisez à présent votre contrôle dans le fichier XAML.
    ///
    ///     <MyNamespace:CartesianCanvas/>
    ///
    /// </summary>
    public class CartesianCanvas : Canvas
    {
        static CartesianCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CartesianCanvas), new FrameworkPropertyMetadata(typeof(CartesianCanvas)));
        }

        public CartesianCanvas() : base()
        {
            LayoutUpdated += CartesianCanvas_LayoutUpdated;

            _transformGroup = new TransformGroup();
            _translateTransform = new TranslateTransform();
            _scaleTransform = new ScaleTransform();

            _transformGroup.Children.Add(_translateTransform);
            _transformGroup.Children.Add(_scaleTransform);

            

            UpdateScaleTransform();
        }


        // PRIVATE ATTRIBUTS
        private System.Windows.Point _startPoint;
        private double _xDisplacement;
        private double _yDisplacement;
        private TransformGroup _transformGroup;
        private TranslateTransform _translateTransform;
        private ScaleTransform _scaleTransform;


        // PROPERTIES

        public double WheelScaleRatioCoef
        {
            get { return (double)GetValue(WheelScaleRatioCoefProperty); }
            set { SetValue(WheelScaleRatioCoefProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WheelScaleRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WheelScaleRatioCoefProperty =
            DependencyProperty.Register("WheelScaleRatioCoef", typeof(double), typeof(CartesianCanvas), new PropertyMetadata(0.02));



        public double ScaleRatio
        {
            get { return (double)GetValue(ScaleRatioProperty); }
            set { SetValue(ScaleRatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScaleRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleRatioProperty =
            DependencyProperty.Register("ScaleRatio", typeof(double), typeof(CartesianCanvas), new PropertyMetadata(1d));



        public bool IsEditable
        {
            get { return (bool)GetValue(CanMoveProperty); }
            set { SetValue(CanMoveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanMove.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanMoveProperty =
            DependencyProperty.Register("CanMove", typeof(bool), typeof(CartesianCanvas), new PropertyMetadata(true));






        public bool MirrorXAxis
        {
            get { return (bool)GetValue(MirrorXAxisProperty); }
            set { SetValue(MirrorXAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MirrorXAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MirrorXAxisProperty =
            DependencyProperty.Register("MirrorXAxis", typeof(bool), typeof(CartesianCanvas), new PropertyMetadata(false));



        public bool MirrorYAxis
        {
            get { return (bool)GetValue(MirrorYAxisProperty); }
            set { SetValue(MirrorYAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MirrorYAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MirrorYAxisProperty =
            DependencyProperty.Register("MirrorYAxis", typeof(bool), typeof(CartesianCanvas), new PropertyMetadata(false));






        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            _startPoint = e.GetPosition(this);
        }


        private void CartesianCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if(IsEditable)
            {

                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    System.Windows.Point currentPoint = e.GetPosition(this);
                    _translateTransform.X += (currentPoint.X - _startPoint.X) / ScaleRatio;
                    _translateTransform.Y += (currentPoint.Y - _startPoint.Y) / ScaleRatio;

                    _startPoint = currentPoint;
                }
            }
        }


        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded != null)
            {
                ((UIElement)visualAdded).RenderTransform = _transformGroup;

                if(visualAdded.GetType() == typeof(Path))
                    ((Path)visualAdded).SizeChanged += CartesianCanvasChildren_SizeChanged;
            }
        }

        private void CartesianCanvasChildren_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PathGeometry geometry = new PathGeometry();
            foreach (UIElement child in Children)
            {
                if (child.GetType() == typeof(Path) && ((Path)child).Data != null)
                {
                    geometry.AddGeometry(((Path)child).Data);
                }
            }

            if (!geometry.Bounds.IsEmpty)
                FitAllIn(geometry);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            _xDisplacement = this.ActualWidth / 2 - _translateTransform.X;
            _yDisplacement = this.ActualHeight / 2 - _translateTransform.Y;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if(IsEditable)
            {
                if (e.Delta > 0)
                    ZoomIn(WheelScaleRatioCoef);
                else
                    ZoomOut(WheelScaleRatioCoef);
            }
        }


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            _scaleTransform.CenterX = this.ActualWidth / 2;
            _scaleTransform.CenterY = this.ActualHeight / 2;

            _translateTransform.X = this.ActualWidth / 2 - _xDisplacement;
            _translateTransform.Y = this.ActualHeight / 2 - _yDisplacement;
        }

        public void ZoomIn(double scaleRatioCoef)
        {
            ScaleRatio += scaleRatioCoef * ScaleRatio;

            UpdateScaleTransform();
        }

        public void ZoomOut(double scaleRatioCoef)
        {
            ScaleRatio -= scaleRatioCoef * ScaleRatio;

            UpdateScaleTransform();
        }

        public void FitAllIn(Geometry geometry)
        {
            _translateTransform.X = this.ActualWidth / 2 - (geometry.Bounds.Left + geometry.Bounds.Width / 2);
            _translateTransform.Y = this.ActualHeight / 2 - (geometry.Bounds.Top + geometry.Bounds.Height / 2);

            _xDisplacement = this.ActualWidth / 2 - _translateTransform.X;
            _yDisplacement = this.ActualHeight / 2 - _translateTransform.Y;

            double scaleWidth = this.ActualWidth / geometry.Bounds.Width;
            double scaleHeight = this.ActualHeight / geometry.Bounds.Height;

            ScaleRatio = Math.Min(scaleWidth, scaleHeight) * 0.7;

            UpdateScaleTransform();
        }

        private void UpdateScaleTransform()
        {
            _scaleTransform.ScaleX = MirrorXAxis ? -ScaleRatio: ScaleRatio;
            _scaleTransform.ScaleY = MirrorYAxis ? -ScaleRatio : ScaleRatio;

            foreach (UIElement child in Children)
            {
                if (child.GetType() == typeof(Path))
                    ((Path)child).StrokeThickness = 1d / ScaleRatio;
            }
        }
    }
}
