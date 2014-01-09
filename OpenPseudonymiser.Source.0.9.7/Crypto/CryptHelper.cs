/*
    Copyright Julia Hippisley-Cox, University of Nottingham 2011 
 
    This file is part of the OpenPseudonymiser CryptoLib
  
    OpenPseudonymiser CryptoLib is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    OpenPseudonymiser CryptoLib is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
    
    For more information about this project, see  http://www.openpseudonymiser.org 
 
    This software is issued under the GNU General Public License. The university
    has made reasonable enquiries regarding granted and pending patent
    applications in the general area of this technology and is not aware of any
    granted or pending patent in Europe which restricts the use of this
    software. In the event that the university receives a notice of perceived patent
    infringement, then the university will inform users that their use of the
    software may need to or, if appropriate, must cease in the appropriate
    territory. The university does not make any warranties in this respect and each
    user shall be solely responsible for ensuring that they do not infringe any
    third party patent.
 */

using System.Security.Cryptography;
using System.Text;

namespace OpenPseudonymiser
{
    public static class CryptHelper
    {

        public static string md5encrypt(string phrase)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            using (MD5CryptoServiceProvider md5hasher = new MD5CryptoServiceProvider())
            {
                byte[] hashedDataBytes = md5hasher.ComputeHash(encoder.GetBytes(phrase));
                return byteArrayToString(hashedDataBytes);
            }
        }


        public static byte[] StringToUTF8ByteArray(string input)
        {
            return System.Text.Encoding.UTF8.GetBytes(input);
        }

        public static string UTF8ByteArrayToString(byte[] input)
        {
            return System.Text.Encoding.UTF8.GetString(input);
        }
        

        public static string byteArrayToString(byte[] inputArray)
        {
            StringBuilder output = new StringBuilder("");
            for (int i = 0; i < inputArray.Length; i++)
            {
                output.Append(inputArray[i].ToString("X2"));
            }
            return output.ToString();
        }

        public static string sha256encrypt(string phrase)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            using (SHA256Managed sha256hasher = new SHA256Managed())
            {         
                byte[] hashedDataBytes = sha256hasher.ComputeHash(encoder.GetBytes(phrase));
                return byteArrayToString(hashedDataBytes);
            }
        }
    }

    
}
