using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF_UI_Tutorial02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get all drives in the machine
            foreach (var drive in Directory.GetLogicalDrives())
            {
                //Create new tree view item
                var item = new TreeViewItem()
                {
                    //Set the header and path
                    Header = drive,
                    Tag = drive
                };

                //Add dummy item
                item.Items.Add(null);

                item.Expanded += Folder_Expanded;

                FolderView.Items.Add(item);
            }


        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            //If the item only contains dummy data
            if (item.Items.Count != 1 || item.Items[0] != null) return;

            //Clear dummy data
            item.Items.Clear();

            //Get the full path
            var fullPath = (string)item.Tag;

            #region Get Folders

            //Create a blank list
            var directories = new List<string>();

            //Try and get directories from the folder
            //Ignoring any errors (Bad practise)
            try
            {
                var dirs = Directory.GetDirectories(fullPath);

                if (dirs.Length > 0)
                {
                    directories.AddRange(dirs);
                }
            }
            catch { }


            directories.ForEach(directoryPath =>
            {
                //Create Directory Item
                var subItem = new TreeViewItem()
                {
                    //Set header as folder name
                    Header = GetFileFolderName(directoryPath),
                    //And tag as full path
                    Tag = directoryPath
                };
                //Add dummy item
                subItem.Items.Add(null);

                //Handle the expanding folder
                subItem.Expanded += Folder_Expanded;
                
                //Add this item to the parent
                item.Items.Add(subItem);
            });

            #endregion

            //Create a blank list
            var files = new List<string>();

            //Try and get files from the folder
            //Ignoring any errors (Bad practice)
            try
            {
                var fs = Directory.GetFiles(fullPath);

                if (fs.Length > 0)
                {
                    files.AddRange(fs);
                }
            }
            catch { }


            files.ForEach(filePath =>
            {
                //Create Directory Item
                var subItem = new TreeViewItem()
                {
                    //Set header as file name
                    Header = GetFileFolderName(filePath),
                    //And tag as full path
                    Tag = filePath
                };

                //Add this item to the parent
                item.Items.Add(subItem);
            });
        }

        public static string GetFileFolderName(string directoryPath)
        {
            //If we have no path return empty
            if (string.IsNullOrEmpty(directoryPath)) return string.Empty;

            //Replaces '/' with '\'
            var normalizedPath = directoryPath.Replace('/', '\\');

            //Find the last backslash in the path
            var lastIndex = normalizedPath.LastIndexOf('\\');

            if (lastIndex <= 0) return directoryPath;

            //Return the name after the last backslash
            return directoryPath.Substring(lastIndex + 1);

        }



    }
}
