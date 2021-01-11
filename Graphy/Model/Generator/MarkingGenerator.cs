using System;
using System.Collections.Generic;
using System.Linq;
using INFITF;
using MECMOD;
using PARTITF;
using HybridShapeTypeLib;
using Graphy.Model.CatiaShape;
using System.Windows.Media;
using Graphy.Enum;

namespace Graphy.Model.Generator
{
    public class MarkingGenerator
    {
        public event EventHandler<(double progressRate, int currentStep)> ProgressUpdated;

        public MarkingGenerator()
        {

        }

        private double _progressRate;
        private int _step;

        public double ProgressRate
        {
            get => _progressRate;
            set
            {
                if (value != ProgressRate)
                {
                    _progressRate = value;
                    ProgressUpdated?.Invoke(this, (ProgressRate, Step));
                }
            }
        }

        public int Step
        {
            get => _step;
            set
            {
                if (value != Step)
                {
                    _step = value;
                    ProgressUpdated?.Invoke(this, (ProgressRate, Step));
                }
            }
        }


        /*public void RunForCatalogPart(CatiaEnv catiaEnv, MarkingData markingData, string designTableFullPath, List<CatiaFile> partList,
            double toleranceFactor, bool keepHistoric, bool createVolume, VerticalAlignment verticalAlignment)
        {
            // Create a design table reader
            DesignTableReader designTableReader = new DesignTableReader();
            if (designTableReader.OpenDesignTable(designTableFullPath))
            {
                Step = 1;
                foreach (CatiaFile part in partList)
                {
                    // Check if the part is in the design table excel file
                    List<string> partListFromDesignTableFile = new List<string>();
                    designTableReader.GetPartList(partListFromDesignTableFile);

                    List<CatiaCharacter> characterList = new List<CatiaCharacter>();

                    // If Ok, then run the marking generation
                    if (partListFromDesignTableFile.Contains(part.Name))
                    {
                        CatiaPartDocument catiaPartDocument = new CatiaPartDocument(catiaEnv, catiaEnv.OpenDocument(part));

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
                            Run(catiaPartDocument, tempMarkingData, characterList, toleranceFactor, keepHistoric, createVolume, verticalAlignment);
                        }
                        else
                        {
                            // Run the marking generation without linked values
                            Run(catiaPartDocument, markingData, characterList, toleranceFactor, keepHistoric, createVolume, verticalAlignment);
                        }

                        // Save and close the document
                        catiaPartDocument.Document.Save();
                        catiaPartDocument.Document.Close();
                    }

                    Step++;
                }

                // Close the design table Excel file
                designTableReader.CloseDesignTable();
            }
            else
            {
                throw new Exception(designTableReader.ExceptionMessage);
            }
        }*/

