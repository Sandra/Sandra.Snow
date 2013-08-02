namespace Sandra.Snow.PreCompiler.Extensions
{
    using Nancy.ViewEngines.Razor;

    public static class RazorHelpers
    {
        private const string ImageFormat = @"<img src=""{0}"" />";

        public static IHtmlString RenderGravatarImage<T>(this HtmlHelpers<T> html, string email, int size = 0)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return html.Raw("");
            }

            var url = email.EmailToGravatar(size);
            var result = string.Format(ImageFormat, url);

            return html.Raw(result);
        }
    }
}