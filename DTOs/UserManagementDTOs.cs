namespace CafePOS.API.DTOs
{
    public class DeleteUserResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsSoftDelete { get; set; }
        public bool IsHardDelete { get; set; }
    }
}
