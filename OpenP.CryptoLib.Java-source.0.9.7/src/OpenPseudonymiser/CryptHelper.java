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

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

/**
 * @version 1.0.0.0
 */
public class CryptHelper {
    
    private static String generateHash(String plainText, String hashType) throws NoSuchAlgorithmException
    {
      MessageDigest md = MessageDigest.getInstance(hashType); // SHA or MD5
    
      byte[] data = plainText.getBytes();

      md.update(data); // Reads it all at one go. Might be better to chunk it.

      byte[] digest = md.digest();

      return byteArrayToString(digest).toUpperCase();
    }
    
    public static String byteArrayToString(byte[] inputArray)
    {
        String output = "";
        
        for (int i = 0; i < inputArray.length; i++)
        {
            String hex = Integer.toHexString(inputArray[i]);
            if (hex.length() == 1) hex = "0" + hex;
            hex = hex.substring(hex.length() - 2);
            output += hex;
        }

      return output;
    }
    
    public static String md5encrypt(String phrase) throws NoSuchAlgorithmException
    {
        return generateHash(phrase, "MD5");
    }

    public static String sha256encrypt(String phrase) throws NoSuchAlgorithmException
    {
        return generateHash(phrase, "SHA-256");
    }
    
}
