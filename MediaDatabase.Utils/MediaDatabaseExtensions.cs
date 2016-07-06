using System.Collections.Generic;
using System.Linq;

namespace MediaDatabase.Utils
{
    public static class MediaDatabaseExtensions
    {

        public static IDictionary<string, string> GetTvShowMap(this IEnumerable<string> inputs, string sourceTemplate, string targetTemplate, string numberFormat, Dictionary<string, string> stringReplaces)
        {

            var ret = new Dictionary<string, string>();
            var index = 1;
            foreach (var fileInput in inputs)
            {
                var spec = fileInput.Tokenize<TvShowSpec>(sourceTemplate);


                spec.Season      = spec.Season.Transform(numberFormat);

                spec.Episode     = spec.Episode.Transform(numberFormat);

                spec.EpisodeName = spec.EpisodeName.Transform(replace: stringReplaces);

                spec.TvShow      = spec.TvShow.Transform(replace: stringReplaces);

                spec.Index       = index++.Transform(numberFormat);

                var deTokenizeToString = spec.DeTokenizeToString(targetTemplate);

                ret.Add(fileInput, deTokenizeToString);
            }

            return ret;
        }

        public static T[] Tokenize<T>(this IEnumerable<string> inputs, string template) where T : new()
        {
            return inputs.Select(s => new TokenTemplate<T>(template).Tokenize(s)).ToArray();
        }

        public static T Tokenize<T>(this string input, string template) where T : new()
        {
            return new TokenTemplate<T>(template).Tokenize(input);
        }

        public static string DeTokenizeToString<T>(this T input, string template) where T : new()
        {
            return new TokenTemplate<T>(template).DeTokenize(input);
        }

        public static string[] DeTokenizeToString<T>(this IEnumerable<T> input, string template) where T : new()
        {
            return input.Select(obj => new TokenTemplate<T>(template).DeTokenize(obj)).ToArray();
        }

        public static string Transform<T>(this T input, string format = null, Dictionary<string, string> replace = null)
        {
            var strInput = input?.ToString();

            if (string.IsNullOrEmpty(strInput))
            {
                return strInput;
            }

            var rep = replace ?? new Dictionary<string, string>();

            var fmt = string.IsNullOrWhiteSpace(format) ? "{0}" : "{0:"+ format + "}";

            strInput = rep.Aggregate(strInput, (current, kv) => current.Replace(kv.Key, kv.Value));

            int intInput;

            return int.TryParse(strInput, out intInput) ? string.Format(fmt, intInput) : strInput;
        }

        public static PlayList LoadPlayList(this string path)
        {
            return new PlayListFactory().LoadPlaylist(path);
        }

        public static void SavePlayList(this PlayList playlist)
        {
            new PlayListFactory().SavePlaylist(playlist);
        }

        

    }
    public class TvShowSpec
    {
        public string TvShow;
        public string Season;
        public string Episode;
        public string EpisodeName;
        public string Extension;
        public string Index;
    }
}