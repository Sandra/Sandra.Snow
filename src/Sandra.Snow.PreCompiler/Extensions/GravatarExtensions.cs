namespace Sandra.Snow.PreCompiler.Extensions
{
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public static class GravatarExtensions
    {
        private const string UrlFormat = "http://www.gravatar.com/avatar/{0}.jpg?{1}";

        public static string EmailToGravatar(this string email, int size = 0)
        {
            var alg = HashAlgorithm.Create("MD5");

            if (alg == null)
            {
                return string.Empty;
            }

            var buff = Encoding.Default.GetBytes(email.Trim());
            var hash = alg.ComputeHash(buff);
            var result = hash.Aggregate(string.Empty, (current, h) => current + h.ToString("x2"));

            var sizeFormatted = GetSize(size);

            return string.Format(UrlFormat, result, sizeFormatted);
        }

        private static string GetSize(int size)
        {
            if (size == 0)
            {
                return string.Empty;
            }

            return "s=" + size;
        }
    }
}