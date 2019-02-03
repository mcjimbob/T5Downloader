using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace TennentsT5Downloader
{
    class Program
    {

        static string venueName = "SoccerWorld Dundee";
        static int pitchId = 23;
        static string apiPath = "http://api.t5s.me";

        static void Main(string[] args)
        {
            var client = new RestClient(apiPath);

            try
            {
                var startTime = BuildGameTime(DayOfWeek.Monday, 20, 30);
                var endTime = BuildGameTime(DayOfWeek.Monday, 21, 30);

                Console.WriteLine("*** Tennents T5 Video Downloader ***");
                Console.WriteLine("");
                Console.WriteLine("Searching for videos");
                Console.WriteLine("");

                Console.WriteLine("Venue: {0}", venueName);
                Console.WriteLine("Pitch ID: {0}", pitchId);
                Console.WriteLine("Start Date: {0}", startTime);
                Console.WriteLine("End Date: {0}", endTime);

                var request = new RestRequest("videos/search.json", Method.POST);

                request.AddQueryParameter("page", "1");
                request.AddQueryParameter("per_page", "12");

                request.AddParameter("filter_by_venue", venueName);
                request.AddParameter("filter_by_pitch", pitchId);
                request.AddParameter("filter_by_time_start", startTime);
                request.AddParameter("filter_by_time_end", endTime);

                IRestResponse<List<VideoObject>> response = client.Execute<List<VideoObject>>(request);

                foreach(VideoObject video in response.Data)
                {
                    Console.WriteLine("Found new video: {0} [{1}]", video.title, video.mp4_vid_url);

                    var fileSavePath = GetDownloadSavePath(startTime, video.mp4_vid_url);
                    Console.WriteLine("Downloading (Saving to {0})...", fileSavePath);

                    using (var downloadClient = new WebClient())
                    {
                        downloadClient.DownloadFile(video.mp4_vid_url, fileSavePath);
                    }

                    Console.WriteLine("Done");
                    Console.WriteLine("");
                }
            }
            finally
            {
                Console.WriteLine("[Complete]");
                Console.ReadLine();
            }
        }

        public static string GetDownloadSavePath(DateTime gameTime, Uri downloadFile)
        {
            var folderName = CreateAndGetFolder(gameTime);
            return String.Format("{0}/{1}", folderName, downloadFile.Segments.Last());
        }

        public static string CreateAndGetFolder(DateTime gameTime)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var folderName = string.Format("{0}/{1}{2}{3}", path, gameTime.Day, gameTime.Month, gameTime.Year);
            CreateFolder(folderName);
            return folderName;

        }

        private static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static DateTime BuildGameTime(DayOfWeek day, int hours, int minutes)
        {
            var dayDiff = DateTime.Now.DayOfWeek - day;
            var gameDate = DateTime.Now.AddDays(-dayDiff);

            return new DateTime(
                gameDate.Year,
                gameDate.Month,
                gameDate.Day,
                hours,
                minutes,
                0,
                0,
                gameDate.Kind);
        }

    }

    public class VideoObject
    {
        public int id { get; set; }
        public string title { get; set; }
        public object url { get; set; }
        public string thumbnail { get; set; }
        public bool featured { get; set; }
        public int pitch_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool listed { get; set; }
        public object yt_video_id { get; set; }
        public object ogg_vid_url { get; set; }
        public string webm_vid_url { get; set; }
        public Uri mp4_vid_url { get; set; }
        public string camera { get; set; }
    }
}
