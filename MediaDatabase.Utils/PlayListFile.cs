using System.IO;

namespace MediaDatabase.Utils
{
    public class PlayListFile
    {
        public PlayListFile()
        {
        }

        public PlayListFile(string path)
        {
            Filename = path;
            var fi = new FileInfo(path);
            Descriptor = new Descriptor(fi.Name);
        }

        public Descriptor Descriptor { get; set; }
        public string Filename { get; set; }

        public override string ToString()
        {
            return $"{Descriptor}\r\n{Filename}";
        }
    }
}