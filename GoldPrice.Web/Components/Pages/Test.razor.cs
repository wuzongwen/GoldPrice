using AntDesign;
using GoldPrice.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace GoldPrice.Web.Components.Pages
{
    public partial class Test
    {
        [Inject] public MessageService Message { get; set; }
        [Inject] public AppDbContext appDbContext { get; set; }
        private GoldPriceSettings _model = new GoldPriceSettings();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _model = await appDbContext.GoldPriceSettings.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            await base.OnInitializedAsync();
        }

        private async Task HandleSubmit()
        {
            await Message.Error("登录失败");
        }
    }
}