        public void Run(CatiaPart catiaPart, MarkingData markingData, List<CatiaCharacter> characterList,
            double toleranceFactor, bool keepHistoric, bool createVolume, VerticalAlignment verticalAlignment)
        {
            ProgressRate = 0;

            if (Step == 0)
                Step = 1;


            // ***** PREPARE MARKING ***** //
            #region

            // Creates the marking set
            HybridBody markingSet = catiaPart.PartDocument.Part.HybridBodies.Add();
            if (markingData.IsText)
                markingSet.set_Name("MARKING SET: \"" + markingData.Text + "\"");
            else
                markingSet.set_Name("MARKING SET: \"" + markingData.Icon.Name + "\"");

            // Creates the origin axis system to the marking set
            catiaPart.PartDocument.Part.InWorkObject = markingSet;
            AxisSystem originAxisSystem = catiaPart.PartDocument.Part.AxisSystems.Add();
            originAxisSystem.set_Name("Origin axis system");

            // Creates the point set
            HybridBody pointSet = markingSet.HybridBodies.Add();
            pointSet.set_Name("Points");

            // Creates the local axis system set
            HybridBody localAxisSet = markingSet.HybridBodies.Add();
            localAxisSet.set_Name("Local Axis systems");

            // Creates the input data references
            Reference originAxisSystemRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(originAxisSystem);

            AnyObject trackingCurve = catiaPart.PartDocument.Part.FindObjectByName(markingData.TrackingCurveName);
            Reference trackingCurveRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(trackingCurve);

            AnyObject projectionSurface = catiaPart.PartDocument.Part.FindObjectByName(markingData.ProjectionSurfaceName);
            Reference projectionSurfaceRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(projectionSurface);

            AnyObject startPoint = catiaPart.PartDocument.Part.FindObjectByName(markingData.StartPointName);
            Reference startPointRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(startPoint);

            AxisSystem startAxisSystem = (AxisSystem)catiaPart.PartDocument.Part.FindObjectByName(markingData.AxisSystemName);

            // Factories
            HybridShapeFactory hybridShapeFactory = (HybridShapeFactory)catiaPart.PartDocument.Part.HybridShapeFactory;
            ShapeFactory shapeFactory = (ShapeFactory)catiaPart.PartDocument.Part.ShapeFactory;

            // Creates the origin point
            HybridShapePointCoord originPoint = hybridShapeFactory.AddNewPointCoord(0, 0, 0);
            Reference originPointRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(originPoint);

            // Creates the marking body
            Body markingBody = catiaPart.PartDocument.Part.Bodies.Add();
            markingBody.set_Name("MARKING BODY");


            // Updates
            catiaPart.PartDocument.Part.Update();

            #endregion
            // ***** END OF PREPARATIONS ***** //



            // ***** CREATE A REFERENTIEL FOR COMPARISON ***** //
            #region

            // Check if surface natural normal passing by the start point is in the same direction as user Z axis
            object[] ZAxisDirection = new object[3];
            startAxisSystem.GetZAxis(ZAxisDirection);

            object[] naturalNormalLineDirection = new object[3];
            HybridShapeLineNormal naturalNormalLine = hybridShapeFactory.AddNewLineNormal(projectionSurfaceRef, startPointRef, 0, 10, false);
            naturalNormalLine.Compute();
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
            Reference tangentPlaneRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(tangentPlane);

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

            #endregion
            // ***** END OF REFERENTIEL ***** //


            // ***** PREPARE THE DATAS WHETER WE DRAW TEXT OR ICON ***** //

            // DRAW TEXT
            if (markingData.IsText)
            {
                int characterIndex = 0;
                double currentLength = 0;
                CatiaCharacter previousCharacter = null;

                // Compute the scale ratio to obtain the character height with a fixed character reference
                // 'M' character is used as reference.
                PathGeometry refCharacterGeometry = markingData.FontFamily.GetCharacterGeometry('M', toleranceFactor, markingData.IsBold, markingData.IsItalic);
                double scaleRatio = markingData.MarkingHeight / refCharacterGeometry.Bounds.Height;

                // Compute the kerningPair table
                markingData.FontFamily.ComputeKerningPairs();

                // Create a list to store local axis system references in order to delete them if keep historic option is uncheck.
                List<Reference> localAxisSystemRefList = new List<Reference>();

                // Creates the working set
                HybridBody writingSet = markingSet.HybridBodies.Add();
                writingSet.set_Name("Characters construction");

                // Create a catia character list from input text and character height
                foreach (char character in markingData.Text)
                {
                    // Get the associatedCatiaCharacter
                    CatiaCharacter catiaCharacter = GetCatiaCharacter(catiaPart.PartDocument, markingData, character, characterList, toleranceFactor,
                        verticalAlignment, refCharacterGeometry);

                    // Create the local point
                    Reference localPointRef = ComputeLocalPoint(catiaCharacter, previousCharacter, markingData,
                        trackingCurveRef, startPointRef, pointSet,
                        isXSameDirection, ref currentLength, scaleRatio);


                    if (!catiaCharacter.IsSpaceCharacter)
                    {
                        // Create the local axis system
                        AxisSystem localAxisSystem = ComputeLocalAxisSystem(catiaPart.PartDocument, localAxisSet, localPointRef, projectionSurfaceRef, trackingCurveRef,
                            isXSameDirection, isYSameDirection, isZSameDirection);

                        // Create the local axis system reference
                        Reference localAxisSystemRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(localAxisSystem);

                        // Store the local axis system reference
                        localAxisSystemRefList.Add(localAxisSystemRef);


                        if (!keepHistoric)
                        {
                            Compute(catiaCharacter, scaleRatio, keepHistoric, createVolume, writingSet, hybridShapeFactory, shapeFactory, markingData, markingBody,
                                originPointRef, originAxisSystemRef, localAxisSystem, localAxisSystemRef, projectionSurfaceRef, localPointRef);
                        }
                        else
                        {
                            // Create the character set
                            HybridBody characterSet = writingSet.HybridBodies.Add();
                            characterSet.set_Name(character.ToString() + "." + characterIndex);

                            Compute(catiaCharacter, scaleRatio, keepHistoric, createVolume, characterSet, hybridShapeFactory, shapeFactory, markingData, markingBody,
                                originPointRef, originAxisSystemRef, localAxisSystem, localAxisSystemRef, projectionSurfaceRef, localPointRef);
                        }

                    }

                    previousCharacter = catiaCharacter;
                    ProgressRate += 1d / (double)markingData.Text.Count();
                    characterIndex++;
                }

                // If Underligned
                if (true)
                {
                    HybridBody underlineBody = markingSet.HybridBodies.Add();

                    DrawLine(catiaPart, LineType.Underline, markingData, verticalAlignment, refCharacterGeometry, underlineBody, scaleRatio, isYSameDirection,
                        keepHistoric, createVolume, trackingCurveRef, projectionSurfaceRef, hybridShapeFactory);
                }

                // Remove local axis systems
                if (!keepHistoric)
                {
                    foreach (Reference reference in localAxisSystemRefList)
                    {
                        hybridShapeFactory.DeleteObjectForDatum(reference);
                    }
                }


            }

            // DRAW ICON
            else
            {
                CatiaDrawableShape iconShape = new CatiaDrawableShape(catiaPart.PartDocument);
                Geometry test = Geometry.Parse(markingData.Icon.PathData);
                iconShape.PathGeometry = test.GetFlattenedPathGeometry(toleranceFactor, ToleranceType.Relative);
                iconShape.FillSurfaceList();
                iconShape.Draw(verticalAlignment);
                double scaleRatio = markingData.MarkingHeight / iconShape.PathGeometry.Bounds.Height;

                Reference startAxisSystemRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(startAxisSystem);

                HybridBody iconSet = markingSet.HybridBodies.Add();
                iconSet.set_Name("Icon");

                Compute(iconShape, scaleRatio, keepHistoric, createVolume, iconSet, hybridShapeFactory, shapeFactory, markingData, markingBody, originPointRef, originAxisSystemRef,
                    startAxisSystem, startAxisSystemRef, projectionSurfaceRef, startPointRef);
            }



            if (!keepHistoric)
            {
                hybridShapeFactory.DeleteObjectForDatum(catiaPart.PartDocument.Part.CreateReferenceFromObject(pointSet));
                hybridShapeFactory.DeleteObjectForDatum(catiaPart.PartDocument.Part.CreateReferenceFromObject(localAxisSet));
                hybridShapeFactory.DeleteObjectForDatum(originAxisSystemRef);
            }

            if (createVolume)
            {
                // Add or remove marking body from main body
                catiaPart.PartDocument.Part.InWorkObject = catiaPart.PartDocument.Part.MainBody;
                if (markingData.ExtrusionHeight > 0)
                {
                    _ = shapeFactory.AddNewAdd(markingBody);
                }
                else
                {
                    _ = shapeFactory.AddNewRemove(markingBody);
                }

                // Updates
                catiaPart.PartDocument.Part.Update();
            }
            else
                hybridShapeFactory.DeleteObjectForDatum(catiaPart.PartDocument.Part.CreateReferenceFromObject(markingBody));


                // ***** END ***** //

        }



