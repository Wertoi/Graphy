using DRAFTINGITF;
using INFITF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaDocument
{
    public class CatiaDrawingDocument : CatiaGenericDocument
    {
        public CatiaDrawingDocument(CatiaEnv catiaEnv) : base(catiaEnv)
        {

        }

        public CatiaDrawingDocument(CatiaGenericDocument catiaGenericDocument) : base(catiaGenericDocument.CatiaEnv)
        {
            Name = catiaGenericDocument.Name;
            Document = catiaGenericDocument.Document;
            DocumentFormat = catiaGenericDocument.DocumentFormat;
            File = catiaGenericDocument.File;
        }

        private DrawingDocument _drawingDocument;

        public DrawingDocument DrawingDocument
        {
            get => _drawingDocument;
            set
            {
                Set(() => DrawingDocument, ref _drawingDocument, value);
            }
        }

        new public Document Document
        {
            get => _document;
            set
            {
                Set(() => Document, ref _document, value);

                if (File != null)
                    File.FullPath = Document.FullName;
                else
                    File = new CatiaFile(Document.FullName);

                DocumentFormat = File.GetDocumentFormat();
                Name = Document.get_Name();

                try
                {
                    DrawingDocument = (DrawingDocument)Document;
                }
                catch (Exception)
                {
                    throw new InvalidDocumentType("Impossible to convert " + Document.GetType().ToString() + " to Drawing document");
                }
            }
        }
    }
}
