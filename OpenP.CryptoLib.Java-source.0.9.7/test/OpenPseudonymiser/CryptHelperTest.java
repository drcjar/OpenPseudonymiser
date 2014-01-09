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

import org.junit.After;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import static org.junit.Assert.*;

public class CryptHelperTest {
    
    public CryptHelperTest() {
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

    /**
     * Test of byteArrayToString method, of class CryptHelper.
     *//*
    @Test
    public void testByteArrayToString() {
        System.out.println("byteArrayToString");
        byte[] inputArray = null;
        String expResult = "";
        String result = CryptHelper.byteArrayToString(inputArray);
        assertEquals(expResult, result);
        // TODO review the generated test code and remove the default call to fail.
        fail("The test case is a prototype.");
    }*/

    /**
     * Test of md5encrypt method, of class CryptHelper.
     */
    @Test
    public void testMd5encrypt() throws Exception {
        System.out.println("md5encrypt");
        String phrase = "pie";
        String expResult = "EA702BA4205CB37A88CC84851690A7A5";
        String result = CryptHelper.md5encrypt(phrase);
        assertEquals(expResult, result);
    }

    /**
     * Test of sha256encrypt method, of class CryptHelper.
     */
    @Test
    public void testSha256encrypt() throws Exception {
        System.out.println("sha256encrypt");
        String phrase = "pie";
        String expResult = "558211ED72B2D6967037419DFF6F1E7CFD002D178C8FDEEB1239760D4E4C4059";
        String result = CryptHelper.sha256encrypt(phrase);
        assertEquals(expResult, result);
    }
}
