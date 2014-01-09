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

namespace OpenPseudonymiser
{
    /// <summary>
    /// Interaction logic for Licence.xaml
    /// </summary>
    public partial class Licence : Window
    {
        public Licence()
        {            
            InitializeComponent();
            
            //this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();

            // check for acceptance file. if found close this window and open the main form
            if (AcceptanceFileExists())
            {
                OpenMainWindow();
            }
            else 
            {
                textBlock1.Text = OpenPResources.LicenceText;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            WriteAcceptanceFile();
            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            var w = new MainWindow();
            w.Show();
            this.Hide();
        }
        


        private bool AcceptanceFileExists()
        {
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenPsuedonymiser");
			string acceptanceFile = Path.Combine(path, ".LicenceAcceptance");

			return File.Exists(acceptanceFile);
        }

        private void WriteAcceptanceFile()
        {
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenPsuedonymiser");

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			string acceptanceFile = Path.Combine(path, ".LicenceAcceptance");

            File.WriteAllText(acceptanceFile, String.Format("Licence Accepted on : {0}", DateTime.Now));
        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            // close the app
            Application.Current.Shutdown();
        }


    }
}
