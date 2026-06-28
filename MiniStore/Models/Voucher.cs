using System;

namespace MiniStore.Models
{
    public enum DiscountType
    {
        Percentage = 0,
        FixedAmount = 1
    }
    public enum ApplicableType
    {
        All = 0,
        ByCategory = 1,
        ByProducts = 2
    }

    public class Voucher
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
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Applicability fields
        public ApplicableType ApplicableType { get; set; } = ApplicableType.All;
        public int? ApplicableCategoryId { get; set; }
        // JSON array of product ids when ApplicableType == ByProducts
        public string? ApplicableProductIds { get; set; }
    }
}
