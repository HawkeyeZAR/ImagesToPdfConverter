using System;
using System.IO;

namespace ImagesToPdfConverter
{
    /// <summary>
    /// Backups all image files to backup folder that have been converted to a pdf
    /// </summary>
    internal class BackupImages
    {
        internal void FileBackup(string source, string destination)
        {
            string path = source;
            string path2 = destination;
            try
            {
                if (!File.Exists(path))
                {
                    // This statement ensures that the file is created,
                    // but the handle is not kept.
                    using (FileStream fs = File.Create(path)) { }
                }

                // Ensure that the target does not exist.
                if (File.Exists(path2))
                    File.Delete(path2);

                // Move the file.
                File.Move(path, path2);
                using (StreamWriter sw = File.AppendText("Convert Error.logs"))
                {
                    sw.WriteLine($"{path} was moved to {path2}. - {DateTime.Now.ToString()}");
                }
            }
            catch (Exception e)
            {
                using (StreamWriter sw = File.AppendText("Convert Error.logs"))
                {
                    sw.WriteLine($"{path} failed moving to the backup location. - {DateTime.Now.ToString()}");
                }
            }
        }
    }
}
