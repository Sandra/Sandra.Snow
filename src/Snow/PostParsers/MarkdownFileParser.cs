namespace Snow.PostParsers
{
    using System;

    public class MarkdownFileParser
    {
        public static ParsedFile ParseDataFromFile(string rawPost)
        {
            var parsedFile = new ParsedFile();

            //Get the Header info from a Post Markdown File
            //Find the first index of ---
            var startOfSettingsIndex = rawPost.IndexOf("---", StringComparison.InvariantCultureIgnoreCase);
            if (startOfSettingsIndex >= 0)
            {
                //Find the second index of --- after the first
                var endOfSettingsIndex = rawPost.IndexOf("---", startOfSettingsIndex + 3,
                    StringComparison.InvariantCultureIgnoreCase);

                //If we find the 2nd index, parse the settings
                //Otherwise we assume there's no header or settings...
                if (endOfSettingsIndex >= 0)
                {
                    parsedFile.Header = rawPost.Substring(startOfSettingsIndex, endOfSettingsIndex + 3);
                    parsedFile.Body = rawPost.Substring(endOfSettingsIndex + 3, rawPost.Length - (endOfSettingsIndex + 3));
                }
            }
            else
            {
                parsedFile.Body = rawPost;
            }
            
            //Everything that goes here will be defined as the excerpt of your blog post...
            //---end

            var startOfExcerpt = rawPost.IndexOf("---excerpt", StringComparison.InvariantCultureIgnoreCase);
            var endOfExcerpt = rawPost.IndexOf("---end", startOfExcerpt + 10, StringComparison.InvariantCultureIgnoreCase);
            if (startOfExcerpt >= 0 && endOfExcerpt > startOfExcerpt)
            {
                parsedFile.Excerpt = rawPost.Substring(startOfExcerpt + 10, endOfExcerpt - (startOfExcerpt + 10));
                parsedFile.Body = rawPost.Substring(endOfExcerpt + 6, rawPost.Length - (endOfExcerpt + 6));
            }

            return parsedFile;
        }

        public class ParsedFile
        {
            public string Header { get; set; }
            public string Excerpt { get; set; }
            public string Body { get; set; }
        }
    }
}