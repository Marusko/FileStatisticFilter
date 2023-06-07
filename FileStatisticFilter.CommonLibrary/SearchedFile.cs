using System.Drawing;
using System.Text;

namespace FileStatisticFilter.CommonLibrary
{
    public class SearchedFile
    {
        public DateTime CreatedTime { get; set; }
        public string Directory { get; set; }
        public string Extension { get; set; }
        public string FileName => FileNameWithoutExtension + Extension;
        public string FileNameWithoutExtension { get; set; }
        public string FullName => Directory + "\\" + FileNameWithoutExtension + Extension;
        public bool IsReadOnly { get; set; }
        public DateTime ModifiedTime { get; set; }
        public long Size { get; set; }

        public string SizeReadable
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double tmp = Size;
                int ext = 0;
                string returnSize;
                while (tmp / 1000 > 1)
                {
                    tmp /= 1000;
                    ext++;
                }

                tmp = Math.Round(tmp, 2);
                returnSize = tmp + " " + sizes[ext];
                return returnSize;
            }
        }

        public SearchedFile(FileInfo file)
        {
            CreatedTime = file.CreationTime;
            Directory = file.DirectoryName;
            Extension = file.Extension;

            var name = file.Name;
            name = name.Substring(0, name.Length - Extension.Length);

            FileNameWithoutExtension = name;
            IsReadOnly = file.IsReadOnly;
            ModifiedTime = file.LastWriteTime;
            Size = file.Length;
        }

        public SearchedFile(string csvLine, char delimiter = '\t')
        {
            var splitted = csvLine.Split(delimiter);

            DateTime ct;
            DateTime.TryParse(splitted[4], out ct);
            CreatedTime = ct;

            Directory = splitted[0];
            Extension = splitted[2];
            FileNameWithoutExtension = splitted[1];

            bool b;
            bool.TryParse(splitted[6], out b);
            IsReadOnly = b;

            DateTime mt;
            DateTime.TryParse(splitted[5], out mt);
            ModifiedTime = mt;

            long size;
            long.TryParse(splitted[3], out size);
            Size = size;
        }

        public static string ToCsvHeaderLine(char delimiter = '\t')
        {
            return "Directory" + delimiter + "FileNameWithoutExtension" + delimiter + 
                   "Extension" + delimiter + "Size" + delimiter + 
                   "CreatedTime" + delimiter + "ModifiedTime" + delimiter + "IsReadOnly";
        }

        public string ToCsvLine(char delimiter = '\t')
        {
            return Directory + delimiter + FileNameWithoutExtension + delimiter +
                   Extension + delimiter + Size + delimiter +
                   CreatedTime + delimiter + ModifiedTime + delimiter + IsReadOnly;
        }
    }

    public class SearchedFiles
    {
        public SearchedFile[] Files { get; set; }

        public SearchedFiles()
        {
        }

        public SearchedFiles(IEnumerable<FileInfo> files)
        {
            var tmpFiles = new List<SearchedFile>();
            foreach (var file in files)
            {
                tmpFiles.Add(new SearchedFile(file));
            }

            Files = tmpFiles.ToArray();
        }

        public void LoadFromCsv(FileInfo file)
        {
            var stream = file.Open(FileMode.Open);
            using var reader = new StreamReader(stream);
            var tmpFiles = new List<SearchedFile>();
            var header = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!header)
                {
                    tmpFiles.Add(new SearchedFile(line, ';'));
                }
                else
                {
                    header = false;
                }
            }

            Files = tmpFiles.ToArray();
            reader.Close();
            stream.Close();
        }

        public void SaveToCsv(FileInfo file)
        {
            var stream = file.Open(FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.WriteLine(SearchedFile.ToCsvHeaderLine(';'));
            for (int i = 0; i < Files.Length; i++)
            {
                writer.WriteLine(Files[i].ToCsvLine(';'));
            }
            writer.Flush();
            writer.Close();
            stream.Close();
        }
    }
}