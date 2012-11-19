using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace photorganiser
{
    class Program
    {
        static int Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Photorganiser started at {0:yyyy-MM-dd HH:mm:ss.ffff}", 
                DateTime.Now);
            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
            finally
            {
                sw.Stop();
                Console.WriteLine("Photorganiser finished. Elapsed time: {0}", 
                    sw.ElapsedMilliseconds);
            }
        }

        static void MoveToNextAvailableFilename(FileInfo file, string directory, string filename, string extension)
        {
            var newFullName = Path.Combine(directory, string.Format("{0}{1}", filename, extension));
            int i = 0;
            while (i < int.MaxValue)
            {
                if (!File.Exists(newFullName))
                {
                    if (!Directory.Exists(directory))
                    {
                        Console.WriteLine("Creating folder {0}", directory);
                        Directory.CreateDirectory(directory);
                    }
                    Console.WriteLine("Moving {0} to {1}", file.FullName, newFullName);
                    file.MoveTo(newFullName);
                    return;
                }
                else
                {
                    Console.WriteLine("Found a file with name {0}; Comparing", newFullName);
                    if (FileUtils.FileCompare(file, new FileInfo(newFullName)))
                    {
                        Console.WriteLine("Deleting duplicate file {0}", file.FullName);
                        file.Delete();
                        return;
                    }
                    newFullName = Path.Combine(directory,
                        string.Format("{0}_{1}{2}", filename, ++i, extension));
                }
            }
        }

        static int Run(string[] args)
        {
            if (args.Length != 2) return Syntax();
            if (!Directory.Exists(args[0])) return Syntax();

            var source = new DirectoryInfo(args[0]);

            var target = Directory.Exists(args[1]) ? new DirectoryInfo(args[1]) :
                                                     Directory.CreateDirectory(args[1]);

            var extensions = new HashSet<string> { ".jpg", ".jpe", ".jpeg", ".png", 
                ".bmp", ".gif", ".mov", ".mpg", ".mpeg", ".avi", ".wmv" };

            var jpeg_extensions = new HashSet<string> { ".jpg", ".jpe", ".jpeg" };

            var files = source.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => !f.Name.StartsWith(".") && 
                    extensions.Contains(f.Extension.ToLowerInvariant()));

            foreach (var file in files)
            {
                try
                {
                    var dest_file = Path.GetFileNameWithoutExtension(file.Name);
                    var date_taken = FileUtils.GetDateTakenFromImage(file);
                    if (date_taken == null)
                    {
                        date_taken = file.LastWriteTime;
                        if (jpeg_extensions.Contains(file.Extension.ToLowerInvariant()))
                            dest_file = "NDT_" + dest_file;
                    }

                    var dest_folder = Path.Combine(target.FullName,
                        string.Format("{0:yyyy}{1}{0:MM}{1}{0:dd}",
                        date_taken.Value, Path.DirectorySeparatorChar));

                    MoveToNextAvailableFilename(file, dest_folder, dest_file, 
                        file.Extension.ToLowerInvariant());

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("{0}:: {1}", file.FullName, ex.Message);
                }
            }
            return 0;
        }

        static int Syntax()
        {
            Console.WriteLine("Syntax: Photorganiser source/path destination/path");
            return 1;
        }
    }
}
