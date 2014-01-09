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


public class NHSNumberValidatorTest {
    
    public NHSNumberValidatorTest() {
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
     * Test of SetPlainTextSalt method, of class Crypto.
     */    
    @Test
    public void testNHSNumberValidator() throws NoSuchAlgorithmException, Exception {
        System.out.println("testNHSNumberValidator");

        Crypto crypto = new Crypto();

        assertTrue("4505577104".equals(crypto.ProcessNHSNumber("450-557-710-4")));
        
        assertTrue(NHSNumberValidator.isValidNHSNumber(crypto.ProcessNHSNumber("4505577104")));
        assertFalse(NHSNumberValidator.isValidNHSNumber(crypto.ProcessNHSNumber("1")));
        assertFalse(NHSNumberValidator.isValidNHSNumber(crypto.ProcessNHSNumber("fish")));
        assertFalse(NHSNumberValidator.isValidNHSNumber(crypto.ProcessNHSNumber("1267716276817687612")));
        assertTrue(NHSNumberValidator.isValidNHSNumber(crypto.ProcessNHSNumber("450 557 7104")));
        assertTrue(NHSNumberValidator.isValidNHSNumber(crypto.ProcessNHSNumber("450-557-7104")));
        
    }    
}
