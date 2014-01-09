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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;


namespace OpenPseudonymiser
{
    public partial class MainWindow : Window
    {
        /* APP GLOBALS */
        string _inputFile; // selected input file and path
        string _outputFolder; // selected output path        
        string _outputFileNameOnly;
        string _outputRunLogFileName; // a file that gets written when the processing is done (called the RunLog)
        string _runLogFileHash;

        string _salt;
        bool _userSelectedSalt = false;

        bool _usingPreConfigSettings = false;
        bool _usingSettingFile; // using a settings file? (ratehr than pre-build config with settings in the resources file..)
        string _settingFile;

        long _inputFileStreamLength;
        long _rowsProcessed;
        long _jaggedLines;
        long _validNHSNumCount = 0;
        long _inValidNHSNumCount = 0;
        long _missingNHSNumCount = 0;
        string[] _preConfigInputColumns;

        List<string> _preConfigOutputColumns = new List<string>();
        List<string> _processDateColumns = new List<string>(); // set either in the config or by the super user on page 2
        protected DateTime _processingStartTime;
        
        bool usingEncryptedSalt; // using an encrypted salt file?
        string encryptedSaltFile; // path and filename for encrypted salt file

        bool _performNHSNumberValidation; // run any NHSNumber columns through a NHSNumber checksum validator?

        bool inputFileIsOK = false;
        bool saltFileIsOK;
        bool settingsFileIsOK;
        

        // Column headings we find in the input file. Bound on page 2
        ObservableCollection<ColumnData> _ColumnCollection = new ObservableCollection<ColumnData>();
        public ObservableCollection<ColumnData> ColumnCollection { get { return _ColumnCollection; } }

        public class ColumnData
        {
            public bool UseForDigest { get; set; }
            public bool UseForOutput { get; set; }
            public bool ProcessAsDate{ get; set; }
            public string ColumnHeading { get; set; }
        }

        internal string GetInputFileName()
        {
            return _inputFile;
        }

        /// <summary>
        /// If the user is allowed to slect which date columns to process (as opposed to this being configured) then we look and see which ones they selected and set the global here
        /// </summary>
        private void SetDateColumnsForQAdmin()
        {
            _processDateColumns.Clear();
            foreach (ColumnData columnData in ColumnCollection)
            {
                if (columnData.ProcessAsDate)
                {
                    _processDateColumns.Add(columnData.ColumnHeading);
                }
            }
        }
      

