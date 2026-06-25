using Bogus;
using Bogus.DataSets;
using ITransitionTask5.Data;
using ITransitionTask5.Data.Entities;
using Newtonsoft.Json;
using System.Globalization;
using System.Speech.Synthesis;
using static System.Net.Mime.MediaTypeNames;

namespace ITransitionTask5.Services
{
    public class SongsService
    {
        private static readonly Dictionary<string, List<string>> _genres = LoadGenres();


        private static readonly HashSet<string> _supported = ["en", "ru", "es"];




        private static Dictionary<string, List<string>> LoadGenres()
        {
            var json = File.ReadAllText("./Data/Configurations/genres.json");
            var models = JsonConvert.DeserializeObject<List<GenreModel>>(json)!;
            return models.ToDictionary(g => g.Locale, g => g.Genres);
        }

        private List<string> GetGenres(string locale) =>
            _genres.TryGetValue(locale, out var g) ? g : _genres["en"];

        public List<Song> GetSongs(long seed, int page, int pageSize, string locale, double likes)
        {
            if (page < 1) page = 1;
            var safeLocale = SafeLocale(locale);
            if (pageSize < 1) pageSize = 1;

            var genres = GetGenres(locale);
            var songs = new List<Song>();
            int startIndex = (page - 1) * pageSize + 1;

            for (int i = startIndex; i < startIndex + pageSize; i++)
            {
                var rng = new Random(CombineSeed(seed, i));

                var faker = new Faker<Song>(safeLocale)
                    .StrictMode(true)
                    .RuleFor(s => s.Index, _ => i)
                    .RuleFor(s => s.Title, f => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(f.Hacker.Noun()) + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(f.Hacker.Noun()))
                    .RuleFor(s => s.Artist, f => rng.Next(2) == 0
                        ? f.Name.FullName()
                        : f.Commerce.ProductAdjective() + " " + f.Hacker.Noun())
                    .RuleFor(s => s.Album, f =>  f.Commerce.ProductAdjective() + " " + f.Hacker.Noun())
                    .RuleFor(s => s.Genre, f => f.PickRandom(genres))
                    .RuleFor(s => s.Likes, _ => CalculateLikes(likes, new Random(CombineSeed(seed + 999, i))))
                    .RuleFor(s => s.Review, f => f.Rant.Review());

                faker.UseSeed(CombineSeed(seed, i));
                songs.Add(faker.Generate());
            }

            return songs;
        }

        public byte[] GetAudio(long seed, int index) =>
            AudioService.Generate(CombineSeed(seed, index));

        public byte[] GetCover(long seed, int index, string title, string artist, string album) =>
            ImageService.GenerateCover(CombineSeed(seed, index), title, artist, album);


        //public async Task<byte[]> GetSpeech(string text)
        //{
        //    var apiKey = _config["ElevenLabs:ApiKey"]!;
        //    var voiceId = _config["ElevenLabs:VoiceId"]!;

        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Add("xi-api-key", apiKey);

        //    var body = new
        //    {
        //        text = text,
        //        model_id = "eleven_turbo_v2_5",
        //        voice_settings = new { stability = 0.5, similarity_boost = 0.75 }
        //    };

        //    var response = await client.PostAsJsonAsync(
        //        $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}", body);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        throw new Exception($"ElevenLabs error: {error}");
        //    }

        //    return await response.Content.ReadAsByteArrayAsync();
        //}

        public byte[] GetSpeech(string text, string locale = "en")
        {
            var voicePath = locale switch
            {
                "ru" => "/app/voices/ru.onnx",
                "es" => "/app/voices/es.onnx",
                _ => "/app/voices/en.onnx"
            };

            var tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/usr/local/piper/piper",
                    Arguments = $"--model {voicePath} --output_file \"{tmpFile}\" --espeak_data /usr/local/piper/espeak-ng-data",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            process.StandardInput.WriteLine(text.Replace("\"", ""));
            process.StandardInput.Close();
            process.WaitForExit();
            var bytes = File.ReadAllBytes(tmpFile);
            File.Delete(tmpFile);
            return bytes;
        }

        public List<LyricLine> GetLyrics(long seed, int index, string locale)
        {
            var rnd = new Random(CombineSeed(seed, index));
            var faker = new Faker(SafeLocale(locale));
            var lines = new List<LyricLine>();
            double time = 0.5;

            int lineCount = rnd.Next(8, 16);
            for (int i = 0; i < lineCount; i++)
            {

                string line = rnd.Next(3) switch
                {
                    0 => $"{faker.Lorem.Word()} {faker.Lorem.Word()}, {faker.Lorem.Word()} {faker.Lorem.Word()}",
                    1 => $"{faker.Commerce.ProductAdjective()} {faker.Lorem.Word()} {faker.Lorem.Word()}",
                    _ => $"{faker.Address.City()} {faker.Lorem.Word()} {faker.Lorem.Word()}"
                };

                lines.Add(new LyricLine
                {
                    Text = char.ToUpper(line[0]) + line[1..],
                    StartSeconds = time
                });

                time += rnd.Next(3, 6);
            }

            return lines;
        }

        private int CalculateLikes(double avgLikes, Random rng)
        {
            int whole = (int)avgLikes;
            double fraction = avgLikes - whole;
            return whole + (rng.NextDouble() < fraction ? 1 : 0);
        }

        private int CombineSeed(long seed, int index)
        {
            return (int)((42 * (seed % int.MaxValue) + index) % int.MaxValue);
        }

        private string SafeLocale(string locale) =>
            _supported.Contains(locale) ? locale : "en";

    }

    public class LyricLine
    {
        public string Text { get; set; } = "";
        public double StartSeconds { get; set; }
    }
}