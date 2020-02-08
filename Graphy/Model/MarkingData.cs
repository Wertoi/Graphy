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
        public MarkingData(string textValue, double charactereHeightValue, double extrusionHeightValue)
        {
            Font = new SelectableFont(new System.Windows.Media.FontFamily("Arial"));
            Text = new LinkableData<string>()
            {
                Value = textValue
            };

            CharacterHeight = new LinkableData<double>()
            {
                Value = charactereHeightValue
            };

            ExtrusionHeight = new LinkableData<double>()
            {
                Value = extrusionHeightValue
            };
        }

        public const string DEFAULT_TRACKING_CURVE_NAME = "No curve selected";
        public const string DEFAULT_START_POINT_NAME = "No point selected";
        public const string DEFAULT_PROJECTION_SURFACE_NAME = "No surface selected";
        public const string DEFAULT_AXIS_SYSTEM_NAME = "No axis system selected";

        private string _name;
        private LinkableData<string> _text;
        private SelectableFont _font;
        private LinkableData<double> _characterHeight;
        private LinkableData<double> _extrusionHeight;
        private string _trackingCurveName = "No curve selected";
        private string _startPointName = "No point selected";
        private string _projectionSurfaceName = "No surface selected";
        private string _axisSystemName = "No axis system selected";

        public string Name
        {
            get => _name;
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public LinkableData<string> Text
        {
            get => _text;
            set
            {
                Set(() => Text, ref _text, value);
            }
        }

        public SelectableFont Font
        {
            get => _font;
            set
            {
                Set(() => Font, ref _font, value);
            }
        }

        public LinkableData<double> CharacterHeight
        {
            get => _characterHeight;
            set
            {
                Set(() => CharacterHeight, ref _characterHeight, value);
            }
        }

        public LinkableData<double> ExtrusionHeight
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
    }
}
