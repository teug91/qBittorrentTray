using System;
using System.IO;
using System.Threading.Tasks;
using qBittorrentSharp;

namespace TorrentOpener
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("File: " + args[0]);

            if (args[0] != null)
                if (args[0].EndsWith(".torrent"))
                    AddTorrent(args[0]).Wait();
        }

        private static async Task AddTorrent(string filepath)
        {
			await Task.Delay(1000);
            /*bool? loggedIN = await API.Login

            if (loggedIN == true)
                await qBittorrentTray.API.WebUiCommunicator.AddTorrent(filepath);*/
        }
    }
}
