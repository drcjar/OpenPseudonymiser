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


using System.Collections.Generic;
using System.Text;
using System;
using System.Globalization;
using RSAEncryptionLib;


namespace OpenPseudonymiser
{
    public class Crypto
    {
        /// <summary>
        /// QResearch public key for decrypting salt. Salt should be created using QResearch private key (not available in the source code)
        /// See http://www.openpseudonymiser.org/ for details on creating and storing encrypted salt files
        /// </summary>
        public static readonly string PublicKey = @"<RSAKeyValue><Modulus>kcVhdr4DaGLAE2BUEPQSYTJ8JRw9NGsms45r2CEYKcElP4BUGEQnN9R4A8CMM1YZCqu5VbXvPoLZ9i/G8AL6g5YuD7MRTI60Xf930yHjCRNX2NiYX/FrKZrA6+T/GHoh9LjuZXBX75kwj53/8yP4uppW5pWRi/diDmPNrH4qnxk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        string _salt = null;

        public Crypto()
        {            
        }

        /// <summary>
        /// Set the salt used for digest creation.
        /// Salt must be used and must not be blank.
        /// An exception is throw if GetDigest() is called without first setting the salt, or setting the salt to a blank string.
        /// </summary>        
        public void SetPlainTextSalt(string salt)
        {
            _salt = salt;
        }

        /// <summary>
        /// Set the salt used for digest creation.
        /// Salt must be used and must not be blank.
        /// An exception is throw if GetDigest() is called without first setting the salt, or setting the salt to a blank string.
        /// </summary>
        /// <param name="salt">encrypted salt file created at http://www.openpseudonymiser.org/</param>
        public void SetEncryptedSalt(byte[] encryptedSalt)
        {            
            _salt = ReadEncryptedMessageUsingPublicKey(encryptedSalt);
        }

        

 

        /// <summary>
        /// Takes a name value pair collection and produces the digest
        /// An exception is throw if GetDigest() is called without first setting the salt, or setting the salt to a blank string.
        /// Note all whitespace is stripped from the data in any field used here.
        /// </summary>
        /// <param name="nameValuePairs">One or more pairs of key/data to be used when creating the digest.</param>
        /// <returns>The Digest</returns>
        public string GetDigest(SortedList<string, string> nameValuePairs)
        {
            if (_salt == null || _salt == "")
            {
                throw new ApplicationException("Salt must be set before calling this method. Salt cannot be a blank string.");
            }

            // the fields get appended in alphabetical order, with the salt on the end
            string hashThis = "";

            // Get the columns for the digest, this is a sorted list we always get aphabetically ordered keys
            foreach (string key in nameValuePairs.Keys)
            {
                hashThis += nameValuePairs[key];
            }
            hashThis = RemoveBlanks(hashThis);
            hashThis += _salt;
            return CryptHelper.sha256encrypt(hashThis);
        }

       
        /// <summary>
        /// Strips all non numerical characters from the string
        /// </summary>        
        public string ProcessNHSNumber(string nhsNumber)
        {
            nhsNumber = nhsNumber.Trim();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < nhsNumber.Length; i++)
            {
                char c = nhsNumber[i];
                byte b = (byte)c;
                if (b > 47 && b < 58) // only the numerical stuff from ascii
                {
                    result.Append(c);
                }
                
            }
            return result.ToString();
        }

        /// <summary>
        /// As of 26/1/2012 we now always remove blanks from any field used in the digest
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>The inputString minus carriage return, new line, tab or space characters</returns>
        public string RemoveBlanks(string inputString)
        {         
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < inputString.Length; i++)
            {
                char c = inputString[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                    case '\t':
                    case ' ':
                        continue;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Rounds a date down to YYYY0101
        /// If a date string cannot be processed (see notes on input field) then it is returned unaltered
        /// </summary>
        /// <param name="dateString">
        /// Works with string with dates in the following formats:
        /// dd.mm.yyyy        
        /// dd.mm.yy
        /// dd/mm/yyyy
        /// dd.mm.yy
        /// yyyymmdd        
        /// The returned format is always in this format: 'yyyymmdd'
        /// </param>
        /// <returns>Returns the data rounded down to yyyy0101 the returned format is always in this format: 'yyyymmdd'</returns>
        public string RoundDownDate(string dateString)
        {
            string ret = dateString;
            DateTime result;
            string[] formats = { "yyyyMMdd", "dd/MM/yy", "dd/MM/yyyy", "dd.MM.yy", "dd.MM.yyyy" };
            bool success = DateTime.TryParseExact(dateString, formats, CultureInfo.CurrentCulture, DateTimeStyles.None, out result);

            if (success)
            {
                ret = result.Year + @"0101";
            }
            return ret;
        }


        private static string ReadEncryptedMessageUsingPublicKey(byte[] encryptedMessage)
        {
            RSAEncryption myRsa = new RSAEncryption();
            myRsa.LoadPublicFromEmbedded(PublicKey);

            byte[] decryptMsg = myRsa.PublicDecryption(encryptedMessage);
            return Encoding.UTF8.GetString(decryptMsg);
        }





    }
}
