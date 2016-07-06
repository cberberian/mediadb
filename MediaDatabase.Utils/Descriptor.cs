namespace MediaDatabase.Utils
{
    public class Descriptor
    {
        public Descriptor()
        {
        }

        public Descriptor(string path)
        {
            MediaDurationInSeconds = 0;
            Name = path;
        }

        public int MediaDurationInSeconds { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"#EXTINF:{MediaDurationInSeconds},{Name}";
        }
    }
}