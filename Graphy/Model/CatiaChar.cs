using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INFITF;
using MECMOD;
using HybridShapeTypeLib;
using GalaSoft.MvvmLight;

namespace Graphy.Model
{
    public class CatiaChar : ObservableObject
    {
        // CONSTRUCTOR
        public CatiaChar(char value)
        {
            Value = value;
            OriginalContourShapeList = new List<HybridShape>();
            SurfaceList = new List<Surface>();
        }


        // ATTRIBUTS
        private char _value;
        private double _defaultHeight;
        private double _defaultWidth;
        private double _scaleRatio;
        private double _scaledHeight;
        private double _scaleWidth;
        private List<HybridShape> _originalContourShapeList;
        private List<Surface> _surfaceList;
        private Reference _surfaceListAssembledRef;


        public char Value
        {
            get => _value;
            set
            {
                Set(() => Value, ref _value, value);
            }
        }


        public double DefaultHeight
        {
            get => _defaultHeight;
            set
            {
                Set(() => DefaultHeight, ref _defaultHeight, value);
            }
        }

        public double DefaultWidth
        {
            get => _defaultWidth;
            set
            {
                Set(() => DefaultWidth, ref _defaultWidth, value);
            }
        }

        public double ScaleRatio
        {
            get => _scaleRatio;
            set
            {
                Set(() => ScaleRatio, ref _scaleRatio, value);

                ScaledHeight = DefaultHeight * ScaleRatio;
                ScaleWidth = DefaultWidth * ScaleRatio;
            }
        }

        public double ScaledHeight
        {
            get => _scaledHeight;
            set
            {
                Set(() => ScaledHeight, ref _scaledHeight, value);
            }
        }

        public double ScaleWidth
        {
            get => _scaleWidth;
            set
            {
                Set(() => ScaleWidth, ref _scaleWidth, value);
            }
        }

        public List<Surface> SurfaceList
        {
            get => _surfaceList;
            set
            {
                Set(() => SurfaceList, ref _surfaceList, value);
            }
        }
        public Reference SurfaceListAssembledRef
        {
            get => _surfaceListAssembledRef;
            set
            {
                Set(() => SurfaceListAssembledRef, ref _surfaceListAssembledRef, value);
            }
        }
        public List<HybridShape> OriginalContourShapeList
        {
            get => _originalContourShapeList;
            set
            {
                Set(() => OriginalContourShapeList, ref _originalContourShapeList, value);
            }
        }


        // METHODS

        public void LoadSurfaceListFromSet(HybridBody set, Contour.ContourStatus contourStatus)
        {
            SurfaceList.Clear();

            foreach (HybridShape shape in set.HybridShapes)
            {
                // Reminder, shape name format : "Ext.#" for EXT contour and "Ext.#-Int.#" for INT contour.
                string[] shapeSplitName = shape.get_Name().Split('-');

                // If the name cannot be splitted by '-', it means we have an EXT contour.
                if (shapeSplitName.Count() == 1)
                {
                    SurfaceList.Add(new Surface()
                    {
                        ExtContour = new Contour(shape, contourStatus)
                    });
                }
                else // Otherwise it's an INT contour
                {
                    foreach (Surface surface in SurfaceList)
                    {
                        if (surface.ExtContour.Name(contourStatus) == shapeSplitName.First())
                        {
                            surface.IntContourList.Add(new Contour(shape, contourStatus));
                            surface.IsIntContourListEmpty = false;
                            break;
                        }
                    }
                }
            }
        }
    }

    public class Surface
    {
        public Surface()
        {
            IntContourList = new List<Contour>();
        }

        private Contour _extContour;
        private List<Contour> _intContourList;
        private bool _isIntContourListEmpty;
        private Reference _intContourListAssembledRef;
        private Reference _surfaceRef;

        public Contour ExtContour { get => _extContour; set => _extContour = value; }
        public List<Contour> IntContourList
        {
            get => _intContourList;
            set
            {
                _intContourList = value;

                if (IntContourList.Count > 0)
                    IsIntContourListEmpty = false;
                else
                    IsIntContourListEmpty = true;
            }
        }

        public bool IsIntContourListEmpty { get => _isIntContourListEmpty; set => _isIntContourListEmpty = value; }
        public Reference IntContourListAssembledRef { get => _intContourListAssembledRef; set => _intContourListAssembledRef = value; }
        public Reference SurfaceRef { get => _surfaceRef; set => _surfaceRef = value; }
    }
}
