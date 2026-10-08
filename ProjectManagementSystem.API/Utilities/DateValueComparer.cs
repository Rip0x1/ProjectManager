namespace ProjectManagementSystem.API.Utilities
{
    public static class DateValueComparer
    {
        public static bool AreEquivalent(DateTime? a, DateTime? b)
        {
            if (!a.HasValue && !b.HasValue)
            {
                return true;
            }

            if (!a.HasValue || !b.HasValue)
            {
                return false;
            }

            return AreEquivalent(a.Value, b.Value);
        }

        public static bool AreEquivalent(DateTime a, DateTime b)
        {
            if (a.Ticks == b.Ticks)
            {
                return true;
            }

            return CalendarDate(a) == CalendarDate(b);
        }

        public static bool AreEquivalentValues(object? original, object? current)
        {
            if (Equals(original, current))
            {
                return true;
            }

            if (original is DateTime originalDate && current is DateTime currentDate)
            {
                return AreEquivalent(originalDate, currentDate);
            }

            if (original is string || current is string)
            {
                return string.Equals(
                    (original as string)?.Trim() ?? string.Empty,
                    (current as string)?.Trim() ?? string.Empty,
                    StringComparison.Ordinal);
            }

            return false;
        }

        public static DateTime? Normalize(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return DateTime.SpecifyKind(CalendarDate(value.Value), DateTimeKind.Unspecified);
        }

        public static DateTime? KeepIfEquivalent(DateTime? stored, DateTime? incoming)
        {
            return AreEquivalent(stored, incoming) ? stored : Normalize(incoming);
        }

        public static DateTime CalendarDate(DateTime value)
        {
            var local = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
            return local.Date;
        }
    }
}
