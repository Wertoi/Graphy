using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INFITF;
using MECMOD;
using PARTITF;
using HybridShapeTypeLib;
using Graphy.Model;
using System.Globalization;
using Graphy.Model.CatiaShape;
using Graphy.Model.CatiaDocument;

namespace Graphy.Model.Generator
{
    public class MarkingGenerator : IGenerator
    {
        public event EventHandler<ProgressRateChangedEventArgs> ProgressRateChanged;
        public event EventHandler<ProgressRateChangedEventArgs> StepProgressRateChanged;

        public MarkingGenerator()
        {

        }

        private double _progressRate;
        private int _stepProgressRate;

        public double ProgressRate
        {
            get => _progressRate;
            set
            {
                if (value != ProgressRate)
                {
                    _progressRate = value;
                    ProgressRateChanged?.Invoke(this, new ProgressRateChangedEventArgs(ProgressRate));
                }
            }
        }

        public int StepProgressRate
        {
            get => _stepProgressRate;
            set
            {
                if (value != StepProgressRate)
                {
                    _stepProgressRate = value;
                    StepProgressRateChanged?.Invoke(this, new ProgressRateChangedEventArgs(StepProgressRate));
                }
            }
        }


        public void RunForCatalogPart(CatiaEnv catiaEnv, MarkingData markingData, string designTableFullPath, List<CatiaFile> partList)
        {
            // Create a design table reader
            DesignTableReader designTableReader = new DesignTableReader();
            if (designTableReader.OpenDesignTable(designTableFullPath))
            {
                foreach (CatiaFile part in partList)
                {
                    // Check if the part is in the design table excel file
                    List<string> partListFromDesignTableFile = new List<string>();
                    designTableReader.GetPartList(partListFromDesignTableFile);

                    // If Ok, then run the marking generation
                    if (partListFromDesignTableFile.Contains(part.Name))
                    {
                        CatiaPartDocument catiaPartDocument = new CatiaPartDocument(catiaEnv.OpenDocument(part));

                        // Get the value from the design table for Text, Character height and Extrusion height datas if link is activated
                        if (markingData.Text.IsLinkOn || markingData.CharacterHeight.IsLinkOn || markingData.ExtrusionHeight.IsLinkOn)
                        {
                            MarkingData tempMarkingData = markingData;
                            int row = designTableReader.GetRow(part.Name);

                            if (markingData.Text.IsLinkOn)
                                tempMarkingData.Text.Value = (string)designTableReader.GetValue(row, markingData.Text.LinkedParameter.ColumnIndex);

                            if (markingData.CharacterHeight.IsLinkOn)
                                tempMarkingData.CharacterHeight.Value = (double)designTableReader.GetValue(row, markingData.CharacterHeight.LinkedParameter.ColumnIndex);

                            if (markingData.ExtrusionHeight.IsLinkOn)
                                tempMarkingData.ExtrusionHeight.Value = (double)designTableReader.GetValue(row, markingData.ExtrusionHeight.LinkedParameter.ColumnIndex);

                            // Run the marking generation with linked values
                            Run(catiaEnv, catiaPartDocument.PartDocument, tempMarkingData);
                        }
                        else
                        {
                            // Run the marking generation without linked values
                            Run(catiaEnv, catiaPartDocument.PartDocument, markingData);
                        }
                    }
                }

                // Close the design table Excel file
                designTableReader.CloseDesignTable();
            }
            else
            {
                throw new Exception(designTableReader.ExceptionMessage);
            }
        }

