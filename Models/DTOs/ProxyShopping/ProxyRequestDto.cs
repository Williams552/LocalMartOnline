using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class ProxyRequestDto
    {   
        public string BuyerId { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public List<ProxyItem> Items { get; set; } = new();
    }
}