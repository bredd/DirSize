using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirSize
{
    class Program
    {
        static void Main(string[] args)
        {

            const string c_syntax =
@"Syntax: DirSize [options] [path] ...
Options:
   -h           Print this syntax message. (also -?)
   -levels  <n> Number of directory levels to display. Default is 1.
                (-l, and -level are equivalent to -levels)

Prints the sizes of all directories in a hierarchy.
";

            try
            {
                bool help = false;
                int levels = 1;
                var dirs = new List<string>();

                // Parse Arguments
                for (int i = 0; i < args.Length; ++i)
                {
                    string arg = args[i];
                    switch (arg.ToLower())
                    {
                        case "-h":
                        case "-?":
                            help = true;
                            break;

                        case "-l":
                        case "-level":
                        case "-levels":
                            ++i;
                            levels = int.Parse(args[i]);
                            break;

                        default:
                            dirs.Add(arg);
                            break;
                    }
                }

                if (help)
                {
                    Console.WriteLine(c_syntax);
                    return;
                }

                if (dirs.Count == 0)
                {
                    dirs.Add(Environment.CurrentDirectory);
                }


                foreach(string dir in dirs)
                {
                    var output = new List<string>();
                    DirSize(new DirectoryInfo(dir), 0, levels, output);
                    Console.WriteLine(Path.GetFullPath(dir));
                    output.Reverse();
                    foreach(string line in output)
                    {
                        Console.WriteLine(line);
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception err)
            {
#if DEBUG
                Console.WriteLine(err.Message);
#else
                Console.WriteLine(err.ToString());
#endif
            }

            Win32Interop.ConsoleHelper.WaitToCloseConsole();
        }

        static long DirSize(DirectoryInfo di, int level, int showLevels, List<string> output)
        {
            long fullSize = 0;
            foreach(var fsi in di.EnumerateFileSystemInfos())
            {
                DirectoryInfo di2 = fsi as DirectoryInfo;
                if (di2 != null)
                {
                    fullSize += DirSize(di2, level + 1, showLevels, output);
                }

                FileInfo fi = fsi as FileInfo;
                if (fi != null)
                {
                    fullSize += fi.Length;
                }
            }

            if (level <= showLevels)
            {
                output.Add(string.Format("{0}{1,6}B {2}", new String(' ', level * 3), FmtKMG(fullSize), di.Name));
            }

            return fullSize;
        }

        static char[] s_kmg = new char[] {' ', 'K', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y', 'X', 'S', 'D' };

        static string FmtKMG(long count)
        {
            int power = 0;
            decimal d = count;
            while (d > 1000m)
            {
                ++power;
                d /= 1024m;
            }
            return string.Format("{0:F1}{1}", d, s_kmg[power]);
        }
    }
}