        public void Run(CatiaEnv catiaEnv, PartDocument partDocument, MarkingData markingData)
        {
            ProgressRate = 0;


            // ***** PREPARE MARKING ***** //

            // Creates the marking set
            HybridBody markingSet = partDocument.Part.HybridBodies.Add();
            markingSet.set_Name("MARKING SET - " + markingData.Name);

            // Creates the origin axis system to the marking set
            partDocument.Part.InWorkObject = markingSet;
            AxisSystem originAxisSystem = partDocument.Part.AxisSystems.Add();
            originAxisSystem.set_Name("Origin axis system");

            // Creates the point set
            HybridBody pointSet = markingSet.HybridBodies.Add();
            pointSet.set_Name("Points");

            // Creates the local axis system set
            HybridBody localAxisSet = markingSet.HybridBodies.Add();
            localAxisSet.set_Name("Local Axis systems");

            // Creates the working set
            HybridBody writingSet = markingSet.HybridBodies.Add();
            writingSet.set_Name("Characters construction");

            // Creates the input data references
            Reference originAxisSystemRef = partDocument.Part.CreateReferenceFromObject(originAxisSystem);

            AnyObject trackingCurve = partDocument.Part.FindObjectByName(markingData.TrackingCurveName);
            Reference trackingCurveRef = partDocument.Part.CreateReferenceFromObject(trackingCurve);

            AnyObject projectionSurface = partDocument.Part.FindObjectByName(markingData.ProjectionSurfaceName);
            Reference projectionSurfaceRef = partDocument.Part.CreateReferenceFromObject(projectionSurface);

            AnyObject startPoint = partDocument.Part.FindObjectByName(markingData.StartPointName);
            Reference startPointRef = partDocument.Part.CreateReferenceFromObject(startPoint);

            AxisSystem startAxisSystem = (AxisSystem)partDocument.Part.FindObjectByName(markingData.AxisSystemName);
            Reference startAxisSystemRef = partDocument.Part.CreateReferenceFromObject(startAxisSystem);

            // Factories
            HybridShapeFactory hybridShapeFactory = (HybridShapeFactory)partDocument.Part.HybridShapeFactory;
            ShapeFactory shapeFactory = (ShapeFactory)partDocument.Part.ShapeFactory;

            // Creates the origin point
            HybridShapePointCoord originPoint = hybridShapeFactory.AddNewPointCoord(0, 0, 0);
            Reference originPointRef = partDocument.Part.CreateReferenceFromObject(originPoint);

            // Creates the marking body
            Body markingBody = partDocument.Part.Bodies.Add();
            markingBody.set_Name("MARKING BODY - " + markingData.Name);

            // Add or remove marking body from main body
            partDocument.Part.InWorkObject = partDocument.Part.MainBody;
            if (markingData.ExtrusionHeight.Value > 0)
            {
                Add addBody = shapeFactory.AddNewAdd(markingBody);
            }
            else
            {
                Remove removeBody = shapeFactory.AddNewRemove(markingBody);
            }

            // Updates
            partDocument.Part.Update();

            // ***** END OF PREPARATIONS ***** //



            // ***** CREATE A REFERENTIEL FOR COMPARISON ***** //

            // Check if surface natural normal passing by the start point is in the same direction as user Z axis
            object[] ZAxisDirection = new object[3];
            startAxisSystem.GetZAxis(ZAxisDirection);

            object[] naturalNormalLineDirection = new object[3];
            HybridShapeLineNormal naturalNormalLine = hybridShapeFactory.AddNewLineNormal(projectionSurfaceRef, startPointRef, 0, 10, false);
            naturalNormalLine.Compute();
            pointSet.AppendHybridShape(naturalNormalLine);
            naturalNormalLine.GetDirection(naturalNormalLineDirection);

            bool isZSameDirection = false;
            if (Math.Abs((double)ZAxisDirection[0] - (double)naturalNormalLineDirection[0]) < 0.1
                && Math.Abs((double)ZAxisDirection[1] - (double)naturalNormalLineDirection[1]) < 0.1
                && Math.Abs((double)ZAxisDirection[2] - (double)naturalNormalLineDirection[2]) < 0.1)
            {
                isZSameDirection = true;
            }

            // Check if natural angle line passing by the start point is in the same direction as user Y axis
            object[] YAxisDirection = new object[3];
            startAxisSystem.GetYAxis(YAxisDirection);

            object[] naturalAngleLineDirection = new object[3];
            HybridShapePlaneTangent tangentPlane = hybridShapeFactory.AddNewPlaneTangent(projectionSurfaceRef, startPointRef);
            tangentPlane.Compute();
            Reference tangentPlaneRef = partDocument.Part.CreateReferenceFromObject(tangentPlane);

            HybridShapeLineAngle naturalAngleLine = hybridShapeFactory.AddNewLineAngle(trackingCurveRef, tangentPlaneRef, startPointRef, false, 0, 10, 90, false);
            naturalAngleLine.Compute();
            naturalAngleLine.GetDirection(naturalAngleLineDirection);

            bool isYSameDirection = false;
            if (Math.Abs((double)YAxisDirection[0] - (double)naturalAngleLineDirection[0]) < 0.1
                && Math.Abs((double)YAxisDirection[1] - (double)naturalAngleLineDirection[1]) < 0.1
                && Math.Abs((double)YAxisDirection[2] - (double)naturalAngleLineDirection[2]) < 0.1)
            {
                isYSameDirection = true;
            }

            // Check if natural tangent line passing by the start point is in the same direction as user X axis
            object[] XAxisDirection = new object[3];
            startAxisSystem.GetXAxis(XAxisDirection);

            object[] naturalTangentLineCoordinate = new object[3];
            HybridShapeLineTangency naturalTangentLine = hybridShapeFactory.AddNewLineTangency(trackingCurveRef, startPointRef, 0, 10, false);
            naturalTangentLine.Compute();
            naturalTangentLine.GetDirection(naturalTangentLineCoordinate);

            bool isXSameDirection = false;
            if (Math.Abs((double)XAxisDirection[0] - (double)naturalTangentLineCoordinate[0]) < 0.1
                && Math.Abs((double)XAxisDirection[1] - (double)naturalTangentLineCoordinate[1]) < 0.1
                && Math.Abs((double)XAxisDirection[2] - (double)naturalTangentLineCoordinate[2]) < 0.1)
            {
                isXSameDirection = true;
            }

            // ***** END OF REFERENTIEL ***** //



            // ***** WRITE CHARACTERS ***** //

            List<CatiaCharacter> characterList = new List<CatiaCharacter>(); // Inutile pour le moment à voir pour optimiser le code et ne pas redessiner chaque lettre à chaque fois !
            int characterIndex = 0;
            double currentLength2 = 0;

            // Compute the scale ratio to obtain the character height with a fixed character reference
            double scaleRatio = markingData.CharacterHeight.Value / markingData.Font.GetCharacterGeometry('M').Bounds.Height;

            // Create a catia character list from input text and character height
            foreach (char character in markingData.Text.Value)
            {
                // Create the character set
                HybridBody characterSet = writingSet.HybridBodies.Add();
                characterSet.set_Name(character.ToString() + "." + characterIndex);

                // Get the associatedCatiaCharacter
                CatiaCharacter associatedCatiaCharacter = GetCatiaCharacter(partDocument, markingData, characterSet, character);

                // Create the local point
                Reference localPointRef = ComputeLocalPoint(associatedCatiaCharacter, markingData, trackingCurveRef, startPointRef, pointSet, isXSameDirection, ref currentLength2, scaleRatio);

                if (!associatedCatiaCharacter.IsSpaceCharacter)
                {
                    // Create the local axis system
                    AxisSystem localAxisSystem = ComputeLocalAxisSystem(partDocument, markingData, localAxisSet, localPointRef, projectionSurfaceRef, trackingCurveRef,
                        isXSameDirection, isYSameDirection, isZSameDirection);

                    Reference localAxisSystemRef = partDocument.Part.CreateReferenceFromObject(localAxisSystem);

                    // For each surface composing the CatiaCharacter
                    foreach (CatiaSurface surface in associatedCatiaCharacter.SurfaceList)
                    {
                        // Scale external contour
                        surface.ExternalContour.Scale(scaleRatio, originPointRef);

                        // Move external contour
                        surface.ExternalContour.Move(originAxisSystemRef, localAxisSystemRef);

                        // Project external contour
                        surface.ExternalContour.Project(projectionSurfaceRef);

                        foreach (CatiaContour contour in surface.InternalContourList)
                        {
                            // Scale external contour
                            contour.Scale(scaleRatio, originPointRef);

                            // Move external contour
                            contour.Move(originAxisSystemRef, localAxisSystemRef);

                            // Project external contour
                            contour.Project(projectionSurfaceRef);
                        }

                        surface.ComputeSurface(projectionSurfaceRef);

                        characterSet.AppendHybridShape(surface.Shape);
                    }

                    associatedCatiaCharacter.AssembleSurfaces();

                    // ADD THICKNESS TO THE ASSEMBLED SURFACE
                    int thickSurfaceDirection = GetThickSurfaceDirection(localAxisSystem, associatedCatiaCharacter.ShapeReference, localPointRef, hybridShapeFactory);
                    partDocument.Part.InWorkObject = markingBody;
                    ThickSurface characterThickSurface = shapeFactory.AddNewThickSurface(associatedCatiaCharacter.ShapeReference, thickSurfaceDirection, markingData.ExtrusionHeight.Value, 0);

                    currentLength2 += scaleRatio * associatedCatiaCharacter.PathGeometry.Bounds.Width * (0.5d + markingData.Font.GetRightSideBearing(character) / markingData.Font.GetWidth(character));

                }
                else
                    currentLength2 += scaleRatio * associatedCatiaCharacter.PathGeometry.Bounds.Width * 0.5d;

                ProgressRate += 1d / (double)markingData.Text.Value.Count();

            }


            // ***** END ***** //

        }

