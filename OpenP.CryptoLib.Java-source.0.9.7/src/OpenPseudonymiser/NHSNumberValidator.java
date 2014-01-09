
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
  
    The NHSNumber validation code is based on an Open Source work by Peter Fisher (http://peterfisher.me.uk)
    The original code can be found on GitHub: https://github.com/pfwd/NHSNumber-Validation
 * 
 *     This software is issued under the GNU General Public License. The university
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
package OpenPseudonymiser;

public class NHSNumberValidator {

    public static boolean isValidNHSNumber(String NHSNumber) {            

            int checkNumber;
            
            int[] multiplers;            

            multiplers = new int[9];

            multiplers[0] = 10;
            multiplers[1] = 9;
            multiplers[2] = 8;
            multiplers[3] = 7;
            multiplers[4] = 6;
            multiplers[5] = 5;
            multiplers[6] = 4;
            multiplers[7] = 3;
            multiplers[8] = 2;

            /// Make sure the input is valid
            if (validateInput(NHSNumber))
            {
                /// The current number
                int currentNumber = 0;
                /// The sum of the multiplers
                int currentSum = 0;
                /// The current Multipler in use
                int currentMultipler = 0;
                /// Get the check number
                String currentString = "";
                String checkDigit = NHSNumber.substring(NHSNumber.length() - 1, NHSNumber.length());
                checkNumber = Integer.parseInt(checkDigit);
                /// The remainder after the sum calculations
                int remainder = 0;
                /// The total to be checked against the check number
                int total = 0;

                // Loop over each number in the string and calculate the current sum
                for (int i = 0; i <= 8; i++)
                {
                    currentString = NHSNumber.substring(i, i+1);
                    currentNumber = Integer.parseInt(currentString);
                    currentMultipler = multiplers[i];
                    currentSum = currentSum + (currentNumber * currentMultipler);
                }

                /// Calculate the remainder and get the total
                remainder = currentSum % 11;
                total = 11 - remainder;

                /// Now we have our total we can validate it against the check number
                if (total == 11)
                {
                    total = 0;
                }                

                if (total == checkNumber)
                {
                    return true;
                }
                
            }
            return false;
        }
        /// <summary>
        /// Validates the input
        /// Makes sure that the NHSNumber is numeric and 10 digits long
        /// </summary>
        static boolean validateInput(String NHSNumber)
        {            
            if (NHSNumber.length() == 10)
            {
                if (isDigitsOnly(NHSNumber))
                {
                    return true;
                }
            }
            return false;
        }

        static boolean isDigitsOnly(String str)
        {
            for(int i = 0; i < str.length(); i++)
            {
                char c = str.charAt(i);
            
                if (c < '0' || c > '9')
                    return false;
            }
            
            return true;
        }
}
