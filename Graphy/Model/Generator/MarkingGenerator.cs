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

        public enum ShapeType
        {
            Unknown = 0,
            Point = 1,
            Curve = 2,
            Line = 3,
            Circle = 4,
            Surface = 5,
            Plane = 6,
            Solid = 7,
            AxisSystem = 12
        };


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
            test(partDocument, markingData);
            ProgressRate = 0;

            List<CatiaChar> characterList = new List<CatiaChar>();

            CatiaPartDocument fontPartDocument = new CatiaPartDocument(catiaEnv.OpenDocument(new CatiaFile(markingData.Font.GeneratedFileFullPath)));

            if (CanDrawAllCharacters(fontPartDocument.PartDocument, markingData, characterList))
            {
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


                int i = 1;
                double currentLength = 0;
                ProgressRate = 0;
                foreach (char c in markingData.Text.Value)
                {
                    originAxisSystem.IsCurrent = true;

                    if (c == ' ')
                    {
                        WriteSpace(characterList, markingData, ref currentLength, partDocument, pointSet, trackingCurveRef, startPointRef, hybridShapeFactory, isXSameDirection);
                    }
                    else
                    {
                        WriteCharacter(i, c, characterList, markingData, ref currentLength,
                        fontPartDocument.PartDocument, partDocument, writingSet, localAxisSet, pointSet, markingBody,
                        originAxisSystemRef, trackingCurveRef, projectionSurfaceRef, startPointRef, startAxisSystemRef,
                        hybridShapeFactory, shapeFactory, originPointRef, isXSameDirection, isYSameDirection, isZSameDirection);

                        i++;
                    }

                    partDocument.Part.Update();
                    ProgressRate += 1 / (double)markingData.Text.Value.Length;
                }

                partDocument.Part.Update();
            }
            else
            {
                throw new Exception("Tous les caractères n'ont pas été trouvés.");
            }

            fontPartDocument.PartDocument.Close();
        }

        private void WriteSpace(List<CatiaChar> catiaCharList, MarkingData markingData, ref double currentLength, PartDocument markingPartDocument, HybridBody pointSet,
            Reference trackingCurveRef, Reference startPointRef, HybridShapeFactory hybridShapeFactory, bool isXSameDirection)
        {
            foreach (CatiaChar catiaChar in catiaCharList)
            {
                if (catiaChar.Value == '_')
                {
                    // Local point on the tracking curve positionning the current character
                    currentLength += catiaChar.ScaleWidth / 2;
                    HybridShapePointOnCurve localPoint = hybridShapeFactory.AddNewPointOnCurveWithReferenceFromDistance(trackingCurveRef, startPointRef, currentLength, !isXSameDirection);
                    Reference localPointRef = markingPartDocument.Part.CreateReferenceFromObject(localPoint);
                    pointSet.AppendHybridShape(localPoint);

                    currentLength += catiaChar.ScaleWidth / 2;
                }
            }
        }


        private void WriteCharacter(int index, char c, List<CatiaChar> catiaCharList, MarkingData markingData, ref double currentLength,
            PartDocument fontPartDocument, PartDocument markingPartDocument, HybridBody writingSet, HybridBody localAxisSet, HybridBody pointSet, Body markingBody,
            Reference originAxisSystemRef, Reference trackingCurveRef, Reference projectionSurfaceRef, Reference startPointRef, Reference startAxisSystemRef,
            HybridShapeFactory hybridShapeFactory, ShapeFactory shapeFactory, Reference originPointRef, bool isXSameDirection, bool isYSameDirection, bool isZSameDirection)
        {
            foreach (CatiaChar catiaChar in catiaCharList)
            {
                if (c == catiaChar.Value)
                {
                    // Create char set
                    HybridBody charSet = writingSet.HybridBodies.Add();
                    charSet.set_Name(c.ToString() + "." + index);

                    // Compute base positionning values
                    double charWidth = markingData.Font.GetWidth(c);
                    double charLeftSideBearing = 0;
                    double charRightSideBearing = 0;

                    if (charWidth != 0)
                    {
                        charLeftSideBearing = Math.Abs(markingData.Font.GetLeftSideBearing(c)) * catiaChar.ScaleWidth / charWidth;

                        charRightSideBearing = Math.Abs(markingData.Font.GetLeftSideBearing(c)) * catiaChar.ScaleWidth / charWidth;
                    }


                    // ***** CREATE LINES FOR LOCAL AXIS SYSTEMS ***** //

                    // Local point on the tracking curve positionning the current character
                    currentLength += catiaChar.ScaleWidth / 2 + charLeftSideBearing;
                    HybridShapePointOnCurve localPoint = hybridShapeFactory.AddNewPointOnCurveWithReferenceFromDistance(trackingCurveRef, startPointRef, currentLength, !isXSameDirection);
                    Reference localPointRef = markingPartDocument.Part.CreateReferenceFromObject(localPoint);
                    pointSet.AppendHybridShape(localPoint);


                    // Normal of the surface passing by the local point
                    HybridShapeLineNormal localLineNormal = hybridShapeFactory.AddNewLineNormal(projectionSurfaceRef, localPointRef, 0, 10, false);
                    localLineNormal.Compute();
                    Reference localLineNormalRef = markingPartDocument.Part.CreateReferenceFromObject(localLineNormal);


                    // Tangent of the tracking curve passing by the local point
                    HybridShapeLineTangency localLineTangent = hybridShapeFactory.AddNewLineTangency(trackingCurveRef, localPointRef, 0, 10, false);
                    localLineTangent.Compute();
                    Reference localLineTangentRef = markingPartDocument.Part.CreateReferenceFromObject(localLineTangent);


                    // 90° angle line considering tracking curve passing by the local point
                    HybridShapePlaneTangent localPlaneTangent = hybridShapeFactory.AddNewPlaneTangent(projectionSurfaceRef, localPointRef);
                    localPlaneTangent.Compute();
                    Reference localPlaneTangentRef = markingPartDocument.Part.CreateReferenceFromObject(localPlaneTangent);

                    HybridShapeLineAngle localLineAngle = hybridShapeFactory.AddNewLineAngle(trackingCurveRef, localPlaneTangentRef, localPointRef, false, 0, 10, 90, false);
                    localLineAngle.Compute();

                    Reference localLineAngleRef = markingPartDocument.Part.CreateReferenceFromObject(localLineAngle);

                    // ***** END OF LOCAL LINES ***** //


                    // ***** LOCAL AXIS SYSTEM ***** //

                    markingPartDocument.Part.InWorkObject = localAxisSet;
                    AxisSystem localAxisSystem = markingPartDocument.Part.AxisSystems.Add();

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
                    markingPartDocument.Part.UpdateObject(localAxisSystem);

                    // Local axis system : Reference
                    Reference localAxisSystemRef = markingPartDocument.Part.CreateReferenceFromObject(localAxisSystem);

                    // ***** END LOCAL AXIS SYSTEM ***** //



                    // ***** CREATE THE CHARACTER ***** //

                    // Paste character contours from font file
                    //CopyPasteShape(catiaChar.GetAllContours(), fontPartDocument, charSet, markingPartDocument);

                    // ******************************** NEWWW !!! TEST
                    CopyPasteShape(catiaChar.OriginalContourShapeList, fontPartDocument, charSet, markingPartDocument);

                    catiaChar.LoadSurfaceListFromSet(charSet, Contour.ContourStatus.Imported);


                    foreach (Surface surface in catiaChar.SurfaceList)
                    {
                        // >------- EXTERIOR CONTOUR -----------

                        // Get Scale / Positionned / Projected EXT contour reference
                        surface.ExtContour.ModifiedContourRef = ContourScalePositionProjection(surface.ExtContour.ImportedContourShape, catiaChar.ScaleRatio,
                            markingPartDocument, hybridShapeFactory, originPointRef, originAxisSystemRef, localAxisSystemRef, projectionSurfaceRef, charSet);

                        surface.ExtContour.ScaledPerimeter = GetCurveLenght(markingPartDocument, surface.ExtContour.ModifiedContourRef);

                        // Initialize splitOrientation
                        int splitOrientation = 1;

                        // Create exterior split surface
                        HybridShapeSplit exteriorSplitSurface;

                        HybridShapeSplit tempSplit = hybridShapeFactory.AddNewHybridSplit(projectionSurfaceRef, surface.ExtContour.ModifiedContourRef, splitOrientation);
                        tempSplit.Compute();
                        Reference tempSplitRef = markingPartDocument.Part.CreateReferenceFromObject(tempSplit);


                        if (!IsSplitOrientationOK(markingPartDocument, tempSplitRef, surface.ExtContour.ModifiedContourRef, hybridShapeFactory, projectionSurfaceRef))
                        {
                            exteriorSplitSurface = hybridShapeFactory.AddNewHybridSplit(projectionSurfaceRef, surface.ExtContour.ModifiedContourRef, -splitOrientation);
                        }
                        else
                        {
                            exteriorSplitSurface = hybridShapeFactory.AddNewHybridSplit(projectionSurfaceRef, surface.ExtContour.ModifiedContourRef, splitOrientation);
                            splitOrientation = -1;
                        }

                        exteriorSplitSurface.Compute();
                        surface.SurfaceRef = markingPartDocument.Part.CreateReferenceFromObject(exteriorSplitSurface);

                        // >------- END EXTERIOR CONTOUR -----------

                        // IF WE HAVE INTERIOR CONTOURS
                        if (!surface.IsIntContourListEmpty)
                        {
                            foreach (Contour intContour in surface.IntContourList)
                            {
                                // Get Scale / Positionned / Projected int contour reference
                                intContour.ModifiedContourRef = ContourScalePositionProjection(intContour.ImportedContourShape, catiaChar.ScaleRatio,
                                    markingPartDocument, hybridShapeFactory, originPointRef, originAxisSystemRef, localAxisSystemRef, projectionSurfaceRef, charSet);
                            }

                            // Assemble interior contours
                            if (surface.IntContourList.Count > 1)
                            {
                                HybridShapeAssemble interiorContourAssy = hybridShapeFactory.AddNewJoin(surface.IntContourList[0].ModifiedContourRef, surface.IntContourList[1].ModifiedContourRef);
                                interiorContourAssy.SetConnex(false);
                                foreach (Contour intContour in surface.IntContourList)
                                {
                                    interiorContourAssy.AddElement(intContour.ModifiedContourRef);
                                }

                                surface.IntContourListAssembledRef = markingPartDocument.Part.CreateReferenceFromObject(interiorContourAssy);
                            }
                            else
                            {
                                surface.IntContourListAssembledRef = surface.IntContourList[0].ModifiedContourRef;
                            }

                            // Cut surface result by interior contours
                            HybridShapeSplit finalSurface = hybridShapeFactory.AddNewHybridSplit(surface.SurfaceRef, surface.IntContourListAssembledRef, splitOrientation);
                            finalSurface.Compute();

                            // Hide previous cut
                            hybridShapeFactory.GSMVisibility(surface.SurfaceRef, 0);

                            // Add surface result
                            charSet.AppendHybridShape(finalSurface);

                            surface.SurfaceRef = markingPartDocument.Part.CreateReferenceFromObject(finalSurface);
                        }
                        else
                        {
                            charSet.AppendHybridShape(exteriorSplitSurface);
                        }
                    }


                    // ASSEMBLE CATIA CHARACTER SURFACES
                    if (catiaChar.SurfaceList.Count > 1)
                    {
                        HybridShapeAssemble surfaceAssy = hybridShapeFactory.AddNewJoin(catiaChar.SurfaceList.First().SurfaceRef, catiaChar.SurfaceList[1].SurfaceRef);
                        surfaceAssy.SetConnex(false);

                        foreach (Surface surface in catiaChar.SurfaceList)
                        {
                            surfaceAssy.AddElement(surface.SurfaceRef);
                        }

                        catiaChar.SurfaceListAssembledRef = markingPartDocument.Part.CreateReferenceFromObject(surfaceAssy);
                    }
                    else
                    {
                        catiaChar.SurfaceListAssembledRef = catiaChar.SurfaceList.First().SurfaceRef;
                    }


                    // ADD THICKNESS TO THE ASSEMBLED SURFACE
                    int thickSurfaceDirection = GetThickSurfaceDirection(localAxisSystem, catiaChar.SurfaceListAssembledRef, localPointRef, hybridShapeFactory);
                    markingPartDocument.Part.InWorkObject = markingBody;
                    ThickSurface characterThickSurface = shapeFactory.AddNewThickSurface(catiaChar.SurfaceListAssembledRef, thickSurfaceDirection, markingData.ExtrusionHeight.Value, 0);


                    currentLength += catiaChar.ScaleWidth / 2 + charRightSideBearing;
                }
            }
        }


        private Reference ContourScalePositionProjection(HybridShape contour, double scaleRatio, PartDocument partDocument, HybridShapeFactory hybridShapeFactory, Reference originPointRef,
            Reference originAxisSystemRef, Reference localAxisSystemRef, Reference projectionSurfaceRef, HybridBody set)
        {

            Reference shapeRef = partDocument.Part.CreateReferenceFromObject(contour);

            // Scaling
            HybridShapeScaling scaledShape = hybridShapeFactory.AddNewHybridScaling(shapeRef, originPointRef, scaleRatio);
            scaledShape.Compute();
            Reference scaledShapeRef = partDocument.Part.CreateReferenceFromObject(scaledShape);

            // Positionning
            HybridShapeAxisToAxis positionedShape = hybridShapeFactory.AddNewAxisToAxis(scaledShapeRef, originAxisSystemRef, localAxisSystemRef);
            positionedShape.Compute();
            Reference positionedShapeRef = partDocument.Part.CreateReferenceFromObject(positionedShape);

            // Projecting
            HybridShapeProject projectedShape = hybridShapeFactory.AddNewProject(positionedShapeRef, projectionSurfaceRef);
            projectedShape.Compute();
            set.AppendHybridShape(projectedShape);

            Reference projectedShapeRef = partDocument.Part.CreateReferenceFromObject(projectedShape);

            return projectedShapeRef;
        }


        /// <summary>
        /// Check if the orientation of the split should be inverted by comparing split surface area and the character filled surface area.
        /// </summary>
        /// <param name="partDocument">Part of the working document.</param>
        /// <param name="cutSurfaceRef">Reference of the splited surface.</param>
        /// <param name="characterContourRef">Reference of the contour which split the surface.</param>
        /// <param name="factory">HybridShapeFactory of the working part.</param>
        /// <param name="supportRef">Reference of the surface before split.</param>
        /// <returns></returns>
        private bool IsSplitOrientationOK(PartDocument partDocument, Reference cutSurfaceRef, Reference characterContourRef, HybridShapeFactory factory, Reference supportRef)
        {
            double cutArea = GetSurfaceArea(partDocument, cutSurfaceRef);

            // Create a filled contour
            HybridShapeFill filledContour = factory.AddNewFill();
            filledContour.AddBound(characterContourRef);
            filledContour.AddSupportAtBound(characterContourRef, supportRef);
            filledContour.Compute();

            Reference filledContourRef = partDocument.Part.CreateReferenceFromObject(filledContour);

            double charArea = GetSurfaceArea(partDocument, filledContourRef);

            // If the difference between areas are equal with a marge of 10% => OK
            if (Math.Abs(cutArea - charArea) < 0.1 * charArea)
                return true;
            else
                return false;
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

            // Normalisazion of surfaceNormalLineDirection for comparison
            /*object[] surfaceNormalLineDirectionNormalized = new object[3];
            surfaceNormalLineDirectionNormalized[0] = (double)surfaceNormalLineDirection[0] / GetVectorNorme(surfaceNormalLineDirection);
            surfaceNormalLineDirectionNormalized[1] = (double)surfaceNormalLineDirection[1] / GetVectorNorme(surfaceNormalLineDirection);
            surfaceNormalLineDirectionNormalized[2] = (double)surfaceNormalLineDirection[2] / GetVectorNorme(surfaceNormalLineDirection);*/


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

        /*private double GetVectorNorme(object[] vector)
        {
            return Math.Sqrt(Math.Pow((double)vector[0], 2) + Math.Pow((double)vector[1], 2) + Math.Pow((double)vector[2], 2));
        }*/


        /// <summary>
        /// Check if each char of markingText is present in the font file.
        /// If the char is present and not already loaded in the list, it is stored in the characterList.
        /// </summary>
        /// <param name="fontPartDocument">Part of the font document</param>
        /// <param name="markingData">Text to mark</param>
        /// <param name="fontCharacterList">List of the characters present in the text to mark</param>
        /// <returns>True if all characters have been found. False if not.</returns>
        public bool CanDrawAllCharacters(PartDocument fontPartDocument, MarkingData markingData, List<CatiaChar> fontCharacterList)
        {
            string charactersAlreadyAdded = "";
            int nbOfCharactersFound = 0;
            string charactersNotFound = "";
            HybridBodies hybridBodieCollection = fontPartDocument.Part.HybridBodies.Item("Set_Font_Disassembled").HybridBodies;

            // For each character in the markingText text
            foreach (char c in markingData.Text.Value.Replace(' ', '_'))
            {
                // See if character is already in the list.
                if (charactersAlreadyAdded.IndexOf(c) == -1)
                {
                    int k = nbOfCharactersFound;

                    if (hybridBodieCollection.Count > 0)
                    {
                        // For each shape in the Set "FontSet" of the font document
                        foreach (HybridBody set in hybridBodieCollection)
                        {
                            if (set.get_Name() == CharUnicodeInfo.GetUnicodeCategory(c).ToString())
                            {
                                foreach (HybridBody charSet in set.HybridBodies)
                                {
                                    if (charSet.get_Name().First() == c)
                                    {
                                        CatiaChar fontCharacter = new CatiaChar(c)
                                        {
                                            DefaultWidth = double.Parse(charSet.get_Name().Substring(3, 4)) / 100,
                                            DefaultHeight = double.Parse(charSet.get_Name().Substring(9, 4)) / 100,
                                        };

                                        foreach (HybridShape shape in charSet.HybridShapes)
                                        {
                                            fontCharacter.OriginalContourShapeList.Add(shape);
                                        }
                                        //fontCharacter.LoadSurfaceListFromSet(charSet, Contour.ContourStatus.FontFileOriginal);

                                        /*foreach (HybridShape shape in set.HybridShapes)
                                        {
                                            if (shape.get_Name().Contains("Ext"))
                                            {
                                                fontCharacter.ExteriorContourList.Add(shape);
                                            }
                                            else if (shape.get_Name().Contains("Int"))
                                            {
                                                fontCharacter.InteriorContourList.Add(shape);
                                            }
                                        }*/

                                        fontCharacterList.Add(fontCharacter);

                                        nbOfCharactersFound++;

                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }

                    // Append to the characters not found list.
                    if (nbOfCharactersFound == k)
                        charactersNotFound += c;

                    // Append to the characters added list.
                    charactersAlreadyAdded += c;
                }
            }

            SetScaleRatio(fontCharacterList, markingData);

            if (nbOfCharactersFound == charactersAlreadyAdded.Count())
                return true;
            else
                return false;
        }

        private void SetScaleRatio(List<CatiaChar> fontCharList, MarkingData markingData)
        {
            List<double> charHeightList = new List<double>();

            foreach (CatiaChar catiaChar in fontCharList)
            {
                charHeightList.Add(catiaChar.DefaultHeight);
            }

            double maxHeight = charHeightList.Max();

            foreach (CatiaChar catiaChar in fontCharList)
            {
                catiaChar.ScaleRatio = markingData.CharacterHeight.Value / maxHeight;
            }
        }

        /// <summary>
        /// Copy the shapeListToCopy and paste it to a set.
        /// </summary>
        /// <param name="shapeListToCopy">List of shapes to copy.</param>
        /// <param name="shapePartDocument">PartDocument of the shapes to copy.</param>
        /// <param name="setToPaste">Set where shapes should be paste.</param>
        /// <param name="setPartDocument">If the PartDocument of shapes to copy and the set to paste are different, specify the PartDocument of the Set.</param>
        private void CopyPasteShape(List<HybridShape> shapeListToCopy, PartDocument shapePartDocument, HybridBody setToPaste, PartDocument setPartDocument = null)
        {
            Selection copySelection = shapePartDocument.Selection;
            copySelection.Clear();
            foreach (HybridShape shape in shapeListToCopy)
            {
                copySelection.Add(shape);
            }
            copySelection.Copy();

            Selection pasteSetSelection;
            if (setPartDocument != null)
            {
                setPartDocument.Activate();
                pasteSetSelection = setPartDocument.Selection;
            }
            else
                pasteSetSelection = shapePartDocument.Selection;

            pasteSetSelection.Clear();
            pasteSetSelection.Add(setToPaste);
            pasteSetSelection.PasteSpecial("CATPrtResultWithOutLink");

            pasteSetSelection.Clear();
        }


        /// <summary>
        /// Get the curve length with SPAWorkbench measurable function.
        /// </summary>
        /// <param name="partDocument">PartDocument of the curve to be measured.</param>
        /// <param name="curveRef">Reference of the curve to be measured.</param>
        /// <returns></returns>
        public static double GetCurveLenght(PartDocument partDocument, Reference curveRef)
        {
            SPATypeLib.SPAWorkbench spaWorkbench = (SPATypeLib.SPAWorkbench)partDocument.GetWorkbench("SPAWorkbench");
            SPATypeLib.Measurable measurable = spaWorkbench.GetMeasurable(curveRef);

            return measurable.Length;
        }



        /// <summary>
        /// Get the surface area with SPAWorkbench measuble function.
        /// </summary>
        /// <param name="partDocument">PartDocument of the surface to be measured.</param>
        /// <param name="surfaceRef">Reference of the surface to be measured.</param>
        /// <returns></returns>
        public static double GetSurfaceArea(PartDocument partDocument, Reference surfaceRef)
        {
            SPATypeLib.SPAWorkbench spaWorkbench = (SPATypeLib.SPAWorkbench)partDocument.GetWorkbench("SPAWorkbench");
            SPATypeLib.Measurable measurable = spaWorkbench.GetMeasurable(surfaceRef);

            return measurable.Area;
        }




        /// <summary>
        /// Disassemble a curve with differents domains. Detect the exterior contour and the interior ones. Fill catiaChar.
        /// </summary>
        /// <param name="partDocument">PartDocument of the curve to be extracted.</param>
        /// <param name="curveShape">Curve to be extracted.</param>
        /// <param name="catiaChar">CatiaChar where the exterior contour and the interior contour list will be stored.</param>
        /*private static void GetContours(PartDocument partDocument, HybridShape curveShape, CatiaChar catiaChar)
        {
            List<HybridShape> SubCurveList = new List<HybridShape>();

            DisassembleCurve(partDocument, curveShape, SubCurveList);

            // Get the max length of contours
            double maxLength = 0;
            HybridShapeFactory factory = (HybridShapeFactory)partDocument.Part.HybridShapeFactory;
            foreach (HybridShape curve in SubCurveList)
            {
                if (curve != null)
                {
                    Reference curveRef = partDocument.Part.CreateReferenceFromObject(curve);
                    double length = GetCurveLenght(partDocument, curveRef);

                    if (length > maxLength)
                        maxLength = length;
                }
            }

            // Store separatly exterior and interiors contours
            foreach (HybridShape curve in SubCurveList)
            {
                if (curve != null)
                {
                    Reference curveRef = partDocument.Part.CreateReferenceFromObject(curve);
                    double length = GetCurveLenght(partDocument, curveRef);

                    if (length == maxLength)
                        catiaChar.ExteriorContourList = curve;
                    else
                        catiaChar.InteriorContourList.Add(curve);
                }
            }
        }*/

        /// <summary>
        /// Get the type of the shape.
        /// </summary>
        /// <param name="part">Part containing the shape</param>
        /// <param name="shapeRef">Reference of the shape</param>
        /// <returns>The type of the shape</returns>
        public static ShapeType GetShapeType(Part part, Reference shapeRef)
        {
            HybridShapeFactory factory = (HybridShapeFactory)part.HybridShapeFactory;
            return (ShapeType)factory.GetGeometricalFeatureType(shapeRef);
        }


        public void test(PartDocument partDocument, MarkingData markingData)
        {
            System.Windows.Media.FormattedText formattedText = new System.Windows.Media.FormattedText("Input test", new CultureInfo("en-US"),
                System.Windows.FlowDirection.LeftToRight, new System.Windows.Media.Typeface("Monospac821 BT"), 12, System.Windows.Media.Brushes.Black, 96);

            HybridShapeFactory hybridShapeFactory = (HybridShapeFactory)partDocument.Part.HybridShapeFactory;
            HybridBody tempset = partDocument.Part.HybridBodies.Add();

            System.Windows.Media.Geometry geometry = formattedText.BuildGeometry(new System.Windows.Point(0, 0));

            System.Windows.Media.PathGeometry pathGeometry = geometry.GetFlattenedPathGeometry(0.001, System.Windows.Media.ToleranceType.Relative);


            // --- TEST ---

            List<CatiaSurface> catiaSurfaceList = new List<CatiaSurface>();
            Reference planeXYReference = partDocument.Part.CreateReferenceFromObject(partDocument.Part.OriginElements.PlaneXY);

            List<System.Windows.Media.PathGeometry> separatePathGeometryCollection = new List<System.Windows.Media.PathGeometry>();
            foreach (System.Windows.Media.PathFigure figure in pathGeometry.Figures)
            {
                separatePathGeometryCollection.Add(new System.Windows.Media.PathGeometry(new List<System.Windows.Media.PathFigure>() { figure }));
            }

            separatePathGeometryCollection.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            int smallestParentContourIndex = -1;
            for (int i = 0; i < separatePathGeometryCollection.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    System.Windows.Media.IntersectionDetail intersectionDetail = separatePathGeometryCollection[i].FillContainsWithDetail(separatePathGeometryCollection[j]);

                    if (intersectionDetail == System.Windows.Media.IntersectionDetail.FullyInside)
                    {
                        smallestParentContourIndex = j;
                    }
                }

                // Create a temp catia contour
                // Draw contour and store it in the temp catia contour

                // Store in the surface collection with code below

                // Then write name


                // >--- FILL THE SURFACE COLLECTION ---

                // If smallestParentContourIndex is not modified, we have an external contour
                if (smallestParentContourIndex == -1)
                {
                    catiaSurfaceList.Add(new CatiaSurface(partDocument)
                    {
                        ExternalContour = new CatiaContour(partDocument, planeXYReference)
                        {
                            PathGeometry = separatePathGeometryCollection[i]
                        }
                    });
                }
                else
                {
                    // Get the parent contour.
                    CatiaContour parentContour = new CatiaContour(partDocument, planeXYReference)
                    {
                        PathGeometry = separatePathGeometryCollection[smallestParentContourIndex]
                    };

                    // Try to find the parent contour in the list of surfaces' external contour.
                    CatiaSurface tempSurface = catiaSurfaceList.Find((surface) => surface.ExternalContour.PathGeometry.ToString() == parentContour.PathGeometry.ToString());

                    // If the parent contour has been found, i-contour is internal.
                    if (tempSurface != null)
                    {
                        tempSurface.InternalContourList.Add(new CatiaContour(partDocument, planeXYReference)
                        {
                            PathGeometry = separatePathGeometryCollection[i]
                        });
                    }
                    // If the parent contour is not found, i-contour is the external contour of a new surface.
                    else
                    {
                        catiaSurfaceList.Add(new CatiaSurface(partDocument)
                        {
                            ExternalContour = new CatiaContour(partDocument, planeXYReference)
                            {
                                PathGeometry = separatePathGeometryCollection[i]
                            }
                        });
                    }
                }

                // >--- END ---
            }




            // --- FIN TEST ----


            System.Windows.Rect bounds = pathGeometry.Bounds;
            double height = bounds.Height;
            double width = bounds.Width;

            foreach (CatiaSurface surface in catiaSurfaceList)
            {
                DrawPathGeometry(partDocument, tempset, surface.ExternalContour.PathGeometry);

                for (int j = 0; j < surface.InternalContourList.Count; j++)
                {
                    DrawPathGeometry(partDocument, tempset, surface.InternalContourList[j].PathGeometry);
                }
            }


            /*foreach (System.Windows.Media.PathGeometry subPathGeometry in separatePathGeometryCollection)
            {
                foreach (System.Windows.Media.PathSegment segment in subPathGeometry.Figures.First().Segments)
                {
                    List<Reference> pointReferenceList = new List<Reference>();
                    List<Line> lineList = new List<Line>();

                    if (segment.GetType() == typeof(System.Windows.Media.PolyLineSegment))
                    {
                        foreach (System.Windows.Point point in ((System.Windows.Media.PolyLineSegment)segment).Points)
                        {
                            HybridShape pointShape = hybridShapeFactory.AddNewPointCoord(point.X, point.Y, 0);
                            pointShape.Compute();
                            Reference pointShapeRef = partDocument.Part.CreateReferenceFromObject(pointShape);

                            pointReferenceList.Add(pointShapeRef);
                        }

                        for (int i = 0; i < pointReferenceList.Count(); i++)
                        {
                            HybridShape line;
                            if (i == 0)
                                line = hybridShapeFactory.AddNewLinePtPt(pointReferenceList[i], pointReferenceList[pointReferenceList.Count - 1]);
                            else
                                line = hybridShapeFactory.AddNewLinePtPt(pointReferenceList[i - 1], pointReferenceList[i]);

                            line.Compute();
                            //lineList.Add(line);
                            tempset.AppendHybridShape(line);
                        }
                    }
                    else
                    {
                        if (segment.GetType() == typeof(System.Windows.Media.PolyBezierSegment))
                        {
                            HybridShapeSpline curve = hybridShapeFactory.AddNewSpline();
                            foreach (System.Windows.Point point in ((System.Windows.Media.PolyBezierSegment)segment).Points)
                            {
                                HybridShape pointShape = hybridShapeFactory.AddNewPointCoord(point.X, point.Y, 0);
                                pointShape.Compute();
                                Reference pointShapeRef = partDocument.Part.CreateReferenceFromObject(pointShape);

                                pointReferenceList.Add(pointShapeRef);

                                curve.AddPoint(pointShapeRef);
                                curve.Compute();
                            }

                            tempset.AppendHybridShape(curve);
                        }
                        else
                        {
                            if (segment.GetType() == typeof(System.Windows.Media.LineSegment))
                            {
                                Reference startPointShapeRef;
                                if (pointReferenceList.Count != 0)
                                {
                                    startPointShapeRef = pointReferenceList.Last();
                                }
                                else
                                {
                                    System.Windows.Point startPoint = subPathGeometry.Figures.First().StartPoint;
                                    HybridShape startPointShape = hybridShapeFactory.AddNewPointCoord(startPoint.X, startPoint.Y, 0);
                                    startPointShape.Compute();
                                    startPointShapeRef = partDocument.Part.CreateReferenceFromObject(startPointShape);

                                    pointReferenceList.Add(startPointShapeRef);
                                }

                                System.Windows.Point point = ((System.Windows.Media.LineSegment)segment).Point;
                                HybridShape pointShape = hybridShapeFactory.AddNewPointCoord(point.X, point.Y, 0);
                                pointShape.Compute();
                                Reference pointShapeRef = partDocument.Part.CreateReferenceFromObject(pointShape);

                                pointReferenceList.Add(pointShapeRef);

                                HybridShape line = hybridShapeFactory.AddNewLinePtPt(startPointShapeRef, pointShapeRef);

                                tempset.AppendHybridShape(line);
                            }
                        }

                    }*/
        }

        private void DrawPathGeometry(PartDocument partDocument, HybridBody tempSet, System.Windows.Media.PathGeometry pathGeometry)
        {
            HybridShapeFactory hybridShapeFactory = (HybridShapeFactory)partDocument.Part.HybridShapeFactory;
            List<Reference> pointReferenceList = new List<Reference>();
            List<Reference> lineList = new List<Reference>();

            foreach (System.Windows.Media.PathSegment segment in pathGeometry.Figures.First().Segments)
            {
                if (segment.GetType() == typeof(System.Windows.Media.PolyLineSegment))
                {
                    foreach (System.Windows.Point point in ((System.Windows.Media.PolyLineSegment)segment).Points)
                    {
                        HybridShape pointShape = hybridShapeFactory.AddNewPointCoord(point.X, point.Y, 0);
                        pointShape.Compute();
                        Reference pointShapeRef = partDocument.Part.CreateReferenceFromObject(pointShape);

                        pointReferenceList.Add(pointShapeRef);
                    }

                    for (int i = 0; i < pointReferenceList.Count(); i++)
                    {
                        HybridShape line;
                        if (i == 0)
                            line = hybridShapeFactory.AddNewLinePtPt(pointReferenceList[i], pointReferenceList[pointReferenceList.Count - 1]);
                        else
                            line = hybridShapeFactory.AddNewLinePtPt(pointReferenceList[i - 1], pointReferenceList[i]);

                        line.Compute();
                        lineList.Add(partDocument.Part.CreateReferenceFromObject(line));
                    }
                }
                else
                {
                    if (segment.GetType() == typeof(System.Windows.Media.PolyBezierSegment))
                    {
                        HybridShapeSpline curve = hybridShapeFactory.AddNewSpline();
                        foreach (System.Windows.Point point in ((System.Windows.Media.PolyBezierSegment)segment).Points)
                        {
                            HybridShape pointShape = hybridShapeFactory.AddNewPointCoord(point.X, point.Y, 0);
                            pointShape.Compute();
                            Reference pointShapeRef = partDocument.Part.CreateReferenceFromObject(pointShape);

                            pointReferenceList.Add(pointShapeRef);

                            curve.AddPoint(pointShapeRef);
                            curve.Compute();
                        }

                        tempSet.AppendHybridShape(curve);
                    }
                    else
                    {
                        if (segment.GetType() == typeof(System.Windows.Media.LineSegment))
                        {
                            Reference startPointShapeRef;
                            if (pointReferenceList.Count != 0)
                            {
                                startPointShapeRef = pointReferenceList.Last();
                            }
                            else
                            {
                                System.Windows.Point startPoint = pathGeometry.Figures.First().StartPoint;
                                HybridShape startPointShape = hybridShapeFactory.AddNewPointCoord(startPoint.X, startPoint.Y, 0);
                                startPointShape.Compute();
                                startPointShapeRef = partDocument.Part.CreateReferenceFromObject(startPointShape);

                                pointReferenceList.Add(startPointShapeRef);
                            }

                            System.Windows.Point point = ((System.Windows.Media.LineSegment)segment).Point;
                            HybridShape pointShape = hybridShapeFactory.AddNewPointCoord(point.X, point.Y, 0);
                            pointShape.Compute();
                            Reference pointShapeRef = partDocument.Part.CreateReferenceFromObject(pointShape);

                            pointReferenceList.Add(pointShapeRef);

                            HybridShape line = hybridShapeFactory.AddNewLinePtPt(startPointShapeRef, pointShapeRef);
                            lineList.Add(partDocument.Part.CreateReferenceFromObject(line));
                        }
                    }
                }
            }

            HybridShapeAssemble assy = hybridShapeFactory.AddNewJoin(lineList.First(), lineList[1]);
            for (int i = 2; i < lineList.Count; i++)
            {
                assy.AddElement(lineList[i]);
            }

            tempSet.AppendHybridShape(assy);
        }
    }

}

