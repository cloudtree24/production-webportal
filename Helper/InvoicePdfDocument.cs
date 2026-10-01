using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using HCMPortal.ViewModel; // Tumhare ViewModel ka namespace

namespace HCMPortal.Helper
{
    public class InvoicePdfDocument
    {
        //private readonly InvoicePrintDetailsViewModel _invoice;

        //public InvoicePdfDocument(InvoicePrintDetailsViewModel invoice)
        //{
        //    _invoice = invoice;
        //}

        //public byte[] GeneratePdf()
        //{
        //    var document = Document.Create(container =>
        //    {
        //        container.Page(page =>
        //        {
        //            page.Size(PageSizes.A4);
        //            page.Margin(20);
        //            page.PageColor(Colors.White);
        //            page.DefaultTextStyle(x => x.FontSize(12));

        //            page.Header().Element(Header);
        //            page.Content().Element(Content);
        //            page.Footer().Element(Footer);
        //        });
        //    });

        //    using (var ms = new MemoryStream())
        //    {
        //        document.GeneratePdf(ms);
        //        return ms.ToArray();
        //    }
        //}

        //private void Header(IContainer container)
        //{
        //    container.Row(row =>
        //    {
        //        row.RelativeColumn(2).Stack(stack =>
        //        {
        //            stack.Item().Text(_invoice.customerDetails?.CompanyName ?? "Your Company").Bold().FontSize(16);
        //            stack.Item().Text(_invoice.customerDetails.Address ?? "");
        //        });

        //        row.ConstantColumn(100).Stack(stack =>
        //        {
        //            if (!string.IsNullOrEmpty(_invoice.Base64LogoString))
        //            {
        //                byte[] logoBytes = Convert.FromBase64String(_invoice.Base64LogoString);
        //                stack.Item().Image(logoBytes, ImageScaling.FitArea);
        //            }
        //        });
        //    });
        //}

        //private void Content(IContainer container)
        //{
        //    container.Stack(stack =>
        //    {
        //        stack.Item().Text($"Invoice ID: {_invoice.InvoiceId}");
        //        stack.Item().Text($"Customer: {_invoice.customerDetails?.ContactPersonName}");
        //        stack.Item().Text($"Email: {_invoice.customerDetails?.EmailAddress}");
        //        stack.Item().Text($"Phone: {_invoice.customerDetails?.Phonenumber}");
        //        stack.Item().Text($"Amount: ₹{_invoice.customerDetails?.amount}");
        //        stack.Item().Text($"Subscription Plan: {_invoice.customerDetails?.}");
        //        stack.Item().Text($"Valid From: {_invoice.customerDetails?.?.ToString("dd-MM-yyyy")}");
        //        stack.Item().Text($"Valid To: {_invoice.customerDetails?.?.ToString("dd-MM-yyyy")}");

        //        if (_invoice.InvoiceItems != null && _invoice.InvoiceItems.Count > 0)
        //        {
        //            stack.Item().Table(table =>
        //            {
        //                table.ColumnsDefinition(columns =>
        //                {
        //                    columns.RelativeColumn(4);
        //                    columns.RelativeColumn(1);
        //                    columns.RelativeColumn(2);
        //                    columns.RelativeColumn(2);
        //                });

        //                table.Header(header =>
        //                {
        //                    header.Cell().Text("Item").Bold();
        //                    header.Cell().AlignCenter().Text("Qty").Bold();
        //                    header.Cell().AlignRight().Text("Price").Bold();
        //                    header.Cell().AlignRight().Text("Total").Bold();
        //                });

        //                foreach (var item in _invoice.InvoiceItems)
        //                {
        //                    table.Cell().Text(item.ItemName);
        //                    table.Cell().AlignCenter().Text(item.Quantity.ToString());
        //                    table.Cell().AlignRight().Text($"₹{item.Price}");
        //                    table.Cell().AlignRight().Text($"₹{item.Total}");
        //                }
        //            });
        //        }
        //    });
        //}

        //private void Footer(IContainer container)
        //{
        //    container.AlignCenter().Text("Thank you for your business!")
        //        .FontSize(10)
        //        .Italic();
        //}
    }
}
