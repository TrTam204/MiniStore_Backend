using MiniStore.Data;
using MiniStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using MiniStore.Models;

namespace MiniStore.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;
        public InvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return Array.Empty<byte>();

            var doc = new InvoiceDocument(order);
            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
    }

    public class InvoiceDocument : IDocument
    {
        private readonly Order _order;
        public InvoiceDocument(Order order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().AlignCenter().Text("MINISTORE").FontSize(28).SemiBold();
                    col.Item().AlignCenter().Text("HÓA ĐƠN BÁN HÀNG").FontSize(12).Bold();
                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(info =>
                        {
                            info.Item().Container().Background(Colors.Black).Padding(6).Text("THÔNG TIN ĐƠN HÀNG").FontSize(11).Bold().FontColor(Colors.White);
                            info.Item().Container().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(data =>
                            {
                                data.Spacing(4);
                                data.Item().Row(r =>
                                {
                                    r.ConstantItem(110).Text("Mã đơn hàng:").FontSize(11);
                                    r.RelativeItem().Text(_order.Id.ToString()).FontSize(11);
                                });
                                data.Item().Row(r =>
                                {
                                    r.ConstantItem(110).Text("Ngày đặt hàng:").FontSize(11);
                                    r.RelativeItem().Text(_order.OrderDate.ToString("dd/MM/yyyy HH:mm")).FontSize(11);
                                });
                                data.Item().Row(r =>
                                {
                                    r.ConstantItem(110).Text("Phương thức thanh toán:").FontSize(11);
                                    r.RelativeItem().Text(GetPaymentMethodLabel()).FontSize(11);
                                });
                                data.Item().Row(r =>
                                {
                                    r.ConstantItem(110).Text("Trạng thái đơn hàng:").FontSize(11);
                                    r.RelativeItem().Text(_order.Status).FontSize(11);
                                });
                            });
                        });

                        row.ConstantItem(12);

                        row.RelativeItem().Column(info =>
                        {
                            info.Item().Container().Background(Colors.Black).Padding(6).Text("THÔNG TIN KHÁCH HÀNG").FontSize(11).Bold().FontColor(Colors.White);
                            info.Item().Container().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(data =>
                            {
                                data.Spacing(4);
                                data.Item().Row(r =>
                                {
                                    r.ConstantItem(110).Text("Khách hàng:").FontSize(11);
                                    r.RelativeItem().Text(_order.User?.FullName ?? string.Empty).FontSize(11);
                                });
                                data.Item().Row(r =>
                                {
                                    r.ConstantItem(110).Text("Số điện thoại:").FontSize(11);
                                    r.RelativeItem().Text(!string.IsNullOrWhiteSpace(_order.ShippingPhone) ? _order.ShippingPhone : _order.User?.Phone ?? string.Empty).FontSize(11);
                                });
                                data.Item().Row(r =>
                                {
                                    r.ConstantItem(110).Text("Địa chỉ giao hàng:").FontSize(11);
                                    r.RelativeItem().Text(!string.IsNullOrWhiteSpace(_order.ShippingAddress) ? _order.ShippingAddress : _order.User?.Address ?? string.Empty).FontSize(11);
                                });
                            });
                        });
                    });

                    col.Item().PaddingTop(10).Text("DANH SÁCH SẢN PHẨM").FontSize(14).Bold();
                    col.Item().PaddingBottom(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(90);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Darken3).Padding(8).Text("STT").FontSize(11).FontColor(Colors.White).AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Darken3).Padding(8).Text("Sản phẩm").FontSize(11).FontColor(Colors.White).AlignLeft();
                            header.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Darken3).Padding(8).Text("Số lượng").FontSize(11).FontColor(Colors.White).AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Darken3).Padding(8).Text("Đơn giá").FontSize(11).FontColor(Colors.White).AlignRight();
                            header.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Darken3).Padding(8).Text("Thành tiền").FontSize(11).FontColor(Colors.White).AlignRight();
                        });

                        var index = 1;
                        foreach (var detail in _order.OrderDetails)
                        {
                            var name = detail.Product?.Name ?? string.Empty;
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(index++.ToString()).AlignCenter().FontSize(10);
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(name).AlignLeft().FontSize(10);
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(detail.Quantity.ToString()).AlignCenter().FontSize(10);
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text($"{detail.Price:N0} đ").AlignRight().FontSize(10);
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text($"{(detail.Price * detail.Quantity):N0} đ").AlignRight().FontSize(10);
                        }
                    });

                    var subtotal = _order.OrderDetails.Sum(d => d.Price * d.Quantity);
                    var shippingFee = _order.TotalPrice - subtotal;
                    if (shippingFee < 0) shippingFee = 0;
                    var voucherText = string.IsNullOrEmpty(_order.VoucherCode) ? "-" : _order.VoucherCode;

                    col.Item().PaddingTop(15).Container().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(summary =>
                    {
                        summary.Spacing(4);
                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Tạm tính").FontSize(11);
                            row.ConstantItem(120).AlignRight().Text($"{subtotal:N0} đ").FontSize(11);
                        });

                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Voucher").FontSize(11);
                            row.ConstantItem(120).AlignRight().Text(voucherText).FontSize(11);
                        });

                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Giảm giá").FontSize(11);
                            row.ConstantItem(120).AlignRight().Text($"{_order.DiscountAmount:N0} đ").FontSize(11);
                        });

                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Phí vận chuyển").FontSize(11);
                            row.ConstantItem(120).AlignRight().Text($"{shippingFee:N0} đ").FontSize(11);
                        });

                        summary.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Tổng thanh toán").FontSize(14).Bold();
                            row.ConstantItem(120).AlignRight().Text($"{_order.FinalAmount:N0} đ").FontSize(14).Bold();
                        });
                    });

                    col.Item().PaddingTop(14).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().AlignCenter().Text("Cảm ơn quý khách đã mua hàng tại MINISTORE!").FontSize(10).Italic();
                    col.Item().AlignCenter().Text("Hóa đơn được tạo tự động, không cần chữ ký.").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private string GetPaymentMethodLabel()
        {
            var status = _order.Status?.Trim().ToLower() ?? string.Empty;
            if (status.Contains("thanh toán"))
            {
                return "QR Pay";
            }
            return "COD";
        }
    }
}