        /// <summary>
        /// If we are running in preset mode then we use the settings found in the QCrypt.Settings file.
        /// This is determined by the QAdminMode = True or false setting
        /// </summary>
        private void LookForSettings()
        {            
            // get the default salt (this can be overridden later if the AllowSaltEntry flag is set to true
            _salt = OpenPResources.DefaultSalt;
            txtSalt.Text = _salt;

            if (OpenPseudonymiser.OpenPResources.AllowExport.ToLower() == "true")
            {
                btnSaveSettings.Visibility = System.Windows.Visibility.Visible;
            }
            else 
            {
                btnSaveSettings.Visibility = System.Windows.Visibility.Hidden;                
            }

            if (OpenPseudonymiser.OpenPResources.SaltMode.ToLower() != "plaintext")
            {
                lblEnterSalt.Visibility = System.Windows.Visibility.Hidden;
                txtSalt.Visibility = System.Windows.Visibility.Hidden;
            }

            if (OpenPseudonymiser.OpenPResources.SaltMode.ToLower() != "encryptedfile")
            {
                lblSelectedSaltFile.Visibility = System.Windows.Visibility.Hidden;
                btnSelectSaltFile.Visibility = System.Windows.Visibility.Hidden;
            }


            if (OpenPseudonymiser.OpenPResources.UseSettingsFile.ToLower() == "false")
            {
                lblSelectedSettingsFile.Visibility = System.Windows.Visibility.Hidden;
                btnSelectSettingsFile.Visibility = System.Windows.Visibility.Hidden;                
                settingsFileIsOK = true;
                _usingSettingFile = false;
            }
            if (OpenPseudonymiser.OpenPResources.UseSettingsFile.ToLower() == "true")
            {
                lblSelectedSettingsFile.Content = "(Optional) Select settings file";
                settingsFileIsOK = true;
                _usingSettingFile = false;
            }
            if (OpenPseudonymiser.OpenPResources.UseSettingsFile.ToLower() == "force")
            {
                _usingSettingFile = true;
                settingsFileIsOK = false;
            }



            if (OpenPseudonymiser.OpenPResources.SaltMode.ToLower() == "encryptedfile")
            {
                usingEncryptedSalt = true;
                saltFileIsOK = false;
            }
            else 
            {
                usingEncryptedSalt = false;
                saltFileIsOK = true;
            }

            

            if (OpenPResources.UsePreconfigSettings == "true")
            {
                _usingPreConfigSettings = true;

                lblSettingFile.Content = "OpenPseudonymiser is configured for " + OpenPResources.Project;
                lblSettingFile.Content += Environment.NewLine;

                _preConfigInputColumns = OpenPResources.PreConfig_DigestCols.Split(',');
                lblSettingFile.Content += "Expected columns to Pseudonymise: " + String.Join(",", _preConfigInputColumns);
                lblSettingFile.Content += Environment.NewLine;

                // Output cols..
                // there is a special case where we use all the input cols, except some specified ones. Look for this first
                if (AllExceptInOutputFile())
                {
                    // we cant bind the output columns yet, nto uuntil they select a file. Just display a message
                    lblSettingFile.Content += "Expected columns to Output: " + OpenPResources.PreConfig_OutputCols;
                }
                else
                {
                    // didn't find the special case, so just split the columns specified
                    _preConfigOutputColumns = OpenPResources.PreConfig_OutputCols.Split(',').ToList();
                    lblSettingFile.Content += "Expected columns to Output: " + String.Join(",", _preConfigOutputColumns.ToArray());
                }

                _processDateColumns = OpenPResources.PreConfig_DateCols.Split(',').ToList();

                lblSettingFile.Content += Environment.NewLine;
                lblSettingFile.Content += "Date columns to process: " + OpenPResources.PreConfig_DateCols;                                

                _runLogFileHash = OpenPResources.SettingsHash;
            }
        }

        // true if the special case of "all except" is found in the settings
        private bool AllExceptInOutputFile()
        {
            return OpenPResources.PreConfig_OutputCols.Length > 10 && OpenPResources.PreConfig_OutputCols.Substring(0, 11) == "all except:";
        }




        /// <summary>
        /// Read the columns from the selected file, build our observable data, which is bound to the control on page 2
        /// Also populate the drop down list on page two used to select an NHS number 
        /// </summary>        
        public void BuildColumns(string filename)
        {

            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _ColumnCollection.Clear();

            cmbNHSNumber.Items.Clear();

            ComboBoxItem item = new ComboBoxItem();
            item.Name = "item_0";
            item.Content = "My data has no NHS Numbers"; 
            cmbNHSNumber.Items.Add(item);
            cmbNHSNumber.SelectedIndex = 0;

            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                string firstLine = streamReader.ReadLine();
                string[] cols = firstLine.Split(',');
                int i = 0;
                foreach (string colname in cols)
                {
                    i++;
                    _ColumnCollection.Add(new ColumnData
                    {
                        UseForDigest = false,
                        UseForOutput = true,
                        ColumnHeading = colname,
                    });

                    item = new ComboBoxItem();
                    item.Name = "item" + i.ToString();
                    item.Content = colname;
                    cmbNHSNumber.Items.Add(item);

                }
                
            }
        }


       

        /// <summary>
        /// Check the selected file conforms to the pre-configured column settings (if applicable)
        /// </summary>        
        private bool CheckInputFileAgainstSettingsFile()
        {
            // not using preconfig? Nothing to check..
            if (!_usingPreConfigSettings || !_usingSettingFile)
            {
                return true;
            }

            string missingColumns = "";
            bool foundAllInputColumns = true;
            if (inputFileIsOK && settingsFileIsOK)
            {                
                foreach (string preConfigInputColumn in _preConfigInputColumns)
                {
                    if (!_ColumnCollection.Any(p => p.ColumnHeading.ToLower() == preConfigInputColumn.ToLower()))
                    {
                        foundAllInputColumns = false;
                        missingColumns += preConfigInputColumn + ",";
                    }
                }

                if (!foundAllInputColumns)
                {
                    lblFileDetails.Content += "File matches pre-config setting...... X";
                    lblFileDetails.Content += Environment.NewLine;
                    lblSettingFile.Content = "Selected file does not match the pre-configured column setting.";
                    lblSettingFile.Content += Environment.NewLine;
                    lblSettingFile.Content += "File is missing the column(s): " + missingColumns;
                    lblStatusInput.Content = "Please select an input file that matches the pre-configured column settings";
                }
            }
            return foundAllInputColumns;
        }


