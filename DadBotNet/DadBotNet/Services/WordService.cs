using DadBotNet.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadBotNet.Services
{
    public class WordService : IWordService
    {
        private IConfigService _config;

        // Wordtype -> Starting Letter -> List of words that start with that letter and is of that type
        private readonly Dictionary<string,Dictionary<char, List<string>>> _words;
        private readonly Random _random;

        public WordService(IConfigService config)
        {
            _config = config;
            _random = new Random();
            string path = _config.GetField("wordFile");

            if(!File.Exists(path))
            {
                Logger.Log($"Could not find file {path}", LoggerLevel.Error);
                return;
            }
            _words = new Dictionary<string, Dictionary<char, List<string>>>();

            string[] allWords = File.ReadAllLines(path);

            _words.Add("noun", new());
            _words.Add("adjective", new());

            foreach (string word in allWords)
            {
                string[] wordInfo = word.Split(',');

                if(!_words[wordInfo[1]].ContainsKey(wordInfo[0][0]))
                {
                    _words[wordInfo[1]][wordInfo[0][0]] = new List<string>();
                }

                _words[wordInfo[1]][wordInfo[0][0]].Add(wordInfo[0]);
            }
        }

        public string GetRandomWord()
        {
            throw new NotImplementedException();
        }

        public string GetRandomWordOfTypeStartingWith(string wordType, char startingLetter)
        {
            if (!_words.ContainsKey(wordType))
            {
                Logger.Log($"Could not get word of type {wordType}", LoggerLevel.Error);
                return "";
            }

            List<string> validWords = _words[wordType][startingLetter];

            string word = validWords[_random.Next(validWords.Count)];

            return word;
        }

        public string GetRandomWordWithStartingLetter(char startingLetter)
        {
            throw new NotImplementedException();
        }
    }
}
