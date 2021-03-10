using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using YoutubeExtractor;

namespace YoutubeDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Console.WindowWidth != 120)
            {
                Console.BufferWidth = 120;
                Console.SetWindowSize(Console.BufferWidth, Console.WindowHeight);
            }
            try
            {
                Console.Write("Enter a Youtube URL: ");
                IEnumerable<VideoInfo> info = DownloadUrlResolver.GetDownloadUrls(Console.ReadLine());
                IOrderedEnumerable<VideoInfo> OrderedInfos = info.OrderBy(yes => -yes.Resolution);

                VideoInfo video = OrderedInfos.First();


                if (video.RequiresDecryption)
                    DownloadUrlResolver.DecryptDownloadUrl(video);

                string SanitizedTitle = string.Join("", video.Title.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), SanitizedTitle + video.VideoExtension);
                var Downloader = new VideoDownloader(video, savePath);

                Double PreviousProgress = 0;
                string CurrentSpin = "|";
                int SpinCount = 0;
                Downloader.DownloadStarted += (s, e) => Console.WriteLine($"Download of {video.Title} started; resolution {video.Resolution}. Saving in {savePath}");
                Downloader.DownloadProgressChanged += (s, e) =>
                {
                    Double LessRoundedPercent = Math.Round(e.ProgressPercentage, 1);
                    Double RoundedPercent = Math.Round(e.ProgressPercentage);
                    
                    if (LessRoundedPercent != PreviousProgress)
                    {
                        SpinCount++;
                        switch (SpinCount)
                        {
                            case 0: CurrentSpin = "|"; break;
                            case 1: CurrentSpin = "/"; break;
                            case 2: CurrentSpin = "-"; break;
                            case 3: CurrentSpin = "\\"; SpinCount = -1; break;
                        };


                        Console.Write("\r{0}         {1}    ", $"Download {RoundedPercent}% complete.", CurrentSpin);
                        PreviousProgress = LessRoundedPercent;
                    }
                };
                Downloader.DownloadFinished += (s, e) => Console.WriteLine($"\nDownload complete... Press any key to continue.");

                Downloader.Execute();

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred. {ex.Message}");
                Console.ReadKey();
            }

        }


    }
}
