using GoldPrice.WebApp.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace GoldPrice.WebApp.Pages.Account.Center
{
    public partial class Articles
    {
        [Parameter] public IList<ListItemDataType> List { get; set; }
    }
}