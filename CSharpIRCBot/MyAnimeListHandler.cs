using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CSharpIRCBot
{
    class MyAnimeListHandler
    {
        public enum AnimeManga {Anime, Manga};

        internal static List<Tuple<string, string>> GetAnimeManga(List<string> tags, AnimeManga tempAnimeManga)
        {
            return GetAnimeManga(tags[0], tempAnimeManga);
        }

        public static List<Tuple<string, string>> GetAnimeManga(string tag, AnimeManga tempAnimeManga)
        {
            string adress = AdressBuilder(tag, tempAnimeManga);
            XmlDocument baseXmlResponse = HttpRequester.GetHttpXmlDocument(adress, "HaruhiBot", "HaruhiBot123");

            if (baseXmlResponse == null)
                return null;

            return GetAnimeMangaDictionary(tempAnimeManga, baseXmlResponse);
        }

        public static string AdressBuilder(string tag, AnimeManga tempAnimeManga)
        {
            StringBuilder sb = new StringBuilder();

            switch(tempAnimeManga)
            {
                case AnimeManga.Anime:
                    sb.Append("http://myanimelist.net/api/anime/search.xml?q=");
                    break;
                case AnimeManga.Manga:
                    sb.Append("http://myanimelist.net/api/manga/search.xml?q=");
                    break;
            }

            sb.Append(tag);

            return sb.ToString();
        }

        public static List<Tuple<string, string>> GetAnimeMangaDictionary(AnimeManga tempAnimeManga, XmlDocument baseXmlResponse)
        {
            List<Tuple<string, string>> tempAnimeMangaDictionary = new List<Tuple<string, string>>();

            switch(tempAnimeManga)
            {
                case AnimeManga.Anime:
                    if (baseXmlResponse["anime"].ChildNodes.Count > 30)
                        return null;
                    foreach (XmlElement tempEntry in baseXmlResponse["anime"].ChildNodes)
                    {
                        tempAnimeMangaDictionary.Add(new Tuple<string, string>(tempEntry["title"].InnerText, tempEntry["image"].InnerText));
                    }
                    break;
                case AnimeManga.Manga:
                    if (baseXmlResponse["manga"].ChildNodes.Count > 30)
                        return null;
                    foreach (XmlElement tempEntry in baseXmlResponse["manga"].ChildNodes)
                    {
                        tempAnimeMangaDictionary.Add(new Tuple<string, string>(tempEntry["title"].InnerText, tempEntry["image"].InnerText));
                    }
                    break;
            }

            return tempAnimeMangaDictionary;
        }
    }
}
