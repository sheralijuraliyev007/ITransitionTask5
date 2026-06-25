using ITransitionTask5.Services;
using Microsoft.AspNetCore.Mvc;

namespace ITransitionTask5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly SongsService _songsService;

        public SongsController(SongsService songsService)
        {
            _songsService = songsService;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] long seed, int page, int pageSize, string locale = "en", double likes = 0)
        {
            var result = _songsService.GetSongs(seed, page, pageSize, locale, likes);
            return Ok(result);
        }

        [HttpGet("{index}/audio")]
        public IActionResult GetAudio(int index, [FromQuery] long seed)
        {
            var bytes = _songsService.GetAudio(seed, index);
            return File(bytes, "audio/wav", $"song_{index}.wav");
        }

        [HttpGet("{index}/lyrics")]
        public IActionResult GetLyrics(int index, [FromQuery] long seed, string locale = "en")
        {
            var lyrics = _songsService.GetLyrics(seed, index, locale);
            return Ok(lyrics);
        }

        [HttpGet("{index}/cover")]
        public IActionResult GetCover(int index, [FromQuery] long seed,
        string title, string artist, string album = "Single")
        {
            var bytes = _songsService.GetCover(seed, index, title, artist, album);
            return File(bytes, "image/png");
        }

        //[HttpGet("export")]
        //public IActionResult Export([FromQuery] long seed, int page, int pageSize, string locale = "en", double likes = 0)
        //{
        //    var songs = _songsService.GetSongs(seed, page, pageSize, locale, likes);
        //    using var zipMs = new MemoryStream();
        //    using (var zip = new System.IO.Compression.ZipArchive(zipMs, System.IO.Compression.ZipArchiveMode.Create, true))
        //    {
        //        foreach (var song in songs)
        //        {
        //            var wavBytes = _songsService.GetAudio(seed, song.Index);

        //            using var wavStream = new MemoryStream(wavBytes);
        //            using var mp3Stream = new MemoryStream();
        //            using var reader = new NAudio.Wave.WaveFileReader(wavStream);
        //            using var writer = new NAudio.Lame.LameMP3FileWriter(mp3Stream, reader.WaveFormat, 128);
        //            reader.CopyTo(writer);
        //            writer.Flush();

        //            var fileName = $"{song.Title} - {song.Album} - {song.Artist}.mp3"
        //                .Replace("/", "-").Replace("\\", "-")
        //                .Replace(":", "-").Replace("?", "").Replace("\"", "");

        //            var entry = zip.CreateEntry(fileName);
        //            using var entryStream = entry.Open();
        //            entryStream.Write(mp3Stream.ToArray());
        //        }
        //    }

        //    zipMs.Position = 0;
        //    return File(zipMs.ToArray(), "application/zip", "songs.zip");
        //}



        [HttpGet("{index}/speech")]
        public IActionResult GetSpeech(int index, [FromQuery] string text, string locale = "en")
        {
            var bytes = _songsService.GetSpeech(text, locale);
            return File(bytes, "audio/wav");
        }
    }
}