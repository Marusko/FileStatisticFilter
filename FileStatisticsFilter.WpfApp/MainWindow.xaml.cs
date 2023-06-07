using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FileStatisticFilter.CommonLibrary;
using Microsoft.Win32;

namespace FileStatisticsFilter.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SearchedFiles _searchedFiles;
        private List<string> _directories;
        private List<SearchedFile> _files;
        private long _totalSize;
        private string _selectedDirectory;
        private bool _subdirectories;
        public MainWindow()
        {
            InitializeComponent();
            _searchedFiles = new SearchedFiles();
            _directories = new List<string>();
            _files = new List<SearchedFile>();
            _totalSize = 0;
            DirectoryComboBox.SelectionChanged += DirectorySelectionChanged;
            SubdirectoryCheckBox.Checked += SubdirectoriesChecked;
            SubdirectoryCheckBox.Unchecked += SubdirectoriesUnChecked;
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            var result = new OpenFileDialog
            {
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            result.ShowDialog();
            if (result.FileName.Length > 0)
            {
                _directories.Clear();
                _files.Clear();
                _searchedFiles.LoadFromCsv(new FileInfo(result.FileName));
                foreach (var file in _searchedFiles.Files)
                {
                    if (Directory.Exists(file.Directory) && !_directories.Contains(file.Directory))
                    {
                        _directories.Add(file.Directory);
                    }
                    if (File.Exists(file.FullName) && !_files.Contains(file))
                    {
                        _files.Add(file);
                        _totalSize += file.Size;
                    }
                }

                _selectedDirectory = _directories.ElementAt(0);

                if (_directories.Count > 1)
                {
                    SubdirectoryCheckBox.IsChecked = true;
                    _subdirectories = true;
                }
                else
                {
                    SubdirectoryCheckBox.IsChecked = false;
                    _subdirectories = false;
                }

                DirectoryComboBoxLogic();
                ListsAndLabelsLogic(_subdirectories);
            }
        }

        private void DirectoryComboBoxLogic()
        {
            DirectoryComboBox.Items.Clear();
            foreach (var dir in _directories)
            {
                DirectoryComboBox.Items.Add(dir);
            }
            DirectoryComboBox.SelectedIndex = 0;
        }

        private void DirectorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!DirectoryComboBox.Items.IsEmpty)
            {
                _selectedDirectory = DirectoryComboBox.SelectedItem.ToString();
                ListsAndLabelsLogic(_subdirectories);
            }
        }

        public void SubdirectoriesChecked(object sender, EventArgs e)
        {
            _subdirectories = true;
            ListsAndLabelsLogic(_subdirectories);
        }
        public void SubdirectoriesUnChecked(object sender, EventArgs e)
        {
            _subdirectories = false;
            ListsAndLabelsLogic();
        }

        private void ListsAndLabelsLogic(bool subDir = false)
        {
            UpdateFiles(subDir);
            UpdateLabels();
            UpdateExtensions();
        }

        private void UpdateLabels()
        {
            int countFiles = 0;
            int countDirs = 0;
            var countDirsList = new List<string>();
            int countReadOnly = 0;
            long size = 0;
            DateTime createdOldest = DateTime.MaxValue;
            DateTime createdNewest = DateTime.MinValue;
            DateTime modifiedOldest = DateTime.MaxValue;
            DateTime modifiedNewest = DateTime.MinValue;
            foreach (var file in FileListView.Items.IsEmpty ? _searchedFiles.Files : FileListView.Items.OfType<SearchedFile>())
            {
                if (Directory.Exists(file.Directory) && !countDirsList.Contains(file.Directory))
                {
                    countDirs++;
                    countDirsList.Add(file.Directory);
                }
                if (File.Exists(file.FullName))
                {
                    countFiles++;
                }

                if (file.IsReadOnly)
                {
                    countReadOnly++;
                }

                if (DateTime.Compare(file.CreatedTime, createdOldest) == -1)
                {
                    createdOldest = file.CreatedTime;
                }
                if (DateTime.Compare(file.CreatedTime, createdNewest) == 1)
                {
                    createdNewest = file.CreatedTime;
                }

                if (DateTime.Compare(file.ModifiedTime, modifiedOldest) == -1)
                {
                    modifiedOldest = file.ModifiedTime;
                }
                if (DateTime.Compare(file.ModifiedTime, modifiedNewest) == 1)
                {
                    modifiedNewest = file.ModifiedTime;
                }
                size += file.Size;
            }
            FilesCountLabel.Content = countFiles + " / " + _files.Count();
            DirectoriesCountLabel.Content = countDirs + " / " + _directories.Count();

            
            string[] sizes = { "B", "KB", "MB", "GB", "TB" }; 
            double tmp = size;
            int ext = 0;
            string returnSize;
            while (tmp / 1000 > 1)
            {
                tmp /= 1000;
                ext++;
            }

            tmp = Math.Round(tmp, 2);
            returnSize = tmp + " " + sizes[ext];
            
            TotalSizeLabel.Content = returnSize;
            CreatedOldestLabel.Content = createdOldest;
            CreatedNewestLabel.Content = createdNewest;
            ModifiedOldestLabel.Content = modifiedOldest;
            ModifiedNewestLabel.Content = modifiedNewest;
            ReadonlyCountLabel.Content = countReadOnly;
        }

        private void UpdateExtensions()
        {
            ExtensionListView.Items.Clear();
            IEnumerable<SearchedFile> files = FileListView.Items.IsEmpty ? _searchedFiles.Files
                : FileListView.Items.OfType<SearchedFile>();
            List<string> filteredExt = new List<string>();
            foreach (var file in files)
            {
                if (!filteredExt.Contains(file.Extension))
                {
                    filteredExt.Add(file.Extension);
                }
            }
            foreach (var ext in filteredExt)
            {
                int count = (from f in files where f.Extension.Equals(ext) select f).Count();
                var sizesList = from f in files where f.Extension.Equals(ext) select f.Size;

                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double tmp = sizesList.Sum();
                int sizeExt = 0;
                string size;
                while (tmp / 1000 > 1)
                {
                    tmp /= 1000;
                    sizeExt++;
                }

                tmp = Math.Round(tmp, 2);
                size = tmp + " " + sizes[sizeExt];

                ExtensionListView.Items.Add(new { name = ext, count = count, size = size});
            }
        }

        private void UpdateFiles(bool subDir = false)
        {
            FileListView.Items.Clear();
            IEnumerable<SearchedFile> files;
            if (subDir)
            {
                files = from file in _files where file.Directory.Contains(_selectedDirectory) select file;
            }
            else
            {
                files = from file in _files where file.Directory.Equals(_selectedDirectory) select file;
            }
            foreach (var file in files)
            {
                FileListView.Items.Add(file);
            }
        }
    }
}
