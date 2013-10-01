using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpIRCBot
{
	class DanbooruHandler
	{
		//This method returns a link to an (random)(the last) imagefile, matching the provider and tags
		//this method is an overload to make a list of tags into a single tag using the booru + operator
		public static string GetFileLink(List<string> tags, string provider)
		{
			StringBuilder sb = new StringBuilder();

			//mixing the tags together, two tags are always seperated by a +
			foreach(string tag in tags)
				sb.Append(tag + "+");

			//removing the last +
			sb.Remove(sb.Length - 1, 1);

			//using the actual method, now with only 1 tag
			return GetFileLink(sb.ToString(), provider);
		}

		//This method returns a link to an (random)(the last) imagefile, matching the provider and tags
		public static string GetFileLink(string tags, string provider)
		{
			string adress = AdressBuilder(tags, provider);

			//Since we can pull multiple images at once we recieve a JArray instead of a JObject
			JArray json = HttpRequester.GetHttpJSONArray(adress);

			//json is null if we messed up
			if (json == null)
				return ("Can not get acess to provider, provider-san is an ashole");

			//jsons count is 0 if we got something but the tags found no match
			if (json.Count == 0)
				return ("No File matching the tags found, you are an idiot");

			//we only want to get the first image, so we take the first jsonObject in the JArray (we also request only one)
			var innerJObject = json.First;

			//we extract the field "file_url" from the json, works just like XML
			string file_url = (string)innerJObject["file_url"];

			//we create a link from the relative file_url
			string fileLink = LinkBuilder(file_url, provider);

			return fileLink;
		}

		//This method builds the link to our image, file_url only is a relative link like /image/33563 and not chan.sankaku.com/image/5985
		private static string LinkBuilder(string file_url, string provider)
		{
			StringBuilder sb = new StringBuilder();

			//we only need to add the provider
			switch(provider)
			{
				case "SANKAKU":
					sb.Append("http://chan.sankakucomplex.com");
					break;
				default:
					sb.Append("http://danbooru.donmai.us");
					break;
			}

			//and the link at the end
			sb.Append(file_url);

			return sb.ToString();           
		}

		//This method builds the link for our request, it uses the provider prefix, the tags and the limit
		private static string AdressBuilder(string tags, string provider)
		{
			//This just adds the text neatly, we also can write provider + tags + limit
			StringBuilder sb = new StringBuilder();

			//provider prefix
			switch (provider)
			{
				case "SANKAKU":
					sb.Append("http://chan.sankakucomplex.com/post/index.json?tags=");
					break;
				default:
					sb.Append("http://danbooru.donmai.us/post/index.json?tags=");
					break;
			}

			//adding Tags
			sb.Append(tags);
			//adding Limit
			sb.Append("&limit=1");

			return sb.ToString();
		}
	}
}
