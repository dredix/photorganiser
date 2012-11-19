using System;
using System.IO;
using System.Windows.Media.Imaging;


namespace photorganiser
{
    public static class FileUtils
    {
        // http://stackoverflow.com/a/2281704/1014
        public static DateTime? GetDateTakenFromImage(FileInfo file)
        {
            try
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapSource img = BitmapFrame.Create(fs);
                    BitmapMetadata md = (BitmapMetadata)img.Metadata;
                    return DateTime.Parse(md.DateTaken);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool FileCompare(FileInfo file1, FileInfo file2)
        {
            if (file1.Length != file2.Length)
                return false;
            using (FileStream fs1 = file1.OpenRead())
            using (FileStream fs2 = file2.OpenRead())
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (fs1.ReadByte() != fs2.ReadByte())
                        return false;
                }
            }
            return true;
        }

    }
}
