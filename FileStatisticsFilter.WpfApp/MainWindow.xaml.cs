using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        public MainWindow()
        {
            InitializeComponent();
            _searchedFiles = new SearchedFiles();
            _directories = new List<string>();
            _files = new List<SearchedFile>();
            _totalSize = 0;
            DirectoryComboBox.SelectionChanged += DirectorySelectionChanged;
        }

        public void LoadFile(object sender, RoutedEventArgs e)
        {
            var result = new OpenFileDialog
            {
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            result.ShowDialog();
            if (result.CheckPathExists)
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

                if (_directories.Count > 1)
                {
                    SubdirectoryCheckBox.IsChecked = true;
                }

                _selectedDirectory = _directories.ElementAt(0);

                DirectoryComboBoxLogic();
                ListsAndLabelsLogic();
            }
        }

        public void DirectoryComboBoxUpdate(object sender, RoutedEventArgs e)
        {
            DirectoryComboBoxLogic();
        }
        public void ListsAndLabelsUpdate(object sender, RoutedEventArgs e)
        {
            ListsAndLabelsLogic();
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

        public void DirectorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!DirectoryComboBox.Items.IsEmpty)
            {
                _selectedDirectory = DirectoryComboBox.SelectedItem.ToString();
                ListsAndLabelsLogic();
            }
        }

        private void ListsAndLabelsLogic()
        {
            UpdateFiles();
            UpdateLabels();
            UpdateExtensions();
        }

        private void UpdateLabels()
        {
            int countFiles = 0;
            int countDirs = 0;
            var countDir = new List<string>();
            int countReadOnly = 0;
            long size = 0;
            DateTime createdOldest = DateTime.MaxValue;
            DateTime createdNewest = DateTime.MinValue;
            DateTime modifiedOldest = DateTime.MaxValue;
            DateTime modifiedNewest = DateTime.MinValue;
            foreach (var file in FileListView.Items.IsEmpty ? _searchedFiles.Files : FileListView.Items.OfType<SearchedFile>())
            {
                if (Directory.Exists(file.Directory) && !countDir.Contains(file.Directory))
                {
                    countDirs++;
                    countDir.Add(file.Directory);
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
            foreach (var file in FileListView.Items.IsEmpty
                         ? _searchedFiles.Files
                         : FileListView.Items.OfType<SearchedFile>())
            {
                if (!ExtensionListView.Items.Contains(file.Extension))
                {
                    ExtensionListView.Items.Add(new { name = file.Extension, count = 1, size = 2});
                }
            }
        }

        private void UpdateFiles()
        {
            FileListView.Items.Clear();
            foreach (var file in (from file in _files where file.Directory.Contains(_selectedDirectory) select file))
            {
                FileListView.Items.Add(file);
            }
        }
    }
}
