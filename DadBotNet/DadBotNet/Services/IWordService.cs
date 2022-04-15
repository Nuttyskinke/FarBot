using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadBotNet.Services
{
    public interface IWordService
    {
        string GetRandomWordOfTypeStartingWith(string wordType, char startingLetter);

        string GetRandomWord();
    }
}