        private void Compute(CatiaDrawableShape drawableShape, double scaleRatio, bool keepHistoric, bool createVolume, HybridBody set,
            HybridShapeFactory hybridShapeFactory, ShapeFactory shapeFactory,
            MarkingData markingData, Body markingBody,
            Reference originPointRef, Reference originAxisSystemRef,
            AxisSystem axisSystem, Reference axisSystemRef, Reference projectionSurfaceRef, Reference pointRef)
        {

            // For each surface composing the CatiaCharacter
            foreach (CatiaSurface surface in drawableShape.SurfaceList)
            {
                // Scale external contour
                surface.ExternalContour.Scale(scaleRatio, originPointRef, keepHistoric, set);

                // Move external contour
                surface.ExternalContour.Move(originAxisSystemRef, axisSystemRef, keepHistoric, set);

                // Project external contour
                surface.ExternalContour.Project(projectionSurfaceRef, keepHistoric, set);


                foreach (CatiaContour contour in surface.InternalContourList)
                {
                    // Scale external contour
                    contour.Scale(scaleRatio, originPointRef, keepHistoric, set);

                    // Move external contour
                    contour.Move(originAxisSystemRef, axisSystemRef, keepHistoric, set);

                    // Project external contour
                    contour.Project(projectionSurfaceRef, keepHistoric, set);

                }

                surface.ComputeSurface(projectionSurfaceRef);
            }

            drawableShape.AssembleSurfaces();


            // IF SETTING KEEP HISTORY IS NOT CHECKED
            if (!keepHistoric)
            {
                HybridShapeSurfaceExplicit surfaceWithoutHistory = hybridShapeFactory.AddNewSurfaceDatum(drawableShape.ShapeReference);
                surfaceWithoutHistory.Compute();
                drawableShape.Shape = surfaceWithoutHistory;
            }

            set.AppendHybridShape(drawableShape.Shape);


            // IF SETTING CREATE VOLUME IS CHECKED
            if (createVolume)
            {
                int thickSurfaceDirection = GetThickSurfaceDirection(axisSystem, drawableShape.ShapeReference, pointRef, hybridShapeFactory);
                drawableShape.PartDocument.Part.InWorkObject = markingBody;
                _ = shapeFactory.AddNewThickSurface(drawableShape.ShapeReference, thickSurfaceDirection, markingData.ExtrusionHeight, 0);
            }

        }



