using GoldPrice.WebApp.Models;
using GoldPrice.WebApp.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace GoldPrice.WebApp.Pages.Account.Settings
{
    public partial class BaseView
    {
        private CurrentUser _currentUser = new CurrentUser();

        [Inject] protected IUserService UserService { get; set; }

        private void HandleFinish()
        {
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _currentUser = await UserService.GetCurrentUserAsync();
        }
    }
}