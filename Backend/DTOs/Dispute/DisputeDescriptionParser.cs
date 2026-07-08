namespace Backend.DTOs.Dispute
{
    using System.Text.RegularExpressions;

    public class DisputeDescriptionInfo
    {
        public string ResolutionRequest { get; set; } = "Không rõ";
        public string MainReason { get; set; } = "Không rõ";
        public string DetailReason { get; set; } = "Không rõ";
        public string UserContent { get; set; } = "Không rõ";
    }

    public static class DisputeDescriptionParser
    {
        // Regex tách 4 phần theo nhãn tiếng Việt
        private static readonly Regex _regex = new Regex(
            @"Yêu cầu giải quyết:\s*(?<resolution>.*?)\s+Lý do chính:\s*(?<main>.*?)\s+Chi tiết lý do:\s*(?<detail>.*?)\s*---\s*Nội dung từ người dùng:\s*(?<user>.*)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled
        );

        public static DisputeDescriptionInfo Parse(string? description)
        {
            var info = new DisputeDescriptionInfo();

            if (string.IsNullOrWhiteSpace(description))
                return info;

            var match = _regex.Match(description);
            if (!match.Success)
            {
                // Nếu format khác / bị lỗi, bạn có thể:
                // - Trả nguyên description vào UserContent
                info.UserContent = description;
                return info;
            }

            info.ResolutionRequest = match.Groups["resolution"].Value.Trim();
            info.MainReason = match.Groups["main"].Value.Trim();
            info.DetailReason = match.Groups["detail"].Value.Trim();
            info.UserContent = match.Groups["user"].Value.Trim();

            return info;
        }
    }

}