        private CatiaCharacter GetCatiaCharacter(PartDocument partDocument, MarkingData markingData, HybridBody characterSet, char character)
        {
            CatiaCharacter CatiaCharacter = new CatiaCharacter(partDocument, character);

            /*if (!characterList.Contains(tempCatiaCharacter))
            {
                characterList.Add(tempCatiaCharacter);*/
            CatiaCharacter.PathGeometry = markingData.Font.GetCharacterGeometry(CatiaCharacter.Value);

            if (!CatiaCharacter.IsSpaceCharacter)
                CatiaCharacter.DrawCharacter(characterSet);

            return CatiaCharacter;
            /*}
            else
            {
                return characterList.Find((catiaCharacter) => catiaCharacter.Value == character);
            }*/
        }

        private Reference ComputeLocalPoint(CatiaCharacter character, MarkingData markingData, Reference trackingCurveRef, Reference startPointRef, HybridBody pointSet,
            bool isXSameDirection, ref double currentLength, double scaleRatio)
        {
            // ***** CREATE LINES FOR LOCAL AXIS SYSTEMS ***** //

            // Local point on the tracking curve positionning the current character
            if (currentLength != 0)
            {
                if (character.IsSpaceCharacter)
                    currentLength += scaleRatio * character.PathGeometry.Bounds.Width * 0.5d;
                else
                    currentLength += scaleRatio * character.PathGeometry.Bounds.Width * (0.5d + markingData.Font.GetLeftSideBearing(character.Value) / markingData.Font.GetWidth(character.Value));
            }

            HybridShapePointOnCurve localPoint = character.HybridShapeFactory.AddNewPointOnCurveWithReferenceFromDistance(trackingCurveRef, startPointRef, currentLength, !isXSameDirection);
            localPoint.Compute();
            Reference localPointRef = character.PartDocument.Part.CreateReferenceFromObject(localPoint);
            pointSet.AppendHybridShape(localPoint);

            return localPointRef;
        }



