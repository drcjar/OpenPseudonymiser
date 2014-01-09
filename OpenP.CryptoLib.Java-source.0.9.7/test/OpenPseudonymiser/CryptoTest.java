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

/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package OpenPseudonymiser;

import java.io.File;
import java.security.NoSuchAlgorithmException;
import java.util.TreeMap;
import org.junit.After;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import static org.junit.Assert.*;


public class CryptoTest {
    
    public CryptoTest() {
    }

    @BeforeClass
    public static void setUpClass() throws Exception {
    }

    @AfterClass
    public static void tearDownClass() throws Exception {
    }
    
    @Before
    public void setUp() {
    }
    
    @After
    public void tearDown() {
    }

  
        
    @Test
    public void testSetPlainTextSaltMultipleProperties1() throws NoSuchAlgorithmException, Exception {
        System.out.println("testSetPlainTextSaltMultipleProperties1");

        Crypto instance = new Crypto();
        instance.SetPlainTextSalt("mackerel");
        
        TreeMap tm = new TreeMap();
         
        // any spaces in the digest will be stripped out
        tm.put("NHSNumber", "943 476 5919");                
        // even though we add DOB after we add NHS, it will come before NHSNumber in the input, since the SortedList will always order by alphabetical key
        tm.put("DOB", "29.11.1973");    

        assertEquals("ED72F814B7905F3D3958749FA90FE657C101EC657402783DB68CBE3513E76087", instance.GetDigest(tm));
    }
    
    @Test
    public void testSetPlainTextSaltMultipleProperties2() throws NoSuchAlgorithmException, Exception {
        System.out.println("testSetPlainTextSaltMultipleProperties2");

        Crypto instance = new Crypto();
        instance.SetPlainTextSalt("mackerel");
        
        TreeMap tm = new TreeMap();
         
        // This time we put DOB first
        tm.put("DOB", "29.11.1973");    
        // any spaces in the digest will be stripped out
        tm.put("NHSNumber", "943 476 5919");                
        
        assertEquals("ED72F814B7905F3D3958749FA90FE657C101EC657402783DB68CBE3513E76087", instance.GetDigest(tm));
    }
    
    
    /**
     * Test of SetEncryptedSalt method, of class Crypto.
     */
    
    @Test
    public void testSetEncryptedSalt_File() throws Exception {
        System.out.println("SetEncryptedSalt");
        File file = new File("mackerel.EncryptedSalt");
        Crypto instance = new Crypto();
        instance.SetEncryptedSalt(file);
        
        TreeMap tm = new TreeMap();
                
        tm.put("DOB", "29.11.1973");            
        tm.put("NHSNumber", "943 476 5919");         
        
        assertEquals("ED72F814B7905F3D3958749FA90FE657C101EC657402783DB68CBE3513E76087", instance.GetDigest(tm));
    }

    /**
     * Test of RoundDownDate method, of class Crypto.
     */
    
    @Test
    public void testRoundDownDate() {
        System.out.println("RoundDownDate");        
        Crypto instance = new Crypto();

        assertEquals("19790101", instance.RoundDownDate("19790305"));
        assertEquals("19790101", instance.RoundDownDate("05/03/79"));
        assertEquals("19790101", instance.RoundDownDate("05/03/1979"));
        assertEquals("19790101", instance.RoundDownDate("05.03.79"));
        assertEquals("19790101", instance.RoundDownDate("05.03.1979"));

        assertEquals("20080101", instance.RoundDownDate("20081231"));
        assertEquals("20080101", instance.RoundDownDate("31/12/08"));
        assertEquals("20080101", instance.RoundDownDate("31/12/2008"));
        assertEquals("20080101", instance.RoundDownDate("31.12.08"));
        assertEquals("20080101", instance.RoundDownDate("31.12.2008"));
    }
    
    
    @Test
    public void testRemoveBlanks() 
    {
        System.out.println("RemoveBlanks");
        String testString = "test String" +"\n" +"\r"+ "\t" ;
        Crypto instance = new Crypto();

        assertEquals("testString", instance.RemoveBlanks(testString));        
    }
}
