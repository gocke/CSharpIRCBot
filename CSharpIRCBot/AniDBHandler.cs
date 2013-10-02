using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CSharpIRCBot
{
    class AniDBHandler
    {
        public static string GetNextEpisodeDate(string animeID)
        {
            string adress = AdressBuilder(animeID);

            XmlDocument AniDBEntry = HttpRequester.GetHttpXmlDocument(adress);

            return "";
        }

        private static string AdressBuilder(string animeID)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("http://api.anidb.net:9001/httpapi?request=anime");
            sb.Append("&client=");//{str}
            sb.Append("haruhibot");
            sb.Append("&clientver=");//{int}
            sb.Append("1");
            sb.Append("&protover=1");//{int}
            sb.Append("&aid=");
            sb.Append(animeID);

            return sb.ToString();
        }
    }
}
