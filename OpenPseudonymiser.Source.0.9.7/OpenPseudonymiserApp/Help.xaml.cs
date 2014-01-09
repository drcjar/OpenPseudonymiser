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

using System.Windows;

namespace OpenPseudonymiser
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();

            textBlock1.Text = OpenPResources.HelpText1;
            textBlock2.Text =OpenPResources.HelpText2;            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }
    }
}
