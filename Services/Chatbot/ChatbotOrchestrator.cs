using System.Text;
using AutoCarShowroom.Models;

namespace AutoCarShowroom.Services.Chatbot
{
    public sealed partial class ChatbotOrchestrator
    {
        private static readonly string[] BodyTypes = ["SUV", "Sedan", "Hatchback", "MPV", "Crossover", "Bán tải", "Coupe", "Mui trần"];
        private static readonly string[] PurposeKeywords = ["gia dinh", "ca nhan", "di pho", "dich vu", "duong dai", "doanh nhan", "mua lan dau", "offroad"];
        private static readonly string[] KnownBrandKeywords =
        [
            "Toyota", "Hyundai", "Kia", "Ford", "Mazda", "Honda", "Mitsubishi", "VinFast",
            "Ferrari", "Lamborghini", "BMW", "Mercedes", "Mercedes-Benz", "Audi", "Lexus",
            "Peugeot", "Nissan", "Suzuki", "Subaru", "Porsche", "Isuzu", "MG"
        ];
        private static readonly string[] SearchClarificationReplies =
        [
            "Ngân sách dưới 1 tỷ",
            "Xe gia đình 5 chỗ",
            "SUV đi phố",
            "Xe chạy dịch vụ"
        ];

        private readonly ChatbotInventoryTools _inventoryTools;
        private readonly ChatbotFinanceTools _financeTools;
        private readonly ChatbotBookingTools _bookingTools;
        private readonly ChatbotFaqTools _faqTools;

        public ChatbotOrchestrator(
            ChatbotInventoryTools inventoryTools,
            ChatbotFinanceTools financeTools,
            ChatbotBookingTools bookingTools,
            ChatbotFaqTools faqTools)
        {
            _inventoryTools = inventoryTools;
            _financeTools = financeTools;
            _bookingTools = bookingTools;
            _faqTools = faqTools;
        }

        public async Task<ChatbotReply> AskAsync(ChatbotRequest request)
        {
            var message = request.Message?.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return BuildWelcomeReply();
            }

            var normalizedMessage = ChatbotTextParser.Normalize(message);
            var state = request.State ?? new ChatbotConversationState();

            if (request.CurrentCarId.HasValue && !state.CurrentCarId.HasValue)
            {
                state.CurrentCarId = request.CurrentCarId;
            }

            if (TryResetConversation(normalizedMessage, state, out var resetReply))
            {
                return resetReply;
            }

            var flowReply = await TryHandleActiveFlowAsync(message, normalizedMessage, state);
            if (flowReply != null)
            {
                return flowReply;
            }

            if (ChatbotTextParser.LooksLikeGreeting(normalizedMessage))
            {
                return BuildWelcomeReply(state);
            }

            if (IsInstallmentIntent(normalizedMessage))
            {
                return await StartInstallmentFlowAsync(message, normalizedMessage, state);
            }

            if (IsBookingIntent(normalizedMessage))
            {
                return await StartBookingFlowAsync(message, normalizedMessage, state);
            }

            var comparisonReply = await TryBuildComparisonReplyAsync(message, normalizedMessage, state);
            if (comparisonReply != null)
            {
                return comparisonReply;
            }

            var detailReply = await TryBuildCarDetailReplyAsync(normalizedMessage, state);
            if (detailReply != null)
            {
                return detailReply;
            }

            var missingSpecificCarReply = await TryBuildUnavailableSpecificCarReplyAsync(message, normalizedMessage, state);
            if (missingSpecificCarReply != null)
            {
                return missingSpecificCarReply;
            }

            var inventoryOverviewReply = await TryBuildInventoryOverviewReplyAsync(normalizedMessage, state);
            if (inventoryOverviewReply != null)
            {
                return inventoryOverviewReply;
            }

            if (IsPromotionIntent(normalizedMessage))
            {
                return await BuildPromotionReplyAsync(message, normalizedMessage, state);
            }

            if (IsPaymentQuestion(normalizedMessage))
            {
                return BuildPaymentMethodReply(state);
            }

            var faqReply = TryBuildFaqReply(normalizedMessage, state);
            if (faqReply != null)
            {
                return faqReply;
            }

            return await BuildRecommendationReplyAsync(message, normalizedMessage, state);
        }

        private static bool TryResetConversation(string normalizedMessage, ChatbotConversationState state, out ChatbotReply reply)
        {
            if (!ChatbotTextParser.ContainsAny(normalizedMessage, "dat lai", "lam lai", "xoa hoi thoai", "huy tu van", "dung lai"))
            {
                reply = null!;
                return false;
            }

            var currentCarId = state.CurrentCarId;
            state.ActiveMode = null;
            state.ActiveIntent = null;
            state.PendingField = null;
            state.ClarifyingQuestionsAsked = 0;
            state.SearchProfile = new ChatbotSearchProfile();
            state.BookingDraft = new ChatbotBookingDraft();
            state.InstallmentDraft = new ChatbotInstallmentDraft();
            state.CurrentCarId = currentCarId;

            reply = new ChatbotReply
            {
                Message = "Em đã làm mới phiên tư vấn. Anh/chị cứ nhắn nhu cầu mới như ngân sách, kiểu xe, mẫu muốn so sánh hoặc nhu cầu đặt lịch xem xe nhé.",
                QuickReplies = SearchClarificationReplies
                    .Select(value => new ChatbotQuickReply { Label = value, Message = value })
                    .ToList(),
                State = state
            };

            return true;
        }

