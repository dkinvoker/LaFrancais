using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LaFrancais.Code
{
    internal static class QuizManager
    {

        private static readonly HttpClient HttpClient = new HttpClient();

        public static int Count => QuizEntries?.Length ?? 0;
        public static int UsedCount => QuizEntries?.Count(e => e.Used) ?? 0;

        private static async Task<QuizEntry[]> GetQuizEntries()
        {
            var response = await HttpClient.GetAsync("https://dkinvoker.github.io/LaFrancais/dictionary.json");
            return JsonSerializer.Deserialize<QuizEntry[]>(await response.Content.ReadAsStringAsync())!;
        }

        private static QuizEntry[]? QuizEntries = null;

        public static async Task<QuizEntry?> GetNextEntry()
        {
            if (QuizEntries == null)
            {
                QuizEntries = await GetQuizEntries();
            }

            var unused = QuizEntries.Where(e => e.Used == false);
            var randomEntry = unused.OrderBy(e => Random.Shared.Next()).FirstOrDefault();

            if (randomEntry is null)
            {
                foreach (var item in QuizEntries)
                {
                    item.Used = false;
                }
                randomEntry = unused.OrderBy(e => Random.Shared.Next()).FirstOrDefault();
            }

            randomEntry!.Used = true;
            return randomEntry;
        }
    }
}
