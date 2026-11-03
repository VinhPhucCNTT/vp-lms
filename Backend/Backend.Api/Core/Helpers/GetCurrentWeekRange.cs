namespace Backend.Api.Core.Helpers
{
    public static class GetCurrentWeekRange
    {
        public static (DateTime Start, DateTime End) Get()
        {
            var today = DateTime.Today;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;

            var startOfWeek = today.AddDays(-diff);
            var endOfWeek = startOfWeek.AddDays(7);

            return (startOfWeek, endOfWeek);
        }
    }
}
