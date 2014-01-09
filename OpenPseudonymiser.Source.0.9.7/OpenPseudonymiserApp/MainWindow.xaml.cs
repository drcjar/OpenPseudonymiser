/*
    Copyright Julia Hippisley-Cox, University of Nottingham 2011 
  
    This file is part of OpenPseudonymiser.

    OpenPseudonymiser is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    OpenPseudonymiser is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with OpenPseudonymiser.  If not, see <http://www.gnu.org/licenses/>.
 
    The university has made reasonable enquiries regarding granted and pending patent
    applications in the general area of this technology and is not aware of any
    granted or pending patent in Europe which restricts the use of this
    software. In the event that the university receives a notice of perceived patent
    infringement, then the university will inform users that their use of the
    software may need to or, if appropriate, must cease in the appropriate
    territory. The university does not make any warranties in this respect and each
    user shall be solely responsible for ensuring that they do not infringe any
    third party patent.
 
 */

using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Diagnostics;


namespace OpenPseudonymiser
{    

    /// <summary>
    /// This partial contains only UI hookups
    /// Logic is split into the different partial classes. 
    /// Start at Main.cs if you are unsure
    /// </summary>
    public partial class MainWindow : Window
    {   
        
        public MainWindow()
        {
            InitializeComponent();
            LookForSettings();
            SetPageHeader(Pages.ChooseFile);
            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();
        }
        
        /// <summary>
        /// The HyperLink for the location of the output files
        /// </summary>        
        private void OnUrlClick(object sender, RoutedEventArgs e)
        {            
            var runExplorer = new System.Diagnostics.ProcessStartInfo();
            runExplorer.FileName = "explorer.exe";
            runExplorer.Arguments = _outputFolder;
            System.Diagnostics.Process.Start(runExplorer); 
            
        }
        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "Comma Separated Data Files(.csv)|*.csv"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true) // they clicked OK, rather than cancel
            {
                string filename = dlg.FileName;

                // See if we can use this document
                if (CanUseThisInputFile(filename))
                {
                    BuildColumns(filename);
                    SetOutputLocationToSameFolderAsInputLocation(filename);
                    CheckInputFileAgainstSettingsFile();
                    inputFileIsOK = true;                                            
                }
            }
            DetermineIfPageOneIsCorrectlyFilledIn();
        }


        private void btnSelectSaltFile_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".EncryptedSalt"; // Default file extension
            dlg.Filter = "Encrypted Salt Files(.EncryptedSalt)|*.EncryptedSalt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true) // they clicked OK, rather than cancel
            {
                string filename = dlg.FileName;

                if (CanOpenFile(filename))
                {
                    saltFileIsOK = true;
                    encryptedSaltFile = filename;
                    lblSelectedSaltFile.Content = encryptedSaltFile;
                }
            }
            DetermineIfPageOneIsCorrectlyFilledIn();
        }

        private void btnSelectSettingsFile_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".OpenPseudonymiserSettings"; // Default file extension
            dlg.Filter = "OpenPseudonymiser Settings Files(.OpenPseudonymiserSettings)|*.OpenPseudonymiserSettings"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true) // they clicked OK, rather than cancel
            {
                string filename = dlg.FileName;
                
                if (PullSettinsFromFile(filename))
                {
                    _usingSettingFile = true;
                    _settingFile = filename;
                    lblSelectedSettingsFile.Content = filename;
                    CheckInputFileAgainstSettingsFile();
                }
                
            }
            DetermineIfPageOneIsCorrectlyFilledIn();
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SortedList<string, int> inputFields;
            SortedList<int, string> outputFields;
            GetInputAndOutputFields(out inputFields, out outputFields);
            string filename = DateTime.Now.ToString().Replace("/", ".").Replace(":", ".");
            string settingsFile = _outputFolder + "\\" + filename + ".OpenPseudonymiserSettings";
            WriteSettingsFile(inputFields, outputFields, settingsFile);
            lblSettingsSaved.Content = "Settings saved: " + settingsFile;
        }

        

        private void SetOutputLocationToSameFolderAsInputLocation(string filename)
        {
            string path = Path.GetDirectoryName(filename);                
            _outputFolder = path;
            lblSelectedOutput.Content = path;
        }
        /// <summary>
        /// Pop open a select folder dialog so the user can select where they want the output files to go
        /// </summary>
        private void btnSelectOutputFile_Click(object sender, RoutedEventArgs e)
        {
            // select the folder to write the output files to:
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "Select folder to write output files to";
            // set the select folder to be the path of the input file
            folderDialog.SelectedPath = Path.GetDirectoryName(_inputFile);

            folderDialog.ShowDialog();

            if (folderDialog.SelectedPath != "")
            {
                SetOutputLocation(folderDialog.SelectedPath);
                btnFinish.IsEnabled = true;
            }
        }

        
        
        /// <summary>
        /// Each checkbox on page two is wired up to this. If the user's selection is OK then the next button is enabled
        /// </summary>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            DetermineIfPageTwoIsCorrectlyFilledIn();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            var h = new Help();
            h.ShowDialog();
            
        }


        // data preview pending upgrade to .NET 4 or another way to bind the data to the columns and show it easily.
        //private void btnPreview_Click(object sender, RoutedEventArgs e)
        //{
        //    var w = new DataPreview(_inputFile);
        //    w.Show();
        //    //this.Hide();
        
        //}
    }
}
