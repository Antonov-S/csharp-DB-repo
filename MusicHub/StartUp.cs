namespace MusicHub
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context = 
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            //var result = ExportAlbumsInfo(context, 9);
            //Console.WriteLine(result);
            //File.WriteAllText("../../../result.txt", result);

            var resultDuration = ExportSongsAboveDuration(context, 4);
            Console.WriteLine(resultDuration);
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albumsInfo = context.Producers
                .FirstOrDefault(x => x.Id == producerId)
                .Albums
                .Select(album => new
                {
                    AlbumName = album.Name,
                    ReleaseDate = album.ReleaseDate,
                    ProducerName = album.Producer.Name,
                    Songs = album.Songs.Select(song => new
                    {
                        SongName = song.Name,
                        SongPrice = song.Price,
                        SongWriter = song.Writer.Name
                    })
                    .OrderByDescending(x => x.SongName)
                    .ThenBy(x => x.SongWriter)
                    .ToList(),
                    AlbumPrice = album.Price
                })
                .OrderByDescending(x => x.AlbumPrice)
                .ToList();

            StringBuilder sb = new StringBuilder();
            

            foreach (var album in albumsInfo)
            {
                sb.AppendLine($"-AlbumName: {album.AlbumName}")
                    .AppendLine($"-ReleaseDate: {album.ReleaseDate:MM/dd/yyyy}")
                    .AppendLine($"-ProducerName: {album.ProducerName}")
                    .AppendLine("-Songs:");

                int coounter = 1;
                foreach (var song in album.Songs)
                {
                    sb.AppendLine($"---#{coounter++}")
                        .AppendLine($"---SongName: {song.SongName}")
                        .AppendLine($"---Price: {song.SongPrice:f2}")
                        .AppendLine($"---Writer: {song.SongWriter}");
                }
                sb.AppendLine($"-AlbumPrice: {album.AlbumPrice:F2}");
            }

            return sb.ToString().TrimEnd();            
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songInfo = context.Songs
                .Include(x => x.Album)
                .ThenInclude(x => x.Producer)
                .Include(x => x.Writer)
                .ToList()
                .Where(x => x.Duration.TotalSeconds > duration)
                .Select(x => new
                {
                    SongName = x.Name,
                    Writer = x.Writer.Name,
                    Performer = x.SongPerformers.Select(x => x.Performer.FirstName + " " + x.Performer.LastName).FirstOrDefault(),
                    AlbumProducer = x.Album.Producer.Name,
                    Duration = x.Duration
                })
                .OrderBy(x => x.SongName)
                .ThenBy(x => x.Writer)
                .ThenBy(x => x.Performer);

            int counter = 1;
            StringBuilder sb = new StringBuilder();

            foreach (var song in songInfo)
            {
                sb.AppendLine($"-Song #{counter++}")
                    .AppendLine($"---SongName: {song.SongName}")
                    .AppendLine($"---Writer: {song.Writer}")
                    .AppendLine($"---Performer: {song.Performer}")
                    .AppendLine($"---AlbumProducer: {song.AlbumProducer}")
                    .AppendLine($"---Duration: {song.Duration:c}");
            }
            
            return sb.ToString().TrimEnd();
        }
    }
}