        private AxisSystem ComputeLocalAxisSystem(PartDocument partDocument, MarkingData markingData, HybridBody localAxisSystemSet, Reference localPointRef, Reference projectionSurfaceRef, Reference trackingCurveRef,
            bool isXSameDirection, bool isYSameDirection, bool isZSameDirection)
        {

            HybridShapeFactory hybridShapeFactory = (HybridShapeFactory)partDocument.Part.HybridShapeFactory;

            // Normal of the surface passing by the local point
            HybridShapeLineNormal localLineNormal = hybridShapeFactory.AddNewLineNormal(projectionSurfaceRef, localPointRef, 0, 10, false);
            localLineNormal.Compute();
            Reference localLineNormalRef = partDocument.Part.CreateReferenceFromObject(localLineNormal);


            // Tangent of the tracking curve passing by the local point
            HybridShapeLineTangency localLineTangent = hybridShapeFactory.AddNewLineTangency(trackingCurveRef, localPointRef, 0, 10, false);
            localLineTangent.Compute();
            Reference localLineTangentRef = partDocument.Part.CreateReferenceFromObject(localLineTangent);


            // 90° angle line considering tracking curve passing by the local point
            HybridShapePlaneTangent localPlaneTangent = hybridShapeFactory.AddNewPlaneTangent(projectionSurfaceRef, localPointRef);
            localPlaneTangent.Compute();
            Reference localPlaneTangentRef = partDocument.Part.CreateReferenceFromObject(localPlaneTangent);

            HybridShapeLineAngle localLineAngle = hybridShapeFactory.AddNewLineAngle(trackingCurveRef, localPlaneTangentRef, localPointRef, false, 0, 10, 90, false);
            localLineAngle.Compute();
            Reference localLineAngleRef = partDocument.Part.CreateReferenceFromObject(localLineAngle);

            // ***** END OF LOCAL LINES ***** //


            // ***** LOCAL AXIS SYSTEM ***** //

            partDocument.Part.InWorkObject = localAxisSystemSet;
            AxisSystem localAxisSystem = partDocument.Part.AxisSystems.Add();

            // Local axis system : point
            localAxisSystem.OriginType = CATAxisSystemOriginType.catAxisSystemOriginByPoint;
            localAxisSystem.OriginPoint = localPointRef;

            // Local axis system : X direction
            if (isXSameDirection)
                localAxisSystem.XAxisType = CATAxisSystemAxisType.catAxisSystemAxisSameDirection;
            else
                localAxisSystem.XAxisType = CATAxisSystemAxisType.catAxisSystemAxisOppositeDirection;

            localAxisSystem.XAxisDirection = localLineTangentRef;

            // Local axis system : Y direction
            if (isYSameDirection)
                localAxisSystem.YAxisType = CATAxisSystemAxisType.catAxisSystemAxisSameDirection;
            else
                localAxisSystem.YAxisType = CATAxisSystemAxisType.catAxisSystemAxisOppositeDirection;

            localAxisSystem.YAxisDirection = localLineAngleRef;

            // Local axis system : Z direction
            if (isZSameDirection)
                localAxisSystem.ZAxisType = CATAxisSystemAxisType.catAxisSystemAxisSameDirection;
            else
                localAxisSystem.ZAxisType = CATAxisSystemAxisType.catAxisSystemAxisOppositeDirection;

            localAxisSystem.ZAxisDirection = localLineNormalRef;

            // Local axis system : Update
            partDocument.Part.UpdateObject(localAxisSystem);

            // ***** END LOCAL AXIS SYSTEM ***** //

            return localAxisSystem;
        }

   

