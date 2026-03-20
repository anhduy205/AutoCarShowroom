namespace AutoCarShowroom.Models
{
    public static class CarCatalogMetadata
    {
        public static readonly IReadOnlyList<string> MainstreamBrands =
        [
            "VinFast",
            "Toyota",
            "Hyundai",
            "Ford",
            "Mazda",
            "Mitsubishi",
            "Honda",
            "Kia"
        ];

        public static readonly IReadOnlyList<string> SupercarBrands =
        [
            "Ferrari",
            "Lamborghini"
        ];

        public static readonly IReadOnlyList<string> AllBrands =
        [
            ..MainstreamBrands,
            ..SupercarBrands
        ];
    }
}
