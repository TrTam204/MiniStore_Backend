namespace MiniStore.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(int orderId);
    }
}
