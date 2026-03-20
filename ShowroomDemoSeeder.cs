using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Data
{
    public static class ShowroomDemoSeeder
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
                    Image = BuildImageDataUri("Camry", "#8b4513", "#f2d4b6"),
                    Description = "Mau sedan cao cap, van hanh em, phu hop nhu cau gia dinh va di chuyen cong viec."
                },
                new Car
                {
                    CarName = "Honda CR-V RS",
                    Price = 1320000000m,
                    Year = 2024,
                    Image = BuildImageDataUri("CR-V", "#4b5d67", "#c9d6df"),
                    Description = "Dong SUV 5+2 rong rai, noi that hien dai va duoc nhieu gia dinh lua chon."
                },
                new Car
                {
                    CarName = "Mazda CX-5 Premium",
                    Price = 979000000m,
                    Year = 2023,
                    Image = BuildImageDataUri("CX-5", "#7f5539", "#e6ccb2"),
                    Description = "Mau crossover dep, de lai va can bang tot giua gia ban, trang bi va thiet ke."
                },
                new Car
                {
                    CarName = "Kia Carnival Signature",
                    Price = 1589000000m,
                    Year = 2024,
                    Image = BuildImageDataUri("Carnival", "#5a3e36", "#d8c2b0"),
                    Description = "MPV cao cap voi khong gian noi that rong, phu hop kinh doanh va gia dinh dong thanh vien."
                },
                new Car
                {
                    CarName = "Ford Everest Titanium",
                    Price = 1468000000m,
                    Year = 2023,
                    Image = BuildImageDataUri("Everest", "#344e41", "#dad7cd"),
                    Description = "SUV khung gam chac chan, khoang sang rong va phu hop nhieu dieu kien van hanh."
                },
                new Car
                {
                    CarName = "Hyundai Accent AT",
                    Price = 569000000m,
                    Year = 2024,
                    Image = BuildImageDataUri("Accent", "#4361ee", "#dee2ff"),
                    Description = "Sedan hang B thuc dung, chi phi hop ly va phu hop cho nguoi mua xe lan dau."
                });

            await context.SaveChangesAsync();
        }

        private static string BuildImageDataUri(string title, string primary, string secondary)
        {
            var svg = $"""
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 800 480'>
                  <defs>
                    <linearGradient id='bg' x1='0' y1='0' x2='1' y2='1'>
                      <stop offset='0%' stop-color='{primary}' />
                      <stop offset='100%' stop-color='{secondary}' />
                    </linearGradient>
                  </defs>
                  <rect width='800' height='480' fill='url(#bg)' />
                  <circle cx='170' cy='360' r='72' fill='rgba(255,255,255,0.16)' />
                  <circle cx='630' cy='140' r='100' fill='rgba(255,255,255,0.12)' />
                  <path d='M160 275h392l62 48H110l50-48z' fill='rgba(20,20,20,0.82)' />
                  <path d='M270 198h175c38 0 71 19 89 52H208c16-34 34-52 62-52z' fill='rgba(255,255,255,0.9)' />
                  <circle cx='235' cy='334' r='38' fill='#111' />
                  <circle cx='515' cy='334' r='38' fill='#111' />
                  <circle cx='235' cy='334' r='17' fill='#d9d9d9' />
                  <circle cx='515' cy='334' r='17' fill='#d9d9d9' />
                  <text x='60' y='96' font-family='Segoe UI, Arial' font-size='54' font-weight='700' fill='white'>{title}</text>
                  <text x='60' y='138' font-family='Segoe UI, Arial' font-size='24' fill='rgba(255,255,255,0.84)'>AutoCarShowroom Demo</text>
                </svg>
                """;

            return $"data:image/svg+xml,{Uri.EscapeDataString(svg)}";
        }
    }
}
