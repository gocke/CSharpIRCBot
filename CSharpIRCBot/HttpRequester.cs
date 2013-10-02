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
        public static XmlDocument GetHttpXmlDocument(string adress)
        {
            //first we get a Stream of the Xml, for that we use a webrequest
            Stream resStream = GetHttpStream(adress);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            //Pushing the Stream in an XmlReader
            XmlReader tempXmlReader = XmlReader.Create(resStream);

            //Creating a new XmlDocument, and loading the Xml from the XmlReader
            XmlDocument tempXmlDocument = new XmlDocument();
            tempXmlDocument.Load(tempXmlReader);

            return tempXmlDocument;
        }

        public static XmlDocument GetHttpXmlDocument(string adress, string username, string password)
        {
            //first we get a Stream of the Xml, for that we use a webrequest
            Stream resStream = GetAuthHttpStream(adress, username, password);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            try
            {
                //Pushing the Stream in an XmlReader
                XmlReader tempXmlReader = XmlReader.Create(resStream);
                
                //Creating a new XmlDocument, and loading the Xml from the XmlReader
                XmlDocument tempXmlDocument = new XmlDocument();
                tempXmlDocument.Load(tempXmlReader);

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
            Stream resStream = GetHttpStream(adress);
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
            Stream resStream = GetHttpStream(adress);
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
        private static Stream GetHttpStream(string adress)
        {
            try
            {
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(adress);

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

        //This method returns a stream on the response of a RESTful webrequest, i think it should be a standard function in Net package
        private static Stream GetAuthHttpStream(string adress, string username, string password)
        {
            try
            {
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(adress);
                request.Credentials = new NetworkCredential(username, password);

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
