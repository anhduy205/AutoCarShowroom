using System.Net;
using System.Net.Mail;
using System.Text;
using AutoCarShowroom.Models;
using Microsoft.Extensions.Options;

namespace AutoCarShowroom.Services
{
    public sealed class BookingEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<BookingEmailService> _logger;

        public BookingEmailService(
            IOptions<EmailOptions> options,
            ILogger<BookingEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<EmailSendResult> SendBookingConfirmedAsync(Booking booking)
        {
            if (!_options.Enabled)
            {
                return EmailSendResult.Skipped("Chức năng gửi email chưa được bật trong cấu hình.");
            }

            if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
                string.IsNullOrWhiteSpace(_options.SenderEmail))
            {
                return EmailSendResult.Skipped("Thiếu cấu hình SMTP hoặc địa chỉ email người gửi.");
            }

            if (string.IsNullOrWhiteSpace(booking.Email))
            {
                return EmailSendResult.Skipped("Khách hàng chưa có email để nhận thông báo.");
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_options.SenderEmail, _options.SenderName, Encoding.UTF8),
                    Subject = $"Xác nhận lịch hẹn showroom - {booking.BookingCode}",
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = false,
                    Body = BuildConfirmationBody(booking)
                };

                message.To.Add(new MailAddress(booking.Email, booking.CustomerName, Encoding.UTF8));

                using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
                {
                    EnableSsl = _options.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrWhiteSpace(_options.Username))
                {
                    client.Credentials = new NetworkCredential(_options.Username, _options.Password);
                }

                await client.SendMailAsync(message);
                return EmailSendResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to send booking confirmation email for booking {BookingCode}.", booking.BookingCode);
                return EmailSendResult.Failed("Không gửi được email xác nhận cho khách.");
            }
        }

        private static string BuildConfirmationBody(Booking booking)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Xin chào {booking.CustomerName},");
            builder.AppendLine();
            builder.AppendLine("Showroom đã xác nhận lịch hẹn của bạn thành công.");
            builder.AppendLine();
            builder.AppendLine($"Mã lịch hẹn: {booking.BookingCode}");
            builder.AppendLine($"Mẫu xe: {booking.CarName}");
            builder.AppendLine($"Loại dịch vụ: {booking.ServiceType}");
            builder.AppendLine($"Thời gian hẹn: {booking.AppointmentAt:dd/MM/yyyy HH:mm}");
            builder.AppendLine();

            if (!string.IsNullOrWhiteSpace(booking.AdminNote))
            {
                builder.AppendLine($"Ghi chú từ showroom: {booking.AdminNote}");
                builder.AppendLine();
            }

            builder.AppendLine("Vui lòng đến đúng giờ hoặc liên hệ lại showroom nếu cần thay đổi lịch.");
            builder.AppendLine();
            builder.AppendLine("Trân trọng,");
            builder.AppendLine("AutoCarShowroom");

            return builder.ToString();
        }
    }

    public sealed class EmailSendResult
    {
        public bool Succeeded { get; init; }

        public bool WasSkipped { get; init; }

        public string? Message { get; init; }

        public static EmailSendResult Success()
        {
            return new EmailSendResult
            {
                Succeeded = true
            };
        }

        public static EmailSendResult Skipped(string message)
        {
            return new EmailSendResult
            {
                WasSkipped = true,
                Message = message
            };
        }

        public static EmailSendResult Failed(string message)
        {
            return new EmailSendResult
            {
                Message = message
            };
        }
    }
}