        private async Task<ChatbotReply?> TryHandleActiveFlowAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            return state.ActiveMode switch
            {
                "booking" => await ContinueBookingFlowAsync(message, normalizedMessage, state),
                "installment" => await ContinueInstallmentFlowAsync(message, normalizedMessage, state),
                "search" => await ContinueSearchFlowAsync(message, normalizedMessage, state),
                _ => null
            };
        }

        private async Task<ChatbotReply?> ContinueSearchFlowAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            if (!string.Equals(state.PendingField, "search_context", StringComparison.Ordinal))
            {
                return null;
            }

            MergeSearchProfile(state.SearchProfile, message, normalizedMessage);
            state.ActiveMode = null;
            state.PendingField = null;

            return await BuildRecommendationReplyAsync(message, normalizedMessage, state);
        }

                private async Task<ChatbotReply> StartBookingFlowAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            state.ActiveMode = "booking";
            state.ActiveIntent = "booking";
            state.BookingDraft.ServiceType ??= ChatbotTextParser.ExtractBookingServiceType(message);

            var candidateCars = await _inventoryTools.MatchCarsByMessageAsync(message, state.CurrentCarId, take: 1);
            var car = candidateCars.FirstOrDefault();

            if (car == null && state.CurrentCarId.HasValue)
            {
                car = await _inventoryTools.GetCarByIdAsync(state.CurrentCarId.Value);
            }

            if (car == null)
            {
                state.PendingField = "booking_car";
                return new ChatbotReply
                {
                    Message = "Em có thể ghi nhận yêu cầu đặt lịch xem xe, tư vấn hoặc lái thử ngay trong chat. Anh/chị cho em biết rõ mẫu xe đang quan tâm trước nhé, ví dụ: Toyota Camry 2.5Q hoặc VinFast VF 8 Plus.",
                    RequiresInput = true,
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "Gợi ý xe gia đình", Message = "Gợi ý xe gia đình dưới 1 tỷ" },
                        new ChatbotQuickReply { Label = "Xem SUV", Message = "Gợi ý SUV đang có" }
                    ],
                    State = state
                };
            }

            state.CurrentCarId = car.CarID;
            state.BookingDraft.CarId = car.CarID;

            return await AskNextBookingQuestionAsync(state, car);
        }
                private async Task<ChatbotReply> ContinueBookingFlowAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            if (ChatbotTextParser.ContainsAny(normalizedMessage, "huy", "dung", "thoat"))
            {
                state.ActiveMode = null;
                state.PendingField = null;
                state.BookingDraft = new ChatbotBookingDraft();

                return new ChatbotReply
                {
                    Message = "Em đã dừng luồng đặt lịch. Khi cần lại, anh/chị chỉ cần nhắn “đặt lịch xem xe” là được.",
                    State = state
                };
            }

            state.BookingDraft.ServiceType ??= ChatbotTextParser.ExtractBookingServiceType(message);

            Car? car = null;
            if (state.BookingDraft.CarId.HasValue)
            {
                car = await _inventoryTools.GetCarByIdAsync(state.BookingDraft.CarId.Value);
            }

            if (car == null && state.CurrentCarId.HasValue)
            {
                car = await _inventoryTools.GetCarByIdAsync(state.CurrentCarId.Value);
                if (car != null)
                {
                    state.BookingDraft.CarId = car.CarID;
                }
            }

            if (string.Equals(state.PendingField, "booking_car", StringComparison.Ordinal))
            {
                car = (await _inventoryTools.MatchCarsByMessageAsync(message, state.CurrentCarId, take: 1)).FirstOrDefault();
                if (car == null)
                {
                    return new ChatbotReply
                    {
                        Message = "Em vẫn chưa xác định được mẫu xe anh/chị muốn đặt lịch. Anh/chị nhắn rõ tên xe giúp em nhé.",
                        RequiresInput = true,
                        State = state
                    };
                }

                state.CurrentCarId = car.CarID;
                state.BookingDraft.CarId = car.CarID;
                return await AskNextBookingQuestionAsync(state, car);
            }

            if (car == null)
            {
                state.PendingField = "booking_car";
                return new ChatbotReply
                {
                    Message = "Em cần xác định lại mẫu xe trước khi tạo yêu cầu đặt lịch. Anh/chị nhắn giúp em tên xe đang muốn xem nhé.",
                    RequiresInput = true,
                    State = state
                };
            }

            switch (state.PendingField)
            {
                case "customer_name":
                    state.BookingDraft.CustomerName = message.Trim();
                    break;
                case "phone_number":
                    state.BookingDraft.PhoneNumber = ChatbotTextParser.ExtractPhoneNumber(message) ?? message.Trim();
                    break;
                case "email":
                    state.BookingDraft.Email = ChatbotTextParser.ExtractEmail(message) ?? message.Trim();
                    break;
                case "service_type":
                    state.BookingDraft.ServiceType = ChatbotTextParser.ExtractBookingServiceType(message);
                    if (string.IsNullOrWhiteSpace(state.BookingDraft.ServiceType))
                    {
                        return new ChatbotReply
                        {
                            Message = "Anh/chị giúp em chọn 1 loại dịch vụ: xem xe, tư vấn hoặc lái thử nhé.",
                            RequiresInput = true,
                            QuickReplies =
                            [
                                new ChatbotQuickReply { Label = "Xem xe", Message = "Xem xe" },
                                new ChatbotQuickReply { Label = "Tư vấn", Message = "Tư vấn" },
                                new ChatbotQuickReply { Label = "Lái thử", Message = "Lái thử" }
                            ],
                            State = state
                        };
                    }
                    break;
                case "appointment_at":
                    state.BookingDraft.AppointmentAt = ChatbotTextParser.ExtractAppointment(message);
                    if (state.BookingDraft.AppointmentAt == null)
                    {
                        return new ChatbotReply
                        {
                            Message = "Em chưa đọc được ngày giờ hẹn. Anh/chị giúp em nhắn theo dạng `dd/MM/yyyy HH:mm`, ví dụ `12/04/2026 09:30` nhé.",
                            RequiresInput = true,
                            State = state
                        };
                    }
                    break;
            }

            return await AskNextBookingQuestionAsync(state, car);
        }
                private async Task<ChatbotReply> AskNextBookingQuestionAsync(ChatbotConversationState state, Car car)
        {
            if (string.IsNullOrWhiteSpace(state.BookingDraft.CustomerName))
            {
                state.PendingField = "customer_name";
                return new ChatbotReply
                {
                    Message = $"Em đang hỗ trợ ghi nhận yêu cầu đặt lịch cho `{car.CarName}`. Anh/chị cho em xin họ và tên để em tạo yêu cầu đặt lịch nhé.",
                    RequiresInput = true,
                    Actions = BuildCarActions(car),
                    State = state
                };
            }

            if (string.IsNullOrWhiteSpace(state.BookingDraft.PhoneNumber))
            {
                state.PendingField = "phone_number";
                return new ChatbotReply
                {
                    Message = "Anh/chị cho em xin số điện thoại 10 số để showroom tiện liên hệ xác nhận lịch hẹn sau khi Admin duyệt.",
                    RequiresInput = true,
                    State = state
                };
            }

            if (string.IsNullOrWhiteSpace(state.BookingDraft.Email))
            {
                state.PendingField = "email";
                return new ChatbotReply
                {
                    Message = "Anh/chị cho em xin email để em hoàn tất thông tin đặt lịch nhé.",
                    RequiresInput = true,
                    State = state
                };
            }

            if (string.IsNullOrWhiteSpace(state.BookingDraft.ServiceType))
            {
                state.PendingField = "service_type";
                return new ChatbotReply
                {
                    Message = "Anh/chị muốn đặt lịch theo loại dịch vụ nào: xem xe, tư vấn hay lái thử?",
                    RequiresInput = true,
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "Xem xe", Message = "Xem xe" },
                        new ChatbotQuickReply { Label = "Tư vấn", Message = "Tư vấn" },
                        new ChatbotQuickReply { Label = "Lái thử", Message = "Lái thử" }
                    ],
                    State = state
                };
            }

            if (!state.BookingDraft.AppointmentAt.HasValue)
            {
                state.PendingField = "appointment_at";
                return new ChatbotReply
                {
                    Message = "Anh/chị muốn hẹn ngày giờ nào? Anh/chị nhắn theo dạng `dd/MM/yyyy HH:mm`, ví dụ `12/04/2026 09:30` giúp em nhé. Lưu ý lịch này sẽ cần chờ Admin xác nhận trước khi chốt chính thức.",
                    RequiresInput = true,
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "Ngày mai 09:00", Message = "mai 9h" },
                        new ChatbotQuickReply { Label = "Ngày mai 14:00", Message = "mai 14h" }
                    ],
                    State = state
                };
            }

            state.PendingField = null;
            state.ActiveMode = null;

            var createResult = await _bookingTools.CreateBookingAsync(state.BookingDraft);
            if (!createResult.Succeeded || createResult.Booking == null || createResult.Car == null)
            {
                state.ActiveMode = "booking";
                state.PendingField = InferBookingRetryField(createResult.Errors);

                return new ChatbotReply
                {
                    Message = $"Em chưa tạo được yêu cầu đặt lịch vì: {string.Join("; ", createResult.Errors)}",
                    RequiresInput = true,
                    State = state
                };
            }

            var booking = createResult.Booking;
            state.BookingDraft = new ChatbotBookingDraft();

            var quickReplies = new List<ChatbotQuickReply>
            {
                new ChatbotQuickReply { Label = "Gợi ý xe tương tự", Message = $"Gợi ý xe giống {createResult.Car.BodyType}" },
                new ChatbotQuickReply { Label = "Xem thanh toán", Message = "Showroom có những phương thức thanh toán nào?" }
            };

            if (createResult.SuggestedAppointmentAt.HasValue)
            {
                quickReplies.Insert(0, new ChatbotQuickReply
                {
                    Label = $"Đổi sang {createResult.SuggestedAppointmentAt.Value:HH:mm dd/MM}",
                    Message = $"Tôi muốn đặt lịch vào {createResult.SuggestedAppointmentAt.Value:dd/MM/yyyy HH:mm}"
                });
            }

            return new ChatbotReply
            {
                Message = BuildBookingSuccessMessage(createResult.Car, booking, createResult.CustomerMessage, createResult.SuggestedAppointmentAt),
                Actions =
                [
                    CreateLinkAction("Xem chi tiết xe", $"/Cars/Details/{createResult.Car.CarID}", "secondary"),
                    CreateLinkAction("Xem yêu cầu đã tạo", $"/Bookings/Success?bookingCode={booking.BookingCode}", "primary")
                ],
                QuickReplies = quickReplies,
                State = state
            };
        }
                private static string InferBookingRetryField(IReadOnlyList<string> errors)
        {
            var joinedErrors = string.Join(' ', errors).ToLowerInvariant();
            if (joinedErrors.Contains("điện thoại", StringComparison.Ordinal) || joinedErrors.Contains("dien thoai", StringComparison.Ordinal))
            {
                return "phone_number";
            }

            if (joinedErrors.Contains("email", StringComparison.Ordinal))
            {
                return "email";
            }

            if (joinedErrors.Contains("dịch vụ", StringComparison.Ordinal) || joinedErrors.Contains("dich vu", StringComparison.Ordinal))
            {
                return "service_type";
            }

            if (joinedErrors.Contains("ngày giờ", StringComparison.Ordinal) || joinedErrors.Contains("ngay gio", StringComparison.Ordinal) || joinedErrors.Contains("thời điểm", StringComparison.Ordinal) || joinedErrors.Contains("thoi diem", StringComparison.Ordinal))
            {
                return "appointment_at";
            }

            if (joinedErrors.Contains("mẫu xe", StringComparison.Ordinal) || joinedErrors.Contains("mau xe", StringComparison.Ordinal))
            {
                return "booking_car";
            }

            return "customer_name";
        }
        private async Task<ChatbotReply> StartInstallmentFlowAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            state.ActiveMode = "installment";
            state.ActiveIntent = "installment";

            var candidateCars = await _inventoryTools.MatchCarsByMessageAsync(message, state.CurrentCarId, take: 1);
            var car = candidateCars.FirstOrDefault();

            if (car == null && state.CurrentCarId.HasValue)
            {
                car = await _inventoryTools.GetCarByIdAsync(state.CurrentCarId.Value);
            }

            if (car == null)
            {
                state.PendingField = "installment_car";
                return new ChatbotReply
                {
                    Message = "Em có thể tính trả góp tham khảo, nhưng cần biết anh/chị muốn tính cho mẫu xe nào trước nhé.",
                    RequiresInput = true,
                    State = state
                };
            }

            state.CurrentCarId = car.CarID;
            state.InstallmentDraft.CarId = car.CarID;

            var extractedDownPayment = ChatbotTextParser.ExtractMoneyValue(message);
            if (extractedDownPayment.HasValue)
            {
                state.InstallmentDraft.DownPayment = extractedDownPayment;
            }

            var extractedTermMonths = ChatbotTextParser.ExtractTermMonths(message);
            if (extractedTermMonths.HasValue)
            {
                state.InstallmentDraft.TermMonths = extractedTermMonths;
            }

            return await BuildInstallmentReplyOrAskAsync(state, car);
        }

        private async Task<ChatbotReply> ContinueInstallmentFlowAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            if (ChatbotTextParser.ContainsAny(normalizedMessage, "huy", "dung", "thoat"))
            {
                state.ActiveMode = null;
                state.PendingField = null;
                state.InstallmentDraft = new ChatbotInstallmentDraft();

                return new ChatbotReply
                {
                    Message = "Em đã dừng phần tính trả góp. Khi cần tính lại, anh/chị chỉ cần nhắn số tiền trả trước hoặc tên mẫu xe nhé.",
                    State = state
                };
            }

            Car? car = null;
            if (state.InstallmentDraft.CarId.HasValue)
            {
                car = await _inventoryTools.GetCarByIdAsync(state.InstallmentDraft.CarId.Value);
            }

            if (string.Equals(state.PendingField, "installment_car", StringComparison.Ordinal))
            {
                car = (await _inventoryTools.MatchCarsByMessageAsync(message, state.CurrentCarId, take: 1)).FirstOrDefault();
                if (car == null)
                {
                    return new ChatbotReply
                    {
                        Message = "Em vẫn chưa nhận ra mẫu xe cần tính trả góp. Anh/chị nhắn rõ tên xe giúp em nhé.",
                        RequiresInput = true,
                        State = state
                    };
                }

                state.CurrentCarId = car.CarID;
                state.InstallmentDraft.CarId = car.CarID;
            }

            if (car == null && state.CurrentCarId.HasValue)
            {
                car = await _inventoryTools.GetCarByIdAsync(state.CurrentCarId.Value);
                if (car != null)
                {
                    state.InstallmentDraft.CarId = car.CarID;
                }
            }

            if (car == null)
            {
                state.PendingField = "installment_car";
                return new ChatbotReply
                {
                    Message = "Em cần xác định lại mẫu xe trước khi tính trả góp. Anh/chị nhắn tên xe giúp em nhé.",
                    RequiresInput = true,
                    State = state
                };
            }

            if (string.Equals(state.PendingField, "down_payment", StringComparison.Ordinal))
            {
                state.InstallmentDraft.DownPayment = ChatbotTextParser.ExtractMoneyValue(message);
                if (!state.InstallmentDraft.DownPayment.HasValue)
                {
                    return new ChatbotReply
                    {
                        Message = "Em chưa đọc được số tiền trả trước. Anh/chị nhắn giúp em theo dạng như `300 triệu` hoặc `500tr` nhé.",
                        RequiresInput = true,
                        State = state
                    };
                }
            }

            if (string.Equals(state.PendingField, "term_months", StringComparison.Ordinal))
            {
                state.InstallmentDraft.TermMonths = ChatbotTextParser.ExtractTermMonths(message);
            }

            return await BuildInstallmentReplyOrAskAsync(state, car);
        }

        private async Task<ChatbotReply> BuildInstallmentReplyOrAskAsync(ChatbotConversationState state, Car car)
        {
            if (!state.InstallmentDraft.DownPayment.HasValue)
            {
                state.PendingField = "down_payment";
                return new ChatbotReply
                {
                    Message = $"Em đang tính trả góp tham khảo cho `{car.CarName}`. Anh/chị dự kiến trả trước khoảng bao nhiêu để em tính gần đúng nhất?",
                    RequiresInput = true,
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "300 triệu", Message = "Trả trước 300 triệu" },
                        new ChatbotQuickReply { Label = "30% giá xe", Message = "Trả trước khoảng 30%" }
                    ],
                    Actions = BuildCarActions(car),
                    State = state
                };
            }

            state.PendingField = null;
            state.ActiveMode = null;

            var estimate = await _financeTools.CalculateInstallmentEstimateAsync(
                car.CarID,
                state.InstallmentDraft.DownPayment,
                state.InstallmentDraft.TermMonths,
                state.InstallmentDraft.AnnualInterestRate);

            state.InstallmentDraft = new ChatbotInstallmentDraft();

            if (estimate == null)
            {
                return new ChatbotReply
                {
                    Message = "Hiện em chưa tính được phương án trả góp chính xác cho mẫu xe này trong hệ thống. Anh/chị thử lại sau hoặc để showroom hỗ trợ trực tiếp nhé.",
                    State = state
                };
            }

            return new ChatbotReply
            {
                Message = BuildInstallmentMessage(estimate),
                Actions =
                [
                    CreateLinkAction("Mua xe", $"/Orders/Checkout?carId={car.CarID}", "primary"),
                    CreateLinkAction("Đặt lịch xem xe", $"/Bookings/Create?carId={car.CarID}", "secondary")
                ],
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Tính lại 84 tháng", Message = $"Tính trả góp {car.CarName} trong 84 tháng" },
                    new ChatbotQuickReply { Label = "Giải thích thanh toán", Message = "Showroom có những phương thức thanh toán nào?" }
                ],
                State = state
            };
        }

        private async Task<ChatbotReply?> TryBuildComparisonReplyAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            if (!ChatbotTextParser.ContainsAny(normalizedMessage, "so sanh", "compare"))
            {
                return null;
            }

            var comparedCars = await _inventoryTools.MatchCarsByMessageAsync(message, state.CurrentCarId, take: 3);
            if (comparedCars.Count < 2)
            {
                var faqReply = TryBuildFaqReply(normalizedMessage, state);
                if (faqReply != null)
                {
                    return faqReply;
                }

                return new ChatbotReply
                {
                    Message = "Để em so sánh chính xác theo dữ liệu showroom, anh/chị nhắn rõ 2 mẫu xe đang phân vân nhé. Ví dụ: `So sánh Toyota Camry và BMW X5`.",
                    RequiresInput = true,
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "So sánh Camry và C300", Message = "So sánh Toyota Camry và Mercedes-Benz C300 AMG" },
                        new ChatbotQuickReply { Label = "So sánh Santa Fe và Fortuner", Message = "So sánh Hyundai Santa Fe và Toyota Fortuner" }
                    ],
                    State = state
                };
            }

            var firstCar = comparedCars[0];
            var secondCar = comparedCars[1];
            state.CurrentCarId = firstCar.CarID;

            return new ChatbotReply
            {
                Message = BuildComparisonMessage(firstCar, secondCar),
                Suggestions =
                [
                    BuildSuggestion(firstCar, "Phù hợp để đối chiếu trực tiếp về giá, kiểu xe và phong cách sử dụng."),
                    BuildSuggestion(secondCar, "Nên xem cùng lúc để chốt nhanh hơn theo nhu cầu thật của anh/chị.")
                ],
                Actions =
                [
                    CreateLinkAction("Xem xe 1", $"/Cars/Details/{firstCar.CarID}"),
                    CreateLinkAction("Xem xe 2", $"/Cars/Details/{secondCar.CarID}"),
                    CreateLinkAction("Mua xe 1", $"/Orders/Checkout?carId={firstCar.CarID}", "primary")
                ],
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Gợi ý xe tiết kiệm hơn", Message = "Gợi ý xe tiết kiệm hơn" },
                    new ChatbotQuickReply { Label = "Gợi ý xe gia đình", Message = "Gợi ý xe gia đình 5 người" }
                ],
                State = state
            };
        }

        private async Task<ChatbotReply?> TryBuildCarDetailReplyAsync(string normalizedMessage, ChatbotConversationState state)
        {
            if (!ChatbotTextParser.ContainsAny(normalizedMessage, "xe nay", "mau nay", "chi tiet", "gia bao nhieu", "con hang", "mo ta", "co gi noi bat"))
            {
                return null;
            }

            Car? car = null;
            if (state.CurrentCarId.HasValue)
            {
                car = await _inventoryTools.GetCarByIdAsync(state.CurrentCarId.Value);
            }

            if (car == null)
            {
                return null;
            }

            return new ChatbotReply
            {
                Message = BuildCarDetailMessage(car),
                Suggestions = [BuildSuggestion(car, "Đây là mẫu xe anh/chị đang xem hoặc vừa nhắc tới trong hội thoại.")],
                Actions = BuildCarActions(car),
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Xem thanh toán", Message = "Showroom có những phương thức thanh toán nào?" },
                    new ChatbotQuickReply { Label = "Đặt lịch xem xe", Message = $"Đặt lịch xem xe {car.CarName}" }
                ],
                State = state
            };
        }

        private async Task<ChatbotReply?> TryBuildInventoryOverviewReplyAsync(string normalizedMessage, ChatbotConversationState state)
        {
            if (!IsInventoryOverviewIntent(normalizedMessage))
            {
                return null;
            }

            state.ActiveMode = null;
            state.PendingField = null;
            state.SearchProfile = new ChatbotSearchProfile();
            state.ClarifyingQuestionsAsked = 0;

            var overview = await _inventoryTools.GetInventoryOverviewAsync();
            if (overview.TotalVisibleCars <= 0)
            {
                return new ChatbotReply
                {
                    Message = "Hiện tại hệ thống chưa ghi nhận xe nào đang mở bán trong kho showroom. Anh/chị có thể quay lại sau hoặc liên hệ nhân viên để kiểm tra tồn kho thực tế.",
                    Actions = [CreateLinkAction("Xem danh sách xe", "/Cars/Index", "primary")],
                    State = state
                };
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Hiện tại showroom đang có {overview.TotalVisibleCars} xe mở bán trong kho.");
            builder.AppendLine($"- Tầm giá tham khảo: từ {overview.MinPrice:N0} VNĐ đến {overview.MaxPrice:N0} VNĐ.");
            builder.AppendLine($"- Nhóm xe đang có: {string.Join(", ", overview.BodyTypeSummaries)}.");
            builder.AppendLine($"- Hãng nổi bật: {string.Join(", ", overview.BrandSummaries)}.");
            builder.AppendLine();
            builder.Append("Kết luận: kho xe hiện khá đa dạng từ sedan, SUV đến MPV và bán tải. Nếu anh/chị nói rõ thêm ngân sách, số chỗ hoặc kiểu xe, em sẽ lọc ngay các mẫu hợp nhất.");

            return new ChatbotReply
            {
                Message = builder.ToString().Trim(),
                Suggestions = overview.FeaturedCars
                    .Select(car => BuildSuggestion(car, "Đang mở bán trong kho showroom và phù hợp để xem nhanh trước khi lọc sâu hơn."))
                    .ToList(),
                Actions = [CreateLinkAction("Xem danh sách xe", "/Cars/Index", "primary")],
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "7 chỗ dưới 900 triệu", Message = "Tôi cần xe gia đình 7 chỗ dưới 900 triệu" },
                    new ChatbotQuickReply { Label = "Sedan đi làm hằng ngày", Message = "Tôi muốn xe 5 chỗ, tiết kiệm xăng, đi làm hằng ngày, kiểu Sedan" }
                ],
                State = state
            };
        }

        private async Task<ChatbotReply?> TryBuildUnavailableSpecificCarReplyAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            if (IsInventoryOverviewIntent(normalizedMessage))
            {
                return null;
            }

            if (!TryExtractSpecificCarReference(message, normalizedMessage, out _))
            {
                return null;
            }

            var candidateCars = await _inventoryTools.MatchCarsByMessageAsync(message, state.CurrentCarId, take: 1);
            return candidateCars.Count == 0
                ? TryBuildMissingSpecificCarReply(message, normalizedMessage, state)
                : null;
        }

        private ChatbotReply? TryBuildMissingSpecificCarReply(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            if (!TryExtractSpecificCarReference(message, normalizedMessage, out var carReference))
            {
                return null;
            }

            return new ChatbotReply
            {
                Message = $"Hiện tại em chưa có dữ liệu chính xác cho `{carReference}` trong hệ thống showroom, nên em không dám khẳng định về giá, khuyến mãi hay tình trạng của mẫu này.\n\nNếu anh/chị muốn, em có thể:\n- gợi ý mẫu tương đương đang có trong showroom\n- lọc theo tầm giá hoặc kiểu xe gần giống\n- hoặc anh/chị gửi tên mẫu xe khác để em kiểm tra tiếp.",
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Gợi ý xe tương đương", Message = "Gợi ý xe tương đương đang có trong showroom" },
                    new ChatbotQuickReply { Label = "Xem xe đang mở bán", Message = "Showroom hiện có những xe nào trong kho?" }
                ],
                Actions = [CreateLinkAction("Xem danh sách xe", "/Cars/Index", "primary")],
                State = state
            };
        }

        private async Task<ChatbotReply> BuildPromotionReplyAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            var promotionCar = (await _inventoryTools.MatchCarsByMessageAsync(message, state.CurrentCarId, take: 1)).FirstOrDefault();
            if (promotionCar != null)
            {
                state.CurrentCarId = promotionCar.CarID;

                var messageBuilder = new StringBuilder();
                if (string.Equals(promotionCar.Status, OrderWorkflow.CarStatusPromotion, StringComparison.OrdinalIgnoreCase))
                {
                    messageBuilder.AppendLine($"Hiện tại `{promotionCar.CarName}` đang ở trạng thái khuyến mãi trong hệ thống showroom.");
                    messageBuilder.AppendLine($"- Giá tham khảo: {promotionCar.Price:N0} VNĐ.");
                    messageBuilder.AppendLine($"- Loại xe: {promotionCar.BodyType}.");
                    messageBuilder.AppendLine($"- Năm sản xuất: {promotionCar.Year}.");
                    messageBuilder.AppendLine();
                    messageBuilder.Append("Kết luận: đây là một trong những mẫu đang có ưu đãi hiển thị trong kho. Anh/chị có thể mở chi tiết để xem thêm mô tả, hình ảnh và đặt lịch xem xe.");
                }
                else
                {
                    messageBuilder.AppendLine($"Hiện tại em chưa thấy `{promotionCar.CarName}` ở trạng thái khuyến mãi trong hệ thống.");
                    messageBuilder.AppendLine($"- Trạng thái hiện tại: {promotionCar.Status}.");
                    messageBuilder.AppendLine($"- Giá tham khảo: {promotionCar.Price:N0} VNĐ.");
                    messageBuilder.AppendLine();
                    messageBuilder.Append("Kết luận: mẫu này vẫn đang có dữ liệu trong showroom nhưng chưa có cờ khuyến mãi ở thời điểm hiện tại.");
                }

                return new ChatbotReply
                {
                    Message = messageBuilder.ToString().Trim(),
                    Suggestions = [BuildSuggestion(promotionCar, "Mẫu xe anh/chị vừa hỏi về trạng thái khuyến mãi.")],
                    Actions = BuildCarActions(promotionCar),
                    State = state
                };
            }

            var missingCarReply = TryBuildMissingSpecificCarReply(message, normalizedMessage, state);
            if (missingCarReply != null)
            {
                return missingCarReply;
            }

            var promotionCars = await _inventoryTools.GetPromotionCarsAsync();
            if (promotionCars.Count == 0)
            {
                return new ChatbotReply
                {
                    Message = "Hiện tại em chưa thấy mẫu xe nào đang ở trạng thái khuyến mãi trong hệ thống showroom. Anh/chị muốn em gợi ý theo ngân sách hoặc loại xe đang còn hàng không?",
                    QuickReplies = SearchClarificationReplies
                        .Select(value => new ChatbotQuickReply { Label = value, Message = value })
                        .ToList(),
                    State = state
                };
            }

            return new ChatbotReply
            {
                Message = "Em đã lọc nhanh các mẫu đang ở trạng thái khuyến mãi trong showroom. Anh/chị nên xem trước các mẫu dưới đây để tiện so giá và đặt lịch.",
                Suggestions = promotionCars
                    .Select(car => BuildSuggestion(car, "Đang có trạng thái khuyến mãi trong hệ thống showroom."))
                    .ToList(),
                Actions = [CreateLinkAction("Xem toàn bộ xe", "/Cars/Index", "primary")],
                State = state
            };
        }

        private ChatbotReply BuildPaymentMethodReply(ChatbotConversationState state)
        {
            var methods = _financeTools.GetPaymentMethods();
            var builder = new StringBuilder();
            builder.AppendLine("Hiện showroom đang hỗ trợ các phương thức thanh toán sau:");

            foreach (var method in methods)
            {
                builder.AppendLine($"- {method.ShortLabel}: {method.Description}");
            }

            builder.AppendLine();
            builder.Append("Kết luận: nếu anh/chị muốn chốt nhanh thì nghiêng về QR hoặc chuyển khoản; nếu muốn xem xe kỹ trước khi xuống tiền thì thanh toán tại showroom sẽ linh hoạt hơn.");
            builder.AppendLine();
            builder.Append("Bước tiếp theo: nếu anh/chị đang nhắm một mẫu cụ thể, em có thể dẫn thẳng sang trang mua xe hoặc đặt lịch xem xe.");

            var actions = new List<ChatbotAction>();
            if (state.CurrentCarId.HasValue)
            {
                actions.Add(CreateLinkAction("Mua xe", $"/Orders/Checkout?carId={state.CurrentCarId.Value}", "primary"));
                actions.Add(CreateLinkAction("Đặt lịch xem xe", $"/Bookings/Create?carId={state.CurrentCarId.Value}"));
            }

            return new ChatbotReply
            {
                Message = builder.ToString().Trim(),
                Actions = actions,
                State = state
            };
        }

        private ChatbotReply? TryBuildFaqReply(string normalizedMessage, ChatbotConversationState state)
        {
            var article = _faqTools.Lookup(normalizedMessage);
            if (article == null)
            {
                return null;
            }

            return new ChatbotReply
            {
                Message = $"{article.Title}\n\n{article.Answer}\n\n{article.Disclaimer}\n\nBước tiếp theo: {article.NextStep}",
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Tư vấn theo ngân sách", Message = "Gợi ý xe theo ngân sách" },
                    new ChatbotQuickReply { Label = "Đặt lịch xem xe", Message = "Tôi muốn đặt lịch xem xe" }
                ],
                State = state
            };
        }

        private async Task<ChatbotReply> BuildRecommendationReplyAsync(
            string message,
            string normalizedMessage,
            ChatbotConversationState state)
        {
            if (ShouldResetSearchProfile(normalizedMessage, state))
            {
                state.SearchProfile = new ChatbotSearchProfile();
                state.ClarifyingQuestionsAsked = 0;
            }

            MergeSearchProfile(state.SearchProfile, message, normalizedMessage);

            if (NeedMoreSearchContext(normalizedMessage, state.SearchProfile))
            {
                state.ActiveMode = "search";
                state.ActiveIntent = "search";
                state.PendingField = "search_context";
                state.ClarifyingQuestionsAsked++;

                return new ChatbotReply
                {
                    Message = "Để em gợi ý đúng hơn, anh/chị cho em 3 ý ngắn thôi nhé:\n- Ngân sách khoảng bao nhiêu?\n- Mua đi gia đình, cá nhân hay dịch vụ?\n- Anh/chị nghiêng về sedan, SUV hay 7 chỗ?",
                    RequiresInput = true,
                    QuickReplies = SearchClarificationReplies
                        .Select(value => new ChatbotQuickReply { Label = value, Message = value })
                        .ToList(),
                    State = state
                };
            }

            state.ActiveMode = null;
            state.ActiveIntent = "search";
            state.PendingField = null;

            var searchCriteria = new ChatbotSearchCriteria
            {
                MinBudget = state.SearchProfile.MinBudget,
                MaxBudget = state.SearchProfile.MaxBudget ?? state.SearchProfile.Budget,
                Brand = state.SearchProfile.Brand,
                BodyType = state.SearchProfile.BodyType,
                SeatCount = state.SearchProfile.SeatCount,
                Purpose = state.SearchProfile.Purpose,
                Keyword = state.SearchProfile.RawKeywords ?? message,
                MaxResults = 4
            };

            var matches = await _inventoryTools.SearchCarsAsync(searchCriteria);
            if (matches.Count == 0)
            {
                return new ChatbotReply
                {
                    Message = "Hiện tại em chưa tìm thấy mẫu khớp hoàn toàn trong showroom. Nếu anh/chị muốn, em có thể nới ngân sách, đổi loại xe hoặc gợi ý mẫu gần nhất đang còn hàng.",
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "Nới lên 1,2 tỷ", Message = "Gợi ý xe dưới 1.2 tỷ" },
                        new ChatbotQuickReply { Label = "Xem SUV đang có", Message = "Gợi ý SUV đang có" }
                    ],
                    Actions = [CreateLinkAction("Xem toàn bộ xe", "/Cars/Index", "primary")],
                    State = state
                };
            }

            var topMatch = matches[0];
            state.CurrentCarId = topMatch.Car.CarID;

            return new ChatbotReply
            {
                Message = BuildRecommendationMessage(matches, state.SearchProfile),
                Suggestions = matches.Select(match => BuildSuggestion(match.Car, BuildSuggestionReason(match))).ToList(),
                Actions = BuildRecommendationActions(matches),
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "So sánh 2 xe đầu", Message = BuildComparisonPrompt(matches) },
                    new ChatbotQuickReply { Label = "Đặt lịch xe đầu", Message = $"Đặt lịch xem xe {topMatch.Car.CarName}" }
                ],
                State = state
            };
        }

        private static bool NeedMoreSearchContext(string normalizedMessage, ChatbotSearchProfile profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.BodyType)
                || !string.IsNullOrWhiteSpace(profile.Purpose)
                || !string.IsNullOrWhiteSpace(profile.Brand)
                || profile.Budget.HasValue
                || profile.MinBudget.HasValue
                || profile.MaxBudget.HasValue
                || profile.SeatCount.HasValue)
            {
                return false;
            }

            return ChatbotTextParser.ContainsAny(
                normalizedMessage,
                "xe nao tot",
                "mua xe nao hop",
                "xe nao dang tien",
                "goi y xe",
                "tu van xe",
                "xe nao hop");
        }

        private static void MergeSearchProfile(ChatbotSearchProfile profile, string message, string normalizedMessage)
        {
            var budgetConstraint = ChatbotTextParser.ExtractBudgetConstraint(message);
            if (budgetConstraint != null)
            {
                profile.Budget = budgetConstraint.Amount;
                profile.MinBudget = budgetConstraint.MinBudget;
                profile.MaxBudget = budgetConstraint.MaxBudget;
                profile.BudgetMode = budgetConstraint.Mode;
            }

            var bodyType = ExtractBodyType(normalizedMessage);
            if (!string.IsNullOrWhiteSpace(bodyType))
            {
                profile.BodyType = bodyType;
            }

            var purpose = ExtractPurpose(normalizedMessage);
            if (!string.IsNullOrWhiteSpace(purpose))
            {
                profile.Purpose = purpose;
            }

            var seatCount = ChatbotTextParser.ExtractSeatCount(message);
            if (seatCount.HasValue)
            {
                profile.SeatCount = seatCount;
            }

            var brand = ExtractBrand(normalizedMessage);
            if (!string.IsNullOrWhiteSpace(brand))
            {
                profile.Brand = brand;
            }

            profile.RawKeywords = message.Trim();
        }

        private static string? ExtractBodyType(string normalizedMessage)
        {
            return BodyTypes.FirstOrDefault(type =>
                normalizedMessage.Contains(ChatbotTextParser.Normalize(type), StringComparison.Ordinal));
        }

        private static string? ExtractPurpose(string normalizedMessage)
        {
            if (ChatbotTextParser.ContainsAny(normalizedMessage, "gia dinh", "dua don", "7 cho"))
            {
                return "gia dinh";
            }

            if (ChatbotTextParser.ContainsAny(normalizedMessage, "di lam", "hang ngay", "di pho", "ca nhan"))
            {
                return "di pho";
            }

            if (ChatbotTextParser.ContainsAny(normalizedMessage, "dich vu", "kinh doanh", "chay xe"))
            {
                return "dich vu";
            }

            if (ChatbotTextParser.ContainsAny(normalizedMessage, "duong dai", "di tinh", "du lich"))
            {
                return "duong dai";
            }

            return PurposeKeywords.FirstOrDefault(keyword =>
                normalizedMessage.Contains(keyword, StringComparison.Ordinal));
        }

        private static string? ExtractBrand(string normalizedMessage)
        {
            return KnownBrandKeywords.FirstOrDefault(brand =>
                normalizedMessage.Contains(ChatbotTextParser.Normalize(brand), StringComparison.Ordinal));
        }

        private static bool ShouldResetSearchProfile(string normalizedMessage, ChatbotConversationState state)
        {
            if (!HasSearchProfile(state.SearchProfile))
            {
                return false;
            }

            if (ChatbotTextParser.ContainsAny(
                    normalizedMessage,
                    "xe nay",
                    "mau nay",
                    "chi tiet",
                    "so sanh",
                    "thanh toan",
                    "khuyen mai",
                    "dat lich",
                    "tra gop"))
            {
                return false;
            }

            if (ShouldContinueCurrentSearch(normalizedMessage, state.SearchProfile))
            {
                return false;
            }

            return LooksLikeFreshSearchRequest(normalizedMessage) || IsInventoryOverviewIntent(normalizedMessage);
        }

        private static bool HasSearchProfile(ChatbotSearchProfile profile)
        {
            return profile.Budget.HasValue
                   || profile.MinBudget.HasValue
                   || profile.MaxBudget.HasValue
                   || profile.SeatCount.HasValue
                   || !string.IsNullOrWhiteSpace(profile.BodyType)
                   || !string.IsNullOrWhiteSpace(profile.Purpose)
                   || !string.IsNullOrWhiteSpace(profile.Brand)
                   || !string.IsNullOrWhiteSpace(profile.RawKeywords);
        }

        private static bool ShouldContinueCurrentSearch(string normalizedMessage, ChatbotSearchProfile currentProfile)
        {
            if (!HasSearchProfile(currentProfile))
            {
                return false;
            }

            if (ChatbotTextParser.ExtractBudgetConstraint(normalizedMessage) != null)
            {
                return false;
            }

            if (ChatbotTextParser.ContainsAny(normalizedMessage, "xe moi", "doi cau hoi", "cau hoi moi"))
            {
                return false;
            }

            if (ChatbotTextParser.ContainsAny(normalizedMessage, "them", "loc them", "bo sung", "doi sang", "con"))
            {
                return HasSearchTraits(normalizedMessage);
            }

            return IsAttributeOnlySearchMessage(normalizedMessage);
        }

        private static bool LooksLikeFreshSearchRequest(string normalizedMessage)
        {
            if (ChatbotTextParser.ExtractBudgetConstraint(normalizedMessage) != null)
            {
                return true;
            }

            return ChatbotTextParser.ContainsAny(
                normalizedMessage,
                "toi can",
                "toi muon",
                "can xe",
                "muon xe",
                "goi y xe",
                "tu van xe",
                "tim xe",
                "xe nao hop",
                "xe nao tot",
                "tu van mua xe",
                "goi y mau xe",
                "tim mau xe");
        }

        private static bool IsAttributeOnlySearchMessage(string normalizedMessage)
        {
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return false;
            }

            if (ChatbotTextParser.ContainsAny(
                    normalizedMessage,
                    "toi can",
                    "toi muon",
                    "can xe",
                    "muon xe",
                    "goi y xe",
                    "tu van xe",
                    "tim xe"))
            {
                return false;
            }

            var tokens = ChatbotTextParser.TokenizeNormalizedWords(normalizedMessage);
            if (tokens.Count == 0 || tokens.Count > 4)
            {
                return false;
            }

            return HasSearchTraits(normalizedMessage);
        }

        private static bool HasSearchTraits(string normalizedMessage)
        {
            return !string.IsNullOrWhiteSpace(ExtractBodyType(normalizedMessage))
                   || !string.IsNullOrWhiteSpace(ExtractPurpose(normalizedMessage))
                   || !string.IsNullOrWhiteSpace(ExtractBrand(normalizedMessage))
                   || ChatbotTextParser.ExtractSeatCount(normalizedMessage).HasValue;
        }

        private static bool IsBookingIntent(string normalizedMessage)
        {
            return ChatbotTextParser.ContainsAny(normalizedMessage, "dat lich", "hen xem xe", "lai thu", "dang ky xem xe");
        }

        private static bool IsInstallmentIntent(string normalizedMessage)
        {
            return ChatbotTextParser.ContainsAny(normalizedMessage, "tra gop", "tra truoc", "moi thang", "dat coc");
        }

        private static bool IsPaymentQuestion(string normalizedMessage)
        {
            return ChatbotTextParser.ContainsAny(normalizedMessage, "thanh toan", "chuyen khoan", "qr", "phuong thuc thanh toan");
        }

        private static bool IsPromotionIntent(string normalizedMessage)
        {
            return ChatbotTextParser.ContainsAny(normalizedMessage, "khuyen mai", "uu dai", "giam gia");
        }

        private static bool IsInventoryOverviewIntent(string normalizedMessage)
        {
            return ChatbotTextParser.ContainsAny(
                normalizedMessage,
                "trong kho",
                "co nhung xe nao",
                "showroom co nhung xe nao",
                "co bao nhieu xe",
                "bao nhieu xe",
                "nhung loai xe nao",
                "danh muc xe",
                "ton kho xe");
        }

        private static bool TryExtractSpecificCarReference(string message, string normalizedMessage, out string carReference)
        {
            carReference = string.Empty;

            foreach (var brand in KnownBrandKeywords.OrderByDescending(item => item.Length))
            {
                var brandMatch = System.Text.RegularExpressions.Regex.Match(
                    message,
                    $@"(?i)\b{System.Text.RegularExpressions.Regex.Escape(brand)}(?:\s+[A-Za-z0-9\-\+]+){{0,3}}");

                if (brandMatch.Success)
                {
                    var candidate = brandMatch.Value.Trim(' ', '.', '?', '!', ',', ';', ':');
                    var tokenCount = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
                    if (tokenCount >= 2 || candidate.Any(char.IsDigit))
                    {
                        carReference = candidate;
                        return true;
                    }
                }
            }

            var alphaNumericModelMatch = System.Text.RegularExpressions.Regex.Match(
                message,
                @"\b[A-Za-z]{1,5}\-?\d+[A-Za-z0-9\-]*\b");

            if (alphaNumericModelMatch.Success)
            {
                carReference = alphaNumericModelMatch.Value.Trim(' ', '.', '?', '!', ',', ';', ':');
                return true;
            }

            if (ChatbotTextParser.ContainsAny(normalizedMessage, "mau", "xe") &&
                normalizedMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 6)
            {
                carReference = message.Trim(' ', '.', '?', '!', ',', ';', ':');
                return carReference.Length > 0;
            }

            return false;
        }

        private ChatbotReply BuildWelcomeReply(ChatbotConversationState? state = null)
        {
            return new ChatbotReply
            {
                Message = "Em là AI tư vấn showroom. Em có thể gợi ý xe theo ngân sách, so sánh mẫu đang có, giải thích phương thức thanh toán hoặc hỗ trợ đặt lịch xem xe ngay trong chat.",
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Xe gia đình dưới 1 tỷ", Message = "Gợi ý xe gia đình dưới 1 tỷ" },
                    new ChatbotQuickReply { Label = "SUV đi phố", Message = "Gợi ý SUV đi phố" },
                    new ChatbotQuickReply { Label = "Đặt lịch xem xe", Message = "Tôi muốn đặt lịch xem xe" }
                ],
                State = state
            };
        }

        private static string BuildRecommendationMessage(
            IReadOnlyList<ChatbotCarMatch> matches,
            ChatbotSearchProfile profile)
        {
            var summaryParts = new List<string>();
            var budgetSummary = BuildBudgetSummary(profile);
            if (!string.IsNullOrWhiteSpace(budgetSummary))
            {
                summaryParts.Add(budgetSummary);
            }

            if (!string.IsNullOrWhiteSpace(profile.Purpose))
            {
                summaryParts.Add($"nhu cầu {profile.Purpose}");
            }

            if (!string.IsNullOrWhiteSpace(profile.Brand))
            {
                summaryParts.Add($"hãng {profile.Brand}");
            }

            if (!string.IsNullOrWhiteSpace(profile.BodyType))
            {
                summaryParts.Add($"kiểu xe {profile.BodyType}");
            }

            if (profile.SeatCount.HasValue)
            {
                summaryParts.Add($"{profile.SeatCount.Value} chỗ");
            }

            var topMatch = matches[0].Car;
            var balancedMatch = matches.Count > 1 ? matches[1].Car : topMatch;
            var valueMatch = matches.OrderBy(match => match.Car.Price).First().Car;

            var builder = new StringBuilder();
            builder.Append("Em tóm tắt nhu cầu: ");
            builder.Append(summaryParts.Count > 0 ? string.Join(", ", summaryParts) : "đang tìm xe phù hợp trong showroom");
            builder.AppendLine(".");
            builder.AppendLine();
            builder.AppendLine($"- Phương án tối ưu nhất: {topMatch.CarName}.");
            builder.AppendLine($"- Phương án cân bằng nhất: {balancedMatch.CarName}.");
            builder.AppendLine($"- Phương án tiết kiệm nhất: {valueMatch.CarName}.");
            builder.AppendLine();
            builder.Append($"Kết luận: nếu anh/chị muốn chốt nhanh, em nghiêng nhiều nhất về {topMatch.CarName} vì đang khớp nhu cầu rõ nhất trong dữ liệu hiện có. ");
            builder.Append("Bước tiếp theo: anh/chị có thể mở chi tiết, so sánh 2 mẫu đầu hoặc đặt lịch xem xe cho mẫu đang ưng ý.");

            return builder.ToString().Trim();
        }

        private static string? BuildBudgetSummary(ChatbotSearchProfile profile)
        {
            if (profile.MinBudget.HasValue && profile.MaxBudget.HasValue)
            {
                return $"tầm giá từ {profile.MinBudget.Value:N0} đến {profile.MaxBudget.Value:N0} VNĐ";
            }

            if (profile.MinBudget.HasValue)
            {
                return $"mức giá từ {profile.MinBudget.Value:N0} VNĐ trở lên";
            }

            if (profile.MaxBudget.HasValue)
            {
                return $"ngân sách dưới {profile.MaxBudget.Value:N0} VNĐ";
            }

            return profile.Budget.HasValue
                ? $"ngân sách khoảng {profile.Budget.Value:N0} VNĐ"
                : null;
        }

        private static string BuildSuggestionReason(ChatbotCarMatch match)
        {
            var strengths = string.Join(", ", match.MatchedReasons.Take(2));
            var considerations = string.Join(", ", match.Considerations.Take(1));
            var suitableAudience = match.Car.BodyType switch
            {
                "SUV" => "đi gia đình, đi tỉnh và ưu tiên gầm cao",
                "MPV" => "gia đình đông người hoặc chạy dịch vụ",
                "Sedan" => "đi phố, công việc và ưu tiên sự cân bằng",
                "Hatchback" => "di chuyển đô thị thường xuyên",
                "Bán tải" => "đi đường xấu hoặc cần chở thêm đồ",
                _ => "người muốn xem thêm chi tiết thực tế"
            };

            return $"Điểm mạnh: {strengths}. Cần cân nhắc: {considerations}. Phù hợp với: {suitableAudience}.";
        }

        private static string BuildComparisonPrompt(IReadOnlyList<ChatbotCarMatch> matches)
        {
            return matches.Count >= 2
                ? $"So sánh {matches[0].Car.CarName} và {matches[1].Car.CarName}"
                : "So sánh 2 mẫu xe này";
        }

        private static string BuildComparisonMessage(Car firstCar, Car secondCar)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Em đang so sánh {firstCar.CarName} và {secondCar.CarName} theo dữ liệu showroom hiện có.");
            builder.AppendLine();
            builder.AppendLine($"- Giá bán: {firstCar.CarName} ở mức {firstCar.Price:N0} VNĐ, còn {secondCar.CarName} ở mức {secondCar.Price:N0} VNĐ.");
            builder.AppendLine($"- Kiểu xe và trải nghiệm: {firstCar.CarName} thuộc nhóm {firstCar.BodyType}, còn {secondCar.CarName} thuộc nhóm {secondCar.BodyType}.");
            builder.AppendLine($"- Đời xe: {firstCar.CarName} đời {firstCar.Year}, {secondCar.CarName} đời {secondCar.Year}.");
            builder.AppendLine($"- Tình trạng hiển thị: {firstCar.Status} và {secondCar.Status}.");
            builder.AppendLine();
            builder.AppendLine($"Kết luận: nếu anh/chị ưu tiên giá dễ tiếp cận hơn thì nghiêng về {(firstCar.Price <= secondCar.Price ? firstCar.CarName : secondCar.CarName)}.");
            builder.AppendLine($"Nếu ưu tiên kiểu xe thực dụng hơn cho nhu cầu hiện tại, anh/chị nên nghiêng về {(IsMorePractical(firstCar, secondCar) ? firstCar.CarName : secondCar.CarName)}.");
            builder.Append("Bước tiếp theo: anh/chị nên mở chi tiết 2 mẫu để xem ảnh, mô tả và chọn mẫu phù hợp hơn để mua hoặc đặt lịch xem xe.");

            return builder.ToString().Trim();
        }

        private static bool IsMorePractical(Car firstCar, Car secondCar)
        {
            var firstScore = firstCar.BodyType is "SUV" or "MPV" or "Crossover" ? 2 : 1;
            var secondScore = secondCar.BodyType is "SUV" or "MPV" or "Crossover" ? 2 : 1;
            return firstScore >= secondScore;
        }

        private static string BuildCarDetailMessage(Car car)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Nếu anh/chị đang quan tâm {car.CarName}, đây là các ý chính từ dữ liệu showroom:");
            builder.AppendLine($"- Phân khúc/kiểu xe: {car.BodyType}.");
            builder.AppendLine($"- Giá tham khảo: {car.Price:N0} VNĐ.");
            builder.AppendLine($"- Tình trạng: {car.Status}.");
            builder.AppendLine($"- Điểm nổi bật: {TakeFirstSentence(car.Description)}");
            builder.AppendLine($"- Thông tin thực tế: {TakeFirstSentence(car.Specifications)}");
            builder.AppendLine();
            builder.Append("Kết luận: đây là mẫu đáng xem nếu anh/chị đang cần một lựa chọn đúng nhóm xe hiện tại của nó. Bước tiếp theo: anh/chị có thể xem chi tiết sâu hơn, mua xe hoặc đặt lịch xem xe.");

            return builder.ToString().Trim();
        }

        private static string BuildInstallmentMessage(ChatbotInstallmentEstimate estimate)
        {
            var upfrontTotal = estimate.DownPayment + estimate.RegistrationEstimate + estimate.InsuranceEstimate;
            var builder = new StringBuilder();
            builder.AppendLine($"Em đang tạm tính trả góp cho {estimate.Car.CarName} theo dữ liệu hiện có.");
            builder.AppendLine();
            builder.AppendLine($"- Giá xe: {estimate.CarPrice:N0} VNĐ.");
            builder.AppendLine($"- Trả trước: {estimate.DownPayment:N0} VNĐ.");
            builder.AppendLine($"- Khoản vay dự kiến: {estimate.LoanAmount:N0} VNĐ.");
            builder.AppendLine($"- Kỳ hạn tạm tính: {estimate.TermMonths} tháng.");
            builder.AppendLine($"- Lãi suất tham khảo: {estimate.AnnualInterestRate:N2}%/năm.");
            builder.AppendLine($"- Góp mỗi tháng khoảng: {estimate.MonthlyPayment:N0} VNĐ.");
            builder.AppendLine();
            builder.AppendLine("Các khoản thường gặp khi xuống tiền ban đầu:");
            builder.AppendLine($"- Phí đăng ký/ra biển tạm tính: {estimate.RegistrationEstimate:N0} VNĐ.");
            builder.AppendLine($"- Bảo hiểm năm đầu tạm tính: {estimate.InsuranceEstimate:N0} VNĐ.");
            builder.AppendLine($"- Tổng cần chuẩn bị ban đầu khoảng: {upfrontTotal:N0} VNĐ.");
            builder.AppendLine();
            builder.Append($"Chi phí nuôi xe hằng tháng chỉ là tham khảo: nhiên liệu khoảng {estimate.MonthlyFuelEstimate:N0} VNĐ và bảo dưỡng bình quân khoảng {estimate.MonthlyMaintenanceEstimate:N0} VNĐ. ");
            builder.Append("Kết luận: đây là phương án ước tính để anh/chị hình dung dòng tiền, chưa phải báo giá tài chính chính thức của ngân hàng hay showroom.");

            return builder.ToString().Trim();
        }

                private static string BuildBookingSuccessMessage(Car car, Booking booking, string? customerMessage, DateTime? suggestedAppointmentAt)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Em đã ghi nhận yêu cầu {booking.ServiceType.ToLowerInvariant()} cho {car.CarName}.");
            builder.AppendLine();
            builder.AppendLine($"- Mã yêu cầu: {booking.BookingCode}");
            builder.AppendLine($"- Dịch vụ: {booking.ServiceType}");
            builder.AppendLine($"- Ngày giờ anh/chị chọn: {booking.AppointmentAt:dd/MM/yyyy HH:mm}");
            builder.AppendLine($"- Trạng thái hiện tại: {booking.BookingStatus}");
            builder.AppendLine($"- Khách liên hệ: {booking.CustomerName} - {booking.PhoneNumber}");

            if (suggestedAppointmentAt.HasValue)
            {
                builder.AppendLine($"- Khung giờ gợi ý thêm: {suggestedAppointmentAt.Value:dd/MM/yyyy HH:mm}");
            }

            builder.AppendLine();
            builder.Append(string.IsNullOrWhiteSpace(customerMessage)
                ? BookingWorkflow.BuildCustomerConfirmationMessage(booking)
                : customerMessage.Trim());
            builder.Append(" Hiện tại lịch này chưa được chốt chính thức. Sau khi Admin xác nhận, showroom sẽ thông báo lại cho anh/chị.");

            return builder.ToString().Trim();
        }
        private static string TakeFirstSentence(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "Hiện hệ thống chưa có thêm mô tả chi tiết cho mục này.";
            }

            var parts = content.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length == 0 ? content.Trim() : $"{parts[0].Trim()}.";
        }

        private static ChatbotSuggestion BuildSuggestion(Car car, string reason)
        {
            return new ChatbotSuggestion
            {
                CarId = car.CarID,
                CarName = car.CarName,
                Brand = car.Brand,
                ModelName = car.ModelName,
                BodyType = car.BodyType,
                Image = car.Image,
                Price = car.Price,
                Status = car.Status,
                Reason = reason,
                CanOrder = OrderWorkflow.CanOrder(car)
            };
        }

        private static IReadOnlyList<ChatbotAction> BuildRecommendationActions(IReadOnlyList<ChatbotCarMatch> matches)
        {
            var actions = new List<ChatbotAction>
            {
                CreateLinkAction("Xem danh sách xe", "/Cars/Index")
            };

            if (matches.Count > 0)
            {
                actions.Add(CreateLinkAction("Xem chi tiết xe đầu", $"/Cars/Details/{matches[0].Car.CarID}", "primary"));
            }

            return actions;
        }

        private static List<ChatbotAction> BuildCarActions(Car car)
        {
            return
            [
                CreateLinkAction("Xem chi tiết", $"/Cars/Details/{car.CarID}"),
                CreateLinkAction("Mua xe", $"/Orders/Checkout?carId={car.CarID}", "primary"),
                CreateLinkAction("Đặt lịch xem xe", $"/Bookings/Create?carId={car.CarID}")
            ];
        }

        private static ChatbotAction CreateLinkAction(string label, string url, string variant = "secondary")
        {
            return new ChatbotAction
            {
                Label = label,
                Url = url,
                Kind = "link",
                Variant = variant
            };
        }

        private static ChatbotAction CreateMessageAction(string label, string message, string variant = "secondary")
        {
            return new ChatbotAction
            {
                Label = label,
                Message = message,
                Kind = "message",
                Variant = variant
            };
        }
    }
}

