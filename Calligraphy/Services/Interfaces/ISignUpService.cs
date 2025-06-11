using Calligraphy.ViewModel;

namespace Calligraphy.Services.Interfaces
{
    /// <summary>
    /// 註冊介面
    /// </summary>
    public interface ISignUpService
    {
        Task<RegisterResult> SignUpAsync(RegisterViewModel model, Func<string, string, string> confirmLink);
    }
}
