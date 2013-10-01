using Meebey.SmartIrc4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpIRCBot
{
    class Calculators
    {
        private static Random derpRandom = new Random();

        public static string GetRape(string channelname, IrcClient tempIRCClient)
        {
            List<string> users = GetRandomUserList(channelname, tempIRCClient);

            string resultString = @"I rape ";

            foreach(var user in users)
            {
                resultString += user + " ";
            }

            resultString += "and Mikuru!";

            return resultString;
        }

        private static List<string> GetRandomUserList(string channelname, IrcClient tempIRCClient)
        {
            List<string> Userlist = new List<string>();

            foreach (DictionaryEntry user in tempIRCClient.GetChannel(channelname).Users)
                Userlist.Add(user.Key.ToString());

            int randomNumberOfUsers = derpRandom.Next(1, Userlist.Count);
            List<string> selectedUsersList = new List<string>();

            while (randomNumberOfUsers > 0)
            {
                int selectedUserNumber = derpRandom.Next(Userlist.Count);
                selectedUsersList.Add(Userlist[selectedUserNumber]);
                Userlist.Remove(Userlist[selectedUserNumber]);
                randomNumberOfUsers--;
            }

            return selectedUsersList;
        }
    }
}
