using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenPseudonymiser;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
bool success = true;

OpenPseudonymiser.Crypto crypto = new Crypto();

// set the salt to a plain text word/phrase
string salt = "mackerel";
crypto.SetPlainTextSalt(salt);

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

Console.WriteLine("Test for ( nonEncryptedSalt ): " + success);
Console.WriteLine("Press any key to finish...");
Console.ReadKey();
        }
    }
}
