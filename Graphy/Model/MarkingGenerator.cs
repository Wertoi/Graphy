using System;
using System.Collections.Generic;
using System.Linq;
using INFITF;
using MECMOD;
using PARTITF;
using HybridShapeTypeLib;
using Graphy.Model.CatiaObject;
using Graphy.Model.CatiaObject.CatiaShape;
using System.Windows.Media;
using Graphy.Enum;

namespace Graphy.Model
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


        public void RunForCollection(CatiaEnv catiaEnv, ICollection<MarkablePart> markablePartCollection,
            double toleranceFactor, bool keepHistoric, bool createVolume, Enum.HorizontalAxisSystemPosition horizontalAxisSystemPosition)
        {
            Step = 1;

            IEnumerable<IGrouping<CatiaPart, MarkablePart>> groupedMarkablePartList = markablePartCollection.GroupBy(markablePart => markablePart.CatiaPart);
            foreach (IGrouping<CatiaPart, MarkablePart> groupedMarkablePart in groupedMarkablePartList)
            {
                CatiaPart catiaPart = groupedMarkablePart.Key;
                if (catiaPart.TryOpenDocument(catiaEnv, catiaPart.FileFullPath))
                {
                    List<MarkablePart> partMarkablePartList = groupedMarkablePart.Select(markablePart => markablePart).ToList();
                    List<CatiaCharacter> drawnCharacterList = new List<CatiaCharacter>();

                    foreach (MarkablePart markablePart in partMarkablePartList)
                    {
                        Run(catiaPart, markablePart.MarkingData, drawnCharacterList, toleranceFactor, keepHistoric, createVolume, horizontalAxisSystemPosition);
                        Step++;
                    }

                    //catiaPart.CloseDocument(true);
                }
            }
        }

        public void Run(CatiaPart catiaPart, MarkingData markingData, List<CatiaCharacter> characterList,
            double toleranceFactor, bool keepHistoric, bool createVolume, Enum.HorizontalAxisSystemPosition horizontalAxisSystemPosition)
        {
            ProgressRate = 0;

            if (Step == 0)
                Step = 1;


            // ***** PREPARE MARKING ***** //
            #region

            // Creates the marking set
            HybridBody markingSet = catiaPart.PartDocument.Part.HybridBodies.Add();
            markingSet.set_Name(markingData.Name);

            // Creates the origin axis system to the marking set
            CatiaAxisSystem originAxisSystem = CatiaAxisSystem.GetOriginAxisSystem(catiaPart.PartDocument, markingSet);

            // Creates the point set
            HybridBody pointSet = markingSet.HybridBodies.Add();
            pointSet.set_Name("Points");

            // Creates the local axis system set
            HybridBody localAxisSet = markingSet.HybridBodies.Add();
            localAxisSet.set_Name("Axis systems");

            // Creates the input data references

            CatiaSurface supportSurface = CatiaSurface.GetCatiaSurface(catiaPart.PartDocument, markingData.ProjectionSurfaceName);
            CatiaCurve trackingCurve = CatiaCurve.GetCatiaCurve(catiaPart.PartDocument, markingData.TrackingCurveName, supportSurface);
            CatiaPoint referencePoint = CatiaPoint.GetCatiaPoint(catiaPart.PartDocument, markingData.ReferencePointName);
            CatiaAxisSystem referenceAxisSystem = CatiaAxisSystem.GetCatiaAxisSystem(catiaPart.PartDocument, markingData.AxisSystemName);


            // Factories
            HybridShapeFactory hybridShapeFactory = (HybridShapeFactory)catiaPart.PartDocument.Part.HybridShapeFactory;
            ShapeFactory shapeFactory = (ShapeFactory)catiaPart.PartDocument.Part.ShapeFactory;

            // Creates the origin point
            CatiaPoint originPoint = new CatiaPoint(catiaPart.PartDocument, 0, 0, 0);

            // Creates the marking body
            Body markingBody = catiaPart.PartDocument.Part.Bodies.Add();
            markingBody.set_Name("MARKING BODY");


            // Updates
            catiaPart.PartDocument.Part.Update();

            #endregion
            // ***** END OF PREPARATIONS ***** //




            // Create the natural normal line to the support surface passing by the reference point
            // Then compare its direction with the reference axis system Z direction
            CatiaLine naturalNormalLine2 = supportSurface.GetNaturalNormalLine(referencePoint);
            bool isZSameDirection = IsSameDirection(referenceAxisSystem.ZDirection, naturalNormalLine2.LineDirection);

            // Create the natural radial line to the tracking curve passing by the reference point
            // Then compare its direction with the reference axis system Y direction
            CatiaLine naturalRadialLine2 = trackingCurve.GetNaturalRadialLine(referencePoint);
            bool isYSameDirection = IsSameDirection(referenceAxisSystem.YDirection, naturalRadialLine2.LineDirection);

            // Create the natural tangent line to the tracking curve passing by the reference point
            // Then compare its direction with the reference axis system X direction
            CatiaLine naturalTangentLine2 = trackingCurve.GetNaturalTangentLine(referencePoint);
            bool isXSameDirection = IsSameDirection(referenceAxisSystem.XDirection, naturalTangentLine2.LineDirection);



            // ***** PREPARE THE DATAS WHETER WE DRAW TEXT OR ICON ***** //

            // DRAW TEXT
            if (markingData.IsText)
            {
                // ***** PREPARE DATAS FOR MARKING ***** //
                #region


                // Initialize a counter
                int characterIndex = 0;

                // Compute the scale ratio to obtain the character height with a fixed character reference
                // 'M' character is used as reference.
                CatiaCharacter refCharacter = new CatiaCharacter(catiaPart.PartDocument, FontFamily.REFERENCE_CHARACTER, markingData.FontFamily,
                    markingData.IsBold, markingData.IsItalic);

                refCharacter.ComputeGeometry(markingData.FontFamily, toleranceFactor);
                double scaleRatio = markingData.MarkingHeight / refCharacter.PathGeometry.Bounds.Height;

                // Create a list to store points references in order to create underline and strike through if necessary
                List<CatiaPoint> constructionPointList = new List<CatiaPoint>();

                // Create a list to store local axis system references in order to delete them if keep historic option is uncheck.
                List<CatiaAxisSystem> constructionAxisSystemList = new List<CatiaAxisSystem>();

                // Creates the working set
                HybridBody writingSet = markingSet.HybridBodies.Add();
                writingSet.set_Name("Characters construction");

                // Compute the kerningPair table
                markingData.FontFamily.ComputeKerningPairs();


                // ***** COMPUTE THE CHARACTER POSITIONS AND THE TOTAL LENGTH ***** //
                #region

                // Create a list that will store the positions of the character in the same order.
                List<double> absoluteCharacterPositionList = new List<double>();

                // First character always begin at 0...
                absoluteCharacterPositionList.Add(0);

                // ... So start at 1.
                for (int i = 1; i < markingData.Text.Count(); i++)
                {
                    // Positions are absolute; it represents the distance from the starting point
                    absoluteCharacterPositionList.Add(absoluteCharacterPositionList[i - 1] + GetCharacterRelativePosition(markingData.Text[i - 1], markingData.Text[i], markingData, horizontalAxisSystemPosition));
                }

                // Compute the total width
                // CharacterWidth = AdvanceWidth - LeftSideBearing - RightSideBearing
                double totalWidth = GetTotalWidth(absoluteCharacterPositionList.Last(), markingData, markingData.Text.First(), markingData.Text.Last(), horizontalAxisSystemPosition);

                #endregion
                // ***** END OF POSITIONS COMPUTATION ***** //

                // Get the start point
                CatiaPoint startPoint = GetStartPoint(catiaPart, totalWidth, markingData, trackingCurve, referencePoint, isXSameDirection);

                // Create the line first point if the horizontal position is different of "Left"
                // If the horizontal axis system position is left, there is no need for a correction of the start point.
                // Otherwise we have a superposition of the first local point with the first line point which generates a crash of Catia.
                if (horizontalAxisSystemPosition != HorizontalAxisSystemPosition.Left)
                {
                    CatiaPoint lineStartPoint = new CatiaPoint(catiaPart.PartDocument, trackingCurve, startPoint,
                        GetStartPointCorrectif(markingData, markingData.Text.First(), horizontalAxisSystemPosition),
                        isXSameDirection);
                    pointSet.AppendHybridShape(lineStartPoint.Shape);
                    constructionPointList.Add(lineStartPoint);
                }

                #endregion
                // ***** END OF PREPARATION ***** //


                // Create a catia character list from input text and character height
                foreach (char character in markingData.Text)
                {
                    // Get the associatedCatiaCharacter
                    CatiaCharacter catiaCharacter = GetCatiaCharacter(catiaPart.PartDocument, markingData, character, characterList, toleranceFactor, horizontalAxisSystemPosition);

                    // *** NOTE ***
                    // The local point is constructed even if the character is a space character.
                    // This point will allow the construction of the spline for underline and strike through lines.
                    // Without theses points, if multiple spaces are used, we can lose the correct tracking curve.
                    // I know this is not perfect but it is a correct approximation easier than cuting in the right direction the tracking curve.

                    // Create the local point
                    CatiaPoint localPoint = new CatiaPoint(catiaPart.PartDocument, trackingCurve, startPoint,
                        absoluteCharacterPositionList[characterIndex], !isXSameDirection);

                    // Append the point to the pointSet
                    pointSet.AppendHybridShape(localPoint.Shape);

                    // Store the point in the list for text decorations construction later
                    constructionPointList.Add(localPoint);

                    if (!catiaCharacter.IsSpaceCharacter)
                    {
                        // Create the local axis system
                        CatiaAxisSystem localAxisSystem = new CatiaAxisSystem(catiaPart.PartDocument, localPoint, trackingCurve, supportSurface,
                            isXSameDirection, isYSameDirection, isZSameDirection, localAxisSet);

                        // Store the local axis system reference for deletion later if necessary
                        constructionAxisSystemList.Add(localAxisSystem);

                        // Compute the Y offset depending on the vertical alignment
                        double yOffset = CatiaDrawableShape.GetYOffset(markingData.VerticalAlignment, refCharacter.PathGeometry);

                        // If we do not keep the construction historic
                        if (!keepHistoric)
                        {
                            ComputeDrawing(catiaPart, catiaCharacter, scaleRatio, keepHistoric, createVolume, writingSet, markingData, markingBody,
                                yOffset, originPoint, originAxisSystem, localAxisSystem, supportSurface, localPoint);
                        }
                        else
                        {
                            // Create the character set
                            HybridBody characterSet = writingSet.HybridBodies.Add();
                            characterSet.set_Name(character.ToString() + "." + characterIndex);

                            ComputeDrawing(catiaPart, catiaCharacter, scaleRatio, keepHistoric, createVolume, characterSet, markingData, markingBody,
                                yOffset, originPoint, originAxisSystem, localAxisSystem, supportSurface, localPoint);
                        }

                    }

                    ProgressRate += 1d / (double)markingData.Text.Count();
                    characterIndex++;
                }


                // If Underligned OR Strike through
                if (markingData.IsUnderline || markingData.IsStrikeThrough)
                {
                    #region PREPARATION OF DRAW LINE

                    // Create the final point if the horizontal axis system position is different of "Right"
                    // If the horizontal axis system position is Right, there is no need to generate the last point.
                    // Otherwise we have a superposition of the last local point with the final line point which generates a crash of Catia.
                    if (horizontalAxisSystemPosition != HorizontalAxisSystemPosition.Right)
                    {
                        CatiaPoint finalPoint = new CatiaPoint(catiaPart.PartDocument, trackingCurve, startPoint, totalWidth, !isXSameDirection);
                        pointSet.AppendHybridShape(finalPoint.Shape);

                        // Store the point in the list in order to create the baseline
                        constructionPointList.Add(finalPoint);
                    }

                    // Create a curve passing by all the points in the local point list
                    CatiaCurve baseline = new CatiaCurve(catiaPart.PartDocument, constructionPointList, supportSurface);

                    // Retrieve its natural radial line passing by the reference point
                    CatiaLine baselineNaturalRadialLine = baseline.GetNaturalRadialLine(referencePoint);

                    // Compare the reference axis system Y direction with the baseline natural radial direction
                    // NOTE: The baseline constructed is a new object so we need to verify its direction
                    bool isBaselineYSameDirection = IsSameDirection(referenceAxisSystem.YDirection, baselineNaturalRadialLine.LineDirection);

                    #endregion

                    // If Underligned
                    if (markingData.IsUnderline)
                    {
                        // Create the underline set
                        HybridBody underlineSet = markingSet.HybridBodies.Add();
                        underlineSet.set_Name("Underline");

                        // Compute the underline position curve and thickness
                        (CatiaCurve positionCurve, double textDecorationThickness) = GetTextDecoration(catiaPart, LineType.Underline, markingData,
                            refCharacter.PathGeometry, scaleRatio, isBaselineYSameDirection, baseline, supportSurface);

                        // Draw the underline surface
                        CatiaSurface underlineSurface = DrawTextDecoration(catiaPart, positionCurve, textDecorationThickness, supportSurface, keepHistoric, underlineSet);


                        if (createVolume)
                        {
                            // Create a temp point at the middle of the position curve
                            CatiaPoint tempPoint = new CatiaPoint(catiaPart.PartDocument, positionCurve, 0.5, true);

                            // Create a temp axis system
                            CatiaAxisSystem tempAxisSystem = new CatiaAxisSystem(catiaPart.PartDocument, tempPoint, positionCurve, supportSurface,
                                isXSameDirection, isYSameDirection, isZSameDirection, localAxisSet);

                            // Add the temp axis system to the list for deletion if necessary
                            constructionAxisSystemList.Add(tempAxisSystem);

                            // Create the volume
                            CreateVolume(catiaPart, markingData, tempPoint, tempAxisSystem, underlineSurface, markingBody);
                        }
                    }


                    // If Strike through
                    if (markingData.IsStrikeThrough)
                    {
                        // Create the strike through set
                        HybridBody strikeThroughSet = markingSet.HybridBodies.Add();
                        strikeThroughSet.set_Name("StrikeThrough");

                        // Compute the strike through position curve and thickness
                        (CatiaCurve positionCurve, double textDecorationThickness) = GetTextDecoration(catiaPart, LineType.StrikeThrough, markingData,
                            refCharacter.PathGeometry, scaleRatio, isBaselineYSameDirection, baseline, supportSurface);

                        // Draw the underline surface
                        CatiaSurface strikeThroughSurface = DrawTextDecoration(catiaPart, positionCurve, textDecorationThickness, supportSurface, keepHistoric, strikeThroughSet);

                        if (createVolume)
                        {
                            // Create a temp point at the middle of the position curve
                            CatiaPoint tempPoint = new CatiaPoint(catiaPart.PartDocument, positionCurve, 0.5, true);

                            // Create a temp axis system
                            CatiaAxisSystem tempAxisSystem = new CatiaAxisSystem(catiaPart.PartDocument, tempPoint, positionCurve, supportSurface,
                                isXSameDirection, isYSameDirection, isZSameDirection, localAxisSet);

                            // Add the temp axis system to the list for deletion if necessary
                            constructionAxisSystemList.Add(tempAxisSystem);

                            // Create the volume
                            CreateVolume(catiaPart, markingData, tempPoint, tempAxisSystem, strikeThroughSurface, markingBody);
                        }
                    }
                }


                // Remove construction axis systems
                if (!keepHistoric)
                {
                    foreach (CatiaAxisSystem axisSystem in constructionAxisSystemList)
                    {
                        hybridShapeFactory.DeleteObjectForDatum(axisSystem.SystemReference);
                    }
                }


            }

            // DRAW ICON
            else
            {
                // Create the icon shape object
                CatiaDrawableShape iconShape = new CatiaDrawableShape(catiaPart.PartDocument);

                // Retrieve the geometry from the input
                Geometry iconGeometry = Geometry.Parse(markingData.Icon.PathData);

                // Assign the geometry to the icon shape object
                iconShape.PathGeometry = iconGeometry.GetFlattenedPathGeometry(toleranceFactor, ToleranceType.Relative);

                // Compute the surfaces
                iconShape.FillSurfaceList();

                // Draw the icon
                // For icon drawing, the setting Horizontal Axis System Position is disabled; it does the same thing as for Horizontal Alignment.
                iconShape.Draw(Enum.HorizontalAxisSystemPosition.Left);

                // Compute the scale ratio between the target height and the drawing height
                double scaleRatio = markingData.MarkingHeight / iconShape.PathGeometry.Bounds.Height;

                // Create the start point depending on the horizontal alignment
                CatiaPoint startPoint = GetStartPoint(catiaPart, iconShape.PathGeometry.Bounds.Width * scaleRatio, markingData,
                    trackingCurve, referencePoint, isXSameDirection);

                // Create the axis system at the start point
                CatiaAxisSystem localAxisSystem = new CatiaAxisSystem(catiaPart.PartDocument, startPoint, trackingCurve, supportSurface,
                    isXSameDirection, isYSameDirection, isZSameDirection, localAxisSet);

                // Create a set where the icon construction steps are stored
                HybridBody iconSet = markingSet.HybridBodies.Add();
                iconSet.set_Name("Icon");

                // Compute the y offset depending on the vertical alignment
                double yOffset = CatiaDrawableShape.GetYOffset(markingData.VerticalAlignment, iconShape.PathGeometry);

                ComputeDrawing(catiaPart, iconShape, scaleRatio, keepHistoric, createVolume, iconSet, markingData, markingBody, yOffset,
                    originPoint, originAxisSystem, localAxisSystem, supportSurface, startPoint);
                /*Compute(iconShape, scaleRatio, keepHistoric, createVolume, iconSet, hybridShapeFactory, shapeFactory, markingData, markingBody, originPointRef, originAxisSystemRef,
                    localAxisSystem.System, localAxisSystem.SystemReference, projectionSurfaceRef, referencePointRef);*/

                // If we do not keep the construction historic
                if (!keepHistoric)
                {
                    // Delete the created axis system
                    hybridShapeFactory.DeleteObjectForDatum(localAxisSystem.SystemReference);
                }
            }



            if (!keepHistoric)
            {
                hybridShapeFactory.DeleteObjectForDatum(catiaPart.PartDocument.Part.CreateReferenceFromObject(pointSet));
                hybridShapeFactory.DeleteObjectForDatum(catiaPart.PartDocument.Part.CreateReferenceFromObject(localAxisSet));
                hybridShapeFactory.DeleteObjectForDatum(originAxisSystem.SystemReference);
            }

            // If we create a volume
            if (createVolume)
            {
                // Add or remove the marking part body to/from the main body
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
                // Delete the marking part body
                hybridShapeFactory.DeleteObjectForDatum(catiaPart.PartDocument.Part.CreateReferenceFromObject(markingBody));


            // ***** END ***** //

        }




        private void ComputeDrawing(CatiaPart catiaPart, CatiaDrawableShape drawableShape, double scaleRatio, bool keepHistoric, bool createVolume, HybridBody set,
            MarkingData markingData, Body markingBody, double yOffset,
            CatiaPoint originPoint, CatiaAxisSystem originAxisSystem, CatiaAxisSystem localAxisSystem, CatiaSurface projectionSurface, CatiaPoint localPoint)
        {

            // For each surface composing the CatiaCharacter
            foreach (CatiaSurface surface in drawableShape.SurfaceList)
            {
                // Translate in Y axis for vertical alignment
                surface.ExternalContour.Translate(0d, yOffset, 0d, originAxisSystem, keepHistoric, set);

                // Scale external contour
                surface.ExternalContour.Scale(scaleRatio, originPoint, keepHistoric, set);

                // Move external contour
                surface.ExternalContour.Move(originAxisSystem, localAxisSystem, keepHistoric, set);

                // Project external contour
                surface.ExternalContour.Project(projectionSurface, keepHistoric, set);


                foreach (CatiaContour contour in surface.InternalContourList)
                {
                    // Translate in Y axis for vertical alignment
                    contour.Translate(0d, yOffset, 0d, originAxisSystem, keepHistoric, set);

                    // Scale external contour
                    contour.Scale(scaleRatio, originPoint, keepHistoric, set);

                    // Move external contour
                    contour.Move(originAxisSystem, localAxisSystem, keepHistoric, set);

                    // Project external contour
                    contour.Project(projectionSurface, keepHistoric, set);

                }

                surface.ComputeSurface(projectionSurface.ShapeReference);
            }

            CatiaSurface assembledSurface = drawableShape.GetAssembleSurfaces();


            // IF SETTING KEEP HISTORY IS NOT CHECKED
            if (!keepHistoric)
            {
                assembledSurface = assembledSurface.GetSurfaceWithoutLink();
            }

            set.AppendHybridShape(assembledSurface.Shape);


            // IF SETTING CREATE VOLUME IS CHECKED
            if (createVolume)
            {
                CreateVolume(catiaPart, markingData, localPoint, localAxisSystem, assembledSurface, markingBody);
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
            List<CatiaCharacter> characterList, double toleranceFactor, Enum.HorizontalAxisSystemPosition horizontalAxisSystemPosition)
        {
            // Create a new catia character
            CatiaCharacter catiaCharacter = new CatiaCharacter(partDocument, character, markingData.FontFamily, markingData.IsBold, markingData.IsItalic);

            if (catiaCharacter.IsSpaceCharacter)
                return catiaCharacter;
            else
            {
                // If the list does not contain this character
                if (!characterList.Contains(catiaCharacter))
                {
                    // Add the character to the list
                    characterList.Add(catiaCharacter);

                    // Retrieve the geometry of the character
                    catiaCharacter.ComputeGeometry(markingData.FontFamily, toleranceFactor);

                    // Compute surfaces of the geometry
                    catiaCharacter.FillSurfaceList();

                    // Draw the character
                    catiaCharacter.Draw(horizontalAxisSystemPosition);
                }

                // Return a clone to preserve character in the list from future modifications
                return (characterList.Find((c) => c.Equals(catiaCharacter))).Clone();
            }
        }

        /// <summary>
        /// See for stack-up
        /// https://docs.microsoft.com/fr-fr/dotnet/api/system.windows.media.glyphtypeface?view=net-5.0
        /// </summary>
        /// <param name="previousCharacter"></param>
        /// <param name="currentCharacter"></param>
        /// <param name="markingData"></param>
        /// <param name="horizontalAxisSystemPosition"></param>
        /// <returns></returns>
        private double GetCharacterRelativePosition(char previousCharacter, char currentCharacter, MarkingData markingData,
            Enum.HorizontalAxisSystemPosition horizontalAxisSystemPosition)
        {
            // Get the kerning value between the previous character and the current one.
            double kerningValue = markingData.FontFamily.GetKerningValue(previousCharacter, currentCharacter) * markingData.MarkingHeight;

            // Retrieve the previous character right side bearing
            // If previous character right side bearing is negative, it is not considered.
            double previousCharacterRightSideBearing = markingData.FontFamily.GetRightSideBearing(previousCharacter, markingData.MarkingHeight,
                        markingData.IsBold, markingData.IsItalic);
            previousCharacterRightSideBearing = previousCharacterRightSideBearing < 0 ? 0 : previousCharacterRightSideBearing;

            // Retrieve the current character left side bearing
            // If current character left side bearing is negative, it is not considered.
            double currentCharacterLeftSideBearing = markingData.FontFamily.GetLeftSideBearing(currentCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);
            currentCharacterLeftSideBearing = currentCharacterLeftSideBearing < 0 ? 0 : currentCharacterLeftSideBearing;

            // Compute the previous charactere real width
            double previousCharacterWidth = markingData.FontFamily.GetRealWidth(previousCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);

            // Compute the current character real width
            double currentCharacterWidth = markingData.FontFamily.GetRealWidth(currentCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);


            switch (horizontalAxisSystemPosition)
            {
                case Enum.HorizontalAxisSystemPosition.Left:
                    return previousCharacterWidth + previousCharacterRightSideBearing + currentCharacterLeftSideBearing + kerningValue;

                case HorizontalAxisSystemPosition.Center:
                    return previousCharacterWidth / 2 + previousCharacterRightSideBearing + currentCharacterLeftSideBearing + currentCharacterWidth / 2 + kerningValue;

                case HorizontalAxisSystemPosition.Right:
                    return previousCharacterRightSideBearing + currentCharacterWidth + currentCharacterLeftSideBearing + kerningValue;

                default:
                    return 0d;
            }
        }

        private double GetTotalWidth(double lastRelativePosition, MarkingData markingData, char firstCharacter, char lastCharacter,
            Enum.HorizontalAxisSystemPosition horizontalAxisSystemPosition)
        {
            switch (horizontalAxisSystemPosition)
            {
                case HorizontalAxisSystemPosition.Left:
                    return lastRelativePosition
                        + markingData.FontFamily.GetRealWidth(lastCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);

                case HorizontalAxisSystemPosition.Center:
                    return lastRelativePosition
                        + markingData.FontFamily.GetRealWidth(lastCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic) / 2;

                case HorizontalAxisSystemPosition.Right:
                    return lastRelativePosition;

                default:
                    return lastRelativePosition
                        + markingData.FontFamily.GetRealWidth(lastCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);
            }
        }

        private double GetStartPointCorrectif(MarkingData markingData, char firstCharacter, Enum.HorizontalAxisSystemPosition horizontalAxisSystemPosition)
        {
            switch (horizontalAxisSystemPosition)
            {
                case HorizontalAxisSystemPosition.Left:
                    return 0;

                case HorizontalAxisSystemPosition.Center:
                    return markingData.FontFamily.GetRealWidth(firstCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic) / 2;

                case HorizontalAxisSystemPosition.Right:
                    return markingData.FontFamily.GetRealWidth(firstCharacter, markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);

                default:
                    return 0;
            }
        }


        private enum LineType
        {
            Underline,
            StrikeThrough
        }


        private (CatiaCurve positionCurve, double thickness) GetTextDecoration(CatiaPart catiaPart, LineType lineType, MarkingData markingData,
            PathGeometry refCharacterGeometry, double scaleRatio, bool isYSameDirection, CatiaCurve baseline, CatiaSurface supportSurface)
        {
            // Retrieve the underline and strike through line position and thickness from font file
            (double position, double thickness) lineValue = (position: 0d, thickness: 0d);

            switch (lineType)
            {
                case LineType.Underline:
                    {
                        lineValue = markingData.FontFamily.GetUnderline(markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);

                        break;
                    }

                case LineType.StrikeThrough:
                    {
                        lineValue = markingData.FontFamily.GetStrikeThrough(markingData.MarkingHeight, markingData.IsBold, markingData.IsItalic);

                        break;
                    }
            }

            // Compute the offset baseline
            // Characters are drawn direclty on the tracking curve.
            // In reality there is a little space between the baseline and the characters (see BottomSideBearing).
            // So to compensate:
            // If the offset > 0 (meaning there is a gap between the baseline and the character), we want to move in -y.
            // If the offset < 0 (meaning there is clash between the baseline and the character), we want to move in y.
            double yOffset = CatiaDrawableShape.GetYOffset(markingData.VerticalAlignment, refCharacterGeometry);
            bool yOffsetDirection = yOffset > 0 ? !isYSameDirection : isYSameDirection;
            CatiaCurve offsetBaseline = new CatiaCurve(catiaPart.PartDocument, baseline, supportSurface, Math.Abs(yOffset) * scaleRatio, yOffsetDirection);


            // Compute the position of the underline or strike through curve.
            // The thickness is added symetricaly
            // If the position > 0 (we have an underline), we want to move in -y.
            // If the position < 0 (we have a strike through), we want to move in y. 
            bool textDecorationDirection = lineValue.position > 0 ? !isYSameDirection : isYSameDirection;
            CatiaCurve positionCurve = new CatiaCurve(catiaPart.PartDocument, offsetBaseline, supportSurface, Math.Abs(lineValue.position), textDecorationDirection);

            return (positionCurve, lineValue.thickness);
        }



        private CatiaSurface DrawTextDecoration(CatiaPart catiaPart, CatiaCurve positionCurve, double textDecorationThickness, CatiaSurface supportSurface,
            bool keepHistoric, HybridBody textDecorationSet)
        {
            // Create the underline surface
            CatiaSurface underlineSurface = new CatiaSurface(catiaPart.PartDocument, positionCurve, supportSurface, textDecorationThickness);

            // If we do not keep the construction historic
            if (!keepHistoric)
            {
                // Create a surface without link
                underlineSurface = underlineSurface.GetSurfaceWithoutLink();
            }

            // Append the underline surface to the set
            textDecorationSet.AppendHybridShape(underlineSurface.Shape);

            return underlineSurface;
        }




        private CatiaPoint GetStartPoint(CatiaPart catiaPart, double width, MarkingData markingData, CatiaCurve trackingCurve,
            CatiaPoint referencePoint, bool isXSameDirection)
        {
            // Compute the distance between the reference point and the starting point
            double startPointDistance;
            switch (markingData.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    startPointDistance = width / 2;
                    break;

                case HorizontalAlignment.Right:
                    startPointDistance = width;
                    break;

                default:
                    startPointDistance = 0;
                    break;
            }

            CatiaPoint startPoint = new CatiaPoint(catiaPart.PartDocument, trackingCurve, referencePoint, startPointDistance, isXSameDirection);

            return startPoint;
        }

        private void CreateVolume(CatiaPart catiaPart, MarkingData markingData, CatiaPoint point, CatiaAxisSystem axisSystem, CatiaSurface surfaceToThicken, Body markingBody)
        {
            ShapeFactory shapeFactory = (ShapeFactory)catiaPart.PartDocument.Part.ShapeFactory;

            // Create the natural normal line of the "line surface"
            CatiaLine naturalNormalLine = surfaceToThicken.GetNaturalNormalLine(point);

            // Compare the natural normal line of the "line surface" and the axis system Z direction
            bool isNormalSameDirection = IsSameDirection(axisSystem.ZDirection, naturalNormalLine.LineDirection);
            int thicknessDirection = isNormalSameDirection ? 0 : 1;

            catiaPart.PartDocument.Part.InWorkObject = markingBody;
            _ = shapeFactory.AddNewThickSurface(surfaceToThicken.ShapeReference, thicknessDirection, markingData.ExtrusionHeight, 0);
        }



        private bool IsSameDirection(object[] direction1, object[] direction2)
        {
            if (direction1.Length != 3 && direction2.Length != 3)
                return false;

            bool isSameDirection = false;
            if (Math.Abs((double)direction1[0] - (double)direction2[0]) < 0.1
                && Math.Abs((double)direction1[1] - (double)direction2[1]) < 0.1
                && Math.Abs((double)direction1[2] - (double)direction2[2]) < 0.1)
            {
                isSameDirection = true;
            }

            return isSameDirection;
        }

    }

}

