﻿using System;
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

        public static int Count => ActiveEntries.Count();
        public static int UsedCount => ActiveEntries.Count(e => e.Used);

        public static async Task LoadDictionary()
        {
            var response = await HttpClient.GetAsync("https://dkinvoker.github.io/LaFrancais/dictionary.json");
            Chapters = JsonSerializer.Deserialize<Chapter[]>(await response.Content.ReadAsStringAsync())!;
        }

        public static Module[] AllModules => Chapters!.SelectMany(e => e.Modules).ToArray();

        public static Chapter[]? Chapters = null;

        private static Module[] _activeModules;
        public static Module[] ActiveModules 
        {
            get
            {
                return _activeModules;
            }
            set
            {
                _activeModules = value;
                foreach (var chapter in Chapters!)
                {
                    foreach (var module in chapter.Modules)
                    {
                        foreach (var entry in module.Entries)
                        {
                            entry.Used = false;
                        }
                    }
                }

                ActiveEntries = value.SelectMany(m => m.Entries);
            }
        }

        private static IEnumerable<QuizEntry> ActiveEntries = Array.Empty<QuizEntry>();

        public static QuizEntry? GetNextEntry()
        {
            var unused = ActiveEntries.Where(e => e.Used == false);
            var randomEntry = unused.OrderBy(e => Random.Shared.Next()).FirstOrDefault();

            if (randomEntry is null)
            {
                foreach (var item in ActiveEntries)
                {
                    item.Used = false;
                }
                randomEntry = unused.OrderBy(e => Random.Shared.Next()).FirstOrDefault();
            }

            if (randomEntry is not null)
            {
                randomEntry!.Used = true;
            }
            
            return randomEntry;
        }
    }
}
