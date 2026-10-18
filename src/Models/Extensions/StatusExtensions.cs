namespace Models.Extensions
{
    public static class StatusExtensions
    {
        public static bool Active(this Status status) => status == Status.Active;
        public static bool Completed(this Status status) => status == Status.Completed;
        public static bool Cancelled(this Status status) => status == Status.Cancelled;
    }
}