        /// <summary>
        /// Compare Z local axis direction with the normal line of the surface at the local point.
        /// </summary>
        /// <param name="localAxisSystem">Axis system of the local point</param>
        /// <param name="surfaceRef">Reference of the surface to obtain th normal line.</param>
        /// <param name="localPointRef">Reference of the point where normal line is passing by.</param>
        /// <param name="factory"></param>
        /// <returns>0 if the directions are the same. 1 if not.</returns>
        private int GetThickSurfaceDirection(AxisSystem localAxisSystem, Reference surfaceRef, Reference localPointRef, HybridShapeFactory factory)
        {
            // Create the normal line of the surface
            HybridShapeLineNormal surfaceNormalLine = factory.AddNewLineNormal(surfaceRef, localPointRef, 0, 10, false);
            surfaceNormalLine.Compute();

            // Get the direction of the normal line
            object[] surfaceNormalLineDirection = new object[3];
            surfaceNormalLine.GetDirection(surfaceNormalLineDirection);

            // Get the direction of the Z axis of the local axis system
            object[] ZLocalAxisDirection = new object[3];
            localAxisSystem.GetZAxis(ZLocalAxisDirection);

            // Compare Z axis direction and Normal line direction to know if we have to invert
            int thickSurfaceDirection = 1;
            if (Math.Abs((double)ZLocalAxisDirection[0] - (double)surfaceNormalLineDirection[0]) < 0.1 &&
                Math.Abs((double)ZLocalAxisDirection[1] - (double)surfaceNormalLineDirection[1]) < 0.1 &&
                Math.Abs((double)ZLocalAxisDirection[2] - (double)surfaceNormalLineDirection[2]) < 0.1)
            {
                thickSurfaceDirection = 0;
            }

            return thickSurfaceDirection;
        }

    }

}

