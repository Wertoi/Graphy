using HybridShapeTypeLib;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Graphy.Enum;

namespace Graphy.Model.CatiaObject.CatiaShape
{

    public class CatiaDrawableShape : CatiaGenericShape
    {
        public CatiaDrawableShape(PartDocument partDocument) : base(partDocument)
        {
            SurfaceList = new List<CatiaSurface>();
        }

        private List<CatiaSurface> _surfaceList;
        private PathGeometry _pathGeometry;


        public List<CatiaSurface> SurfaceList { get => _surfaceList; set => _surfaceList = value; }

        public PathGeometry PathGeometry
        {
            get => _pathGeometry;
            set
            {
                _pathGeometry = value;
            }
        }


        public void FillSurfaceList()
        {
            SurfaceList.Clear();

            // Compute the XY plane reference
            INFITF.Reference planeXYReference = PartDocument.Part.CreateReferenceFromObject(PartDocument.Part.OriginElements.PlaneXY);

            // Separate the pathGeometry in a list of contour PathGeometry
            List<PathGeometry> pathGeometryContourList = new List<PathGeometry>();
            foreach (PathFigure figure in PathGeometry.Figures)
            {
                pathGeometryContourList.Add(new PathGeometry(new List<PathFigure>() { figure }));
            }

            // Sort the list by area
            pathGeometryContourList.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            // FILL THE CATIA SURFACE LIST
            // Compute to know if the contour is an internal contour
            int smallestParentContourIndex = -1;
            for (int i = 0; i < pathGeometryContourList.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    IntersectionDetail intersectionDetail = pathGeometryContourList[i].FillContainsWithDetail(pathGeometryContourList[j]);

                    if (intersectionDetail == IntersectionDetail.FullyInside)
                    {
                        smallestParentContourIndex = j;
                    }
                }

                // If smallestParentContourIndex is not modified, we have an external contour
                if (smallestParentContourIndex == -1)
                {
                    SurfaceList.Add(new CatiaSurface(PartDocument)
                    {
                        ExternalContour = new CatiaContour(PartDocument, planeXYReference)
                        {
                            PathGeometry = pathGeometryContourList[i]
                        }
                    });
                }
                else
                {
                    // Get the parent contour.
                    CatiaContour parentContour = new CatiaContour(PartDocument, planeXYReference)
                    {
                        PathGeometry = pathGeometryContourList[smallestParentContourIndex]
                    };

                    // Try to find the parent contour in the list of surfaces' external contour.
                    CatiaSurface tempSurface = SurfaceList.Find((surface) => surface.ExternalContour.PathGeometry.ToString() == parentContour.PathGeometry.ToString());

                    // If the parent contour has been found, i-contour is internal.
                    if (tempSurface != null)
                    {
                        tempSurface.InternalContourList.Add(new CatiaContour(PartDocument, planeXYReference)
                        {
                            PathGeometry = pathGeometryContourList[i]
                        });
                    }

                    // If the parent contour is not found, i-contour is the external contour of a new surface.
                    else
                    {
                        SurfaceList.Add(new CatiaSurface(PartDocument)
                        {
                            ExternalContour = new CatiaContour(PartDocument, planeXYReference)
                            {
                                PathGeometry = pathGeometryContourList[i]
                            }
                        });
                    }
                }
            }
        }

        public void Draw()
        {
            double xCorrectif = PathGeometry.Bounds.Left;

            for (int i = 0; i < SurfaceList.Count; i++)
            {
                SurfaceList[i].ExternalContour.DrawContour(xCorrectif);

                for (int j = 0; j < SurfaceList[i].InternalContourList.Count; j++)
                {
                    SurfaceList[i].InternalContourList[j].DrawContour(xCorrectif);
                }
            }
        }

        public static double GetYOffset(VerticalAlignment verticalAlignment, PathGeometry refGeometry)
        {
            switch (verticalAlignment)
            {
                case VerticalAlignment.Top:
                    return refGeometry.Bounds.Top;

                case VerticalAlignment.Center:
                    return (refGeometry.Bounds.Top + refGeometry.Bounds.Bottom) / 2;

                case VerticalAlignment.Bottom:
                    return refGeometry.Bounds.Bottom;

                default:
                    return refGeometry.Bounds.Bottom;
            }
        }


        public CatiaSurface GetAssembleSurfaces()
        {
            CatiaSurface assembledSurface = new CatiaSurface(PartDocument);

            if (SurfaceList.Count > 1)
            {
                HybridShapeAssemble surfaceAssy = HybridShapeFactory.AddNewJoin(SurfaceList.First().ShapeReference, SurfaceList[1].ShapeReference);
                surfaceAssy.SetConnex(false);

                foreach (CatiaSurface surface in SurfaceList)
                {
                    surfaceAssy.AddElement(surface.ShapeReference);
                }

                surfaceAssy.Compute();

                assembledSurface.Shape = surfaceAssy;
            }
            else
            {
                assembledSurface.Shape = SurfaceList.First().Shape;
            }

            return assembledSurface;
        }

        public CatiaDrawableShape Clone()
        {
            CatiaDrawableShape catiaDrawableShape = new CatiaDrawableShape(PartDocument)
            {
                PathGeometry = PathGeometry.Clone()
            };

            foreach (CatiaSurface surface in SurfaceList)
            {
                catiaDrawableShape.SurfaceList.Add(surface.Copy());
            }

            return catiaDrawableShape;
        }



        // EQUALS OVERRIDE
        public override bool Equals(object obj)
        {
            // Is null?
            if (obj is null)
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            // Is the same type?
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            CatiaDrawableShape catiaDrawableShape = (CatiaDrawableShape)obj;
            return (PathGeometry == catiaDrawableShape.PathGeometry);
        }

        // GETHASHCODE OVERRIDE
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
