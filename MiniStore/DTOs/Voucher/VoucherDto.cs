using System;
using MiniStore.Models;
using System.Collections.Generic;

namespace MiniStore.DTOs.Voucher
{
    public class VoucherDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Quantity { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        // Applicability fields
        public ApplicableType ApplicableType { get; set; } = ApplicableType.All;
        public int? ApplicableCategoryId { get; set; }
        public List<int>? ApplicableProductIds { get; set; }
    }
}
