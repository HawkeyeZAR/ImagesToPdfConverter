using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImagesToPdfConverter
{
    internal class ToPDF
    {
        internal string destinaton = @"ConvertToPDF\Converted\";
        internal string source = @"ConvertToPDF\ImagesToConvert";
        internal string backPath = @"ConvertToPDF\ImageBackups\";

        private static Timer _aTimer;

        internal void StartTimer()
        {
            // Create a timer with a ten second interval.
            _aTimer = new Timer(80000);

            // Hook up the Elapsed event for the timer.
            #pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            _aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            #pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            // Set the Interval to 2 seconds (2000 milliseconds).
            _aTimer.Interval = 60000;
            _aTimer.Enabled = true;

            Console.WriteLine("Checking for new images in 60 seconds.\tPlease wait.....\n");
            //  Pressing enter key will close the program.
            Console.ReadLine();

            // KeepAlive to prevent garbage collection from occurring 
            //GC.KeepAlive(_aTimer);
        }

        // Specify what you want to happen when the Elapsed event is raised. 
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            CheckForImages();
        }
        /// <summary>
        /// Search source folder for any image files.
        /// If images found, call imageConvert and try to convert to pdf.
        /// </summary>
        internal void CheckForImages()
        {
            string[] _extArray = new string[7] { "bmp", "gif", "png", "jpeg", "jpg", "tiff", "tif" };

            DirectoryInfo dirs = new DirectoryInfo(source);
            FileInfo[] files = dirs.GetFiles("*.*");
            // Make sure this message is printed every time the timer executes
            Console.WriteLine("Checking for new images in 60 seconds.\tPlease wait.....");
            Console.WriteLine("Press the Enter key to exit the program.\n");
            foreach (FileInfo file in files)
            {
                string[] _currentFile = file.ToString().Split('.');
                if (_extArray.Contains(_currentFile[1].ToLower()))
                {
                    Console.WriteLine($"Image Found.....Converting!");
                    string tmpFileName = dirs.ToString() + @"\" + file.ToString();
                    imageConvert(tmpFileName, _currentFile[0]);
                }
            }

        }
        /// <summary>
        /// Converts images to pdf
        /// </summary>
        /// <param name="fPath">Full Path</param>
        /// <param name="fName">Name of the file</param>
        internal void imageConvert(string fPath, string fName)
        {
            string imgName = fPath;                   
            try
            {
                //Image MyImage = Image.FromFile(imgName);
                PdfDocument doc = new PdfDocument();

                using (Image MyImage = Image.FromFile(imgName))
                {
                    for (int PageIndex = 0; PageIndex < MyImage.GetFrameCount(FrameDimension.Page); PageIndex++)
                    {
                        // Setup pdf document to write too.
                        // If image has more than one frame, save each image to a seperate page
                        MyImage.SelectActiveFrame(FrameDimension.Page, PageIndex);
                        XImage img = XImage.FromFile(imgName);

                        var page = new PdfPage();

                        if (img.PointWidth > img.PointHeight)
                        {
                            page.Orientation = PageOrientation.Landscape;
                        }
                        else
                        {
                            page.Orientation = PageOrientation.Portrait;
                        }
                        // Set the pdf page file size to the size of the actual image
                        page.Height = XUnit.FromPoint(img.PointHeight);
                        page.Width = XUnit.FromPoint(img.PointWidth);
                        doc.Pages.Add(page);

                        XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[PageIndex]);
                        xgr.DrawImage(img, 0, 0);
                        // Close file used by XImage
                        img.Dispose();
                    }
                }

                // Create new filename using destination path, filename without extension, append pdf extension
                string newFileName = destinaton.ToString() + fName + ".pdf";
                doc.Save(newFileName);
                // close the open file
                doc.Close();

                try
                {
                    // Call Backup function and backup file that was converted
                    BackupImages backupImages = new BackupImages();
                    string bakLocation = backPath + fName + ".bak";
                    backupImages.FileBackup(imgName, bakLocation);
                    //  Delete original file
                    File.Delete(imgName);

                    using (StreamWriter sw = File.AppendText("Convert Error.logs"))
                    {
                        sw.WriteLine($"Converted images have been backed up to the backup folder. - {DateTime.Now.ToString()}");
                    }
                }
                catch (IOException ioExp)
                {
                    using (StreamWriter sw = File.AppendText("Convert Error.logs"))
                    {
                        sw.WriteLine($"Failed to delete Image file after conversion - {DateTime.Now.ToString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Save Error in a log File
                using (StreamWriter sw = File.AppendText("Convert Error.logs"))
                {
                    sw.WriteLine($"Error Converting Image to a PDF - {fName}- {DateTime.Now.ToString()}");
                }
                // Call Backup function to remove from monitor folder
                BackupImages backupImages = new BackupImages();
                string bakLocation = backPath + fName + ".bak";
                backupImages.FileBackup(imgName, bakLocation);
                //  Delete original file
                File.Delete(imgName);
            }
        }

    }
}