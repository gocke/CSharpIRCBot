using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using System.Threading;
using System.Collections;

namespace CSharpIRCBot
{
    class IRCBot
    {
        public IrcClient mainIRCClient;

        public IRCBot()
        {
            mainIRCClient = new IrcClient();

            Thread.CurrentThread.Name = "Main";

            //Making some unkickability
            mainIRCClient.AutoRelogin = true;

            // UTF-8 test
            mainIRCClient.Encoding = System.Text.Encoding.UTF8;

            // wait time between messages, we can set this lower on own irc servers
            mainIRCClient.SendDelay = 200;

            // we use channel sync, means we can use irc.GetChannel() and so on
            mainIRCClient.ActiveChannelSyncing = true;

            // here we connect the events of the API to our written methods
            // most have own event handler types, because they ship different data
            mainIRCClient.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            mainIRCClient.OnError += new ErrorEventHandler(OnError);
            mainIRCClient.OnRawMessage += new IrcEventHandler(OnRawMessage);
        }

        //
        // Connecting, Joining, Listening
        //

        public void Connect(string server, int port, string botName, string pass)
        {
            try
            {
                // here we try to connect to the server and exceptions get handled
                mainIRCClient.Connect(server, port);
            }
            catch (ConnectionException e)
            {
                // something went wrong, the reason will be shown
                System.Console.WriteLine("couldn't connect! Reason: " + e.Message);
            }

            try
            {
                // here we logon and register our nickname and so on
                mainIRCClient.Login(botName, botName);
            }
            catch (ConnectionException)
            {
                // this exception is handled because Disconnect() can throw a not
                // connected exception
            }
            catch (Exception e)
            {
                // this should not happen by just in case we handle it nicely
                System.Console.WriteLine("Error occurred! Message: " + e.Message);
                System.Console.WriteLine("Exception: " + e.StackTrace);
            }
        }

        public void JoinChannel(string channelName)
        {
            try
            {
                // join the channel
                mainIRCClient.RfcJoin(channelName);

                // testing the delay and flood protection (messagebuffer work)
                //mainIRCClient.SendMessage(SendType.Message, channelName, "I heard Aliens?");

            }
            catch (ConnectionException)
            {
                // this exception is handled because Disconnect() can throw a not
                // connected exception
            }
            catch (Exception e)
            {
                // this should not happen by just in case we handle it nicely
                System.Console.WriteLine("Error occurred! Message: " + e.Message);
                System.Console.WriteLine("Exception: " + e.StackTrace);
            }
        }

        public void PartChannel(string channelName)
        {
            try
            {
                // join the channel
                mainIRCClient.RfcPart(channelName, "I'm off to find more Aliens!", Priority.Medium);
            }
            catch (ConnectionException)
            {
                // this exception is handled because Disconnect() can throw a not
                // connected exception
            }
            catch (Exception e)
            {
                // this should not happen by just in case we handle it nicely
                System.Console.WriteLine("Error occurred! Message: " + e.Message);
                System.Console.WriteLine("Exception: " + e.StackTrace);
            }
        }

        public void StartListen()
        {
            try
            {
                // spawn a new thread to read the stdin of the console, this we use
                // for reading IRC commands from the keyboard while the IRC connection
                // stays in its own thread
                new Thread(new ThreadStart(ReadCommands)).Start();

                // here we tell the IRC API to go into a receive mode, all events
                // will be triggered by _this_ thread (main thread in this case)
                // Listen() blocks by default, you can also use ListenOnce() if you
                // need that does one IRC operation and then returns, so you need then
                // an own loop
                mainIRCClient.Listen();

                // when Listen() returns our IRC session is over, to be sure we call
                // disconnect manually
                mainIRCClient.Disconnect();
            }
            catch (ConnectionException)
            {
                // this exception is handled because Disconnect() can throw a not
                // connected exception
            }
            catch (Exception e)
            {
                // this should not happen by just in case we handle it nicely
                System.Console.WriteLine("Error occurred! Message: " + e.Message);
                System.Console.WriteLine("Exception: " + e.StackTrace);
            }
        }

        //Helper Method of Listen, this enables us to still use console commands
        public void ReadCommands()
        {
            // here we read the commands from the stdin and send it to the IRC API
            // WARNING, it uses WriteLine() means you need to enter RFC commands
            // like "JOIN #test" and then "PRIVMSG #test :hello to you"
            while (true)
            {
                string cmd = System.Console.ReadLine();
                if (cmd.StartsWith("/list"))
                {
                    int pos = cmd.IndexOf(" ");
                    string channel = null;
                    if (pos != -1)
                    {
                        channel = cmd.Substring(pos + 1);
                    }

                    IList<ChannelInfo> channelInfos = mainIRCClient.GetChannelList(channel);
                    Console.WriteLine("channel count: {0}", channelInfos.Count);
                    foreach (ChannelInfo channelInfo in channelInfos)
                    {
                        Console.WriteLine("channel: {0} user count: {1} topic: {2}",
                                          channelInfo.Channel,
                                          channelInfo.UserCount,
                                          channelInfo.Topic);
                    }
                }
                else
                {
                    mainIRCClient.WriteLine(cmd);
                }
            }
        }

        //
        // Incoming Messages
        //

