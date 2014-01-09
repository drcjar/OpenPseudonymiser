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



using System.Windows.Threading;
using System;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using OpenPseudonymiser.CryptoLib;



namespace OpenPseudonymiser
{

    /*        
        All the stuff  to do with the processing of the file when Finish is pressed
    */

    public partial class MainWindow : Window
    {
        BackgroundWorker worker;


        public delegate void UpdateProgressDelegate(long recordsRead, long recordCount, long rows, long ValidNHS, long InvalidNHS, long missingNHS);
        public delegate void ErrorProgressDelegate(string errorText);
        public delegate void CancelProgressDelegate(long rows);

        public void UpdateProgressText(long recordsRead, long recordCount, long rows, long ValidNHS, long InvalidNHS, long missingNHS)
        {
            //lblProgress.Content = string.Format("{0} of {1} bytes", recordsRead, recordCount);
            lblProgress.Content = string.Format("{0} rows. ", rows);
            if (_performNHSNumberValidation)
            {
                lblProgress.Content += string.Format("Valid NHS:{0} Invalid: {1} Missing: {2}", ValidNHS, InvalidNHS, missingNHS);
            }
            int percentage = (int)(100 * ((double)recordsRead / (double)recordCount));
            progress.Value = percentage;

            // finished!
            if (recordsRead == recordCount)
            {
                lblOutputDetails.Content = "";
                // reset the progress stuff for the next run and hide it
                lblProgress.Content = "";
                progress.Value = 0;
                SetProgressUIElementsVisibility(System.Windows.Visibility.Hidden);

                // unlock the UI buttons
                LockUIElementsForProcessing(false);

                // grey the finish button
                btnFinish.IsEnabled = false;                

                // update the bold status
                TimeSpan processingTimespan = DateTime.Now.Subtract(_processingStartTime);
                lblStatus.Content = "OpenPseudonymiser process finished";
                lblStatus.Content += Environment.NewLine;
                lblStatus.Content += "Rows processed: " + rows;

                if (_performNHSNumberValidation)
                {
                    lblStatus.Content += Environment.NewLine;
                    lblStatus.Content += "Valid NHS Numbers: " + _validNHSNumCount;
                    lblStatus.Content += Environment.NewLine;
                    lblStatus.Content += "Invalid NHS Numbers: " + _inValidNHSNumCount;
                    lblStatus.Content += Environment.NewLine;
                    lblStatus.Content += "Missing NHS Numbers: " + _missingNHSNumCount;
                }

                lblStatus.Content += Environment.NewLine;
                lblStatus.Content += "Time taken: " + processingTimespan.Minutes +"m " + processingTimespan.Seconds + "s";

                outputLink.Visibility = System.Windows.Visibility.Visible;
            }
        }


        public void CancelProgressText(long rows)
        {            
            // reset the progress stuff for the next run and hide it
            lblProgress.Content = "";
            progress.Value = 0;
            SetProgressUIElementsVisibility(System.Windows.Visibility.Hidden);

            // unlock the UI buttons
            LockUIElementsForProcessing(false);

            // update the bold status
            lblOutputDetails.Content = "";
            TimeSpan processingTimespan = DateTime.Now.Subtract(_processingStartTime);
            lblStatus.Content = "OpenPseudonymiser process cancelled";
            lblStatus.Content += Environment.NewLine;
            lblStatus.Content += "Rows processed: " + rows;
            lblStatus.Content += Environment.NewLine;
            lblStatus.Content += "Time taken: " + processingTimespan.Seconds + " seconds";            
        }

        public void ErrorProgressText(string errorText)
        {
            // reset the progress stuff for the next run and hide it
            lblProgress.Content = "";
            progress.Value = 0;
            SetProgressUIElementsVisibility(System.Windows.Visibility.Hidden);

            // unlock the UI buttons
            LockUIElementsForProcessing(false);
            
            lblOutputDetails.Content= errorText;
            
        }

