using LocalMartOnline.Models;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Interface;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class UserInteractionService : IUserInteractionService
    {
        private readonly IRepository<UserInteraction> _repository;
        public UserInteractionService(IRepository<UserInteraction> repository)
        {
            _repository = repository;
        }
        public async Task AddInteractionAsync(UserInteraction interaction)
        {
            await _repository.CreateAsync(interaction);
        }
    }
}