        /// <summary>
        /// Call the basic checks on the input file and display messages about whether the file is suitable       
        /// </summary>        
        /// <returns>True if the file is OK to use</returns>
        private bool CanUseThisInputFile(string filename)
        {
            _inputFile = filename;
            lblSelectedFile.Content = filename;

            if (!CanOpenFile(filename))
            {
                lblFileDetails.Content = "File opened ....................... X";
                lblFileDetails.Content += Environment.NewLine;
                lblFileDetails.Content += "(is it already open in Excel?)";
                return false;
            }
            lblFileDetails.Content = "File opened ....................... √";
            lblFileDetails.Content += Environment.NewLine;

            int CSVCount = GetFileCSVCount(filename);
            if (CSVCount == 0)
            {
                return false;
            }
            lblFileDetails.Content += "Comma separated values detected .. √";
            lblFileDetails.Content += Environment.NewLine;

            if (!First100RowsConform(CSVCount, filename))
            {
                return false;
            }
            lblFileDetails.Content += "First 100 rows conform ........... √";
            lblFileDetails.Content += Environment.NewLine;

            lblStatusInput.Content = "File is OK";
            

            lblSettingFile.Content = "";

            return true;
        }


        /// <summary>
        /// Returns true if the first 100 lines in the file all have the same column count as the first column
        /// </summary>        
        private bool First100RowsConform(int CSVCount, string filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            int i = 0;
            using (StreamReader sr = new StreamReader(fs))
            {
                string line = sr.ReadLine();
                while (line != "" && i < 100)
                {
                    i++;
                    if (line.Split(',').Length != CSVCount)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// get the number of columns in the first row on this tile
        /// </summary>        
        private int GetFileCSVCount(string filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (StreamReader sr = new StreamReader(fs))
            {                
                return sr.ReadLine().Split(',').Length;
            }
        }

        /// <summary>
        /// Does the file exist, can we open it?
        /// </summary>
        /// <param name="filename"></param>        
        private bool CanOpenFile(string filename)
        {
            bool ret = false;
            try
            {
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                fs = null;
                ret = true;
            }
            catch
            {
                // sometimes get a filesystem error here if we try and open a file to read that is already opened by Excel
                ret = false;
            }
            return ret;
        }


        /// <summary>
        /// Show a summary of input and output file locations on screen
        /// </summary>        
        private void SetOutputLocation(string filename)
        {
            _outputFolder = filename;
            lblSelectedOutput.Content = filename;
            ShowReady();
        }

        private void ShowReady()
        {
            btnFinish.IsEnabled = true;

            lblOutputDetails.Content = "Summary";
            lblOutputDetails.Content += Environment.NewLine;
            lblOutputDetails.Content += "----------";
            lblOutputDetails.Content += Environment.NewLine;
            lblOutputDetails.Content += "Input file: " + _inputFile;
            lblOutputDetails.Content += Environment.NewLine;

            lblStatus.Content = "OpenPseudononymiser is ready";
            lblStatus.Content += Environment.NewLine;
            lblStatus.Content += "Press 'Run' to start the process...";
        }


        
        private bool PullSettinsFromFile(string filename)
        {
            bool ret = false;
            try
            {
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                using (StreamReader sr = new StreamReader(fs))
                {
                    _preConfigInputColumns = sr.ReadLine().Split(':')[1].Split(',');
                    _preConfigOutputColumns = sr.ReadLine().Split(':')[1].Split(',').ToList();
                    _processDateColumns = sr.ReadLine().Split(':')[1].Split(',').ToList();
                }
                ret = true;
                settingsFileIsOK = true;
                lblFileDetails.Content = "Settings file is OK .. √";
            }
            catch
            {                
                settingsFileIsOK = false;
                lblFileDetails.Content = "Settings file is OK .. X";
            }
            return ret;
        }




        
    }
}
