using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace SQLAid.Zip
{
    public class ZipCompressor : IDisposable
    {
        private const int BYTES_BUFFER = 32767;

        public event CompressionStatusEvent CompressionStatus;

        public delegate void CompressionStatusEvent(int currentFile, int quantity);

        public bool IsDisposed { get; private set; }

        public bool Compress(string folderOrFile, string outputFileName, int level = 9, bool overrideFile = true, bool onlyContentsOfFolder = true)
        {
            var compressStats = true;
            FileStream fileStream = null;
            ZipOutputStream zipOutputStream = null;
            var num = 0;
            try
            {
                folderOrFile = new DirectoryInfo(folderOrFile).FullName;
                outputFileName = new DirectoryInfo(outputFileName).FullName;

                if (!Directory.Exists(folderOrFile) && !File.Exists(folderOrFile))
                {
                    compressStats = false;
                    throw new Exception($"Folder of File name \"{folderOrFile}\" does no exist.");
                }

                if (compressStats && !overrideFile && File.Exists(outputFileName))
                {
                    compressStats = false;
                    throw new Exception($"the Compresssed of File name \"{outputFileName}\" already exists.");
                }

                if (compressStats)
                {
                    if (File.Exists(folderOrFile))
                        onlyContentsOfFolder = true;

                    if (overrideFile && File.Exists(outputFileName))
                        File.Delete(outputFileName);

                    var files = GetFiles(folderOrFile);
                    fileStream = File.Create(outputFileName);
                    zipOutputStream = new ZipOutputStream(fileStream);
                    zipOutputStream.SetLevel(level);

                    foreach (var item in files)
                    {
                        AddFilesToZip(ref zipOutputStream, folderOrFile, item, onlyContentsOfFolder);
                        num++;
                        CompressionStatus?.Invoke(num, files.Count);
                    }

                    zipOutputStream.Finish();
                    zipOutputStream.Dispose();
                    zipOutputStream.Close();

                    fileStream.Dispose();
                    fileStream.Close();

                    zipOutputStream = null;
                    fileStream = null;
                }
            }
            catch (Exception)
            {
                if (zipOutputStream != null)
                {
                    zipOutputStream.Dispose();
                    zipOutputStream.Close();
                }

                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream.Close();
                }

                if (File.Exists(outputFileName))
                {
                    File.Delete(outputFileName);
                }

                throw;
            }

            return compressStats;
        }

        public bool Decompress(string compressedFileName, string outputFolder, bool overrideFiles = true)
        {
            var compressStatus = true;
            compressedFileName = new DirectoryInfo(compressedFileName).FullName;
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            if (!Directory.Exists(outputFolder))
            {
                compressStatus = false;
                throw new Exception($"The Output Folder Name \"{outputFolder}\" does no exist.");
            }
            if (!File.Exists(compressedFileName))
            {
                compressStatus = false;
                throw new Exception($"The Compressed File Name \"{compressedFileName}\" does no exist.");
            }
            if (compressStatus)
            {
                var fastZip = new FastZip();
                if (!overrideFiles)
                    fastZip.ExtractZip(compressedFileName, outputFolder, FastZip.Overwrite.Never, default, default, default, default);
                else
                    fastZip.ExtractZip(compressedFileName, outputFolder, FastZip.Overwrite.Always, default, default, default, default);
            }

            return compressStatus;
        }

        private List<string> GetFiles(string folderOrFileName)
        {
            var fileResults = new List<string>();
            var directoryInfo = new DirectoryInfo(folderOrFileName);

            foreach (var file in directoryInfo.GetFiles())
            {
                fileResults.Add(file.FullName);
            }

            foreach (var dir in directoryInfo.GetDirectories())
            {
                fileResults.AddRange(GetFiles(dir.FullName));
            }

            return fileResults;
        }

        private void AddFilesToZip(ref ZipOutputStream zipOutputStream, string passedFolder, string file, bool contentsOfFolder)
        {
            var num = BYTES_BUFFER;

            file = file.Replace("/", "\\");
            var flag = false;
            var array = new byte[BYTES_BUFFER];

            if (!passedFolder.Trim().EndsWith("\\"))
                passedFolder += "\\";

            var fullFolderName = GetFullFolderName(passedFolder);
            if (!contentsOfFolder)
            {
                var lastFolderName = GetLastFolderName(fullFolderName);
                if (!lastFolderName.EndsWith("\\"))
                    lastFolderName += "\\";

                fullFolderName = fullFolderName.Replace(lastFolderName, "");
            }
            var fileInfo = new FileInfo(file);
            var zipEntry = new ZipEntry(file.Replace(fullFolderName, ""))
            {
                DateTime = fileInfo.LastWriteTime,
                Size = fileInfo.Length
            };
            zipOutputStream.PutNextEntry(zipEntry);
            var fileStream = File.OpenRead(file);
            while (!flag)
            {
                num = fileStream.Read(array, 0, array.Length);
                if (num < BYTES_BUFFER)
                    flag = true;

                zipOutputStream.Write(array, 0, num);
            }

            fileStream.Dispose();
            fileStream.Close();
            zipOutputStream.CloseEntry();
        }

        private string GetFullFolderName(string fullFolderOrFileName)
        {
            fullFolderOrFileName = fullFolderOrFileName.Replace("/", "\\");
            var directoryInfo = new DirectoryInfo(fullFolderOrFileName);

            while (!directoryInfo.Exists && fullFolderOrFileName.Contains("\\"))
            {
                if (!directoryInfo.Exists)
                {
                    var text = fullFolderOrFileName.Substring(0, fullFolderOrFileName.LastIndexOf("\\"));
                    text = text.Substring(text.LastIndexOf("\\") + 1);
                    fullFolderOrFileName = fullFolderOrFileName.Substring(0, fullFolderOrFileName.LastIndexOf(text) + text.Length);
                }
                directoryInfo = new DirectoryInfo(fullFolderOrFileName);
            }

            if (!fullFolderOrFileName.EndsWith("\\"))
                fullFolderOrFileName += "\\";

            return fullFolderOrFileName;
        }

        private string GetLastFolderName(string fullFolderOrFileName)
        {
            fullFolderOrFileName = GetFullFolderName(fullFolderOrFileName);
            var text = fullFolderOrFileName.Substring(0, fullFolderOrFileName.LastIndexOf("\\"));
            return text.Substring(text.LastIndexOf("\\") + 1);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                GC.ReRegisterForFinalize(this);
                IsDisposed = true;
            }
        }
    }
}