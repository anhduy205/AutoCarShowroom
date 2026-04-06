using AutoCarShowroom.Models;
using Microsoft.Extensions.Options;

namespace AutoCarShowroom.Services.Chatbot
{
    public sealed class ChatbotFinanceTools
    {
        private readonly ChatbotInventoryTools _inventoryTools;
        private readonly InstallmentOptions _options;

        public ChatbotFinanceTools(ChatbotInventoryTools inventoryTools, IOptions<InstallmentOptions> options)
        {
            _inventoryTools = inventoryTools;
            _options = options.Value;
        }

        public IReadOnlyList<OrderWorkflow.PaymentMethodInfo> GetPaymentMethods()
        {
            return OrderWorkflow.PublicPaymentMethodInfos;
        }

        public async Task<ChatbotInstallmentEstimate?> CalculateInstallmentEstimateAsync(
            int carId,
            decimal? downPayment,
            int? termMonths,
            decimal? annualInterestRate)
        {
            var car = await _inventoryTools.GetCarByIdAsync(carId);
            if (car == null)
            {
                return null;
            }

            var appliedDownPayment = downPayment ?? Math.Round(car.Price * _options.DefaultDownPaymentRatio, 0);
            appliedDownPayment = decimal.Clamp(appliedDownPayment, 0m, car.Price);

            var appliedTermMonths = termMonths.GetValueOrDefault(_options.DefaultTermMonths);
            if (appliedTermMonths <= 0)
            {
                appliedTermMonths = _options.DefaultTermMonths;
            }

            var appliedAnnualRate = annualInterestRate.GetValueOrDefault(_options.DefaultAnnualInterestRate);
            if (appliedAnnualRate < 0)
            {
                appliedAnnualRate = _options.DefaultAnnualInterestRate;
            }

            var loanAmount = Math.Max(0m, car.Price - appliedDownPayment);
            var monthlyRate = appliedAnnualRate / 12 / 100;
            var monthlyPayment = loanAmount == 0
                ? 0
                : monthlyRate == 0
                    ? loanAmount / appliedTermMonths
                    : loanAmount *
                      (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), appliedTermMonths)) /
                      ((decimal)Math.Pow((double)(1 + monthlyRate), appliedTermMonths) - 1);

            return new ChatbotInstallmentEstimate
            {
                Car = car,
                CarPrice = car.Price,
                DownPayment = Math.Round(appliedDownPayment, 0),
                LoanAmount = Math.Round(loanAmount, 0),
                TermMonths = appliedTermMonths,
                AnnualInterestRate = appliedAnnualRate,
                MonthlyPayment = Math.Round(monthlyPayment, 0),
                RegistrationEstimate = Math.Round(car.Price * _options.RegistrationFeeRate, 0),
                InsuranceEstimate = Math.Round(car.Price * _options.InsuranceRate, 0),
                MonthlyFuelEstimate = _options.MonthlyFuelEstimate,
                MonthlyMaintenanceEstimate = _options.MonthlyMaintenanceEstimate
            };
        }
    }
}
