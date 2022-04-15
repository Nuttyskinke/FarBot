using DadBotNet.Services;
using System;
using System.Collections.Generic;
using DadBotNet.Utils;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = DadBotNet.Utils.Logger;

namespace DadBotNet.Modules
{
    public class DadJokeModule : ModuleBase<SocketCommandContext>
    {
        IDadJokeService _dadJokeService;
        AudioService _audioService;
        IConfigService _configService;

        bool isTalking;

        public DadJokeModule(IDadJokeService dadJokeService, AudioService audioService, IConfigService configService)
        {
            _dadJokeService = dadJokeService;
            _audioService = audioService;
            _configService = configService;
        }

        [Command("far")]
        [Summary("Fortæller en far joke")]
        [Alias("dad")]
        public async Task TellJokeInTextChannel()
        {
            await Context.Channel.SendMessageAsync(_dadJokeService.GetJoke().ToString());
        }

        [Command("farmeme")]
        [Summary("Fortæller en far joke")]
        [Alias("dadpic","farmigmig")]
        public async Task TellJokePictureInChannel()
        {
            string path = _dadJokeService.GetJokePicturePath();

            if(path == "")
            {
                return;
            }

            await Context.Channel.SendFileAsync(path);
        }



        [Command("fartts", RunMode = RunMode.Async)]
        [Summary("Fortæller en far joke i en voice kanal")]
        [Alias("dadtts")]
        public async Task TellJokeInVoiceChannel()
        {
            await TellJokeInVoiceChannel();
        }

        [Command("fartts", RunMode = RunMode.Async)]
        [Summary("Fortæller en far joke i en voice kanal")]
        [Alias("dadtts")]
        public async Task TellJokeInVoiceChannel(string arguments = "")
        {
            if(int.TryParse(arguments, out int rate))
            {
                if(rate < -10)
                {
                    rate = -10;
                }

                if(rate > 10)
                {
                    rate = 10;
                }
                await TellJokeInVoice(rate);
            }
            else
            {
                await TellJokeInVoice();
            }
            
        }

        private async Task TellJokeInVoice(int talkRate = 0)
        {
            SocketGuildUser user = ((SocketGuildUser)Context.User);
            if (user.VoiceChannel == null)
            {
                await Context.Message.ReplyAsync("Jeg ved ikke hvordan man sender lyd gennem tekst. Beklager!");

                return;
            }

            Logger.Log("Getting Joke");

            var dadJoke = _dadJokeService.GetJoke();

            byte[] voiceBytes = _dadJokeService.GetJokeByteData(dadJoke);

            Logger.Log("Joining Channel");
            var audioConnection = await _audioService.JoinAudio(user.VoiceChannel);

            Logger.Log("Speaking in Channel");
            await Context.Channel.SendMessageAsync(dadJoke.ToString());
            await _audioService.SendAudioAsyncFFMPEG(audioConnection, voiceBytes);

            Logger.Log("Leaving Channel");
            await _audioService.LeaveAudio(user.VoiceChannel);
            await audioConnection.StopAsync();
            audioConnection.Dispose();
        }
    }
}