        private void ProcessSingleFile(string filename)
        {
           
            
            _rowsProcessed = 0;
            _inputFileStreamLength = 0;
            _jaggedLines = 0;
            // get the salt from the text box, which may or may not be visible..            
            _salt = txtSalt.Text;            

            System.Windows.Threading.Dispatcher dispatcher = Dispatcher;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            bool wasCancelled = false;
            int NHSNumIndex = cmbNHSNumber.SelectedIndex;

            // anonymous delegate, this could be moved out for readability?
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (MainWork(filename, NHSNumIndex, dispatcher, ref wasCancelled))
                {
                    return;
                }
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {                
                string configWriteLine = "";
                if (!wasCancelled)
                {
                    configWriteLine = "Processing Finished At: " + DateTime.Now;
                    UpdateProgressDelegate update = new UpdateProgressDelegate(UpdateProgressText);
                    dispatcher.BeginInvoke(update, _inputFileStreamLength, _inputFileStreamLength, _rowsProcessed, _validNHSNumCount, _inValidNHSNumCount, _missingNHSNumCount);                    
                }
                else                 
                {
                    configWriteLine = "Processing Cancelled At: " + DateTime.Now;
                }

                var writeConfigStream = new FileStream(_outputRunLogFileName, FileMode.Append, FileAccess.Write);
                using (StreamWriter streamConfigWriter = new StreamWriter(writeConfigStream))
                {
                    streamConfigWriter.WriteLine(configWriteLine);
                    streamConfigWriter.WriteLine("Lines Processed: " + _rowsProcessed);                    
                    streamConfigWriter.WriteLine("Jagged Lines: " + _jaggedLines);
                    if (_performNHSNumberValidation)
                    {
                        streamConfigWriter.WriteLine("Valid NHSNumbers (10 character number found and passed checksum) : " + _validNHSNumCount);
                        streamConfigWriter.WriteLine("Invalid NHSNumbers (data was present but failed the checksum) : " + _inValidNHSNumCount);
                        streamConfigWriter.WriteLine("Missing NHSNumbers (blank string or space) : " + _missingNHSNumCount);
                    }
                }

                SignRunLogFile();
            };

            worker.RunWorkerAsync();
        }

