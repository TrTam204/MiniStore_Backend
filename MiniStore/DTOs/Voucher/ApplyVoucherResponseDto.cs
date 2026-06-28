namespace MiniStore.DTOs.Voucher
{
    public class ApplyVoucherResponseDto
    {
        public bool IsValid { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
