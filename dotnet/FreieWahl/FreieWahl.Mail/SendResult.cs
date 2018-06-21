namespace FreieWahl.Mail
{
    public class SendResult
    {
        public SendResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public static SendResult SuccessResult = new SendResult(true, string.Empty);

        public bool Success { get; }
        public string Error { get; }
    }
}
