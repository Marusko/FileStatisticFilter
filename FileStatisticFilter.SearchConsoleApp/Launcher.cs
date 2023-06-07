using FileStatisticFilter.CommonLibrary;

namespace FileStatisticFilter.SearchConsoleApp
{
    internal class Launcher
    {
        /// <summary>
        /// File analyzer
        /// </summary>
        /// <param name="input">Required parameter, path to directory to be analyzed</param>
        /// <param name="output">Required parameter, CSV file for saving information about files</param>
        /// <param name="regex">NETUSIM</param>
        /// <param name="recursive">If entered, the program wil' also search subfolders</param>
        static void Main(string input, string output, string regex = "\t", bool recursive = false)
        {
            var directory = new DirectoryInfo(input);
            SearchedFiles files;
            if (!directory.Exists)
            {
                Console.Error.WriteLine("Entered input file does not exists!");
            }
            else
            {
                files = recursive 
                    ? new SearchedFiles(directory.GetFiles("*", SearchOption.AllDirectories)) 
                    : new SearchedFiles(directory.GetFiles("*", SearchOption.TopDirectoryOnly));
                files.SaveToCsv(new FileInfo(output));
            }
        }
    }
}