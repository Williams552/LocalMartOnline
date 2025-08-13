using LocalMartOnline.Models;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Interface
{
    public interface IUserInteractionService
    {
        Task AddInteractionAsync(UserInteraction interaction);
    }
}
