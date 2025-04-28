using GoldPrice.WebApp.Models;
using AntDesign;
using Microsoft.AspNetCore.Components;
using GoldPrice.WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GoldPrice.WebApp.Pages
{
    public class FormItemLayout
    {
        public ColLayoutParam LabelCol { get; set; }
        public ColLayoutParam WrapperCol { get; set; }
    }

    public partial class GoldPriceSetting
    {
        private GoldPriceSettingsDto _model = new GoldPriceSettingsDto();

        private readonly FormItemLayout _formItemLayout = new FormItemLayout
        {
            LabelCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24 },
                Sm = new EmbeddedProperty { Span = 7 },
            },

            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24 },
                Sm = new EmbeddedProperty { Span = 12 },
                Md = new EmbeddedProperty { Span = 10 },
            }
        };

        private readonly FormItemLayout _submitFormLayout = new FormItemLayout
        {
            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24, Offset = 0 },
                Sm = new EmbeddedProperty { Span = 10, Offset = 7 },
            }
        };
        [Inject] public MessageService Message { get; set; }
        [Inject] public AppDbContext appDbContext { get; set; }
        private async Task HandleSubmit()
        {
            await Message.Error("µÇÂ¼Ê§°Ü");
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var settings = await appDbContext.GoldPriceSettings.FirstOrDefaultAsync();
                _model.UpdateInterval = settings.UpdateInterval;
                _model.UpperThreshold = settings.UpperThreshold;
                _model.LowerThreshold = settings.LowerThreshold;
                _model.EnableNotification = settings.EnableNotification;
                _model.NotifyStartTime = settings.NotifyStartTime;
                _model.StartTime = DateTime.Parse(settings.NotifyStartTime);
                _model.NotifyEndTime = settings.NotifyEndTime;
                _model.EndTime= DateTime.Parse(settings.NotifyEndTime);
                _model.NotifyPath = settings.NotifyPath;
                _model.Sound = settings.Sound;
            }
            catch (Exception ex)
            {
                throw;
            }

            await base.OnInitializedAsync();
        }

        public class GoldPriceSettingsDto: GoldPriceSettings
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
}