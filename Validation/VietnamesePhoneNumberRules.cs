namespace AutoCarShowroom.Validation
{
    public static class VietnamesePhoneNumberRules
    {
        public const string Pattern = @"^0\d{9}$";
        public const string ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng 0.";
    }
}
