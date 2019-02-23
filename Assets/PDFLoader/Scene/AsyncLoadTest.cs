using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using uPDFLoader;

namespace uPDFLoader
{
    class AsyncLoadTest : UnityEngine.MonoBehaviour
    {
        public string pdfPath;
        private void Start()
        {
            var context = SynchronizationContext.Current;
            Load(context);
        }

        async  void Load(SynchronizationContext context)
        {
            SynchronizationContext.SetSynchronizationContext(context);
            var gen = new PDFGenerator();
            var pdfTexturesDirectoryPath = await Task.Run(()=> gen.PDFGenerate(pdfPath));
            print("Finish Generate");
            print($"Directory Path:{pdfTexturesDirectoryPath}");
            if (!string.IsNullOrEmpty(pdfTexturesDirectoryPath))
            {
                print("Load Textures");
                var info = new PDFImageDirInfo(pdfTexturesDirectoryPath);
                foreach (var tex in info.imageFileURIs)
                {
                    print($"Pdf To Texture :{tex}");
                }
            }
        }
    }
}