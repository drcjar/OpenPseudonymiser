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
using System.Linq;
using System.Windows;


namespace OpenPseudonymiser
{

    /*        
        Code that manages navigation through the "pages" of the app, as well as UI element locking/unlocking and visibility setting
    */

    public partial class MainWindow : Window
    {
        // Navigation through the app
        public enum Pages { ChooseFile, ChooseColumns, ChooseDestination };
        public enum NavDirection { Next, Back, Finish };
        private Pages _currentPage;

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NavHandler(NavDirection.Next);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavHandler(NavDirection.Back);
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            // always flip them to page 3, since we may press the Finish button on page 1 or 2 
            cvsPage1.Visibility = System.Windows.Visibility.Hidden;
            cvsPage2.Visibility = System.Windows.Visibility.Hidden;
            cvsPage3.Visibility = System.Windows.Visibility.Visible;
            _currentPage = Pages.ChooseDestination;
            SetPageHeader(_currentPage);

            btnNext.IsEnabled = false;            

            lblStatus.Content = "OpenPseudonymiser is running....";
            _processingStartTime = DateTime.Now;
            LockUIElementsForProcessing(true);
            SetProgressUIElementsVisibility(System.Windows.Visibility.Visible);

            _outputFileNameOnly = _inputFile.Substring(_inputFile.LastIndexOf('\\') + 1, _inputFile.Length - _inputFile.LastIndexOf('\\') - 1);
            _outputRunLogFileName = _outputFolder + "\\" + _outputFileNameOnly + ".OpenPseudonymiserRunLog";

            _performNHSNumberValidation = cmbNHSNumber.SelectedIndex != 0;            

            ProcessSingleFile(_inputFile);
        }

        private void SetProgressUIElementsVisibility(System.Windows.Visibility visibility)
        {
            progress.Visibility = visibility;
            btnCancel.Visibility = visibility;
            lblProgress.Visibility = visibility;
            outputLink.Visibility = visibility;
        }

        public void SetPageHeader(Pages page)
        {
            switch (page)
            {
                case Pages.ChooseFile:
                    lblHeader.Content = "Choose a data file";
                    lblSubHeader.Content = "Specify the CSV (Comma Separated Values) data file";
                    break;
                case Pages.ChooseColumns:
                    lblHeader.Content = "Select columns";
                    lblSubHeader.Content = "Specify which columns to use for the Digest, and which ones to output";
                    

                    break;
                case Pages.ChooseDestination:
                    lblHeader.Content = "Summary & destination folder";
                    lblSubHeader.Content = "Change the destination folder if required and review the summary";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the navigation between the different "pages" in the app. Nav buttons in the footer call this.
        /// </summary>        
        public void NavHandler(NavDirection direction)
        {
            

            switch (_currentPage)
            {
                case Pages.ChooseFile:
                    switch (direction)
                    {
                        case NavDirection.Next:
                            if (_usingPreConfigSettings || _usingSettingFile)
                            {
                                cvsPage1.Visibility = System.Windows.Visibility.Hidden;
                                cvsPage3.Visibility = System.Windows.Visibility.Visible;
                                btnBack.IsEnabled = true;
                                ShowReady();
                                if(AllExceptInOutputFile())
                                {
                                    string[] allExcept = OpenPResources.PreConfig_OutputCols.Substring(11, OpenPResources.PreConfig_OutputCols.Length - 11).Split(',');
                                    foreach (ColumnData cd in _ColumnCollection)
                                    {
                                        // if the cd is not in the AllExcept array then add it as an output column
                                        if (!allExcept.Contains(cd.ColumnHeading))
                                        {
                                            _preConfigOutputColumns.Add(cd.ColumnHeading);
                                        }                        
                                    }
                                }
                                _currentPage = Pages.ChooseDestination;                                
                            }
                            else
                            {
                                cvsPage1.Visibility = System.Windows.Visibility.Hidden;
                                cvsPage2.Visibility = System.Windows.Visibility.Visible;
                                btnBack.IsEnabled = true;
                                _currentPage = Pages.ChooseColumns;
                                DetermineIfPageTwoIsCorrectlyFilledIn();
                            }
                            break;

                    }
                    break;

                case Pages.ChooseColumns:
                    switch (direction)
                    {
                        case NavDirection.Next:
                            cvsPage3.Visibility = System.Windows.Visibility.Visible;
                            btnNext.IsEnabled = false;
                            cvsPage2.Visibility = System.Windows.Visibility.Hidden;
                            _currentPage = Pages.ChooseDestination;
                            if (!_usingPreConfigSettings)
                            {
                                SetDateColumnsForQAdmin();
                            }
                            ShowReady();
                            break;
                        case NavDirection.Back:
                            cvsPage1.Visibility = System.Windows.Visibility.Visible;
                            cvsPage2.Visibility = System.Windows.Visibility.Hidden;
                            _currentPage = Pages.ChooseFile;
                            btnBack.IsEnabled = false;
                            btnNext.IsEnabled = true;
                            break;

                    }
                    break;


                case Pages.ChooseDestination:
                    switch (direction)
                    {                                                
                        case NavDirection.Back:
                            btnNext.IsEnabled = true;
                            btnFinish.IsEnabled = true;                            
                            if (_usingPreConfigSettings)
                            {
                                cvsPage1.Visibility = System.Windows.Visibility.Visible;
                                cvsPage3.Visibility = System.Windows.Visibility.Hidden;                                                                
                                _currentPage = Pages.ChooseFile;
                                break;
                            }
                            else 
                            {
                                cvsPage2.Visibility = System.Windows.Visibility.Visible;
                                cvsPage3.Visibility = System.Windows.Visibility.Hidden;                                
                                _currentPage = Pages.ChooseColumns;
                                break;
                            }
                    }
                    break;

                default:
                    break;
            }

            SetPageHeader(_currentPage);

        }

        private void LockUIElementsForProcessing(bool locked)
        {
            btnBack.IsEnabled = !locked;
            btnFinish.IsEnabled = !locked;
            //btnNext.IsEnabled = !locked;
            btnHelp.IsEnabled = !locked;
            btnSelectOutput.IsEnabled = !locked;
            btnCancel.IsEnabled = locked;
        }


        


        private void DetermineIfPageOneIsCorrectlyFilledIn()
        {
            if (saltFileIsOK && inputFileIsOK && (settingsFileIsOK || !_usingSettingFile) && CheckInputFileAgainstSettingsFile())
            {
                btnNext.IsEnabled = true;
            }
            else
            {
                btnNext.IsEnabled = false;
            }

        }
        

        /// <summary>
        /// If there is a row to use for encryption and one for output then we can allow them to continue..        
        /// </summary>
        private void DetermineIfPageTwoIsCorrectlyFilledIn()
        {
            int usedForDigest = 0;
            int usedForOutput = 0;

            foreach (ColumnData columnData in ColumnCollection)
            {
                if (columnData.UseForDigest)
                {
                    usedForDigest++;
                }
                if (columnData.UseForOutput)
                {
                    usedForOutput++;
                }
            }
            if (usedForDigest > 0 && usedForOutput > 0)
            {
                btnNext.IsEnabled = true;
            }
            else
            {
                btnNext.IsEnabled = false;
            }
        }
    }
}
