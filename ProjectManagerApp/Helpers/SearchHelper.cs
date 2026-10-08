using System;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class SearchHelper
    {
        public static bool MatchesText(string? value, string term)
        {
            return !string.IsNullOrEmpty(value) && value.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        public static bool MatchesId(int id, string term)
        {
            return id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        public static bool MatchesId(int? id, string term)
        {
            return id.HasValue && MatchesId(id.Value, term);
        }
    }
}
