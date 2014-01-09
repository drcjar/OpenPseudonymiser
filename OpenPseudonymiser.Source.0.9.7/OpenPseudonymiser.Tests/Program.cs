
/*
    Copyright Julia Hippisley-Cox, University of Nottingham 2011 
 
    This file is part of the OpenPseudonymiser CryptoLib Tests
  
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
 
 */

using System;
using System.Collections.Generic;
using System.IO;
using OpenPseudonymiser.CryptoLib;


namespace OpenPseudonymiser.Tests
{
    class Program
    {
        /// <summary>
        /// Simple console app to run through some methods of the CryptoLib and display whether everything is working as expected.
        /// This is also the code used for the examples in the documentation
        /// </summary>        
        static void Main(string[] args)
        {
            bool success = true;

            success = success & RunDateTests();

            success = success & RunCryptTests();

            success = success & RunNHSNumberTests();

            Console.WriteLine();
            Console.WriteLine("Overall Test Success: " + success);
            Console.WriteLine();
            Console.WriteLine("Press a key to finish.");
            Console.ReadKey();
        }

       

        private static bool RunCryptTests()
        {
            bool success = true;
            
            success = success & RunPlainTextSaltCryptoLibTest();
            success = success & RunEncryptedSaltCryptoLibTest();

            return success;
        }

      
  
        /// <summary>
        /// Takes a set input and plain text salt, calls the CrypoLib and checks that the expected digest is returned.
        /// </summary>
        /// <returns>True if test suceeds</returns>
        private static bool RunPlainTextSaltCryptoLibTest()
        {
            Console.WriteLine();
            Console.WriteLine("Running Plain Text Salt test");

            bool success = true;
            Crypto crypto = new Crypto();

            string salt = "mackerel";
            crypto.SetPlainTextSalt(salt);

            // The input: a name/value pair
            var nameValue = new SortedList<string, string>();

            // any spaces in the digest will be stripped out
            nameValue.Add("NHSNumber", "943 476 5919");

            // even though we add DOB after we add NHS, it will come before NHSNumber in the input, since the SortedList will always order by alphabetical key
            nameValue.Add("DOB", "29.11.1973");            

            // Call the GetDigest method and receive the digest..
            string digest = crypto.GetDigest(nameValue);

            // we expect the following digest for the above values
            success = (digest == "ED72F814B7905F3D3958749FA90FE657C101EC657402783DB68CBE3513E76087");

            Console.WriteLine("Test for ( NonEncryptedSalt ): " + success);
            crypto = null;
            return success;
        }

        /// <summary>
        /// Takes a set input andn encypted salt (created on the www.pseudonymiser site with the plain text word of mackerel)
        /// calls the CrypoLib and checks that the expected digest is returned.
        /// </summary>
        /// <returns>True if test suceeds</returns>
        private static bool RunEncryptedSaltCryptoLibTest()
        {
            Console.WriteLine();
            Console.WriteLine("Running Encrypted Salt test");

            bool success = true;
            OpenPseudonymiser.Crypto crypto = new Crypto();
            
            string pathToEncryptedSalt = Environment.CurrentDirectory;            
            string encryptedSaltFileLocation = pathToEncryptedSalt + "\\mackerel.EncryptedSalt";
            byte[] encryptedSalt = File.ReadAllBytes(encryptedSaltFileLocation);
            crypto.SetEncryptedSalt(encryptedSalt);

            // The input: a name/value pair
            var nameValue = new SortedList<string, string>();

            // any spaces in the special case field called 'NHSNumber' will be stripped out
            nameValue.Add("NHSNumber", "9434765919");

            // even though we add DOB after we add NHS, it will come before NHSNumber in the input, since the SortedList will always order by alphabetical key
            nameValue.Add("DOB", "29.11.1973");

            // Call the GetDigest method and receive the digest..
            string digest = crypto.GetDigest(nameValue);

            // we expect the following digest for the above values
            success = (digest == "ED72F814B7905F3D3958749FA90FE657C101EC657402783DB68CBE3513E76087");

            Console.WriteLine("Test for ( EncryptedSalt  ): " + success);
            crypto = null;
            return success;
        }

        private static bool RunDateTests()
        {
            Console.WriteLine();
            Console.WriteLine("Running Date Tests");

            bool success = true;
            
            string dateString = "23.12.2006";
            success = success & RunDateTest(dateString);

            dateString = "23/12/2006";
            success = success & RunDateTest(dateString);

            dateString = "23/12/06";
            success = success & RunDateTest(dateString);

            dateString = "23.12.06";
            success = success & RunDateTest(dateString);

            dateString = "20061223";
            success = success & RunDateTest(dateString);

            return success;
        }
  
        private static bool RunDateTest(string dateString)
        {
            bool success = true;
            OpenPseudonymiser.Crypto crypto = new Crypto();
            string result = crypto.RoundDownDate(dateString);
            success = success & (result == "20060101");
            Console.WriteLine("Test for (" + dateString + "): " + success);
            crypto = null;
            return success;
        }


        private static bool RunNHSNumberTests()
        {
            Console.WriteLine();
            Console.WriteLine("Running NHSNumber validation test");

            OpenPseudonymiser.Crypto crypto = new Crypto();

            bool success = true;            
            string processedNHSNumber = "";

            // A call to ProcessNHSNumber will strip all non numeric characters from the string
            processedNHSNumber = crypto.ProcessNHSNumber("4505577104");

            // A call to the static "IsValidNHSNumber" method will return a true if the string passes the NHS number validation checksum as described here:
            // http://www.datadictionary.nhs.uk/data_dictionary/attributes/n/nhs_number_de.asp
            success = success & (NHSNumberValidator.IsValidNHSNumber(processedNHSNumber));
            Console.WriteLine("NHSNumber validation test 1: " + success);

            processedNHSNumber = crypto.ProcessNHSNumber("1");
            success = success & !(NHSNumberValidator.IsValidNHSNumber(processedNHSNumber));
            Console.WriteLine("NHSNumber validation test 2: " + success);

            processedNHSNumber = crypto.ProcessNHSNumber("fish");
            success = success & !(NHSNumberValidator.IsValidNHSNumber(processedNHSNumber));
            Console.WriteLine("NHSNumber validation test 3: " + success);

            processedNHSNumber = crypto.ProcessNHSNumber("1267716276817687612");
            success = success & !(NHSNumberValidator.IsValidNHSNumber(processedNHSNumber));
            Console.WriteLine("NHSNumber validation test 4: " + success);

            processedNHSNumber = crypto.ProcessNHSNumber("450 557 7104");
            success = success & (NHSNumberValidator.IsValidNHSNumber(processedNHSNumber));
            Console.WriteLine("NHSNumber validation test 5: " + success);

            processedNHSNumber = crypto.ProcessNHSNumber("450-557-7104");
            success = success & (NHSNumberValidator.IsValidNHSNumber(processedNHSNumber));
            Console.WriteLine("NHSNumber validation test 6: " + success);            

            Console.WriteLine("NHSNumber validation test: " + success);            
            return success;
        }
    }
}
