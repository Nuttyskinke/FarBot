using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DadBotNet.Utils;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = DadBotNet.Utils.Logger;

namespace DadBotNet.Services
{
    public class AudioService
    {
        public Task<IAudioClient> JoinAudio(IVoiceChannel target)
        {
            return target.ConnectAsync(false,false,false);
        }

        public Task LeaveAudio(IVoiceChannel currentVoice)
        {
            if(currentVoice == null)
            {
                Logger.Log("Tried to leave channel I wasn't in", LoggerLevel.Error);
                return null;
            }

            return currentVoice.DisconnectAsync();
        }

        public async Task SendAudioAsync(IAudioClient audioClient, byte[] data)
        {
            using (var stream = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                MemoryStream memStream = new MemoryStream(data);
                await memStream.CopyToAsync(stream);
                await stream.FlushAsync();
            }
        }

        public async Task SendAudioAsyncFFMPEG(IAudioClient audioClient, byte[] data)
        {
            var watch = new Stopwatch();
            
            while(watch.ElapsedMilliseconds < 2000)
            {
                watch = new Stopwatch();
                watch.Start();
                using (var ffmpeg = CreateProcess(data))
                using (var stream = audioClient.CreatePCMStream(AudioApplication.Music))
                {
                    try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                    finally { await stream.FlushAsync(); }
                }

                watch.Stop();
            }
            
        }

        private Process CreateProcess(byte[] data)
        {
            var argument = "-i pipe:0 -ar 48000 -f wav -ac 2 pipe:1";
            Process process = new Process();
            
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = argument,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            };

            process.Start();

            process.StandardInput.BaseStream.WriteAsync(data);

            process.StandardInput.Close();

            return process;
        }
    }
}
