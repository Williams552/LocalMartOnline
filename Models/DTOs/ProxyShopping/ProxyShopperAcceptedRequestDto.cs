using System;
using System.Collections.Generic;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Models.DTOs.ProxyShopping
{
    public class ProxyShopperAcceptedRequestDto
    {
        // Request Information
        public string? RequestId { get; set; }
        public List<ProxyItem> RequestItems { get; set; } = new();
        public string? RequestStatus { get; set; }
        public DateTime RequestCreatedAt { get; set; }
        public DateTime RequestUpdatedAt { get; set; }
        
        // Buyer Information
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        
        // Order Information (if exists)
        public string? OrderId { get; set; }
        public string? OrderStatus { get; set; }
        public List<ProductDto> OrderItems { get; set; } = new();
        public decimal? TotalAmount { get; set; }
        public decimal? ProxyFee { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Notes { get; set; }
        public string? ProofImages { get; set; }
        public DateTime? OrderCreatedAt { get; set; }
        public DateTime? OrderUpdatedAt { get; set; }
        
        // Combined Status for UI
        public string? CurrentPhase { get; set; }
        public bool CanEditProposal { get; set; }
        public bool CanStartShopping { get; set; }
        public bool CanUploadProof { get; set; }
        public bool CanCancel { get; set; }
    }
}
