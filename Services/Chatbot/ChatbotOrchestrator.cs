using System.Text;
using AutoCarShowroom.Models;

namespace AutoCarShowroom.Services.Chatbot
{
    public sealed partial class ChatbotOrchestrator
    {
        private static readonly string[] BodyTypes = ["SUV", "Sedan", "Hatchback", "MPV", "Crossover", "BÃ¡n táº£i", "Coupe", "Mui tráº§n"];
        private static readonly string[] PurposeKeywords = ["gia dinh", "ca nhan", "di pho", "dich vu", "duong dai", "doanh nhan", "mua lan dau", "offroad"];
        private static readonly string[] KnownBrandKeywords =
        [
            "Toyota", "Hyundai", "Kia", "Ford", "Mazda", "Honda", "Mitsubishi", "VinFast",
            "Ferrari", "Lamborghini", "BMW", "Mercedes", "Mercedes-Benz", "Audi", "Lexus",
            "Peugeot", "Nissan", "Suzuki", "Subaru", "Porsche", "Isuzu", "MG"
        ];
        private static readonly string[] SearchClarificationReplies =
        [
            "NgÃ¢n sÃ¡ch dÆ°á»›i 1 tá»·",
            "Xe gia Ä‘Ã¬nh 5 chá»—",
            "SUV Ä‘i phá»‘",
            "Xe cháº¡y dá»‹ch vá»¥"
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
                Message = "Em Ä‘Ã£ lÃ m má»›i phiÃªn tÆ° váº¥n. Anh/chá»‹ cá»© nháº¯n nhu cáº§u má»›i nhÆ° ngÃ¢n sÃ¡ch, kiá»ƒu xe, máº«u muá»‘n so sÃ¡nh hoáº·c nhu cáº§u Ä‘áº·t lá»‹ch xem xe nhÃ©.",
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
                    Message = "Em cÃ³ thá»ƒ tÃ­nh tráº£ gÃ³p tham kháº£o, nhÆ°ng cáº§n biáº¿t anh/chá»‹ muá»‘n tÃ­nh cho máº«u xe nÃ o trÆ°á»›c nhÃ©.",
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
                    Message = "Em Ä‘Ã£ dá»«ng pháº§n tÃ­nh tráº£ gÃ³p. Khi cáº§n tÃ­nh láº¡i, anh/chá»‹ chá»‰ cáº§n nháº¯n sá»‘ tiá»n tráº£ trÆ°á»›c hoáº·c tÃªn máº«u xe nhÃ©.",
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
                        Message = "Em váº«n chÆ°a nháº­n ra máº«u xe cáº§n tÃ­nh tráº£ gÃ³p. Anh/chá»‹ nháº¯n rÃµ tÃªn xe giÃºp em nhÃ©.",
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
                    Message = "Em cáº§n xÃ¡c Ä‘á»‹nh láº¡i máº«u xe trÆ°á»›c khi tÃ­nh tráº£ gÃ³p. Anh/chá»‹ nháº¯n tÃªn xe giÃºp em nhÃ©.",
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
                        Message = "Em chÆ°a Ä‘á»c Ä‘Æ°á»£c sá»‘ tiá»n tráº£ trÆ°á»›c. Anh/chá»‹ nháº¯n giÃºp em theo dáº¡ng nhÆ° `300 triá»‡u` hoáº·c `500tr` nhÃ©.",
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
                    Message = $"Em Ä‘ang tÃ­nh tráº£ gÃ³p tham kháº£o cho `{car.CarName}`. Anh/chá»‹ dá»± kiáº¿n tráº£ trÆ°á»›c khoáº£ng bao nhiÃªu Ä‘á»ƒ em tÃ­nh gáº§n Ä‘Ãºng nháº¥t?",
                    RequiresInput = true,
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "300 triá»‡u", Message = "Tráº£ trÆ°á»›c 300 triá»‡u" },
                        new ChatbotQuickReply { Label = "30% giÃ¡ xe", Message = "Tráº£ trÆ°á»›c khoáº£ng 30%" }
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
                    Message = "Hiá»‡n em chÆ°a tÃ­nh Ä‘Æ°á»£c phÆ°Æ¡ng Ã¡n tráº£ gÃ³p chÃ­nh xÃ¡c cho máº«u xe nÃ y trong há»‡ thá»‘ng. Anh/chá»‹ thá»­ láº¡i sau hoáº·c Ä‘á»ƒ showroom há»— trá»£ trá»±c tiáº¿p nhÃ©.",
                    State = state
                };
            }

            return new ChatbotReply
            {
                Message = BuildInstallmentMessage(estimate),
                Actions =
                [
                    CreateLinkAction("Mua xe", $"/Orders/Checkout?carId={car.CarID}", "primary"),
                    CreateLinkAction("Äáº·t lá»‹ch xem xe", $"/Bookings/Create?carId={car.CarID}", "secondary")
                ],
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "TÃ­nh láº¡i 84 thÃ¡ng", Message = $"TÃ­nh tráº£ gÃ³p {car.CarName} trong 84 thÃ¡ng" },
                    new ChatbotQuickReply { Label = "Giáº£i thÃ­ch thanh toÃ¡n", Message = "Showroom cÃ³ nhá»¯ng phÆ°Æ¡ng thá»©c thanh toÃ¡n nÃ o?" }
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
                    Message = "Äá»ƒ em so sÃ¡nh chÃ­nh xÃ¡c theo dá»¯ liá»‡u showroom, anh/chá»‹ nháº¯n rÃµ 2 máº«u xe Ä‘ang phÃ¢n vÃ¢n nhÃ©. VÃ­ dá»¥: `So sÃ¡nh Toyota Camry vÃ  BMW X5`.",
                    RequiresInput = true,
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "So sÃ¡nh Camry vÃ  C300", Message = "So sÃ¡nh Toyota Camry vÃ  Mercedes-Benz C300 AMG" },
                        new ChatbotQuickReply { Label = "So sÃ¡nh Santa Fe vÃ  Fortuner", Message = "So sÃ¡nh Hyundai Santa Fe vÃ  Toyota Fortuner" }
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
                    BuildSuggestion(firstCar, "PhÃ¹ há»£p Ä‘á»ƒ Ä‘á»‘i chiáº¿u trá»±c tiáº¿p vá» giÃ¡, kiá»ƒu xe vÃ  phong cÃ¡ch sá»­ dá»¥ng."),
                    BuildSuggestion(secondCar, "NÃªn xem cÃ¹ng lÃºc Ä‘á»ƒ chá»‘t nhanh hÆ¡n theo nhu cáº§u tháº­t cá»§a anh/chá»‹.")
                ],
                Actions =
                [
                    CreateLinkAction("Xem xe 1", $"/Cars/Details/{firstCar.CarID}"),
                    CreateLinkAction("Xem xe 2", $"/Cars/Details/{secondCar.CarID}"),
                    CreateLinkAction("Mua xe 1", $"/Orders/Checkout?carId={firstCar.CarID}", "primary")
                ],
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Gá»£i Ã½ xe tiáº¿t kiá»‡m hÆ¡n", Message = "Gá»£i Ã½ xe tiáº¿t kiá»‡m hÆ¡n" },
                    new ChatbotQuickReply { Label = "Gá»£i Ã½ xe gia Ä‘Ã¬nh", Message = "Gá»£i Ã½ xe gia Ä‘Ã¬nh 5 ngÆ°á»i" }
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
                Suggestions = [BuildSuggestion(car, "ÄÃ¢y lÃ  máº«u xe anh/chá»‹ Ä‘ang xem hoáº·c vá»«a nháº¯c tá»›i trong há»™i thoáº¡i.")],
                Actions = BuildCarActions(car),
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Xem thanh toÃ¡n", Message = "Showroom cÃ³ nhá»¯ng phÆ°Æ¡ng thá»©c thanh toÃ¡n nÃ o?" },
                    new ChatbotQuickReply { Label = "Äáº·t lá»‹ch xem xe", Message = $"Äáº·t lá»‹ch xem xe {car.CarName}" }
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
                    Message = "Hiá»‡n táº¡i há»‡ thá»‘ng chÆ°a ghi nháº­n xe nÃ o Ä‘ang má»Ÿ bÃ¡n trong kho showroom. Anh/chá»‹ cÃ³ thá»ƒ quay láº¡i sau hoáº·c liÃªn há»‡ nhÃ¢n viÃªn Ä‘á»ƒ kiá»ƒm tra tá»“n kho thá»±c táº¿.",
                    Actions = [CreateLinkAction("Xem danh sÃ¡ch xe", "/Cars/Index", "primary")],
                    State = state
                };
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Hiá»‡n táº¡i showroom Ä‘ang cÃ³ {overview.TotalVisibleCars} xe má»Ÿ bÃ¡n trong kho.");
            builder.AppendLine($"- Táº§m giÃ¡ tham kháº£o: tá»« {overview.MinPrice:N0} VNÄ Ä‘áº¿n {overview.MaxPrice:N0} VNÄ.");
            builder.AppendLine($"- NhÃ³m xe Ä‘ang cÃ³: {string.Join(", ", overview.BodyTypeSummaries)}.");
            builder.AppendLine($"- HÃ£ng ná»•i báº­t: {string.Join(", ", overview.BrandSummaries)}.");
            builder.AppendLine();
            builder.Append("Káº¿t luáº­n: kho xe hiá»‡n khÃ¡ Ä‘a dáº¡ng tá»« sedan, SUV Ä‘áº¿n MPV vÃ  bÃ¡n táº£i. Náº¿u anh/chá»‹ nÃ³i rÃµ thÃªm ngÃ¢n sÃ¡ch, sá»‘ chá»— hoáº·c kiá»ƒu xe, em sáº½ lá»c ngay cÃ¡c máº«u há»£p nháº¥t.");

            return new ChatbotReply
            {
                Message = builder.ToString().Trim(),
                Suggestions = overview.FeaturedCars
                    .Select(car => BuildSuggestion(car, "Äang má»Ÿ bÃ¡n trong kho showroom vÃ  phÃ¹ há»£p Ä‘á»ƒ xem nhanh trÆ°á»›c khi lá»c sÃ¢u hÆ¡n."))
                    .ToList(),
                Actions = [CreateLinkAction("Xem danh sÃ¡ch xe", "/Cars/Index", "primary")],
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "7 chá»— dÆ°á»›i 900 triá»‡u", Message = "TÃ´i cáº§n xe gia Ä‘Ã¬nh 7 chá»— dÆ°á»›i 900 triá»‡u" },
                    new ChatbotQuickReply { Label = "Sedan Ä‘i lÃ m háº±ng ngÃ y", Message = "TÃ´i muá»‘n xe 5 chá»—, tiáº¿t kiá»‡m xÄƒng, Ä‘i lÃ m háº±ng ngÃ y, kiá»ƒu Sedan" }
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
                Message = $"Hiá»‡n táº¡i em chÆ°a cÃ³ dá»¯ liá»‡u chÃ­nh xÃ¡c cho `{carReference}` trong há»‡ thá»‘ng showroom, nÃªn em khÃ´ng dÃ¡m kháº³ng Ä‘á»‹nh vá» giÃ¡, khuyáº¿n mÃ£i hay tÃ¬nh tráº¡ng cá»§a máº«u nÃ y.\n\nNáº¿u anh/chá»‹ muá»‘n, em cÃ³ thá»ƒ:\n- gá»£i Ã½ máº«u tÆ°Æ¡ng Ä‘Æ°Æ¡ng Ä‘ang cÃ³ trong showroom\n- lá»c theo táº§m giÃ¡ hoáº·c kiá»ƒu xe gáº§n giá»‘ng\n- hoáº·c anh/chá»‹ gá»­i tÃªn máº«u xe khÃ¡c Ä‘á»ƒ em kiá»ƒm tra tiáº¿p.",
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Gá»£i Ã½ xe tÆ°Æ¡ng Ä‘Æ°Æ¡ng", Message = "Gá»£i Ã½ xe tÆ°Æ¡ng Ä‘Æ°Æ¡ng Ä‘ang cÃ³ trong showroom" },
                    new ChatbotQuickReply { Label = "Xem xe Ä‘ang má»Ÿ bÃ¡n", Message = "Showroom hiá»‡n cÃ³ nhá»¯ng xe nÃ o trong kho?" }
                ],
                Actions = [CreateLinkAction("Xem danh sÃ¡ch xe", "/Cars/Index", "primary")],
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
                    messageBuilder.AppendLine($"Hiá»‡n táº¡i `{promotionCar.CarName}` Ä‘ang á»Ÿ tráº¡ng thÃ¡i khuyáº¿n mÃ£i trong há»‡ thá»‘ng showroom.");
                    messageBuilder.AppendLine($"- GiÃ¡ tham kháº£o: {promotionCar.Price:N0} VNÄ.");
                    messageBuilder.AppendLine($"- Loáº¡i xe: {promotionCar.BodyType}.");
                    messageBuilder.AppendLine($"- NÄƒm sáº£n xuáº¥t: {promotionCar.Year}.");
                    messageBuilder.AppendLine();
                    messageBuilder.Append("Káº¿t luáº­n: Ä‘Ã¢y lÃ  má»™t trong nhá»¯ng máº«u Ä‘ang cÃ³ Æ°u Ä‘Ã£i hiá»ƒn thá»‹ trong kho. Anh/chá»‹ cÃ³ thá»ƒ má»Ÿ chi tiáº¿t Ä‘á»ƒ xem thÃªm mÃ´ táº£, hÃ¬nh áº£nh vÃ  Ä‘áº·t lá»‹ch xem xe.");
                }
                else
                {
                    messageBuilder.AppendLine($"Hiá»‡n táº¡i em chÆ°a tháº¥y `{promotionCar.CarName}` á»Ÿ tráº¡ng thÃ¡i khuyáº¿n mÃ£i trong há»‡ thá»‘ng.");
                    messageBuilder.AppendLine($"- Tráº¡ng thÃ¡i hiá»‡n táº¡i: {promotionCar.Status}.");
                    messageBuilder.AppendLine($"- GiÃ¡ tham kháº£o: {promotionCar.Price:N0} VNÄ.");
                    messageBuilder.AppendLine();
                    messageBuilder.Append("Káº¿t luáº­n: máº«u nÃ y váº«n Ä‘ang cÃ³ dá»¯ liá»‡u trong showroom nhÆ°ng chÆ°a cÃ³ cá» khuyáº¿n mÃ£i á»Ÿ thá»i Ä‘iá»ƒm hiá»‡n táº¡i.");
                }

                return new ChatbotReply
                {
                    Message = messageBuilder.ToString().Trim(),
                    Suggestions = [BuildSuggestion(promotionCar, "Máº«u xe anh/chá»‹ vá»«a há»i vá» tráº¡ng thÃ¡i khuyáº¿n mÃ£i.")],
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
                    Message = "Hiá»‡n táº¡i em chÆ°a tháº¥y máº«u xe nÃ o Ä‘ang á»Ÿ tráº¡ng thÃ¡i khuyáº¿n mÃ£i trong há»‡ thá»‘ng showroom. Anh/chá»‹ muá»‘n em gá»£i Ã½ theo ngÃ¢n sÃ¡ch hoáº·c loáº¡i xe Ä‘ang cÃ²n hÃ ng khÃ´ng?",
                    QuickReplies = SearchClarificationReplies
                        .Select(value => new ChatbotQuickReply { Label = value, Message = value })
                        .ToList(),
                    State = state
                };
            }

            return new ChatbotReply
            {
                Message = "Em Ä‘Ã£ lá»c nhanh cÃ¡c máº«u Ä‘ang á»Ÿ tráº¡ng thÃ¡i khuyáº¿n mÃ£i trong showroom. Anh/chá»‹ nÃªn xem trÆ°á»›c cÃ¡c máº«u dÆ°á»›i Ä‘Ã¢y Ä‘á»ƒ tiá»‡n so giÃ¡ vÃ  Ä‘áº·t lá»‹ch.",
                Suggestions = promotionCars
                    .Select(car => BuildSuggestion(car, "Äang cÃ³ tráº¡ng thÃ¡i khuyáº¿n mÃ£i trong há»‡ thá»‘ng showroom."))
                    .ToList(),
                Actions = [CreateLinkAction("Xem toÃ n bá»™ xe", "/Cars/Index", "primary")],
                State = state
            };
        }

        private ChatbotReply BuildPaymentMethodReply(ChatbotConversationState state)
        {
            var methods = _financeTools.GetPaymentMethods();
            var builder = new StringBuilder();
            builder.AppendLine("Hiá»‡n showroom Ä‘ang há»— trá»£ cÃ¡c phÆ°Æ¡ng thá»©c thanh toÃ¡n sau:");

            foreach (var method in methods)
            {
                builder.AppendLine($"- {method.ShortLabel}: {method.Description}");
            }

            builder.AppendLine();
            builder.Append("Káº¿t luáº­n: náº¿u anh/chá»‹ muá»‘n chá»‘t nhanh thÃ¬ nghiÃªng vá» QR hoáº·c chuyá»ƒn khoáº£n; náº¿u muá»‘n xem xe ká»¹ trÆ°á»›c khi xuá»‘ng tiá»n thÃ¬ thanh toÃ¡n táº¡i showroom sáº½ linh hoáº¡t hÆ¡n.");
            builder.AppendLine();
            builder.Append("BÆ°á»›c tiáº¿p theo: náº¿u anh/chá»‹ Ä‘ang nháº¯m má»™t máº«u cá»¥ thá»ƒ, em cÃ³ thá»ƒ dáº«n tháº³ng sang trang mua xe hoáº·c Ä‘áº·t lá»‹ch xem xe.");

            var actions = new List<ChatbotAction>();
            if (state.CurrentCarId.HasValue)
            {
                actions.Add(CreateLinkAction("Mua xe", $"/Orders/Checkout?carId={state.CurrentCarId.Value}", "primary"));
                actions.Add(CreateLinkAction("Äáº·t lá»‹ch xem xe", $"/Bookings/Create?carId={state.CurrentCarId.Value}"));
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
                Message = $"{article.Title}\n\n{article.Answer}\n\n{article.Disclaimer}\n\nBÆ°á»›c tiáº¿p theo: {article.NextStep}",
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "TÆ° váº¥n theo ngÃ¢n sÃ¡ch", Message = "Gá»£i Ã½ xe theo ngÃ¢n sÃ¡ch" },
                    new ChatbotQuickReply { Label = "Äáº·t lá»‹ch xem xe", Message = "TÃ´i muá»‘n Ä‘áº·t lá»‹ch xem xe" }
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
                state.PendingField = "search_context";
                state.ClarifyingQuestionsAsked++;

                return new ChatbotReply
                {
                    Message = "Äá»ƒ em gá»£i Ã½ Ä‘Ãºng hÆ¡n, anh/chá»‹ cho em 3 Ã½ ngáº¯n thÃ´i nhÃ©:\n- NgÃ¢n sÃ¡ch khoáº£ng bao nhiÃªu?\n- Mua Ä‘i gia Ä‘Ã¬nh, cÃ¡ nhÃ¢n hay dá»‹ch vá»¥?\n- Anh/chá»‹ nghiÃªng vá» sedan, SUV hay 7 chá»—?",
                    RequiresInput = true,
                    QuickReplies = SearchClarificationReplies
                        .Select(value => new ChatbotQuickReply { Label = value, Message = value })
                        .ToList(),
                    State = state
                };
            }

            state.ActiveMode = null;
            state.PendingField = null;

            var searchCriteria = new ChatbotSearchCriteria
            {
                MaxBudget = state.SearchProfile.Budget,
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
                    Message = "Hiá»‡n táº¡i em chÆ°a tÃ¬m tháº¥y máº«u khá»›p hoÃ n toÃ n trong showroom. Náº¿u anh/chá»‹ muá»‘n, em cÃ³ thá»ƒ ná»›i ngÃ¢n sÃ¡ch, Ä‘á»•i loáº¡i xe hoáº·c gá»£i Ã½ máº«u gáº§n nháº¥t Ä‘ang cÃ²n hÃ ng.",
                    QuickReplies =
                    [
                        new ChatbotQuickReply { Label = "Ná»›i lÃªn 1,2 tá»·", Message = "Gá»£i Ã½ xe dÆ°á»›i 1.2 tá»·" },
                        new ChatbotQuickReply { Label = "Xem SUV Ä‘ang cÃ³", Message = "Gá»£i Ã½ SUV Ä‘ang cÃ³" }
                    ],
                    Actions = [CreateLinkAction("Xem toÃ n bá»™ xe", "/Cars/Index", "primary")],
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
                    new ChatbotQuickReply { Label = "So sÃ¡nh 2 xe Ä‘áº§u", Message = BuildComparisonPrompt(matches) },
                    new ChatbotQuickReply { Label = "Äáº·t lá»‹ch xe Ä‘áº§u", Message = $"Äáº·t lá»‹ch xem xe {topMatch.Car.CarName}" }
                ],
                State = state
            };
        }

        private static bool NeedMoreSearchContext(string normalizedMessage, ChatbotSearchProfile profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.BodyType) || !string.IsNullOrWhiteSpace(profile.Purpose) || profile.Budget.HasValue || profile.SeatCount.HasValue)
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
            var budget = ChatbotTextParser.ExtractMoneyValue(message);
            if (budget.HasValue)
            {
                profile.Budget = budget;
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

            return LooksLikeFreshSearchRequest(normalizedMessage) || IsInventoryOverviewIntent(normalizedMessage);
        }

        private static bool HasSearchProfile(ChatbotSearchProfile profile)
        {
            return profile.Budget.HasValue
                   || profile.SeatCount.HasValue
                   || !string.IsNullOrWhiteSpace(profile.BodyType)
                   || !string.IsNullOrWhiteSpace(profile.Purpose)
                   || !string.IsNullOrWhiteSpace(profile.Brand)
                   || !string.IsNullOrWhiteSpace(profile.RawKeywords);
        }

        private static bool LooksLikeFreshSearchRequest(string normalizedMessage)
        {
            return ChatbotTextParser.ContainsAny(
                normalizedMessage,
                "toi can",
                "toi muon",
                "can xe",
                "muon xe",
                "goi y xe",
                "tu van xe",
                "tim xe",
                "7 cho",
                "5 cho",
                "sedan",
                "suv",
                "mpv",
                "crossover",
                "hatchback",
                "ban tai",
                "gia dinh",
                "di lam",
                "hang ngay",
                "di pho",
                "tiet kiem");
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
                Message = "Em lÃ  AI tÆ° váº¥n showroom. Em cÃ³ thá»ƒ gá»£i Ã½ xe theo ngÃ¢n sÃ¡ch, so sÃ¡nh máº«u Ä‘ang cÃ³, giáº£i thÃ­ch phÆ°Æ¡ng thá»©c thanh toÃ¡n hoáº·c há»— trá»£ Ä‘áº·t lá»‹ch xem xe ngay trong chat.",
                QuickReplies =
                [
                    new ChatbotQuickReply { Label = "Xe gia Ä‘Ã¬nh dÆ°á»›i 1 tá»·", Message = "Gá»£i Ã½ xe gia Ä‘Ã¬nh dÆ°á»›i 1 tá»·" },
                    new ChatbotQuickReply { Label = "SUV Ä‘i phá»‘", Message = "Gá»£i Ã½ SUV Ä‘i phá»‘" },
                    new ChatbotQuickReply { Label = "Äáº·t lá»‹ch xem xe", Message = "TÃ´i muá»‘n Ä‘áº·t lá»‹ch xem xe" }
                ],
                State = state
            };
        }

        private static string BuildRecommendationMessage(
            IReadOnlyList<ChatbotCarMatch> matches,
            ChatbotSearchProfile profile)
        {
            var summaryParts = new List<string>();
            if (profile.Budget.HasValue)
            {
                summaryParts.Add($"ngÃ¢n sÃ¡ch khoáº£ng {profile.Budget.Value:N0} VNÄ");
            }

            if (!string.IsNullOrWhiteSpace(profile.Purpose))
            {
                summaryParts.Add($"nhu cáº§u {profile.Purpose}");
            }

            if (!string.IsNullOrWhiteSpace(profile.BodyType))
            {
                summaryParts.Add($"kiá»ƒu xe {profile.BodyType}");
            }

            if (profile.SeatCount.HasValue)
            {
                summaryParts.Add($"{profile.SeatCount.Value} chá»—");
            }

            var topMatch = matches[0].Car;
            var balancedMatch = matches.Count > 1 ? matches[1].Car : topMatch;
            var valueMatch = matches.OrderBy(match => match.Car.Price).First().Car;

            var builder = new StringBuilder();
            builder.Append("Em tÃ³m táº¯t nhu cáº§u: ");
            builder.Append(summaryParts.Count > 0 ? string.Join(", ", summaryParts) : "Ä‘ang tÃ¬m xe phÃ¹ há»£p trong showroom");
            builder.AppendLine(".");
            builder.AppendLine();
            builder.AppendLine($"- PhÆ°Æ¡ng Ã¡n tá»‘i Æ°u nháº¥t: {topMatch.CarName}.");
            builder.AppendLine($"- PhÆ°Æ¡ng Ã¡n cÃ¢n báº±ng nháº¥t: {balancedMatch.CarName}.");
            builder.AppendLine($"- PhÆ°Æ¡ng Ã¡n tiáº¿t kiá»‡m nháº¥t: {valueMatch.CarName}.");
            builder.AppendLine();
            builder.Append($"Káº¿t luáº­n: náº¿u anh/chá»‹ muá»‘n chá»‘t nhanh, em nghiÃªng nhiá»u nháº¥t vá» {topMatch.CarName} vÃ¬ Ä‘ang khá»›p nhu cáº§u rÃµ nháº¥t trong dá»¯ liá»‡u hiá»‡n cÃ³. ");
            builder.Append("BÆ°á»›c tiáº¿p theo: anh/chá»‹ cÃ³ thá»ƒ má»Ÿ chi tiáº¿t, so sÃ¡nh 2 máº«u Ä‘áº§u hoáº·c Ä‘áº·t lá»‹ch xem xe cho máº«u Ä‘ang Æ°ng Ã½.");

            return builder.ToString().Trim();
        }

        private static string BuildSuggestionReason(ChatbotCarMatch match)
        {
            var strengths = string.Join(", ", match.MatchedReasons.Take(2));
            var considerations = string.Join(", ", match.Considerations.Take(1));
            var suitableAudience = match.Car.BodyType switch
            {
                "SUV" => "Ä‘i gia Ä‘Ã¬nh, Ä‘i tá»‰nh vÃ  Æ°u tiÃªn gáº§m cao",
                "MPV" => "gia Ä‘Ã¬nh Ä‘Ã´ng ngÆ°á»i hoáº·c cháº¡y dá»‹ch vá»¥",
                "Sedan" => "Ä‘i phá»‘, cÃ´ng viá»‡c vÃ  Æ°u tiÃªn sá»± cÃ¢n báº±ng",
                "Hatchback" => "di chuyá»ƒn Ä‘Ã´ thá»‹ thÆ°á»ng xuyÃªn",
                "BÃ¡n táº£i" => "Ä‘i Ä‘Æ°á»ng xáº¥u hoáº·c cáº§n chá»Ÿ thÃªm Ä‘á»“",
                _ => "ngÆ°á»i muá»‘n xem thÃªm chi tiáº¿t thá»±c táº¿"
            };

            return $"Äiá»ƒm máº¡nh: {strengths}. Cáº§n cÃ¢n nháº¯c: {considerations}. PhÃ¹ há»£p vá»›i: {suitableAudience}.";
        }

        private static string BuildComparisonPrompt(IReadOnlyList<ChatbotCarMatch> matches)
        {
            return matches.Count >= 2
                ? $"So sÃ¡nh {matches[0].Car.CarName} vÃ  {matches[1].Car.CarName}"
                : "So sÃ¡nh 2 máº«u xe nÃ y";
        }

        private static string BuildComparisonMessage(Car firstCar, Car secondCar)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Em Ä‘ang so sÃ¡nh {firstCar.CarName} vÃ  {secondCar.CarName} theo dá»¯ liá»‡u showroom hiá»‡n cÃ³.");
            builder.AppendLine();
            builder.AppendLine($"- GiÃ¡ bÃ¡n: {firstCar.CarName} á»Ÿ má»©c {firstCar.Price:N0} VNÄ, cÃ²n {secondCar.CarName} á»Ÿ má»©c {secondCar.Price:N0} VNÄ.");
            builder.AppendLine($"- Kiá»ƒu xe vÃ  tráº£i nghiá»‡m: {firstCar.CarName} thuá»™c nhÃ³m {firstCar.BodyType}, cÃ²n {secondCar.CarName} thuá»™c nhÃ³m {secondCar.BodyType}.");
            builder.AppendLine($"- Äá»i xe: {firstCar.CarName} Ä‘á»i {firstCar.Year}, {secondCar.CarName} Ä‘á»i {secondCar.Year}.");
            builder.AppendLine($"- TÃ¬nh tráº¡ng hiá»ƒn thá»‹: {firstCar.Status} vÃ  {secondCar.Status}.");
            builder.AppendLine();
            builder.AppendLine($"Káº¿t luáº­n: náº¿u anh/chá»‹ Æ°u tiÃªn giÃ¡ dá»… tiáº¿p cáº­n hÆ¡n thÃ¬ nghiÃªng vá» {(firstCar.Price <= secondCar.Price ? firstCar.CarName : secondCar.CarName)}.");
            builder.AppendLine($"Náº¿u Æ°u tiÃªn kiá»ƒu xe thá»±c dá»¥ng hÆ¡n cho nhu cáº§u hiá»‡n táº¡i, anh/chá»‹ nÃªn nghiÃªng vá» {(IsMorePractical(firstCar, secondCar) ? firstCar.CarName : secondCar.CarName)}.");
            builder.Append("BÆ°á»›c tiáº¿p theo: anh/chá»‹ nÃªn má»Ÿ chi tiáº¿t 2 máº«u Ä‘á»ƒ xem áº£nh, mÃ´ táº£ vÃ  chá»n máº«u phÃ¹ há»£p hÆ¡n Ä‘á»ƒ mua hoáº·c Ä‘áº·t lá»‹ch xem xe.");

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
            builder.AppendLine($"Náº¿u anh/chá»‹ Ä‘ang quan tÃ¢m {car.CarName}, Ä‘Ã¢y lÃ  cÃ¡c Ã½ chÃ­nh tá»« dá»¯ liá»‡u showroom:");
            builder.AppendLine($"- PhÃ¢n khÃºc/kiá»ƒu xe: {car.BodyType}.");
            builder.AppendLine($"- GiÃ¡ tham kháº£o: {car.Price:N0} VNÄ.");
            builder.AppendLine($"- TÃ¬nh tráº¡ng: {car.Status}.");
            builder.AppendLine($"- Äiá»ƒm ná»•i báº­t: {TakeFirstSentence(car.Description)}");
            builder.AppendLine($"- ThÃ´ng tin thá»±c táº¿: {TakeFirstSentence(car.Specifications)}");
            builder.AppendLine();
            builder.Append("Káº¿t luáº­n: Ä‘Ã¢y lÃ  máº«u Ä‘Ã¡ng xem náº¿u anh/chá»‹ Ä‘ang cáº§n má»™t lá»±a chá»n Ä‘Ãºng nhÃ³m xe hiá»‡n táº¡i cá»§a nÃ³. BÆ°á»›c tiáº¿p theo: anh/chá»‹ cÃ³ thá»ƒ xem chi tiáº¿t sÃ¢u hÆ¡n, mua xe hoáº·c Ä‘áº·t lá»‹ch xem xe.");

            return builder.ToString().Trim();
        }

        private static string BuildInstallmentMessage(ChatbotInstallmentEstimate estimate)
        {
            var upfrontTotal = estimate.DownPayment + estimate.RegistrationEstimate + estimate.InsuranceEstimate;
            var builder = new StringBuilder();
            builder.AppendLine($"Em Ä‘ang táº¡m tÃ­nh tráº£ gÃ³p cho {estimate.Car.CarName} theo dá»¯ liá»‡u hiá»‡n cÃ³.");
            builder.AppendLine();
            builder.AppendLine($"- GiÃ¡ xe: {estimate.CarPrice:N0} VNÄ.");
            builder.AppendLine($"- Tráº£ trÆ°á»›c: {estimate.DownPayment:N0} VNÄ.");
            builder.AppendLine($"- Khoáº£n vay dá»± kiáº¿n: {estimate.LoanAmount:N0} VNÄ.");
            builder.AppendLine($"- Ká»³ háº¡n táº¡m tÃ­nh: {estimate.TermMonths} thÃ¡ng.");
            builder.AppendLine($"- LÃ£i suáº¥t tham kháº£o: {estimate.AnnualInterestRate:N2}%/nÄƒm.");
            builder.AppendLine($"- GÃ³p má»—i thÃ¡ng khoáº£ng: {estimate.MonthlyPayment:N0} VNÄ.");
            builder.AppendLine();
            builder.AppendLine("CÃ¡c khoáº£n thÆ°á»ng gáº·p khi xuá»‘ng tiá»n ban Ä‘áº§u:");
            builder.AppendLine($"- PhÃ­ Ä‘Äƒng kÃ½/ra biá»ƒn táº¡m tÃ­nh: {estimate.RegistrationEstimate:N0} VNÄ.");
            builder.AppendLine($"- Báº£o hiá»ƒm nÄƒm Ä‘áº§u táº¡m tÃ­nh: {estimate.InsuranceEstimate:N0} VNÄ.");
            builder.AppendLine($"- Tá»•ng cáº§n chuáº©n bá»‹ ban Ä‘áº§u khoáº£ng: {upfrontTotal:N0} VNÄ.");
            builder.AppendLine();
            builder.Append($"Chi phÃ­ nuÃ´i xe hÃ ng thÃ¡ng chá»‰ lÃ  tham kháº£o: nhiÃªn liá»‡u khoáº£ng {estimate.MonthlyFuelEstimate:N0} VNÄ vÃ  báº£o dÆ°á»¡ng bÃ¬nh quÃ¢n khoáº£ng {estimate.MonthlyMaintenanceEstimate:N0} VNÄ. ");
            builder.Append("Káº¿t luáº­n: Ä‘Ã¢y lÃ  phÆ°Æ¡ng Ã¡n Æ°á»›c tÃ­nh Ä‘á»ƒ anh/chá»‹ hÃ¬nh dung dÃ²ng tiá»n, chÆ°a pháº£i bÃ¡o giÃ¡ tÃ i chÃ­nh chÃ­nh thá»©c cá»§a ngÃ¢n hÃ ng hay showroom.");

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
                return "Hiá»‡n há»‡ thá»‘ng chÆ°a cÃ³ thÃªm mÃ´ táº£ chi tiáº¿t cho má»¥c nÃ y.";
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
                CreateLinkAction("Xem danh sÃ¡ch xe", "/Cars/Index")
            };

            if (matches.Count > 0)
            {
                actions.Add(CreateLinkAction("Xem chi tiáº¿t xe Ä‘áº§u", $"/Cars/Details/{matches[0].Car.CarID}", "primary"));
            }

            return actions;
        }

        private static List<ChatbotAction> BuildCarActions(Car car)
        {
            return
            [
                CreateLinkAction("Xem chi tiáº¿t", $"/Cars/Details/{car.CarID}"),
                CreateLinkAction("Mua xe", $"/Orders/Checkout?carId={car.CarID}", "primary"),
                CreateLinkAction("Äáº·t lá»‹ch xem xe", $"/Bookings/Create?carId={car.CarID}")
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

