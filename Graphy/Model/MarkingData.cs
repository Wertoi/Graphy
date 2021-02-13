using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MECMOD;
using HybridShapeTypeLib;
using GalaSoft.MvvmLight;

namespace Graphy.Model
{
    public class MarkingData : ObservableObject
    {
        public MarkingData()
        {
            TrackingCurveName = DEFAULT_TRACKING_CURVE_NAME;
            StartPointName = DEFAULT_START_POINT_NAME;
            ProjectionSurfaceName = DEFAULT_PROJECTION_SURFACE_NAME;
            AxisSystemName = DEFAULT_AXIS_SYSTEM_NAME;
        }

        public MarkingData(string text, double charactereHeight, double extrusionHeight)
        {
            Text = text;
            MarkingHeight = charactereHeight;
            ExtrusionHeight = extrusionHeight;

            TrackingCurveName = DEFAULT_TRACKING_CURVE_NAME;
            StartPointName = DEFAULT_START_POINT_NAME;
            ProjectionSurfaceName = DEFAULT_PROJECTION_SURFACE_NAME;
            AxisSystemName = DEFAULT_AXIS_SYSTEM_NAME;
        }

        public const string DEFAULT_TRACKING_CURVE_NAME = "No curve selected";
        public const string DEFAULT_START_POINT_NAME = "No point selected";
        public const string DEFAULT_PROJECTION_SURFACE_NAME = "No surface selected";
        public const string DEFAULT_AXIS_SYSTEM_NAME = "No axis system selected";

        private string _name;
        private bool _isText = true; // true for text and false for icon.
        private string _text;
        private FontFamily _fontFamily;
        private Icon _icon;
        private double _markingHeight;
        private double _extrusionHeight;
        private string _trackingCurveName;
        private string _startPointName;
        private string _projectionSurfaceName;
        private string _axisSystemName;
        private bool _isBold = false;
        private bool _isItalic = false;
        private bool _isStrikeThrough = false;
        private bool _isUnderline = false;


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

        public string StartPointName
        {
            get => _startPointName;
            set
            {
                Set(() => StartPointName, ref _startPointName, value);
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
    }
}
