using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CSharpIRCBot
{
    class HttpRequester
    {
        public static JObject GetHttpJSONObject(string adress)
        {
            //first we get a Stream of the Json, for that we use a webrequest
            Stream resStream = GetHttpStream(adress);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            var rawJson = new StreamReader(resStream).ReadToEnd();
            //turns our raw string into a key value lookup
            var json = JObject.Parse(rawJson);  
            
            return json;
        }


        public static JArray GetHttpJSONArray(string adress)
        {
            //first we get a Stream of the Json, for that we use a webrequest
            Stream resStream = GetHttpStream(adress);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            var rawJson = new StreamReader(resStream).ReadToEnd();
            //turns our raw string into a key value lookup
            var json = JArray.Parse(rawJson);  

            return json;
        }

        //This method returns a stream on the response of a RESTful webrequest, i think it should be a standard function in Net package
        public static Stream GetHttpStream(string adress)
        {
            try
            {
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(adress);

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

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
