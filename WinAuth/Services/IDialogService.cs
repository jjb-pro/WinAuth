using System.Threading.Tasks;

namespace WinAuth.Services;

public interface IDialogService
{
    Task ShowErrorDialogAsync(string title, string content);
}