        private bool MainWork(string filename, int indexOfNHSNumber, Dispatcher dispatcher, ref bool wasCancelled)
        {

            _validNHSNumCount = 0;
            _inValidNHSNumCount = 0;
            _missingNHSNumCount = 0;
            _rowsProcessed = 0;

            // the thing that gets the digest..
            Crypto crypto = new Crypto();            

            if (usingEncryptedSalt)
            {
                crypto.SetEncryptedSalt(File.ReadAllBytes(encryptedSaltFile));
            }
            else 
            {
                crypto.SetPlainTextSalt(_salt);
            }

            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            FileStream writeStream;
            try
            {
                writeStream = new FileStream(_outputFolder + "\\" + "OpenPseudonymised_" + _outputFileNameOnly, FileMode.Create, FileAccess.Write);
            }
            catch (IOException)
            {
                ErrorProgressDelegate error = new ErrorProgressDelegate(ErrorProgressText);
                dispatcher.BeginInvoke(error, "OpenPseudonymiser cannot create the Output file. Is the output file already open?");
                return false;
            }

            _inputFileStreamLength = fileStream.Length;
            long totalCharsRead = 0;

            SortedList<string, int> inputFields;
            SortedList<int, string> outputFields;                
            GetInputAndOutputFields(out inputFields, out outputFields);
                  
            WriteRunLogFile(inputFields, outputFields);

            using (StreamWriter streamWriter = new StreamWriter(writeStream))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    // 'read off' the first line, these are the input columns headings
                    string[] inputHeadings = streamReader.ReadLine().Split(',');

                    // write the first line as the selected column headings
                    string lineToWrite = "Digest,";
                    foreach (int key in outputFields.Keys) // keys int he output are indexes (opposite to input SortedList)
                    {
                        lineToWrite += outputFields[key] + ",";
                    }

                    // Do we want to do any NHSNumber checksum validation?
                    if (_performNHSNumberValidation)
                    {
                        lineToWrite += "validNHS,";
                    }

                    // strip trailing comma
                    lineToWrite = lineToWrite.Substring(0, lineToWrite.Length - 1);
                    streamWriter.WriteLine(lineToWrite);

                    // We have no way of knowing how many lines there are in the file without opening the whole thing, which would kill the app.
                    // So we will use a fixed size buffer and manually look for lines inside it.
                    int _bufferSize = 16384;
                    char[] readBuffer = new char[_bufferSize];

                    StringBuilder workingBuffer = new StringBuilder(); // where we will store the left over stuff after we split the read buffer into lines

                    // read into the buffer
                    long charsRead = streamReader.Read(readBuffer, 0, _bufferSize);
                    totalCharsRead += charsRead;

                    while (charsRead > 0)
                    {
                        if (worker.CancellationPending)
                        {
                            wasCancelled = true;
                            //display cancellation message
                            CancelProgressDelegate canceller = new CancelProgressDelegate(CancelProgressText);
                            dispatcher.BeginInvoke(canceller, _rowsProcessed);
                            return true;
                        }

                        // put the stuff we just read from the file into our working buffer
                        workingBuffer.Append(readBuffer);

                        // slice the workingBuffer into lines
                        string[] linesArray = workingBuffer.ToString().Split('\n');

                        // process all the lines EXCEPT THE LAST ONE in the lines array (the last one is likely to be incomplete)
                        for (int i = 0; i < (linesArray.Length - 1); i++)
                        {
                            string line = linesArray[i];                                                                
                            // the line should have the same number of columns as the ColumnCollection, if not then up the jagged lines count, and skip processing
                            string[] lineColumns = line.Split(',');
                            if (lineColumns.Length != ColumnCollection.Count)
                            {
                                _jaggedLines++;
                            }
                            else
                            {
                                // get the columns for crypting using the inputFields, since this is a sorted list we always get the indexes from aphabetically ordered keys
                                SortedList<string, string> hashNameValueCollection = new SortedList<string, string>();
                                
                                // first column is the digest
                                foreach (string key in inputFields.Keys)
                                {
                                    string theData = lineColumns[inputFields[key]];
                                    
                                    // we always process the one they selected as NHSNumber ..
                                    if (_performNHSNumberValidation)
                                    {
                                        string nhskey = inputHeadings[indexOfNHSNumber - 1];
                                        if (nhskey == key)
                                        {
                                            theData = crypto.ProcessNHSNumber(theData);
                                        }
                                    }                                    
                                    hashNameValueCollection.Add(key, theData);
                                }
                                string digest = crypto.GetDigest(hashNameValueCollection);
                                string validNHS = "";
                                lineToWrite = digest + ",";

                                // output the rest of the columns in the output list
                                foreach (int key in outputFields.Keys) // keys in the output are indexes (opposite to input SortedList)
                                {
                                    // Look for column heading that is a date..
                                    if (_processDateColumns.Contains(outputFields[key]))
                                    {
                                        lineToWrite += crypto.RoundDownDate(lineColumns[key]) + ",";
                                    }
                                    else
                                    {
                                        lineToWrite += lineColumns[key] + ",";
                                    }                                    
                                }

                                // last column is the NHS Validation (if requested)                                
                                if (_performNHSNumberValidation)
                                {
                                    // find the NHSNumber in the list of input columns and validate it
                                    string key = inputHeadings[indexOfNHSNumber-1];
                                    {
                                        

                                        string trimmedNHSNumber = lineColumns[indexOfNHSNumber-1].Trim();
                                        // trimmed data is length < 1 so we call this missing NHS Number
                                        if (trimmedNHSNumber.Length < 1)
                                        {
                                            validNHS = "-1";
                                            _missingNHSNumCount++;
                                        }
                                        else
                                        {
                                            // we have data for the NHS field, is it valid?           
                                            string cleanedNHSNumber = crypto.ProcessNHSNumber(trimmedNHSNumber);
                                            if (NHSNumberValidator.IsValidNHSNumber(cleanedNHSNumber))
                                            {
                                                validNHS = "1";
                                                _validNHSNumCount++;
                                            }
                                            else
                                            {
                                                validNHS = "0";
                                                _inValidNHSNumCount++;
                                            }
                                        }
                                        lineToWrite += validNHS + ",";
                                        
                                    }
                                    
                                }                                    

                                // we're done writing the output line now. Strip trailing comma.
                                lineToWrite = lineToWrite.Substring(0, lineToWrite.Length - 1);
                                // some files have a double line break at the end of the lines, remove this.
                                lineToWrite = lineToWrite.Replace(Environment.NewLine, "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "");  
                                streamWriter.WriteLine(lineToWrite);
                            }
                        }
                        _rowsProcessed += linesArray.Length -1;

                        // set the working buffer to be the last line, so the next pass can concatonate
                        string lastLine = linesArray[linesArray.Length - 1];
                        workingBuffer = new StringBuilder(lastLine);

                        UpdateProgressDelegate update = new UpdateProgressDelegate(UpdateProgressText);
                        dispatcher.BeginInvoke(update, totalCharsRead, _inputFileStreamLength, _rowsProcessed, _validNHSNumCount, _inValidNHSNumCount, _missingNHSNumCount);
                              
                        // empty the readbuffer, or the last read will only partially fill it, and we'll have some old data in the tail
                        readBuffer = new char[_bufferSize];
                        // read the next lot                        
                        charsRead = streamReader.Read(readBuffer, 0, _bufferSize);
                        totalCharsRead += charsRead;
                    }
                }
            }
            return false;
        }
  
