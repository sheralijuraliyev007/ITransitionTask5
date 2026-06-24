using BlazorApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp.Services
{
    public class SongsApiService(HttpClient http)
    {
        public Task<List<SongDto>?> GetSongs(long seed, int page, int pageSize, string locale, double likes = 3.0)
            => http.GetFromJsonAsync<List<SongDto>>(
                $"api/songs?seed={seed}&page={page}&pageSize={pageSize}" +
                $"&locale={locale}&likes={likes.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}");

        public Task<List<LyricLineDto>?> GetLyrics(long seed, int index, string locale)
            => http.GetFromJsonAsync<List<LyricLineDto>>(
                $"api/songs/{index}/lyrics?seed={seed}&locale={locale}");

        public string GetAudioUrl(long seed, int index)
        {
            var baseUrl = http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
            return $"{baseUrl}/api/songs/{index}/audio?seed={seed}";
        }

        public string GetCoverUrl(long seed, int index, string title, string artist, string album, string locale)
        {
            var baseUrl = http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
            return $"{baseUrl}/api/songs/{index}/cover?seed={seed}" +
                   $"&title={Uri.EscapeDataString(title)}" +
                   $"&artist={Uri.EscapeDataString(artist)}" +
                   $"&album={Uri.EscapeDataString(album)}" +
                   $"&locale={locale}";
        }
        //public string GetExportUrl(long seed, int page, int pageSize, string locale, double likes)
        //{
        //    var baseUrl = http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        //    return $"{baseUrl}/api/songs/export?seed={seed}&page={page}&pageSize={pageSize}&locale={locale}&likes={likes.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}";
        //}

        public string GetSpeechUrl(int index, string text, string locale)
        {
            var baseUrl = http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
            return $"{baseUrl}/api/songs/{index}/speech?text={Uri.EscapeDataString(text)}&locale={locale}";
        }
    }
}