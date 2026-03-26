using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Data
{
    public static class ShowroomDemoSeeder
    {
        private static readonly IReadOnlyList<DemoCar> DemoCars =
        [
            new(
                CarName: "Toyota Camry 2.5Q",
                Brand: "Toyota",
                ModelName: "Camry",
                Price: 1450000000m,
                Year: 2024,
                Color: "Xanh thép",
                BodyType: "Sedan",
                Status: "Còn hàng",
                Image: "/images/demo-cars/toyota-camry.svg",
                Specifications: "Động cơ 2.5L, hộp số tự động 8 cấp, 5 chỗ, ghế da, màn hình giải trí lớn.",
                Description: "Sedan cao cấp, vận hành êm và hợp nhu cầu đi lại hằng ngày."),
            new(
                CarName: "Honda CR-V RS",
                Brand: "Honda",
                ModelName: "CR-V",
                Price: 1320000000m,
                Year: 2024,
                Color: "Bạc titan",
                BodyType: "SUV",
                Status: "Khuyến mãi",
                Image: "/images/demo-cars/honda-cr-v.svg",
                Specifications: "Động cơ tăng áp 1.5L, 7 chỗ, gói an toàn Honda Sensing, cốp điện.",
                Description: "SUV gia đình rộng rãi, nội thất hiện đại và rất dễ tiếp cận."),
            new(
                CarName: "Mazda CX-5 Premium",
                Brand: "Mazda",
                ModelName: "CX-5",
                Price: 979000000m,
                Year: 2023,
                Color: "Đỏ Soul Red",
                BodyType: "Crossover",
                Status: "Còn hàng",
                Image: "/images/demo-cars/mazda-cx5.svg",
                Specifications: "Động cơ 2.0L, 5 chỗ, HUD, camera 360, gói hỗ trợ lái i-Activsense.",
                Description: "Crossover cân bằng giữa thiết kế đẹp và trải nghiệm lái dễ chịu."),
            new(
                CarName: "Kia Carnival Signature",
                Brand: "Kia",
                ModelName: "Carnival",
                Price: 1589000000m,
                Year: 2024,
                Color: "Đen ánh kim",
                BodyType: "MPV",
                Status: "Còn hàng",
                Image: "/images/demo-cars/kia-carnival.svg",
                Specifications: "Động cơ diesel 2.2L, 7 chỗ, ghế thương gia hàng 2, cửa lùa điện.",
                Description: "MPV cao cấp, rộng rãi và phù hợp cho gia đình hoặc dịch vụ."),
            new(
                CarName: "Ford Everest Titanium",
                Brand: "Ford",
                ModelName: "Everest",
                Price: 1468000000m,
                Year: 2023,
                Color: "Xanh rêu",
                BodyType: "SUV",
                Status: "Đã bán",
                Image: "/images/demo-cars/ford-everest.svg",
                Specifications: "Động cơ diesel 2.0L turbo, 7 chỗ, 4x4, camera 360, 6 chế độ lái.",
                Description: "SUV khung gầm chắc chắn, hợp đường dài và địa hình đa dạng."),
            new(
                CarName: "Hyundai Accent AT",
                Brand: "Hyundai",
                ModelName: "Accent",
                Price: 569000000m,
                Year: 2024,
                Color: "Xanh dương",
                BodyType: "Sedan",
                Status: "Còn hàng",
                Image: "/images/demo-cars/hyundai-accent.svg",
                Specifications: "Động cơ 1.5L, hộp số CVT, màn hình 8 inch, điều hòa tự động.",
                Description: "Sedan hạng B thực dụng, chi phí hợp lý và dễ tiếp cận."),
            new(
                CarName: "Mercedes-Benz C300 AMG",
                Brand: "Mercedes-Benz",
                ModelName: "C300",
                Price: 2099000000m,
                Year: 2024,
                Color: "Xám graphite",
                BodyType: "Sedan",
                Status: "Còn hàng",
                Image: "/images/demo-cars/mercedes-c300.svg",
                Specifications: "Động cơ mild hybrid, nội thất da cao cấp, màn hình lớn, gói AMG.",
                Description: "Sedan sang trọng, thiên về trải nghiệm lái êm và nội thất đẹp."),
            new(
                CarName: "Toyota Fortuner Legender",
                Brand: "Toyota",
                ModelName: "Fortuner",
                Price: 1350000000m,
                Year: 2024,
                Color: "Nâu cát",
                BodyType: "SUV",
                Status: "Còn hàng",
                Image: "/images/demo-cars/toyota-fortuner.svg",
                Specifications: "Động cơ diesel 2.8L, 7 chỗ, khung gầm rời, camera 360.",
                Description: "SUV 7 chỗ mạnh mẽ, hợp gia đình thích đi xa và đi tỉnh."),
            new(
                CarName: "Hyundai Santa Fe Calligraphy",
                Brand: "Hyundai",
                ModelName: "Santa Fe",
                Price: 1369000000m,
                Year: 2024,
                Color: "Ghi bạc",
                BodyType: "SUV",
                Status: "Khuyến mãi",
                Image: "/images/demo-cars/hyundai-santa-fe.svg",
                Specifications: "Động cơ xăng tăng áp, 6 chỗ, màn hình đôi, gói an toàn SmartSense.",
                Description: "SUV hiện đại, thiết kế mới và nhiều tiện nghi cho gia đình."),
            new(
                CarName: "Peugeot 408 GT",
                Brand: "Peugeot",
                ModelName: "408",
                Price: 1249000000m,
                Year: 2024,
                Color: "Xanh lam",
                BodyType: "Crossover",
                Status: "Còn hàng",
                Image: "/images/demo-cars/peugeot-408.svg",
                Specifications: "Động cơ 1.6 turbo, màn hình i-Cockpit, cửa sổ trời, ghế da.",
                Description: "Mẫu fastback cá tính, khác biệt và khá nổi bật về kiểu dáng."),
            new(
                CarName: "VinFast VF 8 Plus",
                Brand: "VinFast",
                ModelName: "VF 8",
                Price: 1229000000m,
                Year: 2024,
                Color: "Xanh điện",
                BodyType: "SUV",
                Status: "Còn hàng",
                Image: "/images/demo-cars/vinfast-vf8.svg",
                Specifications: "SUV điện, trợ lý lái nâng cao, màn hình trung tâm lớn, cốp rộng.",
                Description: "SUV điện hiện đại, hợp khách thích công nghệ và vận hành êm."),
            new(
                CarName: "BMW X5 xDrive40i",
                Brand: "BMW",
                ModelName: "X5",
                Price: 3899000000m,
                Year: 2024,
                Color: "Bạc khói",
                BodyType: "SUV",
                Status: "Còn hàng",
                Image: "/images/demo-cars/bmw-x5.svg",
                Specifications: "Động cơ I6, dẫn động AWD, nội thất cao cấp, gói hỗ trợ lái đầy đủ.",
                Description: "SUV hạng sang, mạnh mẽ và thiên về trải nghiệm lái cao cấp.")
        ];

        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ShowroomDbContext>();
            var existingCars = await context.Cars.ToListAsync();

            foreach (var demoCar in DemoCars)
            {
                var existing = existingCars.FirstOrDefault(car => IsSameCar(car, demoCar));

                if (existing == null)
                {
                    context.Cars.Add(ToEntity(demoCar));
                    continue;
                }

                if (ShouldRefreshDemoCar(existing))
                {
                    ApplyDemoValues(existing, demoCar);
                }
            }

            await context.SaveChangesAsync();
        }

        private static bool IsSameCar(Car existing, DemoCar demoCar)
        {
            if (string.Equals(existing.CarName, demoCar.CarName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(existing.Brand, demoCar.Brand, StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.ModelName, demoCar.ModelName, StringComparison.OrdinalIgnoreCase)
                && existing.Year == demoCar.Year;
        }

        private static bool ShouldRefreshDemoCar(Car car)
        {
            return string.IsNullOrWhiteSpace(car.Image)
                || car.Image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)
                || car.Image.StartsWith("/images/demo-cars/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(car.Brand, "Chưa cập nhật", StringComparison.OrdinalIgnoreCase)
                || string.Equals(car.ModelName, "Chưa cập nhật", StringComparison.OrdinalIgnoreCase);
        }

        private static Car ToEntity(DemoCar demoCar)
        {
            return new Car
            {
                CarName = demoCar.CarName,
                Brand = demoCar.Brand,
                ModelName = demoCar.ModelName,
                Price = demoCar.Price,
                Year = demoCar.Year,
                Color = demoCar.Color,
                BodyType = demoCar.BodyType,
                Status = demoCar.Status,
                Image = demoCar.Image,
                Specifications = demoCar.Specifications,
                Description = demoCar.Description
            };
        }

        private static void ApplyDemoValues(Car car, DemoCar demoCar)
        {
            car.CarName = demoCar.CarName;
            car.Brand = demoCar.Brand;
            car.ModelName = demoCar.ModelName;
            car.Price = demoCar.Price;
            car.Year = demoCar.Year;
            car.Color = demoCar.Color;
            car.BodyType = demoCar.BodyType;
            car.Status = demoCar.Status;
            car.Image = demoCar.Image;
            car.Specifications = demoCar.Specifications;
            car.Description = demoCar.Description;
        }

        private sealed record DemoCar(
            string CarName,
            string Brand,
            string ModelName,
            decimal Price,
            int Year,
            string Color,
            string BodyType,
            string Status,
            string Image,
            string Specifications,
            string Description);
    }
}
