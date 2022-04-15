using DadBotNet.Models;
using DadBotNet.Utils;
using System.Diagnostics;

namespace DadBotNet.Services
{
    public class DadJokeService : IDadJokeService
    {
        IConfigService _configService;
        private bool allowSaving;
        private IReadOnlyList<Joke> jokes;
        private int lastJokeIndex = -1;
        private Random random;
        private int lastPicIndex = -1;
        private string[] jokePicturePaths;

        public DadJokeService(IConfigService configService)
        {
            CheckBaseDirectories();

            _configService = configService;

            allowSaving = bool.Parse(configService.GetField("allowWriteToJokeFile"));

            random = new Random();

            var path = configService.GetField("jokeFile");

            if (!File.Exists(path))
            {
                Logger.Log($"Could not find file {path}", LoggerLevel.Error);
                return;
            }

            jokes = LoadJokesFromFile(path);

            foreach (var joke in jokes)
            {
                GenerateJokeVoiceBytes(joke);
            }

            jokePicturePaths = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Pictures");
        }

        private static void CheckBaseDirectories()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Jokes"))
            {
                Logger.Log("Could not find /Jokes. Creating it instead", LoggerLevel.Warning);
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Jokes");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Pictures"))
            {
                Logger.Log("Could not find /Pictures. Creating it instead", LoggerLevel.Warning);
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Pictures");
            }
        }

        private IReadOnlyList<Joke> LoadJokesFromFile(string path)
        {
            var jokesFromFile = File.ReadAllLines(path);

            List<Joke> jokeList = new List<Joke>();

            for (int i = 0; i < jokesFromFile.Length; i++)
            {
                jokeList.Add(new Joke(jokesFromFile[i], i));
            }

            return jokeList.AsReadOnly();
        }

        public bool AddJoke(Joke joke)
        {
            throw new NotImplementedException();
        }

        public Joke GetJoke()
        {
            int newJokeIndex = GetUniqueIndex(jokes.Count, lastPicIndex);

            lastJokeIndex = newJokeIndex;

            return jokes[newJokeIndex];
        }

        public string GetJokePicturePath()
        {
            if (Directory.GetFiles(Directory.GetCurrentDirectory() + "/Pictures").Length != jokePicturePaths.Length)
            {
                jokePicturePaths = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Pictures");
            }

            if (jokePicturePaths.Length == 0)
            {
                Logger.Log($"No pictures could be found in {Directory.GetCurrentDirectory()}/Pictures");
                return "";
            }

            int jokePics = jokePicturePaths.Length;

            int newPicIndex = GetUniqueIndex(jokePics, lastPicIndex);

            lastPicIndex = newPicIndex;

            return jokePicturePaths[newPicIndex];
        }

        public byte[] GetJokeByteData(Joke joke)
        {
            return joke.voiceBytes;
        }

        private async Task GenerateJokeVoiceBytes(Joke joke)
        {
            if (joke.AlreadyProcessed)
            {
                joke.voiceBytes = await File.ReadAllBytesAsync($"{Directory.GetCurrentDirectory()}/Jokes/joke{joke.jokeIndex}");
                return;
            }

            Logger.Log($"Generating voice bytes for follow jokes\n{joke}", LoggerLevel.Info);

            var cleanedDadJoke = PrepareJokeForPowershell(joke.ToString());

            Process powershellProcess = CreateTTSProcess(cleanedDadJoke, true);

            powershellProcess.Start();

            string byteResult = await powershellProcess.StandardOutput.ReadToEndAsync();

            byte[] voiceBytes = ConvertJokeResultToVoiceBytes(byteResult);

            Logger.Log($"Saving Data to {Directory.GetCurrentDirectory()}/Jokes/joke{joke.jokeIndex}");
            File.WriteAllBytes($"{Directory.GetCurrentDirectory()}/Jokes/joke{joke.jokeIndex}", voiceBytes);

            powershellProcess.Close();

            powershellProcess.Dispose();
        }

        private byte[] ConvertJokeResultToVoiceBytes(string byteResult)
        {
            string[] splitBytes = byteResult.Split('-');

            byte[] bytes = new byte[splitBytes.Length];

            for (int i = 0; i < splitBytes.Length; i++)
            {
                bytes[i] = byte.Parse(splitBytes[i]);
            }

            return bytes;
        }

        private Process CreateTTSProcess(string cleanedDadJoke, bool ffmpeg = false)
        {
            string voiceType = _configService.GetField("voice");
            var command = "Add-Type -AssemblyName System.speech;$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer;";
            command += $"$speak.SelectVoice(\\\"{voiceType}\\\");";
            command += "$OutStream = [System.IO.MemoryStream]::new(100);";
            if (!ffmpeg)
            {
                command += "$format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(96000,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono);";
                command += "$speak.SetOutputToAudioStream($OutStream,$format);";
            }
            else
            {
                command += "$speak.SetOutputToWaveStream($OutStream);";
            }
            command += $"$speak.Speak(\\\"{cleanedDadJoke}\\\");";
            command += "$speak.Dispose();";
            command += "$data = $OutStream.ToArray() -join '-';";
            command += "Write-Output $data;";

            ProcessStartInfo powerShellProcessInfo = new();

            powerShellProcessInfo.FileName = @"powershell.exe";
            powerShellProcessInfo.Arguments = command;
            powerShellProcessInfo.RedirectStandardOutput = true;
            powerShellProcessInfo.UseShellExecute = false;
            powerShellProcessInfo.CreateNoWindow = false;

            Process process = new();
            process.StartInfo = powerShellProcessInfo;

            return process;

        }

        private string PrepareJokeForPowershell(string dadJoke)
        {
            string alphabet = "abcdefghijklmnopqrstuvwxyzæøåABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ,.-_!? ";
            string newDadJoke = dadJoke;
            foreach (char c in dadJoke)
            {
                if(!alphabet.Contains(c))
                {
                    newDadJoke = newDadJoke.Replace(c.ToString(),"");
                }
            }
            return newDadJoke;
        }

        private int GetUniqueIndex(int length, int oldIndex)
        {
            int newIndex = random.Next(length);

            while (newIndex == oldIndex)
            {
                newIndex = random.Next(length);
            }

            return newIndex;

        }
    }
}
