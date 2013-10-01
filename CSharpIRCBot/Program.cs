using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpIRCBot
{
    class Program
    {
        static void Main(string[] args)
        {
            IRCBot mainIRCBot = new IRCBot();

            mainIRCBot.Connect("irc.pantsu.de", 6667, "Haruhi", "");

            mainIRCBot.JoinChannel("#hbot-test");
            //mainIRCBot.JoinChannel("#krachb00ns");

            mainIRCBot.StartListen();
        }
    }
}
