
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

import com.sun.org.apache.xerces.internal.impl.dv.util.Base64;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.math.BigInteger;
import java.security.InvalidKeyException;
import java.security.KeyFactory;
import java.security.NoSuchAlgorithmException;
import java.security.PublicKey;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.RSAPublicKeySpec;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Iterator;
import java.util.Map.Entry;
import java.util.Set;
import java.util.TreeMap;
import javax.crypto.BadPaddingException;
import javax.crypto.Cipher;
import javax.crypto.IllegalBlockSizeException;
import javax.crypto.NoSuchPaddingException;

/**
 * @version 1.0.0.0
 */
public class Crypto {
    
    private String _salt = "";
    
    public Crypto()
    {
    
    }
    
    public void SetPlainTextSalt(String salt)
    {
        _salt = salt.trim();
        
        // Do not uncomment this line for any release versions!
        // System.out.println("SALT = " + _salt + ", len = " + _salt.getBytes().length);
    }
    
    public void SetEncryptedSalt(byte[] encryptedSalt) throws NoSuchAlgorithmException, InvalidKeySpecException, NoSuchPaddingException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException, UnsupportedEncodingException
    {            
        SetPlainTextSalt(ReadEncryptedMessageUsingPublicKey(encryptedSalt));      
    }
    
    public void SetEncryptedSalt(File file) throws FileNotFoundException, IOException, NoSuchAlgorithmException, InvalidKeySpecException, NoSuchPaddingException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException, UnsupportedEncodingException
    {   
        //E.g., File file = new File("/Users/Adrian/theword_pie.EncryptedSalt");

        byte[] data = new byte[(int)file.length()];
        FileInputStream fis = new FileInputStream(file);
        fis.read(data);
        fis.close();
        
        SetEncryptedSalt(data);
    }
    
    private static String ReadEncryptedMessageUsingPublicKey(byte[] encryptedMessage) throws NoSuchAlgorithmException, InvalidKeySpecException, NoSuchPaddingException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException, UnsupportedEncodingException
    {
        byte[] modulusBytes = Base64.decode("kcVhdr4DaGLAE2BUEPQSYTJ8JRw9NGsms45r2CEYKcElP4BUGEQnN9R4A8CMM1YZCqu5VbXvPoLZ9i/G8AL6g5YuD7MRTI60Xf930yHjCRNX2NiYX/FrKZrA6+T/GHoh9LjuZXBX75kwj53/8yP4uppW5pWRi/diDmPNrH4qnxk=");
        byte[] exponentBytes = Base64.decode("AQAB");

        BigInteger modulus = new BigInteger(1, modulusBytes );
        BigInteger exponent = new BigInteger(1, exponentBytes);

        RSAPublicKeySpec rsaPubKey = new RSAPublicKeySpec(modulus, exponent);
        KeyFactory fact = KeyFactory.getInstance("RSA");
        PublicKey pubKey = fact.generatePublic(rsaPubKey);

        Cipher cipher = Cipher.getInstance("RSA/ECB/NoPadding"); // Took some figuring out :/
        cipher.init(Cipher.DECRYPT_MODE, pubKey);

        byte[] plainBytes = cipher.doFinal(encryptedMessage);

        return new String(plainBytes, "UTF-8");        
    }
    
    public String GetDigest(TreeMap nameValuePairs) throws NoSuchAlgorithmException, Exception
    {
        if (_salt == null || _salt == "")
        {
            throw new Exception("Salt must be set before calling this method. Salt cannot be a blank string.");
        }

        TreeMap processedNameValuePairs = new TreeMap();

        String hashThis = "";        
        
        Set set = nameValuePairs.entrySet(); 
        Iterator i = set.iterator(); 
    
        while(i.hasNext())
        { 
            Entry me = (Entry)i.next(); 
            
            String key = (String)me.getKey();
            String val = (String)me.getValue();
            
            processedNameValuePairs.put(switchCase(key), val);
        }
        
        set = processedNameValuePairs.entrySet(); 
        i = set.iterator(); 
    
        while(i.hasNext())
        { 
            Entry me = (Entry)i.next(); 

            //Previously was...
            //if (((String)me.getKey()).equals("nhsnUMBER"))
            //Note the strange case -- that's to make sort order match .net
                                    
            hashThis += (String)me.getValue();
        }
        hashThis = RemoveBlanks(hashThis);
        hashThis += _salt;
        
        return CryptHelper.sha256encrypt(hashThis);
    }
    
    private String switchCase(String in)
    {
        String out = "";
 
       
        for(int i = 0; i < in.length(); i++)
        {
            char ch = in.charAt(i);
            
            if (Character.isUpperCase(ch))
            {
                out += Character.toLowerCase(ch); 
            }
            else if (Character.isLowerCase(ch))
            {
                out += Character.toUpperCase(ch);
            }
            else
            {
                out += ch;
            }
        }
        
        return out;
    }
    
    public String RoundDownDate(String dateString)
    {
        String ret = dateString;
        
        String[] formats = { "yyyyMMdd", "dd/MM/yy", "dd/MM/yyyy", "dd.MM.yy", "dd.MM.yyyy" };

        for(int i = 0; i < formats.length; i++)
        {
            SimpleDateFormat df = new SimpleDateFormat(formats[i]);
            df.setLenient(false);
            
            try
            {
                Date dt = df.parse(dateString);
                
                SimpleDateFormat yearFormat = new SimpleDateFormat("yyyy");
                
                ret = yearFormat.format(dt) + "0101";
                
                break;
            }
            catch(ParseException pe)
            {
                // This format was no good
            }
        }
        
        return ret;
    }
    
  
    
    public String ProcessNHSNumber(String nhsNumber)
    {
        nhsNumber = nhsNumber.trim();
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < nhsNumber.length(); i++)
        {
            char c = nhsNumber.charAt(i);
            byte b = (byte)c;
            if (b > 47 && b < 58) // only the numerical stuff from ascii
            {
                result.append(c);
            }
        }
        
        return result.toString();
    }

    ///We always remove blanks from strings in the digest as of 1 feb 2011 (version 0.9.7)
    public String RemoveBlanks(String removeBlanksFromThis) 
    {
        String ret =removeBlanksFromThis;
        ret = ret.replaceAll("\\r", "");  
        ret = ret.replaceAll("\\n", "");  
        ret = ret.replaceAll("\\t", "");  
        ret = ret.replaceAll(" ", "");  
        return ret;
    }
}
