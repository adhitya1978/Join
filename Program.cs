using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace Join
{
    class Program
    {
        //! create new directory for destination
        static void CreateDirectory(string directoryname)
        {
            try
            {
                if (System.IO.Directory.Exists(directoryname)) return;
                System.IO.Directory.CreateDirectory(directoryname);
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine(string.Format("Fail Create directory {0}.", directoryname));
            }
        }

        //! geting list zip files by src directory using prefix number or else
        static List<string> ListFilesByPrefix(string srcdirectory)
        {
            List<string> files = new List<string>();
            try
            {
                System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(srcdirectory);
                System.IO.FileInfo[] infofiles = root.GetFiles();
                foreach (System.IO.FileInfo infofile in infofiles)
                {
                    if (infofile.Name.EndsWith(".zip"))
                        files.Add(infofile.Name);
                }
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return files;
        }

        static void ExtractZipDirectory(string archname, string directoryname)
        {
            // 4K is optimum
            byte[] buffer = new byte[4096];
            try
            {
                using (System.IO.Stream source = System.IO.File.Open(archname, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                {
                    //! archive file load to stream 
                    ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(source);
                    try
                    {
                        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry entry in zipFile)
                        {
                            if (!entry.IsFile) continue;
                            if (entry.IsCrypted) throw new Exception("Compress file encrypted.");
                            string filetobecreate = System.IO.Path.Combine(directoryname, entry.Name);
                            using (System.IO.Stream data = zipFile.GetInputStream(entry))
                            {
                                using (System.IO.Stream write = System.IO.File.Open(filetobecreate, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                                {
                                    ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(data, write, buffer);
                                    write.Close();
                                }
                                data.Close();
                            }
                        }
                    }
                    finally
                    {
                        zipFile.IsStreamOwner = true;
                        zipFile.Close();
                    }

                }
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 0)
                return;

            string source = System.IO.Directory.GetCurrentDirectory();;
            string destination = string.Empty;

            for (var i = 0; i < args.Length; ++i)
            {
                if (args[i].ToUpper() == "-D")
                    destination = args[i + 1];

            }
            if (source != string.Empty && destination != string.Empty)
            {
                CreateDirectory(destination);
                foreach (string arch in ListFilesByPrefix(source))
                {
                    ExtractZipDirectory(arch, destination);
                }
            }

        }
    }
}