        // this method we will use to analyse queries (also known as private messages)
        public void OnQueryMessage(object sender, IrcEventArgs e)
        {
            switch (e.Data.MessageArray[0])
            {
                // debug stuff
                case "dump_channel":
                    string requested_channel = e.Data.MessageArray[1];
                    // getting the channel (via channel sync feature)
                    Channel channel = mainIRCClient.GetChannel(requested_channel);

                    // here we send messages
                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, "<channel '" + requested_channel + "'>");

                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, "Name: '" + channel.Name + "'");
                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, "Topic: '" + channel.Topic + "'");
                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, "Mode: '" + channel.Mode + "'");
                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, "Key: '" + channel.Key + "'");
                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, "UserLimit: '" + channel.UserLimit + "'");

                    // here we go through all users of the channel and show their
                    // hashtable key and nickname
                    string nickname_list = "";
                    nickname_list += "Users: ";
                    foreach (DictionaryEntry de in channel.Users)
                    {
                        string key = (string)de.Key;
                        ChannelUser channeluser = (ChannelUser)de.Value;
                        nickname_list += "(";
                        if (channeluser.IsOp)
                        {
                            nickname_list += "@";
                        }
                        if (channeluser.IsVoice)
                        {
                            nickname_list += "+";
                        }
                        nickname_list += ")" + key + " => " + channeluser.Nick + ", ";
                    }
                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, nickname_list);

                    mainIRCClient.SendMessage(SendType.Message, e.Data.Nick, "</channel>");
                    break;
                case "gc":
                    GC.Collect();
                    break;
                // typical commands
                case "join":
                    mainIRCClient.RfcJoin(e.Data.MessageArray[1]);
                    break;
                case "part":
                    mainIRCClient.RfcPart(e.Data.MessageArray[1]);
                    break;
                case "die":
                    break;
                    //If we get a query it ends up here, but if we want to use it for API we have to send it to OnRawMessage
            }
        }

        // this method handles when we receive "ERROR" from the IRC server
        public static void OnError(object sender, ErrorEventArgs e)
        {
            System.Console.WriteLine("Error: " + e.ErrorMessage);
        }

        // this method will get all IRC messages, channel messages
        public void OnRawMessage(object sender, IrcEventArgs e)
        {
            //First we log our ingoing message
            System.Console.WriteLine("Received: " + e.Data.RawMessage);

            //Now we check if the message has a message body, if not we return
            if (e.Data.Message == null)
                return;

            //Now we compare our message to different szenarios
            //parsing our message into UPPER so it doesnt matter if ist .rape or .RaPe          
            string message = e.Data.Message.ToUpper();
            //we split our message in several parts, always at spaces
            List<string> messageParts = message.Split(' ').ToList<string>();

            //storing our Channel
            string channel = e.Data.Channel;
            //storing if we joined a new channel or not
            bool joinedChannel = false; 

            //case we get a message starting with ##channel, then we change the channel to #channel
            //substring throws an error if the message is only 1 letter, so we check that first
            if (messageParts[0].Length > 2 && messageParts[0].Substring(0, 2) == ".#")
            {
                channel = messageParts[0].Substring(1, messageParts[0].Length - 1);
                messageParts.Remove(messageParts[0]);

                if(mainIRCClient.GetChannels().Contains<string>(channel) == false)
                {
                    JoinChannel(channel);
                    joinedChannel = true;
                }
            }

            //if channel is null we can not send anywhere this happens on private messages
            //we could handle PrivMSG but i don't care
            if(channel == null)
                return;

            //case we get the message help
            if (messageParts[0] == ".HELP")
            {
                DoHelp(e);
            }

            //case we get the message re
            if (messageParts[0] == ".RE" || messageParts[0] == "RE")
            {
                DoRe(e, channel);
            }

            //case we get the message rape
            if (messageParts[0] == ".RAPE")
            {
                DoRape(e, channel);                
            }

            //case we get the message booru
            if (messageParts[0] == ".BOORU")
            {
                DoBooru(e, messageParts, channel);
            }

            if (joinedChannel)
                PartChannel(channel);
        }

        //This method only prints the commandlist
        private void DoHelp(IrcEventArgs e)
        {
            string user = e.Data.Ident;
            mainIRCClient.RfcNotice(user, ".Help, [.]re, .Rape, .booru [provider (default=danbooru)] [tags]");
        }

        //this method welcomes you back master
        private void DoRe(IrcEventArgs e, string channel)
        {
            string user = e.Data.Ident;
            mainIRCClient.SendMessage(SendType.Message, channel, "Welcome back " + user + " have a fun but safe time!");
        }

        //Sends a funny sentence with a random number of users involved
        private void DoRape(IrcEventArgs e, string channel)
        {
            mainIRCClient.SendMessage(SendType.Message, channel, Calculators.GetRape(channel, mainIRCClient));
        }

        //Retrieves an imagelink, matching the tag and provider given
        public void DoBooru(IrcEventArgs e, List<string> tags, string channel)
        {
            //we know the first word is booru
            //we remove the booru command
            tags.Remove(tags[0]);

            
            //now we see if we got a provider given
            //the provider would be the first word after booru, booru was removed
            //if we get one we remove it
            string provider = "";
            switch (tags[0])
            {
                case "SANKAKU":
                    provider = tags[0];
                    tags.Remove(tags[0]);
                    break;
                case "DANBOORU":
                    provider = tags[0];
                    tags.Remove(tags[0]);
                    break;
                default:
                    break;
            }

            //safety check that there are still tags left, e.g. #booru sankaku would be empty now
            if (tags.Count == 0)
                return;

            //now the other words all count as tags
            string fileUrl = DanbooruHandler.GetFileLink(tags, provider);
            mainIRCClient.SendMessage(SendType.Message, channel, fileUrl);
        }
    }
}
