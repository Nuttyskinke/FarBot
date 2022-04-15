using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadBotNet.Models
{
    public class Joke
    {
        readonly string _jokeText;
        public readonly int jokeIndex;
        public byte[] voiceBytes;
        public bool AlreadyProcessed => File.Exists($"{Directory.GetCurrentDirectory()}/Jokes/joke{jokeIndex}");

        public Joke(string jokeText, int jokeIndex)
        {
            _jokeText = jokeText;
            this.jokeIndex = jokeIndex;

            if(AlreadyProcessed)
            {
                voiceBytes = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}/Jokes/joke{jokeIndex}");
            }
        }


        public override string ToString()
        {
            return _jokeText;
        }
    }
}
