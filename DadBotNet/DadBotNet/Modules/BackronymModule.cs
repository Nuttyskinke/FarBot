using DadBotNet.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = DadBotNet.Utils.Logger;

namespace DadBotNet.Modules
{
    public class BackronymModule : ModuleBase<SocketCommandContext>
    {
        private IWordService _wordService;
        public BackronymModule(IWordService wordService)
        {
            _wordService = wordService;
        }

        [Command("back")]
        [Summary("Laver et fantastisk akronym baseret på argumenterne.")]
        [Alias("backronym")]
        public async Task MakeBackronymCommand(string arg1, string arg2)
        {
            if (arg1.Length == 0)
            {
                await ReplyAsync("Argument cannot be empty");
                return;
            }

            int backronymsToMake = 1;

            if (arg2.Length > 0)
            {
                if (!int.TryParse(arg2, out backronymsToMake))
                {
                    backronymsToMake = 1;
                }
            }

            string backronyms = "";
            for (int i = 0; i < backronymsToMake; i++)
            {
                string backronym = CreateBackronymFromWord(arg1.ToLower());
                Logger.Log($"Made backronym {backronym}", Utils.LoggerLevel.Info);
                if(backronyms.Length + backronym.Length >= 2000)
                {
                    await ReplyAsync(backronyms);
                    backronyms = backronym + "\n";
                }
                else
                {
                    backronyms += backronym + "\n";
                }
            }
            await ReplyAsync(backronyms);
        }

        [Command("back")]
        [Summary("Laver et fantastisk akronym baseret på argumenterne.")]
        [Alias("backronym")]
        public async Task MakeBackronymCommand(string arg1) => await MakeBackronymCommand(arg1, "1");

        private string CreateBackronymFromWord(string word)
        {
            word = RemoveInvalidCharacters(word);

            if (word.Length == 0)
            {
                return "";
            }

            string res = "";

            for (int i = 0; i < word.Length-1; i++)
            {
                res += CapitalizeFirstLetter(_wordService.GetRandomWordOfTypeStartingWith("adjective", word[i])) + " ";
            }

            res += CapitalizeFirstLetter(_wordService.GetRandomWordOfTypeStartingWith("noun", word[word.Length - 1]));

            return res;
        }

        private string RemoveInvalidCharacters(string input)
        {
            StringBuilder sb = new();

            for (int i = 0; i < input.Length; i++)
            {
                if(char.IsLetter(input[i]))
                {
                    sb.Append(input[i]);
                }
            }

            return sb.ToString();
        }

        private string CapitalizeFirstLetter(string word)
        {
            if(word.Length <= 1)
            {
                return word.ToUpper();
            }

            return word[0].ToString().ToUpper()+word.Substring(1, word.Length - 1);
        }
    }
}
