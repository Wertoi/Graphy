//  --------------------------------
//  Copyright (c) Huy Pham. All rights reserved.
//  This source code is made available under the terms of the Microsoft Public License (Ms-PL)
//  http://www.opensource.org/licenses/ms-pl.html
//  ---------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Threading;

namespace Graphy.CustomControl
{
    /// <summary>
    /// Interaction logic for CircularProgressBar.xaml
    /// </summary>
    public partial class LoadingBox
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingBox"/> class.
        /// </summary>
        public LoadingBox()
        {
            InitializeComponent();

            IsVisibleChanged += OnVisibleChanged;

            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 0, 0, RotationSpeed)
            };
        }

        private readonly DispatcherTimer _animationTimer;


        public Brush LoadingColor
        {
            get { return (SolidColorBrush)GetValue(LoadingColorProperty); }
            set { SetValue(LoadingColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoadingColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadingColorProperty =
            DependencyProperty.Register("LoadingColor", typeof(Brush), typeof(LoadingBox), new PropertyMetadata( new SolidColorBrush(Color.FromRgb(0,0,0)) ));


        /// <summary>
        /// Define the time Span of the timer in milliseconds.
        /// </summary>
        public int RotationSpeed
        {
            get { return (int)GetValue(RotationSpeedProperty); }
            set { SetValue(RotationSpeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RotationSpeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RotationSpeedProperty =
            DependencyProperty.Register("RotationSpeed", typeof(int), typeof(LoadingBox), new PropertyMetadata(75, OnRotationSpeedPropertyChanged));

        private static void OnRotationSpeedPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((LoadingBox)source)._animationTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)e.NewValue);
        }



        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="ellipse">The ellipse.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="posOffSet">The pos off set.</param>
        /// <param name="step">The step to change.</param>
        private static void SetPosition(DependencyObject ellipse, double offset, double posOffSet, double step)
        {
            ellipse.SetValue(Canvas.LeftProperty, 50 + (Math.Sin(offset + (posOffSet * step)) * 50));
            ellipse.SetValue(Canvas.TopProperty, 50 + (Math.Cos(offset + (posOffSet * step)) * 50));
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start()
        {
            _animationTimer.Tick += OnAnimationTick;
            _animationTimer.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        private void Stop()
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= OnAnimationTick;
        }

        /// <summary>
        /// Handles the animation tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAnimationTick(object sender, EventArgs e)
        {
            _spinnerRotate.Angle = (_spinnerRotate.Angle + 36) % 360;
        }

        /// <summary>
        /// Handles the loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCanvasLoaded(object sender, RoutedEventArgs e)
        {
            const double offset = Math.PI;
            const double step = Math.PI * 2 / 10.0;

            SetPosition(_circle0, offset, 0.0, step);
            SetPosition(_circle1, offset, 1.0, step);
            SetPosition(_circle2, offset, 2.0, step);
            SetPosition(_circle3, offset, 3.0, step);
            SetPosition(_circle4, offset, 4.0, step);
            SetPosition(_circle5, offset, 5.0, step);
            SetPosition(_circle6, offset, 6.0, step);
            SetPosition(_circle7, offset, 7.0, step);
            SetPosition(_circle8, offset, 8.0, step);
        }

        /// <summary>
        /// Handles the unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCanvasUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// Handles the visible changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;

            if (isVisible && !System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
    }
}