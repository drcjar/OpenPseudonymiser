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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using OpenPseudonymiser;
using OpenPseudonymiser.CryptoLib;
using System.Collections;
using System.IO;
using System.Security.Principal;
using RSAEncryptionLib;
using System.Data.SqlClient;
using System.Data;

/// <summary>
/// A class that wraps the call to Crypto.GetDigest as a static method so that it can be called from SQL Server (CLR UDF)
/// </summary>
public class SQLCrypto
{

    [SqlFunction()]
    public static string GetDigest(string inputString, string plainTextSalt)
    {
        OpenPseudonymiser.Crypto crypto = new Crypto();
        crypto.SetPlainTextSalt(plainTextSalt);
        // The input: a name/value pair
        var nameValue = new SortedList<string, string>();

        // the input is a name/value pair, so we have to assign an arbritrary "name" element, this is set to "NHSNumber"
        nameValue.Add("NHSNumber", inputString);

        // Call the GetDigest method and receive the digest.
        string digest = crypto.GetDigest(nameValue);

        crypto = null;
        return digest;
    }




    [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
    public static string GetDigestUsingEncryptedSaltFile(string inputString, string locationOfFile)
    {
        // we'll use the digest string to return any errors that might crop up. This proc requires file system access, so SQL needs to be set up correctly and getting the place where the error occured might proove handy in debugging
        string digest = "Error: Pre Init";
        OpenPseudonymiser.Crypto crypto = new Crypto();
        FileStream fs = null;

        string PublicKey = @"<RSAKeyValue><Modulus>kcVhdr4DaGLAE2BUEPQSYTJ8JRw9NGsms45r2CEYKcElP4BUGEQnN9R4A8CMM1YZCqu5VbXvPoLZ9i/G8AL6g5YuD7MRTI60Xf930yHjCRNX2NiYX/FrKZrA6+T/GHoh9LjuZXBX75kwj53/8yP4uppW5pWRi/diDmPNrH4qnxk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        WindowsImpersonationContext OriginalContext = null;
        try
        {
            digest = "Error: Init";
            //Impersonate the current SQL Security context
            WindowsIdentity CallerIdentity = SqlContext.WindowsIdentity;
            //WindowsIdentity might be NULL if calling context is a SQL login
            if (CallerIdentity != null)
            {
                digest = "Error: Before Impersonation";
                OriginalContext = CallerIdentity.Impersonate();
                fs = new FileStream(locationOfFile, FileMode.Open);
                digest = "Error: Before File Open";
                byte[] m_Bytes = ReadToEnd(fs);

                // we have to do the RSA encryption stuff here, rather than in the Crypto lib or SQL can't get a reference to the RSA assembly:
                // see NetRat's final workaround: http://stackoverflow.com/questions/5422158/could-not-load-file-or-assembly-or-one-of-its-dependencies-exception-from-hre
                digest = "Error: Before RSA Encryption";
                RSAEncryption myRsa = new RSAEncryption();
                myRsa.LoadPublicFromEmbedded(PublicKey);

                byte[] decryptMsg = myRsa.PublicDecryption(m_Bytes);
                string unencryptedSalt = Encoding.UTF8.GetString(decryptMsg);

                digest = "Error: Before Plain Salt Set";
                digest = unencryptedSalt;
                crypto.SetPlainTextSalt(unencryptedSalt);
                // The input: a name/value pair
                var nameValue = new SortedList<string, string>();

                nameValue.Add("NHSNumber", inputString);

                // Call the GetDigest method and receive the digest.
                digest = crypto.GetDigest(nameValue);

                crypto = null;
            }
            else fs = null;
        }
        catch (Exception e)
        {
            //If file does not exist or for any problems with opening the file, 
            // set filestream to null and return the error we caught
            digest = e.Message;
            fs.Close();
            fs = null;
        }
        finally
        {
            fs.Close();
            fs = null;
            //Revert the impersonation context; note that impersonation is needed only
            //when opening the file. 
            //SQL Server will raise an exception if the impersonation is not undone 
            // before returning from the function.
            if (OriginalContext != null)
                OriginalContext.Undo();
        }

        return digest;
    }



    public static byte[] ReadToEnd(System.IO.Stream stream)
    {
        long originalPosition = stream.Position;
        stream.Position = 0;

        try
        {
            byte[] readBuffer = new byte[4096];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        byte[] temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }


    [SqlFunction()]
    public static string ProcessNHSNumber(string inputString)
    {
        OpenPseudonymiser.Crypto crypto = new Crypto();
        string ret = crypto.ProcessNHSNumber(inputString);
        crypto = null;
        return ret;
    }

    [SqlFunction()]
    public static string RoundDownDate(string inputString)
    {
        OpenPseudonymiser.Crypto crypto = new Crypto();
        string ret = crypto.RoundDownDate(inputString);
        crypto = null;
        return ret;
    }


    [SqlFunction()]
    public static bool IsValidNHSNumber(string inputString)
    {
        return NHSNumberValidator.IsValidNHSNumber(inputString);
    }




    [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
    public static string GetDigestUsingStoredEncryptedSalt(string inputString)
    {        
        string digest = "ERROR: Unable to get salt from table dbo.EncryptedSalt";
        SqlBytes encryptedSalt;
        OpenPseudonymiser.Crypto crypto = new Crypto();

        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();                       // open the connection            
            SqlCommand cmd = new SqlCommand("SELECT top 1 EncryptedSalt from EncryptedSalt", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                encryptedSalt = reader.GetSqlBytes(0);                    
            }
            else 
            {
                digest = "No encrypted salt found in table EncryptedSalt. Please call the stored procedure: StoreEncryptedSalt with the path to your file";
                reader.Close();
                reader = null;
                return digest;
            }
            reader.Close();
            reader = null;

            // we have to do the RSA encryption stuff here, rather than in the Crypto lib or SQL can't get a reference to the RSA assembly:
            // see NetRat's final workaround: http://stackoverflow.com/questions/5422158/could-not-load-file-or-assembly-or-one-of-its-dependencies-exception-from-hre
            digest = "Error: Before RSA Encryption";
            RSAEncryption myRsa = new RSAEncryption();
            string PublicKey = @"<RSAKeyValue><Modulus>kcVhdr4DaGLAE2BUEPQSYTJ8JRw9NGsms45r2CEYKcElP4BUGEQnN9R4A8CMM1YZCqu5VbXvPoLZ9i/G8AL6g5YuD7MRTI60Xf930yHjCRNX2NiYX/FrKZrA6+T/GHoh9LjuZXBX75kwj53/8yP4uppW5pWRi/diDmPNrH4qnxk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            myRsa.LoadPublicFromEmbedded(PublicKey);

            // we can transform the SQLByyes into a bytes[] for the PublicDecription by accessing the buffer..
            byte[] decryptMsg = myRsa.PublicDecryption(encryptedSalt.Buffer);
            string unencryptedSalt = Encoding.UTF8.GetString(decryptMsg);
            // The input: a name/value pair
            var nameValue = new SortedList<string, string>();
            nameValue.Add("NHSNumber", inputString);
            

            digest = "Error: Before Plain Salt Set";
            digest = unencryptedSalt;
            crypto.SetPlainTextSalt(unencryptedSalt);

            // Call the GetDigest method and receive the digest.
            digest = crypto.GetDigest(nameValue);
        }
        return digest;
    }




    public static void StoreEncryptedSalt(string locationOfFile)
    {        
        FileStream fs = null;
        byte[] m_Bytes = null;

        WindowsImpersonationContext OriginalContext = null;
        try
        {            
            //Impersonate the current SQL Security context
            WindowsIdentity CallerIdentity = SqlContext.WindowsIdentity;
            //WindowsIdentity might be NULL if calling context is a SQL login
            if (CallerIdentity != null)
            {
                OriginalContext = CallerIdentity.Impersonate();
                fs = new FileStream(locationOfFile, FileMode.Open);                
                m_Bytes = ReadToEnd(fs);                
            }
            else fs = null;
        }
        catch (Exception e)
        {
            //If file does not exist or for any problems with opening the file, 
            // set filestream to null and return the error we caught            
            fs.Close();
            fs = null;
        }
        finally
        {
            fs.Close();
            fs = null;
            //Revert the impersonation context; note that impersonation is needed only
            //when opening the file. 
            //SQL Server will raise an exception if the impersonation is not undone 
            // before returning from the function.
            if (OriginalContext != null)
                OriginalContext.Undo();
        }


        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();                       // open the connection            
            //string encryptedSalt = CryptHelper.UTF8ByteArrayToString(m_Bytes);            

            SqlCommand cmd = new SqlCommand("UPDATE EncryptedSalt SET LastSavedAt = GetDate(), EncryptedSalt = @encryptedSalt", conn);
            cmd.Parameters.Add("@encryptedSalt", SqlDbType.VarBinary).Value = m_Bytes;       
            cmd.ExecuteNonQuery();            
            cmd = null;
        }        
    }


}
      











/* we might need this if the inline file access is too slow above on encrypted salt method above
 * Leaving it here in case we want to load the encrypted salt first into a table or something

 * 
 * [SqlFunction(FillRowMethodName = "FillRow", TableDefinition = "FileContents varbinary(max)")]
    public static IEnumerator GetFile(String FileName)
    {
        return new SingleFileLoader(FileName);
    }
    public static void FillRow(Object obj, out SqlBytes sc)
    {
        //If non-existent file, return SQL NULL
        if (obj != null) sc = new SqlBytes((Stream)obj);
        else sc = SqlBytes.Null;
    }
 * 
 * 
public partial class SingleFileLoader : IEnumerator
{
    private FileStream fs;
    private bool IsBegin = true;
    private String fn;
    public SingleFileLoader(String FileName)
    {
        fn = FileName;
        SingleFileLoaderHelper();
    }
    private void SingleFileLoaderHelper()
    {
        WindowsImpersonationContext OriginalContext = null;
        try
        {
            //Impersonate the current SQL Security context
            WindowsIdentity CallerIdentity = SqlContext.WindowsIdentity;
            //WindowsIdentity might be NULL if calling context is a SQL login
            if (CallerIdentity != null)
            {
                OriginalContext = CallerIdentity.Impersonate();
                fs = new FileStream(fn, FileMode.Open);
            }
            else fs = null;
        }
        catch
        {
            //If file does not exist or for any problems with opening the file, 
            // set filestream to null
            fs = null;
        }
        finally
        {
            //Revert the impersonation context; note that impersonation is needed only
            //when opening the file. 
            //SQL Server will raise an exception if the impersonation is not undone 
            // before returning from the function.
            if (OriginalContext != null)
                OriginalContext.Undo();
        }
    }
    public Object Current
    {
        get
        {
            return fs;
        }
    }
    public bool MoveNext()
    {
        //Ensure returns only one row
        if (IsBegin == true)
        {
            IsBegin = false;
            return true;
        }
        else
        {
            //Close the file after SQL Server is done with it
            if (fs != null) fs.Close();
            return false;
        }
    }
    public void Reset()
    {
        IsBegin = true;
        SingleFileLoaderHelper();
    }
}


*/