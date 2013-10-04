using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Xml;


namespace CSharpIRCBot
{
    class HttpRequester
    {
        //We have multiple GetHtmlXmlDocumentOverloads so entering parameters is easy
        //Note we could also let the GetHttpXmlDocument methods call each other, 
        //but that would be slower 
        public static XmlDocument GetHttpXmlDocument(string adress)
        {
            //first we get a Stream of the Xml, for that we use a webrequest
            Stream resStream = GetHttpStream(adress, null, null, DecompressionMethods.None);

            //Then we use the Parser method and return it
            return ParseStreamToXmlDocument(resStream);
        }

        public static XmlDocument GetHttpXmlDocument(string adress, string username, string password)
        {
            //first we get a Stream of the Xml, for that we use a webrequest with username and password
            Stream resStream = GetHttpStream(adress, username, password, DecompressionMethods.None);

            //Then we use the Parser method and return it
            return ParseStreamToXmlDocument(resStream);
        }

        public static XmlDocument GetHttpXmlDocument(string adress, DecompressionMethods decompressionMethod)
        {
            //first we get a Stream of the Xml, for that we use a webrequest with compression
            Stream resStream = GetHttpStream(adress, null, null, decompressionMethod);

            //Then we use the Parser method and return it
            return ParseStreamToXmlDocument(resStream);
        }

        public static XmlDocument GetHttpXmlDocument(string adress, string username, string password, DecompressionMethods decompressionMethod)
        {
            //first we get a Stream of the Xml, for that we use a webrequest with username and password and compression
            Stream resStream = GetHttpStream(adress, username, password, decompressionMethod);

            //Then we use the Parser method and return it
            return ParseStreamToXmlDocument(resStream);
        }

        private static XmlDocument ParseStreamToXmlDocument(Stream resStream)
        {
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            try
            {
                //Creating a new XmlDocument, and loading the Xml from the resStream
                XmlDocument tempXmlDocument = new XmlDocument();
                tempXmlDocument.Load(resStream);

                return tempXmlDocument;
            }
            catch (XmlException e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.InnerException);
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }

        public static JObject GetHttpJSONObject(string adress)
        {
            //first we get a Stream of the Json, for that we use a webrequest
            Stream resStream = GetHttpStream(adress, null, null, DecompressionMethods.None);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            try
            {
                var rawJson = new StreamReader(resStream).ReadToEnd();
                //turns our raw string into a key value lookup
                var json = JObject.Parse(rawJson);

                return json;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            
            return null;
        }


        public static JArray GetHttpJSONArray(string adress)
        {
            //first we get a Stream of the Json, for that we use a webrequest
            Stream resStream = GetHttpStream(adress, null, null, DecompressionMethods.None);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            try
            {
                var rawJson = new StreamReader(resStream).ReadToEnd();
                //turns our raw string into a key value lookup
                var json = JArray.Parse(rawJson);

                return json;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }

        //This method returns a stream on the response of a RESTful webrequest, i think it should be a standard function in Net package
        private static Stream GetHttpStream(string adress, string username, string password, DecompressionMethods decompressionMethod)
        {
            try
            {
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(adress);
                if(username != null)
                    request.Credentials = new NetworkCredential(username, password);

                request.AutomaticDecompression = decompressionMethod;

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Forbidden)
                    return null;

                // we will read data via the response stream
                return response.GetResponseStream();
            }
            catch (WebException e)
            {
                Console.WriteLine(e.StackTrace);
            }

            //if we get no response for whatever reason we return null
            return null;
        }
    }
}
