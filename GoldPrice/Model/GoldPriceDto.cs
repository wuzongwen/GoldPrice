using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldPrice.Model
{
    //如果好用，请收藏地址，帮忙分享。
    public class Datas
    {
        /// <summary>
        /// 
        /// </summary>
        public string upAndDownRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string productSku { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool demode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string priceNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string yesterdayPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string upAndDownAmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
    }

    public class ResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public Datas datas { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string status { get; set; }
    }

    public class GoldPriceDto
    {
        /// <summary>
        /// 
        /// </summary>
        public ResultData resultData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int resultCode { get; set; }
        /// <summary>
        /// 成功
        /// </summary>
        public string resultMsg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int channelEncrypt { get; set; }
    }

}
