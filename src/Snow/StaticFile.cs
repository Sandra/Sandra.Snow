namespace Snow
{
    public class StaticFile
    {
        public string File { get; set; }
        public string Loop { get; set; }
        /// <summary>
        /// File title to use in header title if it's HTML.  May be overridden by loop.
        /// </summary>
        public string Title { get; set; }
    }
}