using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HybridShapeTypeLib;
using MECMOD;

namespace Graphy.Model.CatiaShape
{
    public class CatiaCharacter : CatiaGenericShape
    {
        public CatiaCharacter(PartDocument partDocument, char value) : base(partDocument)
        {
            Value = value;
            SurfaceList = new List<CatiaSurface>();
        }

        private char _value;
        private List<CatiaSurface> _surfaceList;
        private PathGeometry _pathGeometry;
        private bool _isSpaceCharacter = false;


        public List<CatiaSurface> SurfaceList { get => _surfaceList; set => _surfaceList = value; }

        public PathGeometry PathGeometry
        {
            get => _pathGeometry;
            set
            {
                _pathGeometry = value;
            }
        }

        public bool IsSpaceCharacter { get => _isSpaceCharacter; set => _isSpaceCharacter = value; }
        public char Value
        {
            get => _value;
            set
            {
                _value = value;

                if(Value == ' ')
                {
                    Value = '_';
                    IsSpaceCharacter = true;
                }
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

        public void DrawCharacter(HybridBody set)
        {
            for(int i = 0; i < SurfaceList.Count; i++)
            {
                SurfaceList[i].ExternalContour.DrawContour(set);
                SurfaceList[i].ExternalContour.SmoothedShape.set_Name("Ext." + (i+1).ToString());

                for (int j = 0; j < SurfaceList[i].InternalContourList.Count; j++)
                {
                    SurfaceList[i].InternalContourList[j].DrawContour(set);
                    SurfaceList[i].InternalContourList[j].SmoothedShape.set_Name("Ext." + (i+1).ToString() + "-Int." + (j+1).ToString());
                }
            }
        }

        public void AssembleSurfaces()
        {
            if (SurfaceList.Count > 1)
            {
                HybridShapeAssemble surfaceAssy = HybridShapeFactory.AddNewJoin(SurfaceList.First().ShapeReference, SurfaceList[1].ShapeReference);
                surfaceAssy.SetConnex(false);

                foreach (CatiaSurface surface in SurfaceList)
                {
                    surfaceAssy.AddElement(surface.ShapeReference);
                }

                surfaceAssy.Compute();

                Shape = surfaceAssy;
            }
            else
            {
                Shape = SurfaceList.First().Shape;
            }
        }

        public CatiaCharacter Clone()
        {
            CatiaCharacter copyCharacter = new CatiaCharacter(PartDocument, Value);
            copyCharacter.PathGeometry = PathGeometry.Clone();
            copyCharacter.IsSpaceCharacter = IsSpaceCharacter;
            foreach(CatiaSurface surface in SurfaceList)
            {
                copyCharacter.SurfaceList.Add(surface.Copy());
            }

            return copyCharacter;
        }



        // EQUALS OVERRIDE
        public override bool Equals(object obj)
        {
            // Is null?
            if (Object.ReferenceEquals(null, obj))
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

            CatiaCharacter catiaCharacter = (CatiaCharacter)obj;
            return (Value == catiaCharacter.Value && IsSpaceCharacter == catiaCharacter.IsSpaceCharacter);
        }

        // GETHASHCODE OVERRIDE
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