        private void GetInputAndOutputFields(out SortedList<string, int> inputFields, out SortedList<int, string> outputFields)
        {
            // determine which columns to use for Digest, and which ones to use for Output. Store a list of indexes in the arrays
            inputFields = new SortedList<string, int>();    // we want this to sort on Name every time
            outputFields = new SortedList<int, string>();   // we want this to sort on index, to presenve the format of the original file

            int indexInColumnCollection = 0;
            if (!_usingPreConfigSettings)
            {
                foreach (ColumnData columnData in ColumnCollection)
                {
                    if (columnData.UseForDigest)
                    {
                        inputFields.Add(columnData.ColumnHeading, indexInColumnCollection);
                    }
                    if (columnData.UseForOutput)
                    {
                        outputFields.Add(indexInColumnCollection, columnData.ColumnHeading);
                    }
                    indexInColumnCollection++;
                }
            }
            else 
            {
                foreach (ColumnData columnData in ColumnCollection)
                {
                    if (_preConfigInputColumns.ToList<string>().Contains(columnData.ColumnHeading))
                    {
                        inputFields.Add(columnData.ColumnHeading, indexInColumnCollection);
                    }
                    if (_preConfigOutputColumns.ToList<string>().Contains(columnData.ColumnHeading))                    
                    {
                        outputFields.Add(indexInColumnCollection, columnData.ColumnHeading);
                    }
                    indexInColumnCollection++;
                }
            }
        }

        private void WriteSettingsFile(SortedList<string, int> inputFields, SortedList<int, string> outputFields, string settingsFile)
        {            

            var writeConfigStream = new FileStream(settingsFile, FileMode.Create, FileAccess.Write);
            using (StreamWriter streamConfigWriter = new StreamWriter(writeConfigStream))
            {
                
                
                string digestCols = "";
                foreach (string key in inputFields.Keys)
                {
                    digestCols += key + ",";   
                }
                if (digestCols.Length > 0)
                {
                    digestCols = digestCols.Substring(0, digestCols.Length - 1);
                }
                streamConfigWriter.WriteLine("digest:" + digestCols);
                
                

                string outputCols = "";
                foreach (string key in outputFields.Values)
                {
                    outputCols += key + ",";
                }
                if (outputCols.Length > 0)
                {
                    outputCols = outputCols.Substring(0, outputCols.Length - 1);
                }
                streamConfigWriter.WriteLine("output:" + outputCols);


                streamConfigWriter.WriteLine("processAsDate:" + string.Join(",",_processDateColumns.ToArray()));
                
            }
        }
       
  
        private void WriteRunLogFile(SortedList<string, int> inputFields, SortedList<int, string> outputFields)
        {
            var writeConfigStream = new FileStream(_outputRunLogFileName, FileMode.Create, FileAccess.Write);
            using (StreamWriter streamConfigWriter = new StreamWriter(writeConfigStream))
            {
                streamConfigWriter.WriteLine("OpenPseudonymiser - RunLog File");
                streamConfigWriter.WriteLine("----------------------------------------------------------");
                streamConfigWriter.WriteLine("Run on: " + DateTime.Now);
                streamConfigWriter.WriteLine("Input File: " + _inputFile);
                streamConfigWriter.WriteLine("Input File Lengh " + _inputFileStreamLength);
                streamConfigWriter.WriteLine("Output folder: " + _outputFolder);
                streamConfigWriter.WriteLine("----------------------------------------------------------");
                streamConfigWriter.WriteLine("Digest Column(s) Selected:");
                foreach (string key in inputFields.Keys)
                {
                    streamConfigWriter.WriteLine(key);
                }
                streamConfigWriter.WriteLine("----------------------------------------------------------");
                streamConfigWriter.WriteLine("Output Column(s) Used:");
                foreach (string value in outputFields.Values)
                {
                    streamConfigWriter.WriteLine(value);
                }
                streamConfigWriter.WriteLine("----------------------------------------------------------");
                if (_userSelectedSalt)
                {
                    streamConfigWriter.WriteLine("User selected salt: " + _salt);
                }                   
                        
                streamConfigWriter.WriteLine("----------------------------------------------------------");
                streamConfigWriter.WriteLine("Processing Start: " + _processingStartTime);
            }
        }

        // reads the contents of the file and appends an MD5
        private void SignRunLogFile()
        {
            var readConfigStream = new FileStream(_outputRunLogFileName, FileMode.Open, FileAccess.Read);             

            string hashThis = "";
            using (StreamReader streamConfigReader = new StreamReader(readConfigStream))
            {
                hashThis = streamConfigReader.ReadToEnd();
            }

            var writeConfigStream = new FileStream(_outputRunLogFileName, FileMode.Append, FileAccess.Write);
            using (StreamWriter streamConfigWriter = new StreamWriter(writeConfigStream))
            {
                streamConfigWriter.WriteLine("----------------------------------------------------------");
                streamConfigWriter.WriteLine("OpenPseudonymiser Config Security: " + CryptHelper.md5encrypt(hashThis + "seal"));
            }
        }


        

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            worker.CancelAsync();
            btnCancel.IsEnabled = false;
        }

    }
}
