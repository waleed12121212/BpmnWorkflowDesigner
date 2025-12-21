using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

namespace BpmnWorkflow.Client.Services
{
    public class PowerPointExportService
    {
        public byte[] CreatePowerPoint(string title, string description, string imageBase64)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var presentationDocument = PresentationDocument.Create(memoryStream, PresentationDocumentType.Presentation))
                {
                    PresentationPart presentationPart = presentationDocument.AddPresentationPart();
                    presentationPart.Presentation = new Presentation();
                    CreatePresentationParts(presentationPart);

                    // Create the first slide with Title and Image
                    InsertImageSlide(presentationDocument, title, description, imageBase64);
                }
                return memoryStream.ToArray();
            }
        }

        private void CreatePresentationParts(PresentationPart presentationPart)
        {
            SlideMasterPart slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>();
            slideMasterPart.SlideMaster = new SlideMaster(
                new CommonSlideData(new ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = (UInt32Value)1u, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new ApplicationNonVisualDrawingProperties()),
                    new GroupShapeProperties(new A.TransformGroup()),
                    new P.Shape(
                        new P.NonVisualShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = (UInt32Value)2u, Name = "Title" },
                            new P.NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
                            new ApplicationNonVisualDrawingProperties(new PlaceholderShape() { Type = PlaceholderValues.Title })),
                        new P.ShapeProperties(),
                        new P.TextBody(
                            new A.BodyProperties(),
                            new A.ListStyle(),
                            new A.Paragraph()))
                )),
                new P.ColorMap() { Background1 = D.ColorSchemeIndexValues.Light1, Text1 = D.ColorSchemeIndexValues.Dark1, Background2 = D.ColorSchemeIndexValues.Light2, Text2 = D.ColorSchemeIndexValues.Dark2, Accent1 = D.ColorSchemeIndexValues.Accent1, Accent2 = D.ColorSchemeIndexValues.Accent2, Accent3 = D.ColorSchemeIndexValues.Accent3, Accent4 = D.ColorSchemeIndexValues.Accent4, Accent5 = D.ColorSchemeIndexValues.Accent5, Accent6 = D.ColorSchemeIndexValues.Accent6, Hyperlink = D.ColorSchemeIndexValues.Hyperlink, FollowedHyperlink = D.ColorSchemeIndexValues.FollowedHyperlink }
            );

            SlideLayoutPart slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();
            slideLayoutPart.SlideLayout = new SlideLayout(new CommonSlideData(new ShapeTree()));
            slideMasterPart.AddPart(slideLayoutPart);

            presentationPart.Presentation.SlideMasterIdList = new SlideMasterIdList(new SlideMasterId() { Id = (UInt32Value)2147483603U, RelationshipId = presentationPart.GetIdOfPart(slideMasterPart) });
            presentationPart.Presentation.SlideIdList = new SlideIdList();
            presentationPart.Presentation.SlideSize = new SlideSize() { Cx = 9144000, Cy = 6858000, Type = SlideSizeValues.Screen16x9 };
            presentationPart.Presentation.NotesSize = new NotesSize() { Cx = 6858000, Cy = 9144000 };
            presentationPart.Presentation.Save();
        }

        private void InsertImageSlide(PresentationDocument presentationDocument, string title, string description, string base64Image)
        {
            PresentationPart presentationPart = presentationDocument.PresentationPart;
            SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
            
            // Add Image Part
            ImagePart imagePart = slidePart.AddImagePart(ImagePartType.Png);
            using (Stream data = new MemoryStream(Convert.FromBase64String(base64Image)))
            {
                imagePart.FeedData(data);
            }

            // Construct Slide
            slidePart.Slide = new Slide(
                new CommonSlideData(
                    new ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = (UInt32Value)1u, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new ApplicationNonVisualDrawingProperties()
                        ),
                        new GroupShapeProperties(new A.TransformGroup()),
                        // Title
                        CreateTextBox(title, 500000, 500000, 8000000, 1000000, 4800, true),
                        // Image
                        CreatePicture(slidePart.GetIdOfPart(imagePart), 1000000, 1800000, 7000000, 4000000)
                    )
                ),
                new ColorMapOverride(new A.MasterColorMapping())
            );
            
            // Description (if valid)
             if(!string.IsNullOrEmpty(description)) {
                 slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(
                    CreateTextBox(description, 1000000, 6000000, 7000000, 1000000, 1800, false)
                 );
             }

            slidePart.Slide.Save();

            // Link Slide to Presentation
            SlideIdList slideIdList = presentationPart.Presentation.SlideIdList;
            uint maxId = 1;
            if (slideIdList != null && slideIdList.ChildElements.Count > 0)
            {
                maxId = ((SlideId)slideIdList.ChildElements[slideIdList.ChildElements.Count - 1]).Id + 1;
            }
            slideIdList.Append(new SlideId() { Id = (UInt32Value)maxId, RelationshipId = presentationPart.GetIdOfPart(slidePart) });
            presentationPart.Presentation.Save();
        }

        private P.Shape CreateTextBox(string text, long x, long y, long cx, long cy, int fontSize, bool isBold)
        {
             return new P.Shape(
                new P.NonVisualShapeProperties(
                    new P.NonVisualDrawingProperties() { Id = (UInt32Value)(uint)System.Guid.NewGuid().GetHashCode(), Name = "TextBox" },
                    new P.NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
                    new ApplicationNonVisualDrawingProperties(new PlaceholderShape())
                ),
                new P.ShapeProperties(
                    new A.Transform2D(
                        new A.Offset() { X = x, Y = y },
                        new A.Extents() { Cx = cx, Cy = cy }
                    ),
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle },
                    new A.NoFill()
                ),
                new P.TextBody(
                    new A.BodyProperties(),
                    new A.ListStyle(),
                    new A.Paragraph(
                        new A.Run(
                            new A.RunProperties() { Language = "en-US", FontSize = fontSize, Bold = isBold },
                            new A.Text(text)
                        )
                    )
                )
            );
        }

        private P.Picture CreatePicture(string relationshipId, long x, long y, long cx, long cy)
        {
            return new P.Picture(
                new P.NonVisualPictureProperties(
                    new P.NonVisualDrawingProperties() { Id = (UInt32Value)(uint)System.Guid.NewGuid().GetHashCode(), Name = "Picture" },
                    new P.NonVisualPictureDrawingProperties(new A.PictureLocks() { NoChangeAspect = true }),
                    new ApplicationNonVisualDrawingProperties()
                ),
                new P.BlipFill(
                    new A.Blip() { Embed = relationshipId },
                    new A.Stretch(new A.FillRectangle())
                ),
                new P.ShapeProperties(
                    new A.Transform2D(
                        new A.Offset() { X = x, Y = y },
                        new A.Extents() { Cx = cx, Cy = cy }
                    ),
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }
                )
            );
        }
    }
}
