namespace ASH_Translation.Models
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
    }

}
