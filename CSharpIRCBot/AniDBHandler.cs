using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CSharpIRCBot
{
    class AniDBHandler
    {
        //
        // AnimeID handler
        //

        //This method returns the IDs of all Animes matching the query(animeName)
        public static List<string> GetAnimeIDs(List<string> animeNameTags)
        {
            string animeNameTag = BuildAnimeQueryTag(animeNameTags);

            return GetAnimeIDs(animeNameTag);
        }

        //this method pastes all tags to one tag
        private static string BuildAnimeQueryTag(List<string> animeNameTags)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var animeNameTag in animeNameTags)
            {
                sb.Append(animeNameTag);
                sb.Append("+");
            }
            //removing the last +
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        //This method returns the IDs of all Animes matching the query(animeName)
        public static List<string> GetAnimeIDs(string animeName)
        {
            //Creating List
            List<string> animeIDsList = new List<string>();

            //creating the adress for our XmlRequest to receive the IDs
            string adress = BuildIDQueryAdress(animeName);

            //Getting the IDXmlDocument
            XmlDocument aniIDXmlDocument = HttpRequester.GetHttpXmlDocument(adress);

            //adding each ID to our IDList
            //this method returns an empty list if we have found no matches
            if (aniIDXmlDocument["animetitles"] == null
                || aniIDXmlDocument["animetitles"].ChildNodes.Count == 0)
                return animeIDsList;

            //iterating over the entrys, extracting the aid
            foreach (XmlNode animeEntry in aniIDXmlDocument["animetitles"].ChildNodes)
            {

                if (animeEntry.Attributes != null &&
                    animeEntry.Attributes.GetNamedItem("aid") != null &&
                    animeEntry.Attributes.GetNamedItem("aid").Value != null)
                {
                    animeIDsList.Add(animeEntry.Attributes.GetNamedItem("aid").Value);
                }
            }

            return animeIDsList;
        }

        private static string BuildIDQueryAdress(string animeName)
        {
            //Building the IDAdress
            StringBuilder sb = new StringBuilder();

            sb.Append("http://anisearch.outrance.pl/?task=search");
            sb.Append(@"&query=\");//{str}
            sb.Append(animeName);

            return sb.ToString();
        }

        //
        // Anime TimeSpan Handler
        //

        //This method querys AniDB and gets the TimeSpan to the next episode
        //Returns MaxTimeSpan if there is no episode
        //May return a negative Timespan if the end already aired
        public static Tuple<string, TimeSpan> GetNextEpisodeTimeSpan(string animeID)
        {
            //First we create the query Adress
            string adress = BuildAniDBQueryAdress(animeID);

            //We query the XMLDocument containing the airtimes
            XmlDocument aniDBEntry = HttpRequester.GetHttpXmlDocument(adress, DecompressionMethods.GZip);

            //AniDBEntry is null if we found no Entry
            if (aniDBEntry == null)
                return new Tuple<string, TimeSpan>("No entry", TimeSpan.MaxValue);

            //We get the Anime Name
            string animeName = GetAnimeName(aniDBEntry);

            //We get the Airtime of the next episode
            DateTime nextEpisodeDate = ExtractNextDate(aniDBEntry);

            //If there is no next episode DateTime is Maxvalue
            if (nextEpisodeDate == DateTime.MaxValue)
                return new Tuple<string, TimeSpan>(animeName, TimeSpan.MaxValue);

            //Calculating the Timespan to the Next Episode
            TimeSpan timeToNextEpisode = GetTimeUntilDateTime(nextEpisodeDate);

            return new Tuple<string, TimeSpan>(animeName, timeToNextEpisode);
        }

        private static string BuildAniDBQueryAdress(string animeID)
        {
            //Creating the adress for the aniDB query
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

        //this method extracts the title of an anime from its AniDB Entry
        private static string GetAnimeName(XmlDocument aniDBEntry)
        {
            //checking if we have titles
            if (aniDBEntry["anime"] != null && aniDBEntry["anime"]["titles"] != null)
            {
                //looping over the titles, trying to find the main title (usually the first)
                foreach (XmlNode titleXmlNode in aniDBEntry["anime"]["titles"].ChildNodes)
                {
                    if (titleXmlNode.Attributes.GetNamedItem("type").Value == "main")
                        return titleXmlNode.InnerText;
                }
            }

            return "Error: No name given";
        }

        private static DateTime ExtractNextDate(XmlDocument AniDBEntry)
        {
            //We get our currentDate
            DateTime currentDate = DateTime.Now;

            //First we check if we got the root element
            if (AniDBEntry["anime"] != null)
            {
                //and if we have a startdate
                if (AniDBEntry["anime"]["startdate"] != null)
                {
                    //we get the startDate
                    string startDateString = AniDBEntry["anime"]["startdate"].InnerText;
                    DateTime startDate;
                    DateTime.TryParse(startDateString, out startDate);

                    //we didn't reach the start if our startDate is bigger 
                    if (currentDate < startDate)
                        return startDate;
                }

                //now we need to check if we have an enddate, the series could already be over
                if (AniDBEntry["anime"]["enddate"] != null)
                {
                    //we get the endDate
                    string endDateString = AniDBEntry["anime"]["enddate"].InnerText;
                    DateTime endDate;
                    DateTime.TryParse(endDateString, out endDate);

                    //our series already ended if our endDate is smaller
                    if (endDate < currentDate)
                        return endDate;
                }

                //Now we know that we are in the series runtime
                //checking if we got an episodes node which contains all episodereleasedates
                if (AniDBEntry["anime"]["episodes"] != null)
                {
                    //getting the episodesNode
                    XmlNode episodesNode = AniDBEntry["anime"]["episodes"];

                    //iterating over the episodes to find the next one
                    foreach (XmlNode episodeNode in episodesNode.ChildNodes)
                    {
                        //we get the episodeDate
                        string episodeDateString = episodeNode["airdate"].InnerText;
                        DateTime episodeDate;
                        DateTime.TryParse(episodeDateString, out episodeDate);

                        if (currentDate < episodeDate)
                            return episodeDate;
                    }
                }
            }

            return DateTime.MaxValue;
        }

        //May return negative Timespans
        private static TimeSpan GetTimeUntilDateTime(DateTime nextEpisodeDate)
        {
            TimeSpan tempTimeSpan = nextEpisodeDate.Subtract(DateTime.Now);
            return tempTimeSpan;
        }
    }
}
