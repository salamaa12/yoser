namespace Yoser_API.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        // الحقل ده هو اللي كان ناقص ومسبب Error
        public bool IsAuthenticated { get; set; }

        public string Message { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
