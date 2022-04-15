using DadBotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadBotNet.Services
{
    public interface IDadJokeService
    {
        Joke GetJoke();

        bool AddJoke(Joke joke);

        byte[] GetJokeByteData(Joke joke);

        string GetJokePicturePath();
    }
}
