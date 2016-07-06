using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Should;

namespace MediaDatabase.Utils.UnitTests
{
    [TestFixture]
    public class MediaUtilsFixture
    {
           
        [Test]
        public void Test()
        {
            
            const string sourceTemplate = "[TvShow]_-_[Season]-[Episode]_-_[EpisodeName]_-_[IGNORE].[Extension]";
            const string targetTemplate = "[TvShow]_S[Season]E[Episode]_[EpisodeName].[Extension]";

            var sourceDir = @"H:\Videos\TV\Classic Series\Happy Days\Season 4";
            var filenames = Directory.GetFiles(sourceDir).Select(x => new FileInfo(x).Name).ToArray();
            var newFilenames = filenames.GetTvShowMap(sourceTemplate, targetTemplate, "00", new Dictionary<string, string> {{"_", " "}});
            newFilenames.ShouldNotBeNull();

            foreach (var newFilename in newFilenames)
            {
                File.Copy(Path.Combine(sourceDir, newFilename.Key), Path.Combine(sourceDir, newFilename.Value));
            }

        }
   
        [Test]
        public void Test2()
        {
            if (File.Exists(@"C:\Users\Chris\AppData\Roaming\Kodi\userdata\playlists\video\Sitcom Mixup (1).m3u"))
                File.Delete(@"C:\Users\Chris\AppData\Roaming\Kodi\userdata\playlists\video\Sitcom Mixup (1).m3u");

            const string path = @"C:\Users\Chris\AppData\Roaming\Kodi\userdata\playlists\video\Sitcom Mixup.m3u";
            const string newpath = @"C:\Users\Chris\AppData\Roaming\Kodi\userdata\playlists\video\Sitcom Mixup (1).m3u";

            var playlst = path.LoadPlayList();

            playlst.Filename = newpath;

            playlst.SavePlayList();


        }

    }
}
