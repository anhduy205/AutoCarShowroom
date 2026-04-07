using System.ComponentModel.DataAnnotations;
using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Services.Chatbot
{
    public sealed class ChatbotBookingTools
    {
        private readonly ShowroomDbContext _context;
        private readonly ChatbotInventoryTools _inventoryTools;
        private readonly BookingSchedulingService _bookingSchedulingService;

        public ChatbotBookingTools(
            ShowroomDbContext context,
            ChatbotInventoryTools inventoryTools,
            BookingSchedulingService bookingSchedulingService)
        {
            _context = context;
            _inventoryTools = inventoryTools;
            _bookingSchedulingService = bookingSchedulingService;
        }

        public async Task<ChatbotBookingCreationResult> CreateBookingAsync(ChatbotBookingDraft draft)
        {
            if (!draft.CarId.HasValue)
            {
                return new ChatbotBookingCreationResult
                {
                    Errors = ["Anh/chị chưa chọn mẫu xe để đặt lịch."]
                };
            }

            var car = await _inventoryTools.GetCarByIdAsync(draft.CarId.Value);
            if (car == null || !OrderWorkflow.CanOrder(car))
            {
                return new ChatbotBookingCreationResult
                {
                    Errors = ["Mẫu xe này hiện chưa sẵn sàng để nhận đặt lịch."]
                };
            }

            var model = new BookingCreateViewModel
            {
                CarId = car.CarID,
                CustomerName = draft.CustomerName ?? string.Empty,
                PhoneNumber = draft.PhoneNumber ?? string.Empty,
                Email = draft.Email ?? string.Empty,
                ServiceType = draft.ServiceType ?? string.Empty,
                AppointmentAt = draft.AppointmentAt ?? DateTime.Now,
                Note = draft.Note
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (!BookingWorkflow.ServiceTypes.Contains(model.ServiceType, StringComparer.OrdinalIgnoreCase))
            {
                validationResults.Add(new ValidationResult(
                    "Vui lòng chọn loại dịch vụ hợp lệ.",
                    [nameof(BookingCreateViewModel.ServiceType)]));
            }

            if (!draft.AppointmentAt.HasValue || draft.AppointmentAt.Value <= DateTime.Now)
            {
                validationResults.Add(new ValidationResult(
                    "Ngày giờ hẹn phải lớn hơn thời điểm hiện tại.",
                    [nameof(BookingCreateViewModel.AppointmentAt)]));
            }

            if (validationResults.Count > 0)
            {
                return new ChatbotBookingCreationResult
                {
                    Car = car,
                    Errors = validationResults
                        .Select(result => result.ErrorMessage)
                        .Where(message => !string.IsNullOrWhiteSpace(message))
                        .Select(message => message!)
                        .Distinct(StringComparer.Ordinal)
                        .ToList()
                };
            }

            var slotEvaluation = await _bookingSchedulingService.EvaluateAsync(model.AppointmentAt);

            var booking = new Booking
            {
                BookingCode = await GenerateBookingCodeAsync(),
                CarId = car.CarID,
                CarName = car.CarName,
                CarImage = car.Image,
                QuotedPrice = car.Price,
                CustomerName = model.CustomerName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                ServiceType = model.ServiceType,
                AppointmentAt = model.AppointmentAt,
                Note = model.Note,
                BookingStatus = slotEvaluation.InitialStatus,
                AdminNote = slotEvaluation.AdminNote,
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return new ChatbotBookingCreationResult
            {
                Succeeded = true,
                Booking = booking,
                Car = car,
                CustomerMessage = slotEvaluation.CustomerMessage,
                SuggestedAppointmentAt = slotEvaluation.SuggestedAppointmentAt
            };
        }

        private async Task<string> GenerateBookingCodeAsync()
        {
            while (true)
            {
                var bookingCode = $"BK-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..24];
                var exists = await _context.Bookings.AnyAsync(item => item.BookingCode == bookingCode);

                if (!exists)
                {
                    return bookingCode;
                }
            }
        }
    }
}