        /// <summary>
        /// Get the character geometry, draw it in Catia and retrieves it.
        /// </summary>
        /// <param name="partDocument">Working part document.</param>
        /// <param name="markingData">Marking datas.</param>
        /// <param name="character">Character to draw.</param>
        /// <param name="characterList">List of characters. Take the character from the list if it already exists.</param>
        /// <param name="toleranceFactor">Define the precision of the drawing.</param>
        /// <returns></returns>
        private CatiaCharacter GetCatiaCharacter(PartDocument partDocument, MarkingData markingData, char character,
            List<CatiaCharacter> characterList, double toleranceFactor, VerticalAlignment verticalAlignment, PathGeometry refCharacterGeometry)
        {
            CatiaCharacter catiaCharacter = new CatiaCharacter(partDocument, character);

            if (!characterList.Contains(catiaCharacter))
            {
                characterList.Add(catiaCharacter);
                catiaCharacter.PathGeometry = markingData.FontFamily.GetCharacterGeometry(catiaCharacter.Value, toleranceFactor, markingData.IsBold, markingData.IsItalic);
                catiaCharacter.FillSurfaceList();

                if (!catiaCharacter.IsSpaceCharacter)
                {
                    catiaCharacter.Draw(verticalAlignment, refCharacterGeometry);
                }
            }

            if (character == ' ')
                return (characterList.Find((ch) => ch.IsSpaceCharacter)).Clone();
            else
                return (characterList.Find((ch) => ch.Value == character)).Clone();
        }


        /// <summary>
        /// Create the local point to place the character following kerning values.
        /// </summary>
        /// <param name="character">Character to write.</param>
        /// <param name="previousCharacter">Previous character in order to compute kerning.</param>
        /// <param name="markingData">Marking datas.</param>
        /// <param name="trackingCurveRef">Reference of the curve in which the point will be placed.</param>
        /// <param name="startPointRef">Reference of the reference point.</param>
        /// <param name="pointSet">Set where the point will be stored.</param>
        /// <param name="isXSameDirection">Define the direction on the curve.</param>
        /// <param name="currentLength">Length of the previous point on the curve. This parameter will be updated to the new point localization.</param>
        /// <param name="scaleRatio">Scale ratio between the initial character form and the scaled one.</param>
        /// <returns></returns>
        private Reference ComputeLocalPoint(CatiaCharacter character, CatiaCharacter previousCharacter, MarkingData markingData,
            Reference trackingCurveRef, Reference startPointRef, HybridBody pointSet,
            bool isXSameDirection, ref double currentLength, double scaleRatio)
        {
            // Local point on the tracking curve positionning the current character
            if (previousCharacter != null)
            {
                if (character.IsSpaceCharacter || previousCharacter.IsSpaceCharacter)
                    currentLength += scaleRatio * previousCharacter.PathGeometry.Bounds.Width;

                else
                {
                    double kerningValue = 0d;

                    Int16 firstCharInt = Convert.ToInt16(previousCharacter.Value);
                    Int16 secondCharInt = Convert.ToInt16(character.Value);

                    foreach (FontFamily.KerningPair pair in markingData.FontFamily.KerningPairs)
                    {
                        if (pair.wFirst == firstCharInt && pair.wSecond == secondCharInt)
                        {
                            kerningValue = Convert.ToDouble(pair.iKernAmount) / 1000;

                            break;
                        }
                    }

                    currentLength += scaleRatio * previousCharacter.PathGeometry.Bounds.Width
                        + markingData.FontFamily.GetRightSideBearing(previousCharacter.Value, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic)
                        + markingData.FontFamily.GetLeftSideBearing(character.Value, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic)
                        + markingData.MarkingHeight * kerningValue;
                }
            }


            HybridShapePointOnCurve localPoint = character.HybridShapeFactory.AddNewPointOnCurveWithReferenceFromDistance(trackingCurveRef, startPointRef, currentLength, !isXSameDirection);
            localPoint.Compute();
            Reference localPointRef = character.PartDocument.Part.CreateReferenceFromObject(localPoint);
            pointSet.AppendHybridShape(localPoint);

            return localPointRef;
        }


