using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace SoundcloudDownloader_Console
{
    class Program
    {
        static String clientID = "b45b1aa10f1ac2941910a7f0d10f8e28";

        public struct TrackID
        {
            public int index;
            public String id;
            public String title;

            public TrackID(int i, String j, String k)
            {
                index = i;
                id = j;
                title = k;
            }
        }

        static void Main(string[] args)
        {
            bool menu = true;
            while (menu)
            {
                Console.Write("URL > ");
                String url = Console.ReadLine();
                if (url.Contains("/sets/"))
                {
                    Console.WriteLine("Downloading playlist.");
                    downloadSet(url);
                }
                else
                {
                    downloadSong(url);
                }
            }
        }

        /// <summary>
        /// Downloads and returns the trackID of given song
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static TrackID resolveTrackID(String url, int startindex)
        {
            WebClient w = new WebClient();
            String str = w.DownloadString("http://api.soundcloud.com/resolve.json?url=" + url + "&client_id=" + clientID);
            //int index = str.IndexOf("\"id\":", startindex) + 5;
            int index = str.IndexOf("\"track\",\"id\":", startindex) + 13;
            int titleindex = str.IndexOf("\"title\":", index) + 9;
            return new TrackID(index, str.Substring(index, str.IndexOf(",", index) - index), str.Substring(titleindex, str.IndexOf("\",", titleindex) - titleindex));
        }

        /// <summary>
        /// Downloads and returns all trackIDs of all songs in a given playlist
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<TrackID> resolveTrackIDs(String url)
        {
            List<TrackID> ret = new List<TrackID>();
            // https://api.soundcloud.com/resolve.json?url=https://soundcloud.com/instance01/sets/extremly-chilled&client_id=b45b1aa10f1ac2941910a7f0d10f8e28
            // https://api.soundcloud.com/playlists/42506087.json?client_id=b45b1aa10f1ac2941910a7f0d10f8e28
            int currentindex = 0;
            for (int i = 0; i < getTrackCount(url); i++)
            {
                TrackID t = resolveTrackID(url, currentindex);
                currentindex = t.index + 1;
                ret.Add(t);
            }
            return ret;
        }

        public static int getTrackCount(String url)
        {
            WebClient w = new WebClient();
            String str = w.DownloadString("http://api.soundcloud.com/resolve.json?url=" + url + "&client_id=" + clientID);
            int index = str.IndexOf("\"track_count\":") + 14;
            return Convert.ToInt32(str.Substring(index, str.IndexOf(",", index) - index)); 
        }

        /// <summary>
        /// Downloads and returns the download link for given song
        /// </summary>
        /// <param name="trackID"></param>
        /// <returns></returns>
        public static String resolveDownloadURL(String trackID)
        {
            WebClient w = new WebClient();
            String str = w.DownloadString("https://api.soundcloud.com/tracks/" + trackID + "/streams?client_id=" + clientID);
            int index = str.IndexOf(":\"http") + 2;
            return str.Substring(index, str.IndexOf("\"", index) - index);
        }

        /// <summary>
        /// Downloads a single song
        /// </summary>
        /// <param name="url"></param>
        public static void downloadSong(String url)
        {
            TrackID trackID = resolveTrackID(url, 0);
            String downloadurl = resolveDownloadURL(trackID.id).Replace("\\u0026", "&");
            WebClient w = new WebClient();
            String filename = trackID.title + ".mp3";
            Console.WriteLine("Downloading " + filename + ".");
            w.DownloadFile(downloadurl, filename);
            Console.WriteLine("Finished downloading " + filename + ".");
        }

        /// <summary>
        /// Downloads a single Set
        /// </summary>
        /// <param name="url"></param>
        public static void downloadSet(String url)
        {
            List<TrackID> trackIDs = new List<TrackID>(resolveTrackIDs(url));
            foreach (TrackID trackID in trackIDs)
            {
                String downloadurl = resolveDownloadURL(trackID.id).Replace("\\u0026", "&");
                WebClient w = new WebClient();
                String filename = trackID.title + ".mp3";
                Console.WriteLine("Downloading " + filename + ".");
                w.DownloadFile(downloadurl, filename);
                Console.WriteLine("Finished downloading " + filename + ".");
            }
            Console.WriteLine("Finished downloading the playlist.");
        }
    }
}
