using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System;

Console.WriteLine("Please enter Video Url");
var url = Console.ReadLine();
if (url != null) 
    await DownloadVideo(url);


async Task DownloadVideo(string videoUrl)
{
     string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");

    if (!Directory.Exists(outputPath))
    {
        Directory.CreateDirectory(outputPath);
    }

    var youtube = new YoutubeClient();
    var video = await youtube.Videos.GetAsync(videoUrl);

    if (video != null)
    {
        Console.WriteLine($"Downloading video: {video.Title} ");

        var streamInfoSet = await youtube.Videos.Streams.GetManifestAsync(video.Id);
        var streamInfo = streamInfoSet.GetMuxedStreams().GetWithHighestVideoQuality();

        if (streamInfo != null)
        {
            var videoStream = await youtube.Videos.Streams.GetAsync(streamInfo);
            string videoFileName = $"{video.Title}.{streamInfo.Container}";
            string videoFilePath = Path.Combine(outputPath, videoFileName);
            await using var fileStream = File.OpenWrite(videoFilePath);
            const int bufferSize = 81920; // 80 KB buffer
            var buffer = new byte[bufferSize];
            var bytesRead = 0;
            var totalBytesRead = 0L;
            var progress = new Progress<double>(p =>
            {
                Console.WriteLine($"Download progress: {p:P}");
            });

            while ((bytesRead = await videoStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;
                var downloadPercentage = ((double)totalBytesRead / (double)videoStream.Length) * (double)100;
                ((IProgress<double>)progress).Report(downloadPercentage);
            }

            Console.WriteLine($"Download complete! Saved to: {videoFilePath}");
        }
        else
        {
            Console.WriteLine("No suitable video stream found.");
        }
    }
    else
    {
        Console.WriteLine("Video not found.");
    }
}
