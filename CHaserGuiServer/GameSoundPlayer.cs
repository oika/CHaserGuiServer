using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;

namespace Oika.Apps.CHaserGuiServer
{
    class GameSoundPlayer : IDisposable
    {
        static readonly string WavDir = App.Current.StartUpPath + @"\sounds";

        const string ClientDirPrefix = "client_";

        readonly string clientDir;

        static readonly Random random = new Random();
        SoundPlayer player = new SoundPlayer();

        public static IEnumerable<string> EnumerateClientNames()
        {
            if (!Directory.Exists(WavDir)) return Enumerable.Empty<string>();

            return Directory.EnumerateDirectories(WavDir, ClientDirPrefix + "*", SearchOption.TopDirectoryOnly)
                            .Select(d => d.Remove(0, (WavDir + @"\" + ClientDirPrefix).Length));
        }

        public GameSoundPlayer(string clientName)
        {
            this.clientDir = WavDir + @"\" + ClientDirPrefix + clientName;
        }

        public void PlayGameStart()
        {
            var wavPath = WavDir + @"\start.wav";
            tryPlaySync(wavPath);
        }
        
        public void PlayGameSet()
        {
            var wavPath = WavDir + @"\end.wav";
            tryPlaySync(wavPath);
        }

        public void Play(MethodKind method)
        {
            if (!Directory.Exists(clientDir)) return;

            var files = Directory.EnumerateFiles(clientDir, method.ToWavPrefix() + "*").ToArray();
            if (files.Length == 0) return;

            try
            {
                if (files.Length == 1)
                {
                    tryPlayAsync(files[0]);
                    return;
                }

                var idx = random.Next(0, files.Length);
                tryPlayAsync(files[idx]);
                return;
            }
            finally
            {
                Thread.Sleep(160);
            }
        }

        public void PlayWin()
        {
            var wavPath = clientDir + @"\win.wav";
            tryPlaySync(wavPath);
        }
        public void PlayLose()
        {
            var wavPath = clientDir + @"\lose.wav";
            tryPlaySync(wavPath);
        }


        private bool tryPlayAsync(string wavPath)
        {
            if (!File.Exists(wavPath)) return false;
            player.SoundLocation = wavPath;
            player.Play();
            return true;
        }
        private bool tryPlaySync(string wavPath)
        {
            if (!File.Exists(wavPath)) return false;
            player.SoundLocation = wavPath;
            player.PlaySync();
            return true;
        }


        public void Dispose()
        {
            if (player != null)
            {
                player.Dispose();
                player = null;
            }
        }
    }
}
