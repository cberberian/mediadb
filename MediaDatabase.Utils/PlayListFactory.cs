using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaDatabase.Utils
{
    public class PlayListFactory
    {
        public PlayList LoadPlaylist(string path)
        {
            using (var fileData = File.OpenText(path))
            {
                if (fileData.ReadLine() != "#EXTM3U")
                    return null;

                var list = new List<PlayListFile>();

                PlayListFile playListFile;

                while (ReadPlaylistFileEntry(fileData, out playListFile))
                {
                    list.Add(playListFile);
                }

                return new PlayList
                {
                    Files = list.ToArray(),

                };
            }
        }

        private bool ReadPlaylistFileEntry(StreamReader fileData, out PlayListFile playListFile)
        {
            playListFile = null;

            if (fileData.EndOfStream) return false; 

            var descripter = fileData.ReadLine();

            Descriptor descriptorObj;

            if (!IsDescriptor(descripter, out descriptorObj))
                throw new InvalidOperationException("Invalid M3U format");

            playListFile = new PlayListFile
            {
                Descriptor = descriptorObj,
                Filename   = fileData.ReadLine()
            };

            return true;
        }

        private static bool IsDescriptor(string descripter, out Descriptor descriptorObj)
        {
            descriptorObj = null;

            if (string.IsNullOrEmpty(descripter))
                return false;

            if (!descripter.StartsWith("#EXTINF:"))
                return false;

            var match = Regex.Match(descripter, ":[0-9]*,");

            if (string.IsNullOrEmpty(match.Value))
                return false;

            
            var mediaDurationInSeconds = int.Parse(match.Value.TrimStart(':').TrimEnd(','));
            var startOfName            = descripter.IndexOf(match.Value, StringComparison.Ordinal) + 
                                         match.Value.Length;

            descriptorObj = new Descriptor
            {
                MediaDurationInSeconds = mediaDurationInSeconds,
                Name                   = descripter.Substring(startOfName)
            };

            return true;
        }

        public void SavePlaylist(PlayList playlist)
        {
            var stream = File.Open(playlist.Filename, FileMode.OpenOrCreate);

            using (var sr = new StreamWriter(stream))
            {
                sr.WriteLine("#EXTM3U");

                foreach (var file in playlist.Files)
                {
                    sr.Write($"{file}\r\n");
                }

                sr.Flush();
            }
        }
    }
}