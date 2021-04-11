using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MECMOD;
using HybridShapeTypeLib;
using GalaSoft.MvvmLight;
using Graphy.Enum;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Graphy.Model
{
    public class MarkingData : ObservableObject
    {
        public MarkingData()
        {
            Icon = new Icon();

            TrackingCurveName = DEFAULT_TRACKING_CURVE_NAME;
            ReferencePointName = DEFAULT_REFERENCE_POINT_NAME;
            ProjectionSurfaceName = DEFAULT_PROJECTION_SURFACE_NAME;
            AxisSystemName = DEFAULT_AXIS_SYSTEM_NAME; 
        }

        // DEFAULT VALUES
        public const string DEFAULT_TRACKING_CURVE_NAME = "No curve selected";
        public const string DEFAULT_REFERENCE_POINT_NAME = "No point selected";
        public const string DEFAULT_PROJECTION_SURFACE_NAME = "No surface selected";
        public const string DEFAULT_AXIS_SYSTEM_NAME = "No axis system selected";


        // ATTRIBUTS
        private string _name;
        private bool _isText; // true for text and false for icon.
        private string _text;
        private FontFamily _fontFamily;
        private Icon _icon;
        private double _markingHeight;
        private double _extrusionHeight;
        private string _trackingCurveName;
        private string _referencePointName;
        private string _projectionSurfaceName;
        private string _axisSystemName;
        private bool _isBold;
        private bool _isItalic;
        private bool _isStrikeThrough;
        private bool _isUnderline;
        private HorizontalAlignment _horizontalAlignment;
        private VerticalAlignment _verticalAlignment;
        private int _warningNumber;
        private string _logs;


        public string Name
        {
            get => _name;
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public bool IsText
        {
            get => _isText;
            set
            {
                Set(() => IsText, ref _isText, value);
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                Set(() => Text, ref _text, value);
            }
        }

        public FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                Set(() => FontFamily, ref _fontFamily, value);
            }
        }

        public Icon Icon
        {
            get => _icon;
            set
            {
                Set(() => Icon, ref _icon, value);
            }
        }


        public double MarkingHeight
        {
            get => _markingHeight;
            set
            {
                Set(() => MarkingHeight, ref _markingHeight, value);
            }
        }

        public double ExtrusionHeight
        {
            get => _extrusionHeight;
            set
            {
                Set(() => ExtrusionHeight, ref _extrusionHeight, value);
            }
        }

        public string TrackingCurveName
        {
            get => _trackingCurveName;
            set
            {
                Set(() => TrackingCurveName, ref _trackingCurveName, value);
            }
        }

        public string ReferencePointName
        {
            get => _referencePointName;
            set
            {
                Set(() => ReferencePointName, ref _referencePointName, value);
            }
        }

        public string ProjectionSurfaceName
        {
            get => _projectionSurfaceName;
            set
            {
                Set(() => ProjectionSurfaceName, ref _projectionSurfaceName, value);
            }
        }

        public string AxisSystemName
        {
            get => _axisSystemName;
            set
            {
                Set(() => AxisSystemName, ref _axisSystemName, value);
            }
        }

        public bool IsBold
        {
            get => _isBold;
            set
            {
                Set(() => IsBold, ref _isBold, value);
            }
        }

        public bool IsItalic
        {
            get => _isItalic;
            set
            {
                Set(() => IsItalic, ref _isItalic, value);
            }
        }

        public bool IsStrikeThrough
        {
            get => _isStrikeThrough;
            set
            {
                Set(() => IsStrikeThrough, ref _isStrikeThrough, value);
            }
        }

        public bool IsUnderline
        {
            get => _isUnderline;
            set
            {
                Set(() => IsUnderline, ref _isUnderline, value);
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                Set(() => HorizontalAlignment, ref _horizontalAlignment, value);
            } 
        }

        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                Set(() => VerticalAlignment, ref _verticalAlignment, value);
            }
        }


        public int WarningNumber
        {
            get => _warningNumber;
            set
            {
                Set(() => WarningNumber, ref _warningNumber, value);
            }
        }

        public string Logs
        {
            get => _logs;
            set
            {
                Set(() => Logs, ref _logs, value);
            }
        }


        public static MarkingData Default()
        {
            MarkingData defaultMarkingData = new MarkingData()
            {
                Name = "DefaultMarking",
                IsText = true,
                Text = "Hello World !",
                IsBold = false,
                IsItalic = false,
                IsUnderline = false,
                IsStrikeThrough = false,
                FontFamily = new FontFamily("Calibri"),
                Icon = Icon.Default(),
                MarkingHeight = 2d,
                ExtrusionHeight = 0.1,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                ProjectionSurfaceName = "Surface.1",
                TrackingCurveName = "Curve.1",
                ReferencePointName = "Point.1",
                AxisSystemName = "Axis System.1"
            };

            return defaultMarkingData;
        }


        public static MarkingData NoMarkingData()
        {
            MarkingData noMarkingData = new MarkingData()
            {
                Name = "NoMarking",
                IsText = true,
                Text = "",
                IsBold = false,
                IsItalic = false,
                IsUnderline = false,
                IsStrikeThrough = false,
                FontFamily = new FontFamily("Calibri"),
                Icon = new Icon(),
                MarkingHeight = 2d,
                ExtrusionHeight = 0.1,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            return noMarkingData;
        }


    }
}
