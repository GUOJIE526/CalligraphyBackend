namespace Calligraphy.ViewModel
{
    /// <summary>
    /// 註冊商業邏輯用的model
    /// </summary>
    public class RegisterResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public string RedirectEmail { get; set; }

        public static RegisterResult Success(string email) =>
            new RegisterResult { IsSuccess = true, RedirectEmail = email };

        public static RegisterResult Failure(string message) =>
            new RegisterResult { IsSuccess = false, Message = message };
    }
}
