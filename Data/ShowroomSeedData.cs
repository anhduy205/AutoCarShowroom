using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Data
{
    public static class ShowroomSeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ShowroomDbContext>();

            if (await context.Cars.AnyAsync())
            {
                return;
            }

            context.Cars.AddRange(
                new Car
                {
                    CarName = "Toyota Camry 2.5Q",
                    Price = 1450000000m,
                    Year = 2024,
                    Image = "/uploads/seed/toyota-camry.svg",
                    Description = "Mau sedan cao cap, van hanh em, phu hop nhu cau gia dinh va di chuyen cong viec."
                },
                new Car
                {
                    CarName = "Honda CR-V RS",
                    Price = 1320000000m,
                    Year = 2024,
                    Image = "/uploads/seed/honda-crv.svg",
                    Description = "Dong SUV 5+2 rong rai, noi that hien dai va duoc nhieu gia dinh lua chon."
                },
                new Car
                {
                    CarName = "Mazda CX-5 Premium",
                    Price = 979000000m,
                    Year = 2023,
                    Image = "/uploads/seed/mazda-cx5.svg",
                    Description = "Mau crossover dep, de lai va can bang tot giua gia ban, trang bi va thiet ke."
                },
                new Car
                {
                    CarName = "Kia Carnival Signature",
                    Price = 1589000000m,
                    Year = 2024,
                    Image = "/uploads/seed/kia-carnival.svg",
                    Description = "MPV cao cap voi khong gian noi that rong, phu hop kinh doanh va gia dinh dong thanh vien."
                },
                new Car
                {
                    CarName = "Ford Everest Titanium",
                    Price = 1468000000m,
                    Year = 2023,
                    Image = "/uploads/seed/ford-everest.svg",
                    Description = "SUV khung gam chac chan, khoang sang rong va phu hop nhieu dieu kien van hanh."
                },
                new Car
                {
                    CarName = "Hyundai Accent AT",
                    Price = 569000000m,
                    Year = 2024,
                    Image = "/uploads/seed/hyundai-accent.svg",
                    Description = "Sedan hang B thuc dung, chi phi hop ly va phu hop cho nguoi mua xe lan dau."
                });

            await context.SaveChangesAsync();
        }
    }
}
