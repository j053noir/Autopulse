using Ganss.Xss;

namespace AutoPulse.Application.Application.Common.Security
{
    public static class SecuritySanitizer
    {
        private static readonly HtmlSanitizer _sanitizer = new();

        static SecuritySanitizer()
        {
            // HTML tags whitelist
            _sanitizer.AllowedTags.Add("strong");
            _sanitizer.AllowedTags.Add("em");
            _sanitizer.AllowedTags.Add("a");
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("ul");
            _sanitizer.AllowedTags.Add("ol");
            _sanitizer.AllowedTags.Add("li");
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("strike");
            _sanitizer.AllowedTags.Add("s");
            _sanitizer.AllowedTags.Add("h1");
            _sanitizer.AllowedTags.Add("h2");
            _sanitizer.AllowedTags.Add("h3");
            _sanitizer.AllowedTags.Add("h4");
            _sanitizer.AllowedTags.Add("h5");
            _sanitizer.AllowedTags.Add("h6");
            _sanitizer.AllowedTags.Add("blockquote");
            _sanitizer.AllowedTags.Add("code");
            _sanitizer.AllowedTags.Add("pre");
            _sanitizer.AllowedTags.Add("table");
            _sanitizer.AllowedTags.Add("thead");
            _sanitizer.AllowedTags.Add("tbody");
            _sanitizer.AllowedTags.Add("tr");
            _sanitizer.AllowedTags.Add("th");
            _sanitizer.AllowedTags.Add("td");
            _sanitizer.AllowedTags.Add("hr");
        }

        public static string? SanitizeInput(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            return _sanitizer.Sanitize(input);
        }
    }
}
