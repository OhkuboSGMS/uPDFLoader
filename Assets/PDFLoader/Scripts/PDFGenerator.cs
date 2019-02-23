using System;
using UnityEngine;
using UniRx;
using UniRx.Async;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace uPDFLoader
{
    /// <summary>
    /// PDFを連番JPGにするbatファイルを動かす。
    /// </summary>
    public class PDFGenerator
    {
        System.Diagnostics.Process p;
        string outputDirPath;
        string pdfDirPath;
        readonly string gsPath = string.Format("\"{0}\"", @"C:\Program Files\gs\gs9.23\bin\gswin64c.exe");
        readonly string ppi = "200";

        public PDFGenerator()
        {
            p = new Process();
            outputDirPath = Application.temporaryCachePath;
            pdfDirPath = Application.streamingAssetsPath;
        }


        /// <summary>
        /// async でFileSystemからPDFを読み込みテクスチャにする
        /// </summary>
        /// <param name="pdfPath"> パス名に日本語が入ってると動作不良</param>
        /// <returns></returns>
        public async Task<string> PDFGenerate(string pdfPath)
        {
            Debug.Log("Async Start");
            var fileName = Path.GetFileNameWithoutExtension(pdfPath);
            var imagesDirectoryPath = outputDirPath + "/" + fileName + "/";

            if (!Directory.Exists(imagesDirectoryPath))
            {
                Directory.CreateDirectory(imagesDirectoryPath);
            }
            else
            {
                Directory.Delete(imagesDirectoryPath, true);
                Directory.CreateDirectory(imagesDirectoryPath);
            }

            p.StartInfo.FileName = gsPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            var optionString = "-dSAFER -dBATCH -dNOPAUSE -sDEVICE=jpeg -r" + ppi;
            var outPutPath = string.Format("-sOutputFile=\"{0}\"", imagesDirectoryPath + @"%d.jpg");
            var filePathForCommand = string.Format("\"{0}\"", pdfPath);

            p.StartInfo.Arguments = string.Format(@"{0} {1} {2}", optionString, outPutPath, filePathForCommand);
            p.Start();
            p.WaitForExit();
            return imagesDirectoryPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">PDFファイルの名前(拡張子を含まない)</param>
        /// <returns></returns>
        public IObservable<string> PDFGenerateObservable(string fileName)
        {
            return Observable.Create<string>(obs =>
            {
                var pdfFilePath = pdfDirPath + "/" + fileName + ".pdf";
                var imagesDirectoryPath = outputDirPath + "/" + fileName + "/";

                if (!Directory.Exists(imagesDirectoryPath))
                {
                    Directory.CreateDirectory(imagesDirectoryPath);
                }
                else
                {
                    Directory.Delete(imagesDirectoryPath, true);
                    Directory.CreateDirectory(imagesDirectoryPath);
                }

                p.StartInfo.FileName = gsPath;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = false;
                p.StartInfo.CreateNoWindow = true;

                var optionString = "-dSAFER -dBATCH -dNOPAUSE -sDEVICE=jpeg -r" + ppi;
                var outPutPath = string.Format("-sOutputFile=\"{0}\"", imagesDirectoryPath + @"%d.jpg");
                var filePathForCommand = string.Format("\"{0}\"", pdfFilePath);

                p.StartInfo.Arguments = string.Format(@"{0} {1} {2}", optionString, outPutPath, filePathForCommand);

                p.Start();

                p.WaitForExit();
                p.Close();
                //連番画像化されたファイルが格納されたディレクトリを返す。
                obs.OnNext(imagesDirectoryPath);
                obs.OnCompleted();

                return Disposable.Empty;
            }).SubscribeOn(Scheduler.ThreadPool);
        }
    }
}