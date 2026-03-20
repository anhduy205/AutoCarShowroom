using System.Text;
using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Data
{
    public static class ShowroomDemoSeeder
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ShowroomDbContext>();
            var environment = services.GetRequiredService<IWebHostEnvironment>();
            var existingCars = await context.Cars.ToListAsync();
            var seedCars = BuildSeedCars(environment.WebRootPath);
            var seedLookup = seedCars.ToDictionary(
                car => BuildKey(car.Brand, car.CarName, car.Year),
                StringComparer.OrdinalIgnoreCase);

            if (NormalizeExistingCars(existingCars, seedLookup))
            {
                await context.SaveChangesAsync();
            }

            var existingKeys = existingCars
                .Select(car => BuildKey(car.Brand, car.CarName, car.Year))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var newCars = seedCars
                .Where(car => existingKeys.Add(BuildKey(car.Brand, car.CarName, car.Year)))
                .ToList();

            if (newCars.Count == 0)
            {
                return;
            }

            context.Cars.AddRange(newCars);
            await context.SaveChangesAsync();
        }

        private static bool NormalizeExistingCars(IEnumerable<Car> cars, IReadOnlyDictionary<string, Car> seedLookup)
        {
            var changed = false;

            foreach (var car in cars)
            {
                if (string.IsNullOrWhiteSpace(car.Brand))
                {
                    car.Brand = InferBrandFromName(car.CarName) ?? "Khac";
                    changed = true;
                }

                if (!seedLookup.TryGetValue(BuildKey(car.Brand, car.CarName, car.Year), out var seedCar))
                {
                    continue;
                }

                if (IsGeneratedPlaceholderImage(car.Image) && IsCatalogImage(seedCar.Image))
                {
                    car.Image = seedCar.Image;
                    changed = true;
                }
            }

            return changed;
        }

        private static string? InferBrandFromName(string carName)
        {
            return CarCatalogMetadata.AllBrands
                .OrderByDescending(brand => brand.Length)
                .FirstOrDefault(brand => carName.StartsWith(brand, StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildKey(string brand, string carName, int year)
        {
            return $"{brand}|{carName}|{year}";
        }

        private static List<Car> BuildSeedCars(string webRootPath)
        {
            var cars = new List<Car>();
            cars.AddRange(BuildToyotaCars(webRootPath));
            cars.AddRange(BuildHyundaiCars(webRootPath));
            cars.AddRange(BuildKiaCars(webRootPath));
            cars.AddRange(BuildFordCars(webRootPath));
            cars.AddRange(BuildMazdaCars(webRootPath));
            cars.AddRange(BuildHondaCars(webRootPath));
            cars.AddRange(BuildMitsubishiCars(webRootPath));
            cars.AddRange(BuildVinFastCars(webRootPath));
            cars.AddRange(BuildFerrariCars(webRootPath));
            cars.AddRange(BuildLamborghiniCars(webRootPath));
            return cars;
        }

        private static IEnumerable<Car> BuildToyotaCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Toyota",
                    "Vios",
                    "sedan pho thong",
                    "noi tieng ve do ben, de van hanh va chi phi su dung hop ly",
                    "#7d4f2a",
                    "#f3d7bb",
                    new VariantSpec("E MT", 2024, 458000000m, "nguoi mua xe lan dau hoac chay dich vu"),
                    new VariantSpec("E CVT", 2024, 488000000m, "khach hang can sedan de lai trong do thi"),
                    new VariantSpec("G CVT", 2025, 545000000m, "nguoi dung uu tien trang bi an toan va tiet kiem"),
                    new VariantSpec("GR-S", 2026, 630000000m, "nguoi thich ve ngoai tre trung hon trong tam gia de tiep can")),
                new ModelSpec(
                    "Toyota",
                    "Corolla Cross",
                    "SUV/crossover",
                    "can bang giua khong gian gia dinh, do linh hoat va kha nang giu gia",
                    "#495057",
                    "#dce3e8",
                    new VariantSpec("1.8G", 2024, 820000000m, "gia dinh tre can mau SUV gon gon de di pho va di xa"),
                    new VariantSpec("1.8V", 2024, 905000000m, "nguoi muon nang cap trang bi va cam giac lai"),
                    new VariantSpec("1.8HEV", 2025, 955000000m, "khach hang quan tam kha nang tiet kiem nhien lieu"),
                    new VariantSpec("HEV Premium", 2026, 1020000000m, "nguoi can xe gia dinh hien dai cho nhu cau su dung hang ngay")),
                new ModelSpec(
                    "Toyota",
                    "Innova Cross",
                    "MPV",
                    "co khoang cabin rong, thuc dung va phu hop gia dinh dong thanh vien",
                    "#6f4e37",
                    "#ead8c0",
                    new VariantSpec("2.0G", 2024, 810000000m, "gia dinh can xe 7 cho su dung da muc dich"),
                    new VariantSpec("2.0V", 2024, 930000000m, "nguoi dung muon noi that de chiu hon va tien nghi hon"),
                    new VariantSpec("Hybrid", 2025, 1025000000m, "khach hang uu tien van hanh em va tiet kiem"),
                    new VariantSpec("Hybrid Premium", 2026, 1095000000m, "gia dinh can MPV cao cap cho cac chuyen di dai")),
                new ModelSpec(
                    "Toyota",
                    "Fortuner",
                    "SUV khung roi",
                    "manh ve kha nang di duong truong, su on dinh va goc ngoi cao",
                    "#5b4636",
                    "#dfc8b4",
                    new VariantSpec("2.4 AT 4x2", 2024, 1055000000m, "khach hang can SUV 7 cho de di tinh va di gia dinh"),
                    new VariantSpec("Legender 4x2", 2025, 1185000000m, "nguoi thich thiet ke khoe khoan va noi that nang tam"),
                    new VariantSpec("2.8 AT 4x4", 2025, 1310000000m, "nguoi hay di cung duong kho hoac can do da dung"),
                    new VariantSpec("Legender 4x4", 2026, 1375000000m, "khach hang can SUV du lich cao cap hon trong tam gia Toyota")),
                new ModelSpec(
                    "Toyota",
                    "Camry",
                    "sedan hang D",
                    "huong toi nhom khach hang can xe doanh nhan em ai va noi that dep",
                    "#3f3a39",
                    "#e8e1de",
                    new VariantSpec("2.0Q", 2024, 1220000000m, "nguoi can sedan doanh nhan van hanh on dinh"),
                    new VariantSpec("2.5Q", 2024, 1450000000m, "khach hang muon cau hinh manh hon va noi that tot hon"),
                    new VariantSpec("HEV Mid", 2025, 1590000000m, "nguoi can sedan cao cap ket hop tiet kiem nhien lieu"),
                    new VariantSpec("HEV Top", 2026, 1730000000m, "khach hang can mau sedan dau bang trong danh muc Toyota"))
            );
        }

        private static IEnumerable<Car> BuildHyundaiCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Hyundai",
                    "Accent",
                    "sedan hang B",
                    "phu hop di lai do thi, noi that de tiep can va dich vu sau ban pho bien",
                    "#4059ad",
                    "#dee7ff",
                    new VariantSpec("1.5 MT", 2024, 439000000m, "nguoi dung uu tien chi phi so huu thap"),
                    new VariantSpec("AT", 2024, 569000000m, "khach hang can sedan gia dinh de lai nhe nhang"),
                    new VariantSpec("AT Dac biet", 2025, 539000000m, "nguoi can them trang bi tien nghi va an toan"),
                    new VariantSpec("Cao cap", 2026, 579000000m, "khach hang tre muon xe do thi de nhin va de su dung")),
                new ModelSpec(
                    "Hyundai",
                    "Creta",
                    "SUV/crossover",
                    "la lua chon phu hop cho gia dinh tre can xe gam cao nho gon",
                    "#556b2f",
                    "#dee6c5",
                    new VariantSpec("Tieu chuan", 2024, 640000000m, "nguoi muon chuyen tu sedan len SUV de di lai hang ngay"),
                    new VariantSpec("Dac biet", 2025, 705000000m, "gia dinh can them trang bi ho tro lai xe"),
                    new VariantSpec("Cao cap", 2025, 760000000m, "khach hang uu tien goi tien nghi va trang bi noi that"),
                    new VariantSpec("N Line", 2026, 820000000m, "nguoi thich phong cach tre va ngoai hinh the thao")),
                new ModelSpec(
                    "Hyundai",
                    "Tucson",
                    "SUV/crossover hang C",
                    "duoc danh gia cao ve thiet ke, khoang cabin va su de chiu khi di duong dai",
                    "#5d576b",
                    "#ddd7e6",
                    new VariantSpec("2.0 Xang", 2024, 825000000m, "khach hang can SUV gia dinh gia de tiep can"),
                    new VariantSpec("2.0 Dac biet", 2025, 910000000m, "nguoi can can bang giua gia ban va trang bi"),
                    new VariantSpec("1.6 Turbo", 2025, 995000000m, "khach hang muon xe phan hoi lanh hon trong van hanh"),
                    new VariantSpec("1.6 Turbo AWD", 2026, 1060000000m, "nguoi can mau SUV co cau hinh cao va nhieu cong nghe")),
                new ModelSpec(
                    "Hyundai",
                    "Santa Fe",
                    "SUV 7 cho",
                    "mang phong cach gia dinh cao cap, hop cho ca nhu cau di pho lan du lich",
                    "#4b5563",
                    "#dce4ec",
                    new VariantSpec("Exclusive", 2024, 1070000000m, "gia dinh can SUV 7 cho rong rai de di xa"),
                    new VariantSpec("Prestige", 2025, 1190000000m, "nguoi dung can goi trang bi can bang cho su dung lau dai"),
                    new VariantSpec("Calligraphy", 2025, 1290000000m, "khach hang can khoang cabin dep va nhieu tien nghi"),
                    new VariantSpec("Calligraphy Turbo", 2026, 1375000000m, "nguoi muon cau hinh cao nhat de tiep khach va di gia dinh")),
                new ModelSpec(
                    "Hyundai",
                    "Stargazer X",
                    "MPV",
                    "de su dung cho gia dinh va kinh doanh nho voi khoang cabin linh hoat",
                    "#704214",
                    "#f0ddc8",
                    new VariantSpec("Tieu chuan", 2024, 575000000m, "nguoi can MPV rong va gia hop ly"),
                    new VariantSpec("X Ban", 2025, 615000000m, "gia dinh can xe 7 cho de xoay so trong pho"),
                    new VariantSpec("Cao cap", 2025, 675000000m, "khach hang can goi trang bi day du hon"),
                    new VariantSpec("Premium", 2026, 720000000m, "nguoi muon MPV co ve ngoai kha nang dong hanh du lich"))
            );
        }

        private static IEnumerable<Car> BuildKiaCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Kia",
                    "K3",
                    "sedan hang C",
                    "co thiet ke tre, trang bi nhieu va de tiep can trong nhom xe gia dinh",
                    "#7c3e66",
                    "#f1d7ea",
                    new VariantSpec("1.6 MT", 2024, 549000000m, "nguoi can xe hang C gia hop ly"),
                    new VariantSpec("1.6 Luxury", 2024, 629000000m, "gia dinh can sedan rong va de lai"),
                    new VariantSpec("2.0 Premium", 2025, 699000000m, "khach hang uu tien them trang bi va noi that"),
                    new VariantSpec("GT-Line", 2026, 759000000m, "nguoi thich phom dang the thao nhung van can su thuc dung")),
                new ModelSpec(
                    "Kia",
                    "Seltos",
                    "SUV/crossover",
                    "noi bat trong nhom B-SUV voi thiet ke de nhin va nhieu phien ban",
                    "#355070",
                    "#d8e2f0",
                    new VariantSpec("1.5 AT", 2024, 619000000m, "nguoi dung can SUV do thi gon va tien nghi"),
                    new VariantSpec("Deluxe", 2025, 664000000m, "gia dinh tre can xe gam cao cho do thi"),
                    new VariantSpec("Luxury", 2025, 729000000m, "khach hang muon goi trang bi day du hon"),
                    new VariantSpec("Turbo GT-Line", 2026, 799000000m, "nguoi thich cam giac lai the thao trong than xe nho gon")),
                new ModelSpec(
                    "Kia",
                    "Sportage",
                    "SUV/crossover hang C",
                    "mang phong cach hien dai, ghe ngoi de chiu va khoang cabin kha thoang",
                    "#5f0f40",
                    "#f3d7e5",
                    new VariantSpec("2.0G", 2024, 859000000m, "gia dinh can SUV de dung cho cong viec va cuoi tuan"),
                    new VariantSpec("2.0D Luxury", 2025, 959000000m, "nguoi dung uu tien kha nang tiet kiem tren duong dai"),
                    new VariantSpec("1.6T Premium", 2025, 1029000000m, "khach hang can them suc keo va phan hoi tot hon"),
                    new VariantSpec("X-Line AWD", 2026, 1099000000m, "nguoi thich phien ban co ngoai hinh noi bat hon")),
                new ModelSpec(
                    "Kia",
                    "Sorento",
                    "SUV 7 cho",
                    "phu hop nhom gia dinh can mau xe lon, noi that nhieu cong nghe va di duong dai tot",
                    "#4a5759",
                    "#dfe7e8",
                    new VariantSpec("2.2D Luxury", 2024, 1099000000m, "gia dinh can SUV 7 cho de di xa"),
                    new VariantSpec("Premium", 2025, 1189000000m, "khach hang can su can bang giua gia ban va trang bi"),
                    new VariantSpec("Signature AWD", 2025, 1299000000m, "nguoi can goi cau hinh cao va kha nang bam duong tot hon"),
                    new VariantSpec("HEV Signature", 2026, 1419000000m, "khach hang muon xe gia dinh cao cap mang huong tiet kiem")),
                new ModelSpec(
                    "Kia",
                    "Carnival",
                    "MPV cao cap",
                    "co khoang noi that lon, hop cho gia dinh dong thanh vien va dich vu cao cap",
                    "#5e503f",
                    "#eee0cb",
                    new VariantSpec("2.2D Luxury", 2024, 1249000000m, "gia dinh can MPV rong cho nhieu chuyen di"),
                    new VariantSpec("Premium", 2025, 1349000000m, "khach hang can them tien nghi cho hang ghe sau"),
                    new VariantSpec("Signature", 2024, 1589000000m, "nguoi dung can MPV cao cap de don tiep doi tac hoac gia dinh"),
                    new VariantSpec("Hybrid Premium", 2026, 1629000000m, "khach hang muon MPV hien dai va su van hanh em hon"))
            );
        }

        private static IEnumerable<Car> BuildFordCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Ford",
                    "Territory",
                    "SUV/crossover",
                    "de tiep can trong nhom C-SUV va co goi trang bi phu hop nhu cau gia dinh",
                    "#1d3557",
                    "#d8e3f0",
                    new VariantSpec("Trend", 2024, 799000000m, "gia dinh can SUV gam cao cho do thi"),
                    new VariantSpec("Titanium", 2025, 889000000m, "khach hang can su can bang giua gia ban va tien nghi"),
                    new VariantSpec("Sport", 2025, 929000000m, "nguoi muon ngoai hinh tre trung, the thao hon"),
                    new VariantSpec("Titanium X", 2026, 969000000m, "khach hang uu tien goi trang bi cao nhat trong dong xe")),
                new ModelSpec(
                    "Ford",
                    "Everest",
                    "SUV 7 cho",
                    "noi bat ve khoang sang, khung gam chac chan va hop cho nhieu dia hinh",
                    "#4a4e69",
                    "#e2e5f0",
                    new VariantSpec("Ambiente", 2024, 1122000000m, "gia dinh can SUV 7 cho dung cho ca cong viec lan du lich"),
                    new VariantSpec("Sport", 2025, 1178000000m, "nguoi thich thiet ke manh va noi that noi bat"),
                    new VariantSpec("Titanium", 2023, 1468000000m, "khach hang can cau hinh hoan thien, nhieu trang bi va de di xa"),
                    new VariantSpec("Platinum", 2026, 1545000000m, "nguoi can SUV cao cap trong nhom phan khuc khung roi")),
                new ModelSpec(
                    "Ford",
                    "Ranger",
                    "ban tai",
                    "la dong pickup duoc ua chuong nho kha nang cho hang, di da dia hinh va ngoai hinh nam tinh",
                    "#6c584c",
                    "#e9dcc9",
                    new VariantSpec("XLS 4x2 AT", 2024, 707000000m, "nguoi can ban tai phuc vu cong viec hang ngay"),
                    new VariantSpec("Sport 4x4", 2025, 864000000m, "khach hang can xe vua phuc vu viec lam vua di gia dinh"),
                    new VariantSpec("Wildtrak", 2025, 979000000m, "nguoi dung uu tien noi that dep va nhieu ho tro lai xe"),
                    new VariantSpec("Stormtrak", 2026, 1039000000m, "khach hang can pickup noi bat cho di xa va da ngoai")),
                new ModelSpec(
                    "Ford",
                    "Transit",
                    "minibus thuong mai",
                    "huong toi dich vu van chuyen, dua don cong ty va kinh doanh du lich",
                    "#264653",
                    "#d7ecef",
                    new VariantSpec("Tieu chuan", 2024, 905000000m, "doanh nghiep can xe cho nhieu cho ngoi va de bao tri"),
                    new VariantSpec("Premium", 2025, 989000000m, "don vi van tai can them tien nghi va hoan thien noi that"),
                    new VariantSpec("16 cho Luxury", 2025, 1069000000m, "don vi dua don can khoang hanh khach thoang hon"),
                    new VariantSpec("Limousine", 2026, 1239000000m, "doanh nghiep can xe cao cap cho dua don doi tac")),
                new ModelSpec(
                    "Ford",
                    "Explorer",
                    "SUV cao cap",
                    "co khong gian lon, phong cach My va phu hop nhom khach hang can xe gia dinh sang hon",
                    "#283618",
                    "#e7efd2",
                    new VariantSpec("Limited", 2024, 2099000000m, "gia dinh can SUV lon nhap khau de di du lich"),
                    new VariantSpec("Limited+", 2025, 2199000000m, "nguoi can them tien nghi va ngoai hinh noi bat"),
                    new VariantSpec("Platinum", 2025, 2349000000m, "khach hang can SUV My cao cap cho nhu cau su dung hang ngay"),
                    new VariantSpec("Platinum 4WD", 2026, 2469000000m, "nguoi muon mau SUV nhap khau co cau hinh top"))
            );
        }

        private static IEnumerable<Car> BuildMazdaCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Mazda",
                    "Mazda3",
                    "sedan hang C",
                    "noi bat ve thiet ke dep, noi that gon gang va cam giac lai de chiu",
                    "#6d597a",
                    "#e6dff0",
                    new VariantSpec("1.5 Deluxe", 2024, 579000000m, "nguoi can sedan hang C gia hop ly"),
                    new VariantSpec("Luxury", 2024, 639000000m, "gia dinh tre can mau sedan dep va de su dung"),
                    new VariantSpec("Premium", 2025, 719000000m, "khach hang muon nhieu trang bi an toan hon"),
                    new VariantSpec("Signature", 2026, 769000000m, "nguoi uu tien noi that dep trong tam gia hang C")),
                new ModelSpec(
                    "Mazda",
                    "CX-3",
                    "SUV/crossover",
                    "gon gon, phu hop di pho va van giu duoc chat rieng cua dong xe Mazda",
                    "#386641",
                    "#d7ead7",
                    new VariantSpec("1.5 AT", 2024, 559000000m, "nguoi can xe gam cao nho gon cho do thi"),
                    new VariantSpec("Deluxe", 2025, 619000000m, "khach hang can can bang giua gia va tien nghi"),
                    new VariantSpec("Luxury", 2025, 659000000m, "gia dinh tre can them trang bi de di hang ngay"),
                    new VariantSpec("Premium", 2026, 699000000m, "nguoi dung uu tien goi an toan va noi that tot hon")),
                new ModelSpec(
                    "Mazda",
                    "CX-5",
                    "SUV/crossover hang C",
                    "duoc ua chuong nho ngoai hinh dep, van hanh de chiu va gia ban canh tranh",
                    "#7f5539",
                    "#e6ccb2",
                    new VariantSpec("2.0 Deluxe", 2024, 749000000m, "khach hang can SUV gia dinh gia de tiep can"),
                    new VariantSpec("2.0 Luxury", 2024, 809000000m, "nguoi dung muon them tien nghi va ho tro lai xe"),
                    new VariantSpec("2.5 Signature Sport", 2025, 919000000m, "khach hang can dong co manh hon va phong cach the thao"),
                    new VariantSpec("Premium", 2023, 979000000m, "nguoi muon goi trang bi cao de di lai va tiep khach")),
                new ModelSpec(
                    "Mazda",
                    "CX-8",
                    "SUV 7 cho",
                    "mang phong cach lich lam, khoang cabin lon va phu hop gia dinh nhieu thanh vien",
                    "#7b6d8d",
                    "#ece7f0",
                    new VariantSpec("Luxury", 2024, 979000000m, "gia dinh can SUV 7 cho de di xa"),
                    new VariantSpec("Premium", 2025, 1059000000m, "khach hang uu tien noi that dep va tien nghi"),
                    new VariantSpec("Luxury Captain", 2025, 1099000000m, "nguoi dung can bo tri ghe linh hoat hon cho hang hai"),
                    new VariantSpec("Premium AWD", 2026, 1169000000m, "khach hang can SUV 7 cho co cau hinh cao hon")),
                new ModelSpec(
                    "Mazda",
                    "BT-50",
                    "ban tai",
                    "huong toi nhom khach hang can pickup thuc dung va de van hanh",
                    "#6c757d",
                    "#e9ecef",
                    new VariantSpec("1.9 4x2 MT", 2024, 659000000m, "nguoi dung can pickup phuc vu cong viec co ban"),
                    new VariantSpec("1.9 4x2 AT", 2025, 719000000m, "khach hang can ban tai tu dong de su dung hang ngay"),
                    new VariantSpec("1.9 Premium", 2025, 799000000m, "nguoi can them trang bi ma van giu tinh thuc dung"),
                    new VariantSpec("3.0 4x4", 2026, 889000000m, "nguoi hay di duong kho va can suc keo tot hon"))
            );
        }

        private static IEnumerable<Car> BuildHondaCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Honda",
                    "City",
                    "sedan hang B",
                    "co kha nang van hanh de chiu, khoang noi that tot va gia tri su dung lau dai",
                    "#2a9d8f",
                    "#d5f3ef",
                    new VariantSpec("G", 2024, 529000000m, "gia dinh can sedan gon va tiet kiem"),
                    new VariantSpec("L", 2024, 569000000m, "nguoi can can bang giua gia va trang bi"),
                    new VariantSpec("RS", 2025, 609000000m, "khach hang tre thich ngoai hinh the thao"),
                    new VariantSpec("RS Connect", 2026, 639000000m, "nguoi can them cong nghe ket noi va tien nghi")),
                new ModelSpec(
                    "Honda",
                    "Civic",
                    "sedan hang C",
                    "noi bat ve kha nang van hanh, do on dinh va su sac net trong phong cach thiet ke",
                    "#264653",
                    "#dbe9eb",
                    new VariantSpec("G", 2024, 789000000m, "nguoi can sedan hang C de lai va thuc dung"),
                    new VariantSpec("RS", 2025, 889000000m, "khach hang thich phong cach the thao hon"),
                    new VariantSpec("e:HEV RS", 2025, 999000000m, "nguoi muon trai nghiem mau sedan hybrid hien dai"),
                    new VariantSpec("Type R", 2026, 2399000000m, "nguoi me xe hieu nang cao trong nha Honda")),
                new ModelSpec(
                    "Honda",
                    "HR-V",
                    "SUV/crossover",
                    "phu hop gia dinh tre can xe gam cao, nho gon va de xoay so",
                    "#5a189a",
                    "#eddcfb",
                    new VariantSpec("G", 2024, 699000000m, "nguoi can SUV do thi de su dung hang ngay"),
                    new VariantSpec("L", 2025, 789000000m, "khach hang can them tien nghi va an toan"),
                    new VariantSpec("RS", 2025, 869000000m, "nguoi dung thich mau xe co goi ngoai that the thao"),
                    new VariantSpec("e:HEV RS", 2026, 949000000m, "khach hang quan tam su em ai va tiet kiem")),
                new ModelSpec(
                    "Honda",
                    "CR-V",
                    "SUV/crossover hang C",
                    "la dong xe gia dinh duoc nhieu nguoi quan tam nho tinh can bang va su rong rai",
                    "#1b4332",
                    "#d8ebdf",
                    new VariantSpec("G", 2024, 1099000000m, "gia dinh can SUV nhieu khong gian de di xa"),
                    new VariantSpec("L", 2024, 1159000000m, "khach hang can them tien nghi va goi an toan"),
                    new VariantSpec("RS", 2024, 1320000000m, "nguoi dung uu tien goi trang bi cao cap va ngoai hinh noi bat"),
                    new VariantSpec("e:HEV RS", 2026, 1349000000m, "khach hang can SUV gia dinh ket hop van hanh em va tiet kiem")),
                new ModelSpec(
                    "Honda",
                    "BR-V",
                    "MPV",
                    "phu hop cho nhom gia dinh can xe 7 cho nho, de xoay so trong pho",
                    "#6a4c93",
                    "#ede4f8",
                    new VariantSpec("G", 2024, 661000000m, "gia dinh can xe 7 cho trong tam gia hop ly"),
                    new VariantSpec("L", 2025, 705000000m, "nguoi can them trang bi co ban cho nhu cau da dung"),
                    new VariantSpec("L Honda Sensing", 2025, 745000000m, "khach hang muon them ho tro lai xe cho di gia dinh"),
                    new VariantSpec("Premium", 2026, 789000000m, "nguoi can MPV nho gon nhung van day du tien nghi"))
            );
        }

        private static IEnumerable<Car> BuildMitsubishiCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Mitsubishi",
                    "Attrage",
                    "sedan hang B",
                    "huong toi nhom khach hang uu tien tiet kiem nhien lieu va chi phi so huu",
                    "#3a5a40",
                    "#d8ead9",
                    new VariantSpec("MT", 2024, 389000000m, "nguoi can xe gia de tiep can hoac phuc vu dich vu"),
                    new VariantSpec("CVT", 2024, 459000000m, "khach hang can sedan nho gon de di trong pho"),
                    new VariantSpec("Premium", 2025, 499000000m, "nguoi muon them tien nghi ma van de su dung"),
                    new VariantSpec("Premium Black Edition", 2026, 529000000m, "khach hang tre thich phong cach noi bat hon")),
                new ModelSpec(
                    "Mitsubishi",
                    "Xforce",
                    "SUV/crossover",
                    "la dong B-SUV moi, noi bat ve khoang cabin va kha nang phuc vu gia dinh",
                    "#bc6c25",
                    "#f6dfc4",
                    new VariantSpec("GLX", 2024, 599000000m, "nguoi can xe gam cao do thi gia de tiep can"),
                    new VariantSpec("Exceed", 2025, 650000000m, "khach hang can them tien nghi cho gia dinh tre"),
                    new VariantSpec("Premium", 2025, 705000000m, "nguoi dung uu tien goi trang bi day du hon"),
                    new VariantSpec("Ultimate", 2026, 735000000m, "khach hang can phien ban tot nhat trong dong Xforce")),
                new ModelSpec(
                    "Mitsubishi",
                    "Xpander",
                    "MPV",
                    "duoc ua chuong nhieu nam nho tinh thuc dung, rong rai va de van hanh",
                    "#588157",
                    "#dcefd7",
                    new VariantSpec("MT", 2024, 560000000m, "nguoi can MPV gia hop ly cho gia dinh hoac dich vu"),
                    new VariantSpec("AT", 2024, 598000000m, "gia dinh can xe 7 cho de lai hang ngay"),
                    new VariantSpec("Premium", 2025, 658000000m, "khach hang uu tien them trang bi an toan va noi that"),
                    new VariantSpec("Cross", 2026, 699000000m, "nguoi thich phong cach gam cao va ve ngoai manh me hon")),
                new ModelSpec(
                    "Mitsubishi",
                    "Pajero Sport",
                    "SUV khung roi",
                    "co kha nang di da dia hinh, thiet ke chac chan va hop cho nhom khach hay di xa",
                    "#6d6875",
                    "#ece9f0",
                    new VariantSpec("4x2 AT", 2024, 1130000000m, "gia dinh can SUV 7 cho chac chan va de di tinh"),
                    new VariantSpec("4x4 AT", 2025, 1330000000m, "khach hang hay di duong xa hoac dia hinh phuc tap"),
                    new VariantSpec("Athlete", 2025, 1380000000m, "nguoi muon them ngoai hinh va noi that the thao"),
                    new VariantSpec("Premium AWD", 2026, 1450000000m, "khach hang can SUV khung roi co goi cau hinh cao")),
                new ModelSpec(
                    "Mitsubishi",
                    "Triton",
                    "ban tai",
                    "mang lai su ben bi, kha nang cho hang va ngoai hinh manh me cho cong viec",
                    "#414833",
                    "#e5ecd5",
                    new VariantSpec("4x2 AT", 2024, 655000000m, "nguoi can pickup phuc vu cong viec hang ngay"),
                    new VariantSpec("Athlete 4x2", 2025, 785000000m, "khach hang muon pickup co phong cach the thao hon"),
                    new VariantSpec("Athlete 4x4", 2025, 920000000m, "nguoi hay di dia hinh va can cau hinh linh hoat"),
                    new VariantSpec("Premium 4x4", 2026, 995000000m, "khach hang can ban tai co goi trang bi cao va noi that tot hon"))
            );
        }

        private static IEnumerable<Car> BuildVinFastCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "VinFast",
                    "VF 3",
                    "xe dien do thi",
                    "nho gon, de xoay so va phu hop di lai noi thanh trong cac chuyen ngan",
                    "#0096c7",
                    "#d5f4ff",
                    new VariantSpec("Tieu chuan", 2025, 299000000m, "nguoi can xe dien mini cho do thi dong duc"),
                    new VariantSpec("Urban", 2025, 315000000m, "khach hang tre can mau xe de gui va de su dung"),
                    new VariantSpec("Adventure", 2026, 329000000m, "nguoi thich phien ban co phong cach ca tinh hon"),
                    new VariantSpec("Color Pack", 2026, 339000000m, "khach hang uu tien tuy bien ngoai that cho xe dien mini")),
                new ModelSpec(
                    "VinFast",
                    "VF 5",
                    "xe dien do thi",
                    "la lua chon pho thong de tiep can trong danh muc SUV dien nho gon",
                    "#0077b6",
                    "#dbefff",
                    new VariantSpec("Base", 2024, 529000000m, "nguoi muon chuyen sang xe dien nhung van giu tam gia hop ly"),
                    new VariantSpec("Plus", 2025, 548000000m, "gia dinh tre can xe dien gam cao di hang ngay"),
                    new VariantSpec("City Edition", 2025, 569000000m, "khach hang can them trang bi cho nhu cau di noi thanh"),
                    new VariantSpec("Premium", 2026, 589000000m, "nguoi uu tien noi that dep va trang bi day du hon")),
                new ModelSpec(
                    "VinFast",
                    "VF 6",
                    "SUV dien hang B",
                    "can bang giua kich thuoc, tam van hanh va tinh hien dai cua xe dien",
                    "#00b4d8",
                    "#d9f8ff",
                    new VariantSpec("Eco", 2024, 689000000m, "gia dinh tre can xe dien gam cao cho do thi"),
                    new VariantSpec("Plus", 2025, 749000000m, "nguoi can them hieu nang va goi tien nghi"),
                    new VariantSpec("Urban Plus", 2025, 779000000m, "khach hang can them trang bi cho su dung hang ngay"),
                    new VariantSpec("Premium", 2026, 809000000m, "nguoi can SUV dien nho gon nhung hoan thien hon")),
                new ModelSpec(
                    "VinFast",
                    "VF 7",
                    "SUV dien hang C",
                    "mang phong cach the thao, cabin hien dai va phu hop nhom khach hang tre",
                    "#219ebc",
                    "#d6f3f8",
                    new VariantSpec("Eco", 2024, 799000000m, "nguoi can SUV dien co phong cach moi me"),
                    new VariantSpec("Plus", 2025, 899000000m, "khach hang uu tien them suc manh va noi that dep"),
                    new VariantSpec("Plus Sunroof", 2025, 949000000m, "nguoi can goi trang bi thien ve trai nghiem"),
                    new VariantSpec("Premium AWD", 2026, 999000000m, "khach hang muon SUV dien co cau hinh cao nhat trong dong VF 7")),
                new ModelSpec(
                    "VinFast",
                    "VF 8",
                    "SUV dien hang D",
                    "phu hop gia dinh can xe dien lon de di xa va co khoang cabin thoai mai",
                    "#023e8a",
                    "#dce8ff",
                    new VariantSpec("Eco", 2024, 1099000000m, "gia dinh can SUV dien co khong gian rong"),
                    new VariantSpec("Plus", 2025, 1199000000m, "khach hang can can bang giua suc manh va tien nghi"),
                    new VariantSpec("Lux Plus", 2025, 1269000000m, "nguoi muon them noi that dep va cong nghe cao"),
                    new VariantSpec("Premium AWD", 2026, 1349000000m, "khach hang can SUV dien cao cap cho di xa va tiep khach"))
            );
        }

        private static IEnumerable<Car> BuildFerrariCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Ferrari",
                    "296 GTB",
                    "sieu xe hybrid",
                    "co thiet ke khi dong hoc cao, van hanh nhanh va mang tinh suu tam tot",
                    "#b22222",
                    "#ffd6d6",
                    new VariantSpec("Assetto Fiorano", 2025, 15900000000m, "khach hang can mau Ferrari can bang giua hieu nang va trai nghiem hang ngay")),
                new ModelSpec(
                    "Ferrari",
                    "SF90 Spider",
                    "sieu xe plug-in hybrid",
                    "noi bat voi suc manh rat cao, mui xep va phong cach san sang cho su kien",
                    "#8b0000",
                    "#ffd1d1",
                    new VariantSpec("Performance", 2025, 34900000000m, "nguoi can mau Ferrari mui tran cho trai nghiem dac biet")),
                new ModelSpec(
                    "Ferrari",
                    "12Cilindri",
                    "grand tourer hieu nang cao",
                    "mang tinh bieu tuong nhom coupe dong co V12 va rat phu hop suu tam",
                    "#a4133c",
                    "#ffd8e3",
                    new VariantSpec("V12 Coupe", 2026, 24500000000m, "khach hang muon xe GT sang trong ket hop gia tri suu tam")),
                new ModelSpec(
                    "Ferrari",
                    "Purosangue",
                    "SUV hieu nang cao",
                    "mang den khoang cabin 4 cho rong hon nhung van giu chat Ferrari dac trung",
                    "#85182a",
                    "#f7d7de",
                    new VariantSpec("Luxury Performance", 2026, 39500000000m, "nguoi can xe 4 cua cao cap tu thuong hieu Ferrari")),
                new ModelSpec(
                    "Ferrari",
                    "F80",
                    "hypercar gioi han",
                    "huong toi nhom nha suu tam can mau xe dinh cao va cuc ky hiem",
                    "#5f0f40",
                    "#f5d8ea",
                    new VariantSpec("Collector Edition", 2026, 95000000000m, "nha suu tam muon bo sung mau hypercar mang tinh bieu tuong"))
            );
        }

        private static IEnumerable<Car> BuildLamborghiniCars(string webRootPath)
        {
            return BuildCatalog(
                webRootPath,
                new ModelSpec(
                    "Lamborghini",
                    "Temerario",
                    "sieu xe hybrid",
                    "so huu phong cach sac canh, kha nang tang toc cao va rat hop de trung bay",
                    "#7f0000",
                    "#ffd9bf",
                    new VariantSpec("Launch Edition", 2026, 23500000000m, "khach hang can sieu xe moi nhat tu Lamborghini")),
                new ModelSpec(
                    "Lamborghini",
                    "Revuelto",
                    "hypercar hybrid",
                    "la dong xe dau bang cua hang voi kieu mo cua cat canh va hieu nang cuc manh",
                    "#ff5400",
                    "#ffe0cc",
                    new VariantSpec("V12 Hybrid", 2025, 43900000000m, "nguoi choi xe can mau Lamborghini bieu tuong de suu tam")),
                new ModelSpec(
                    "Lamborghini",
                    "Urus SE",
                    "SUV hieu nang cao",
                    "ket hop tinh thuc dung cua SUV va phong cach Lamborghini rat ro rang",
                    "#9d4edd",
                    "#efe1ff",
                    new VariantSpec("Plug-in Hybrid", 2026, 22900000000m, "khach hang can SUV hieu nang cao de di lai hang ngay")),
                new ModelSpec(
                    "Lamborghini",
                    "Huracan STO",
                    "sieu xe dan dong co sau",
                    "mang dinh huong duong dua ro net, ngoai hinh ham ho va gia tri suu tam cao",
                    "#ff7b00",
                    "#ffe8cc",
                    new VariantSpec("Track Focused", 2025, 30900000000m, "nguoi thich Lamborghini thuan chat cho su kien va trung bay")),
                new ModelSpec(
                    "Lamborghini",
                    "Huracan Sterrato",
                    "sieu xe dia hinh",
                    "doc dao nhat trong danh muc khi ket hop khung gam nang va phong cach off-road",
                    "#588157",
                    "#e7f1de",
                    new VariantSpec("All-Terrain", 2025, 26900000000m, "khach hang can mau Lamborghini la, hiem va khac biet"))
            );
        }

        private static IEnumerable<Car> BuildCatalog(string webRootPath, params ModelSpec[] models)
        {
            foreach (var model in models)
            {
                foreach (var variant in model.Variants)
                {
                    yield return CreateCar(model, variant, webRootPath);
                }
            }
        }

        private static Car CreateCar(ModelSpec model, VariantSpec variant, string webRootPath)
        {
            var fullName = $"{model.Brand} {model.Model} {variant.Name}".Trim();

            return new Car
            {
                Brand = model.Brand,
                CarName = fullName,
                Price = variant.Price,
                Year = variant.Year,
                Description = $"{fullName} la mau {model.BodyType} {model.Highlight}. Phien ban nay huong toi {variant.BuyerFocus}, doi {variant.Year}, gia tham khao {variant.Price:N0} VND.",
                Image = ResolveSeedImage(webRootPath, model.Brand, model.Model, variant.Name, variant.Year, model.PrimaryColor, model.SecondaryColor)
            };
        }

        private static string ResolveSeedImage(string webRootPath, string brand, string model, string variant, int year, string primary, string secondary)
        {
            return TryGetCatalogImagePath(webRootPath, brand, model)
                ?? BuildImageDataUri(brand, model, variant, year, primary, secondary);
        }

        private static string? TryGetCatalogImagePath(string webRootPath, string brand, string model)
        {
            var fileBaseName = Slugify($"{brand}-{model}");
            var uploadFolder = Path.Combine(webRootPath, "uploads", "catalog");

            foreach (var extension in new[] { ".jpg", ".jpeg", ".png", ".webp" })
            {
                var filePath = Path.Combine(uploadFolder, $"{fileBaseName}{extension}");

                if (File.Exists(filePath))
                {
                    return $"/uploads/catalog/{fileBaseName}{extension}";
                }
            }

            return null;
        }

        private static bool IsGeneratedPlaceholderImage(string? imagePath)
        {
            return !string.IsNullOrWhiteSpace(imagePath)
                && imagePath.StartsWith("data:image/svg+xml", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCatalogImage(string? imagePath)
        {
            return !string.IsNullOrWhiteSpace(imagePath)
                && imagePath.StartsWith("/uploads/catalog/", StringComparison.OrdinalIgnoreCase);
        }

        private static string Slugify(string value)
        {
            var builder = new StringBuilder(value.Length);
            var previousWasDash = false;

            foreach (var character in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                    previousWasDash = false;
                    continue;
                }

                if (previousWasDash)
                {
                    continue;
                }

                builder.Append('-');
                previousWasDash = true;
            }

            return builder.ToString().Trim('-');
        }

        private static string BuildImageDataUri(string brand, string model, string variant, int year, string primary, string secondary)
        {
            var header = XmlEscape(brand);
            var title = XmlEscape(model);
            var subtitle = XmlEscape($"{variant} - {year}");

            var svg = $"""
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 960 540'>
                  <defs>
                    <linearGradient id='bg' x1='0' y1='0' x2='1' y2='1'>
                      <stop offset='0%' stop-color='{primary}' />
                      <stop offset='100%' stop-color='{secondary}' />
                    </linearGradient>
                    <linearGradient id='carBody' x1='0' y1='0' x2='1' y2='0'>
                      <stop offset='0%' stop-color='rgba(20,20,20,0.9)' />
                      <stop offset='100%' stop-color='rgba(42,42,42,0.78)' />
                    </linearGradient>
                  </defs>
                  <rect width='960' height='540' fill='url(#bg)' />
                  <circle cx='180' cy='120' r='110' fill='rgba(255,255,255,0.14)' />
                  <circle cx='780' cy='92' r='88' fill='rgba(255,255,255,0.11)' />
                  <circle cx='840' cy='420' r='128' fill='rgba(0,0,0,0.08)' />
                  <path d='M162 323h418l84 57H122l40-57z' fill='url(#carBody)' />
                  <path d='M302 225h170c58 0 102 26 138 78H228c24-44 40-78 74-78z' fill='rgba(255,255,255,0.88)' />
                  <path d='M338 236h122c30 0 54 12 76 36H286c14-26 28-36 52-36z' fill='rgba(143,196,255,0.56)' />
                  <circle cx='258' cy='379' r='46' fill='#101010' />
                  <circle cx='544' cy='379' r='46' fill='#101010' />
                  <circle cx='258' cy='379' r='19' fill='#dadada' />
                  <circle cx='544' cy='379' r='19' fill='#dadada' />
                  <text x='62' y='86' font-family='Segoe UI, Arial' font-size='28' font-weight='700' letter-spacing='4' fill='rgba(255,255,255,0.86)'>{header}</text>
                  <text x='60' y='150' font-family='Georgia, Times New Roman, serif' font-size='54' font-weight='700' fill='white'>{title}</text>
                  <text x='60' y='194' font-family='Segoe UI, Arial' font-size='23' fill='rgba(255,255,255,0.82)'>{subtitle}</text>
                  <text x='60' y='458' font-family='Segoe UI, Arial' font-size='24' font-weight='600' fill='rgba(255,255,255,0.76)'>AutoCarShowroom Database Seed</text>
                </svg>
                """;

            return $"data:image/svg+xml,{Uri.EscapeDataString(svg)}";
        }

        private static string XmlEscape(string value)
        {
            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("'", "&apos;", StringComparison.Ordinal);
        }

        private sealed record ModelSpec(
            string Brand,
            string Model,
            string BodyType,
            string Highlight,
            string PrimaryColor,
            string SecondaryColor,
            params VariantSpec[] Variants);

        private sealed record VariantSpec(
            string Name,
            int Year,
            decimal Price,
            string BuyerFocus);
    }
}