        /// <summary>
        /// Create the local axis system in order to orientate the character.
        /// </summary>
        /// <param name="partDocument">Working part document.</param>
        /// <param name="localAxisSystemSet">Set where the local axis system will be stored.</param>
        /// <param name="localPointRef">Reference of the origin point.</param>
        /// <param name="projectionSurfaceRef">Reference of the tangential surface (Plane XY).</param>
        /// <param name="trackingCurveRef">Reference of the tangential curve (X Axis).</param>
        /// <param name="isXSameDirection">Define the X axis sens.</param>
        /// <param name="isYSameDirection">Define the Y axis sens.</param>
        /// <param name="isZSameDirection">Define the Z axis sens.</param>
        /// <returns></returns>
        private AxisSystem ComputeLocalAxisSystem(PartDocument partDocument, HybridBody localAxisSystemSet, Reference localPointRef, Reference projectionSurfaceRef, Reference trackingCurveRef,
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

        private enum LineType
        {
            Underline,
            StrikeThrough
        }

        private void DrawLine(CatiaPart catiaPart, LineType lineType, MarkingData markingData, VerticalAlignment verticalAlignment, PathGeometry refCharacterGeometry,
            HybridBody lineBody, double scaleRatio, bool isYSameDirection, bool keepHistoric, bool createVolume,
            Reference trackingCurveRef, Reference projectionSurfaceRef, HybridShapeFactory hybridShapeFactory)
        {
            double yOffset = 0;
            (double position, double thickness) lineValue = (position: 0d, thickness: 0d);

            switch (lineType)
            {
                case LineType.Underline:
                    {
                        lineValue = markingData.FontFamily.GetUnderline(markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);
                        yOffset = -CatiaDrawableShape.GetYOffset(verticalAlignment, refCharacterGeometry);


                        break;
                    }

                case LineType.StrikeThrough:
                    {
                        lineValue = markingData.FontFamily.GetStrikeThrough(markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);
                        yOffset = CatiaDrawableShape.GetYOffset(verticalAlignment, refCharacterGeometry);


                        break;
                    }
            }

            HybridShapeCurvePar offsetCurve = hybridShapeFactory.AddNewCurvePar(trackingCurveRef, projectionSurfaceRef, yOffset * scaleRatio, !isYSameDirection, false);
            offsetCurve.Compute();
            Reference offsetCurveRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(offsetCurve);

            HybridShapeCurvePar positionCurve = hybridShapeFactory.AddNewCurvePar(offsetCurveRef, projectionSurfaceRef, lineValue.position, !isYSameDirection, false);
            positionCurve.Compute();
            Reference positionCurveRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(positionCurve);

            HybridShapeSweepLine lineSurfaceFull = hybridShapeFactory.AddNewSweepLine(positionCurveRef);
            lineSurfaceFull.FirstGuideSurf = projectionSurfaceRef;
            lineSurfaceFull.Mode = 4;
            lineSurfaceFull.SetFirstLengthLaw(lineValue.thickness / 2, 0d, 0);
            lineSurfaceFull.SetSecondLengthLaw(lineValue.thickness / 2, 0d, 0);
            lineSurfaceFull.SetAngle(1, 0d);
            lineSurfaceFull.SolutionNo = 0;
            lineSurfaceFull.SmoothActivity = false;
            lineSurfaceFull.GuideDeviationActivity = false;
            lineSurfaceFull.SetbackValue = 0.02;
            lineSurfaceFull.FillTwistedAreas = 1;
            lineSurfaceFull.Compute();


            if (keepHistoric)
            {
                offsetCurve.set_Name("Offset_Curve");
                lineBody.AppendHybridShape(offsetCurve);

                positionCurve.set_Name("Position_Curve");
                lineBody.AppendHybridShape(positionCurve);

                lineBody.AppendHybridShape(lineSurfaceFull);
            }
            else
            {
                Reference lineSurfaceFullRef = catiaPart.PartDocument.Part.CreateReferenceFromObject(lineSurfaceFull);
                HybridShapeSurfaceExplicit surfaceWithoutHistory = hybridShapeFactory.AddNewSurfaceDatum(lineSurfaceFullRef);
                surfaceWithoutHistory.Compute();
                lineBody.AppendHybridShape(surfaceWithoutHistory);
            }


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

