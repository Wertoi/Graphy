using INFITF;
using MECMOD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphy.Model.CatiaDocument
{
    public class CatiaPartDocument : CatiaGenericDocument
    {
        public CatiaPartDocument(CatiaEnv catiaEnv) : base(catiaEnv)
        {

        }

        public CatiaPartDocument(CatiaEnv catiaEnv, CatiaGenericDocument catiaGenericDocument) : base(catiaEnv)
        {
            Name = catiaGenericDocument.Name;
            Document = catiaGenericDocument.Document;
            DocumentFormat = catiaGenericDocument.DocumentFormat;
            File = catiaGenericDocument.File;
        }

        private PartDocument _partDocument;

        public PartDocument PartDocument { get => _partDocument; set => _partDocument = value; }

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
                    PartDocument = (PartDocument)Document;
                }
                catch (Exception)
                {
                    throw new InvalidDocumentType("Impossible to convert " + Document.GetType().ToString() + " to Part document");
                }
            }
        }
    }
}
