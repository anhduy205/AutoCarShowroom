using System.Text;
using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Data
{
    public static class ShowroomDemoSeeder
    {
        private static readonly string[] CatalogLines =
        [
            "Toyota|Vios|Sedan|sedan hang B|su ben bi, de lai va chi phi so huu hop ly|Dong co xang 1.5L, hop so san hoac CVT tuy phien ban|5 cho|gia dinh nho, nguoi mua xe lan dau va nhom chay dich vu|E MT^2024^458000000^Trang ngoc^A;E CVT^2024^488000000^Bac kim^A;G CVT^2025^545000000^Den^A;GR-S^2026^630000000^Do^P",
            "Toyota|Corolla Cross|Crossover|crossover hang C|su can bang giua tinh thuc dung, tam nhin cao va kha nang di lai hang ngay|Dong co 1.8L xang hoac hybrid, hop so CVT|5 cho|gia dinh tre can xe gam cao gon gon de di pho va di xa|1.8G^2024^820000000^Trang^A;1.8V^2024^905000000^Den^A;1.8HEV^2025^955000000^Xanh^A;HEV Premium^2026^1020000000^Do^P",
            "Toyota|Innova Cross|MPV|MPV 7 cho|khoang cabin rong rai, linh hoat va hop gia dinh dong thanh vien|Dong co 2.0L xang hoac hybrid, hop so tu dong|7 cho|gia dinh can xe da dung cho cong viec, dua don va du lich|2.0G^2024^825000000^Bac^A;2.0V^2024^930000000^Trang ngoc^A;Hybrid^2025^1025000000^Nau dong^A;Hybrid Premium^2026^1095000000^Den^P",
            "Toyota|Fortuner|SUV|SUV 7 cho khung roi|su chac chan, tam ngoi cao va kha nang di duong truong tot|Dong co diesel 2.4L hoac 2.8L, hop so tu dong, cau sau hoac 4x4|7 cho|khach hang can SUV ben bi de di tinh, di gia dinh va du lich dai ngay|2.4 AT 4x2^2024^1055000000^Bac^A;Legender 4x2^2025^1185000000^Trang^P;2.8 AT 4x4^2025^1310000000^Den^A;Legender 4x4^2026^1375000000^Nau cat^A",
            "Toyota|Camry|Sedan|sedan hang D|phong thai doanh nhan, van hanh em va noi that sang|Dong co 2.0L, 2.5L hoac hybrid, hop so tu dong|5 cho|khach hang can sedan cao cap de di lai hang ngay va tiep khach|2.0Q^2024^1220000000^Den^A;2.5Q^2024^1450000000^Trang^A;HEV Mid^2025^1590000000^Xam^P;HEV Top^2026^1730000000^Do do^A",
            "Hyundai|Accent|Sedan|sedan hang B|thuc dung, de tiep can va nhieu trang bi cho nhu cau pho thong|Dong co xang 1.5L, hop so san hoac CVT|5 cho|nguoi can xe do thi gon gon, de bao duong va hop ngan sach|1.5 MT^2024^439000000^Trang^A;AT^2024^569000000^Xanh duong^A;AT Dac biet^2025^539000000^Xam^A;Cao cap^2026^579000000^Den^P",
            "Hyundai|Creta|Crossover|crossover hang B|de xoay so trong pho, tam nhin cao va hop gia dinh tre|Dong co xang 1.5L, hop so CVT|5 cho|nguoi chuyen tu sedan len SUV nho gon de di hang ngay|Tieu chuan^2024^640000000^Trang^A;Dac biet^2025^705000000^Do^A;Cao cap^2025^760000000^Den^A;N Line^2026^820000000^Xanh^P",
            "Hyundai|Tucson|Crossover|crossover hang C|thiet ke hien dai, cabin thoang va dung hoa cho gia dinh|Dong co 2.0L hoac 1.6 turbo, hop so tu dong, co ban AWD|5 cho|gia dinh can SUV hang C de di pho, di xa va cho nhieu hanh ly|2.0 Xang^2024^825000000^Trang^A;2.0 Dac biet^2025^910000000^Bac^A;1.6 Turbo^2025^995000000^Den^A;1.6 Turbo AWD^2026^1060000000^Xanh reu^P",
            "Hyundai|Santa Fe|SUV|SUV 7 cho|sang hon, rong hon va huong den nhom gia dinh can xe du lich|Dong co xang hoac turbo, hop so tu dong, co goi cau hinh AWD|6 cho hoac 7 cho|gia dinh muon xe gam cao cao cap cho nhieu chuyen di dai|Exclusive^2024^1070000000^Trang^A;Prestige^2025^1190000000^Den^A;Calligraphy^2025^1290000000^Nau^A;Calligraphy Turbo^2026^1375000000^Xam^P",
            "Hyundai|Stargazer X|MPV|MPV 7 cho do thi|de su dung cho gia dinh, kinh doanh nho va cac chuyen di cuoi tuan|Dong co xang 1.5L, hop so CVT|7 cho|khach hang can xe 7 cho nho gon de di pho nhung van de cho nguoi|Tieu chuan^2024^575000000^Bac^A;X Ban^2025^615000000^Trang^A;Cao cap^2025^675000000^Xanh^A;Premium^2026^720000000^Den^P",
            "Kia|K3|Sedan|sedan hang C|ngoai hinh tre trung, noi that de tiep can va trang bi phong phu|Dong co 1.6L hoac 2.0L, hop so san hoac tu dong|5 cho|gia dinh tre can sedan dep va de su dung hang ngay|1.6 MT^2024^549000000^Trang^A;Luxury^2024^629000000^Do^A;2.0 Premium^2025^699000000^Den^A;GT-Line^2026^759000000^Xam^P",
            "Kia|Seltos|Crossover|crossover hang B|hinh khoi gon, de nhin va hop nguoi tre|Dong co 1.5L hoac turbo, hop so CVT/tu dong|5 cho|nguoi muon SUV nho de di pho va co hinh anh nang dong|1.5 AT^2024^619000000^Trang^A;Deluxe^2025^664000000^Bac^A;Luxury^2025^729000000^Cam^A;Turbo GT-Line^2026^799000000^Den^P",
            "Kia|Sportage|Crossover|crossover hang C|thiet ke hien dai, khoang cabin thoang va nhieu cong nghe|Dong co 2.0L xang, 2.0 diesel hoac 1.6 turbo|5 cho|gia dinh can SUV hang C de di lai hang ngay va du lich|2.0G^2024^859000000^Trang^A;2.0D Luxury^2025^959000000^Den^A;1.6T Premium^2025^1029000000^Xanh duong^A;X-Line AWD^2026^1099000000^Xanh reu^P",
            "Kia|Sorento|SUV|SUV 7 cho|mang tinh gia dinh cao cap, cabin rong va di xa de chiu|Dong co diesel, xang hoac hybrid, hop so tu dong, co AWD|6 cho hoac 7 cho|gia dinh can xe lon, nhieu cong nghe va khoang hanh ly tot|2.2D Luxury^2024^1099000000^Trang^A;Premium^2025^1189000000^Nau^A;Signature AWD^2025^1299000000^Den^A;HEV Signature^2026^1419000000^Xam^P",
            "Kia|Carnival|MPV|MPV cao cap|khong gian noi that lon, hop cho gia dinh dong thanh vien va dich vu cao cap|Dong co diesel 2.2L hoac hybrid, hop so tu dong|7 cho hoac 8 cho|khach hang can MPV rong, ngoi thoai mai va dua don nhieu nguoi|2.2D Luxury^2024^1249000000^Trang^A;Premium^2025^1349000000^Den^A;Signature^2024^1589000000^Xanh than^A;Hybrid Premium^2026^1629000000^Xam^P",
            "Ford|Territory|Crossover|crossover hang C|de tiep can trong nhom SUV gia dinh, trang bi kha day va van hanh de lam quen|Dong co 1.5 turbo, hop so tu dong|5 cho|gia dinh tre can SUV thong minh de di lai trong thanh pho|Trend^2024^799000000^Trang^A;Titanium^2025^889000000^Bac^A;Sport^2025^929000000^Xanh^P;Titanium X^2026^969000000^Den^A",
            "Ford|Everest|SUV|SUV 7 cho khung roi|su manh me, chac chan va hop chuyen di dai ngay|Dong co diesel 2.0L, hop so tu dong, cau sau hoac 4x4|7 cho|khach hang can SUV ben bi cho gia dinh, cong viec va dia hinh hon hop|Ambiente^2024^1122000000^Trang^A;Sport^2025^1178000000^Cam^P;Titanium^2023^1468000000^Den^A;Platinum^2026^1545000000^Xam^A",
            "Ford|Ranger|Bán tải|ban tai du lich|ngoai hinh nam tinh, kha nang cho hang tot va di da dia hinh da dung|Dong co diesel, hop so tu dong, co cau sau hoac 4x4|5 cho|nguoi can xe vua phuc vu cong viec vua di gia dinh cuoi tuan|XLS 4x2 AT^2024^707000000^Trang^A;Sport 4x4^2025^864000000^Cam^A;Wildtrak^2025^979000000^Xanh duong^P;Stormtrak^2026^1039000000^Den^A",
            "Ford|Transit|Khác|xe cho khach thuong mai|thuc dung, de bao tri va hop dich vu dua don, van chuyen|Dong co diesel, hop so san hoac tu dong tuy cau hinh|16 cho|doanh nghiep can xe cho nhieu nguoi, phuc vu dua don va du lich|Tieu chuan^2024^905000000^Trang^A;Premium^2025^989000000^Bac^A;16 cho Luxury^2025^1069000000^Nau^A;Limousine^2026^1239000000^Den^P",
            "Ford|Explorer|SUV|SUV co lon nhap khau|phong cach My, than xe lon va khoang cabin rong|Dong co xang tang ap, hop so tu dong, dan dong 4 banh|7 cho|gia dinh can SUV nhap khau cao cap de di xa va su dung hang ngay|Limited^2024^2099000000^Trang^A;Limited+^2025^2199000000^Xam^A;Platinum^2025^2349000000^Den^P;Platinum 4WD^2026^2469000000^Xanh duong^A",
            "Mazda|Mazda3|Sedan|sedan hang C|thiet ke dep, lai de chiu va khoang lai gon gang|Dong co xang 1.5L, hop so tu dong|5 cho|khach hang tre can sedan dep, de su dung va co cam giac lai tot|1.5 Deluxe^2024^579000000^Trang^A;Luxury^2024^639000000^Do^A;Premium^2025^719000000^Xam^A;Signature^2026^759000000^Den^P",
            "Mazda|CX-3|Crossover|crossover hang B|nho gon, kha linh hoat trong do thi va giu ngon ngu thiet ke Mazda|Dong co xang 1.5L, hop so tu dong|5 cho|nguoi can SUV nho cho di lai hang ngay va gui xe de dang|1.5 AT^2024^544000000^Trang^A;Deluxe^2025^589000000^Do^A;Luxury^2025^639000000^Den^A;Premium^2026^699000000^Xam^P",
            "Mazda|CX-5|Crossover|crossover hang C|can bang giua thiet ke dep, su em ai va khoang cabin hop gia dinh|Dong co xang 2.0L hoac 2.5L, hop so tu dong|5 cho|gia dinh can SUV hang C de di xa, de di pho va co gia de tiep can|2.0 Deluxe^2024^769000000^Trang^A;Luxury^2024^819000000^Do^A;Premium Sport^2025^919000000^Den^A;2.5 Signature^2026^999000000^Xam^P",
            "Mazda|CX-8|SUV|SUV 6 cho/7 cho|huong den gia dinh can xe lon hon nhung van giu chat lai Mazda|Dong co xang 2.5L, hop so tu dong, co AWD|6 cho hoac 7 cho|gia dinh can SUV rong de di dai ngay va cho nhieu hanh ly|Luxury^2024^969000000^Trang^A;Premium^2025^1049000000^Nau^A;Signature AWD^2025^1149000000^Den^A;Premium Captain^2026^1189000000^Xanh duong^P",
            "Mazda|BT-50|Bán tải|ban tai du lich|chon loi song manh me, de lam viec va van giu duoc su thoai mai co ban|Dong co diesel 1.9L, hop so tu dong, cau sau hoac 4x4|5 cho|nguoi can pickup cho cong viec nhung van muon noi that nhin gon gang|1.9L 4x2 AT^2024^659000000^Trang^A;Deluxe^2025^739000000^Cam^A;Premium 4x2^2025^809000000^Den^A;Premium 4x4^2026^889000000^Xam^P",
            "Honda|City|Sedan|sedan hang B|de lai, tiet kiem va co goi an toan de tiep can|Dong co xang 1.5L, hop so CVT|5 cho|nguoi mua xe lan dau can sedan gon gang va thong minh|G^2024^559000000^Trang^A;L^2025^589000000^Xam^A;RS^2025^609000000^Do^P;RS Sunroof^2026^629000000^Den^A",
            "Honda|Civic|Sedan|sedan hang C|mang phong cach the thao, noi that hien dai va van hanh linh hoat|Dong co 1.5 turbo hoac hybrid, hop so CVT|5 cho|khach hang tre can sedan co cam giac lai tot va hinh anh ca tinh|G^2024^789000000^Trang^A;RS^2025^889000000^Do^P;e:HEV RS^2025^999000000^Den^A;Sport Touring^2026^1039000000^Xanh duong^A",
            "Honda|HR-V|Crossover|crossover hang B+|chon nguoi di pho nhung van can khoang ngoi thoang hon sedan|Dong co 1.5L hoac hybrid, hop so CVT|5 cho|gia dinh tre can SUV nho co chat van hanh Honda|G^2024^699000000^Trang^A;L^2025^799000000^Xam^A;RS^2025^869000000^Do^P;RS Hybrid^2026^919000000^Den^A",
            "Honda|CR-V|SUV|SUV hang C|rong rai, de dung cho gia dinh va co nhieu goi an toan|Dong co 1.5 turbo hoac hybrid, hop so CVT, co AWD|5 cho hoac 7 cho|gia dinh can SUV dung hoa cho nhieu kieu hanh trinh|G^2024^1109000000^Trang^A;L^2025^1159000000^Bac^A;L AWD^2025^1259000000^Den^A;e:HEV RS^2026^1359000000^Xanh duong^P",
            "Honda|BR-V|MPV|MPV 7 cho|nho gon, de xoay so va huong den nhom gia dinh can xe 7 cho hop ly|Dong co xang 1.5L, hop so CVT|7 cho|gia dinh can xe 7 cho nho de di pho va ve que cuoi tuan|G^2024^661000000^Trang^A;L^2025^705000000^Bac^A;L Honda Sensing^2025^745000000^Do^P;Premium^2026^789000000^Den^A",
            "Mitsubishi|Attrage|Sedan|sedan hang B|chi phi so huu thap, tiet kiem va de tiep can|Dong co xang 1.2L, hop so san hoac CVT|5 cho|nguoi can sedan gia hop ly cho nhu cau di lai co ban|MT^2024^389000000^Trang^A;CVT^2024^459000000^Bac^A;Premium^2025^499000000^Do^P;Premium Black Edition^2026^529000000^Den^A",
            "Mitsubishi|Xforce|Crossover|crossover hang B|phom dang moi me, khoang cabin kha thong thoang va ngon ngu SUV ro net|Dong co xang 1.5L, hop so CVT|5 cho|khach hang can B-SUV de di pho va su dung gia dinh hang ngay|GLX^2024^599000000^Trang^A;Exceed^2025^650000000^Xam^A;Premium^2025^705000000^Vang cat^P;Ultimate^2026^735000000^Den^A",
            "Mitsubishi|Xpander|MPV|MPV 7 cho|rat pho bien nho tinh thuc dung, rong va gia ban hop ly|Dong co xang 1.5L, hop so san hoac tu dong|7 cho|gia dinh dong thanh vien va nhom kinh doanh dich vu can xe ben va de dung|MT^2024^560000000^Trang^A;AT^2024^598000000^Bac^A;Premium^2025^658000000^Do^P;Cross^2026^699000000^Den^A",
            "Mitsubishi|Pajero Sport|SUV|SUV 7 cho khung roi|chon nguoi can xe chac chan, di dia hinh va hanh trinh dai|Dong co diesel, hop so tu dong, cau sau hoac 4x4|7 cho|khach hang can SUV dung ben cho gia dinh va cung duong da dang|4x2 AT^2024^1130000000^Trang^A;4x4 AT^2025^1330000000^Den^A;Athlete^2025^1380000000^Xam^P;Premium AWD^2026^1450000000^Nau^A",
            "Mitsubishi|Triton|Bán tải|ban tai du lich|manh me, kha nang cho hang tot va dien mao moi me hon|Dong co diesel, hop so tu dong, co ban 4x2 va 4x4|5 cho|nguoi can pickup cho cong viec nhung van muon phuc vu gia dinh|4x2 AT^2024^655000000^Trang^A;Athlete 4x2^2025^785000000^Cam^A;Athlete 4x4^2025^920000000^Den^P;Premium 4x4^2026^995000000^Xam^A",
            "VinFast|VF 3|Hatchback|xe dien do thi mini|nho gon, ca tinh va rat de xoay so trong thanh pho dong duc|Mo to dien, hop so don cap, van hanh gon nhe|4 cho|nguoi can xe dien cuc nho cho quang duong ngan va di lai noi thanh|Tieu chuan^2025^299000000^Vang^A;Urban^2025^315000000^Xanh reu^A;Adventure^2026^329000000^Cam^P;Color Pack^2026^339000000^Den^A",
            "VinFast|VF 5|Crossover|SUV dien hang A+|de tiep can, than xe gon va phu hop nhom khach hang tre|Mo to dien, dan dong cau truoc, hop so don cap|5 cho|nguoi muon chuyen sang xe dien voi tam gia hop ly|Base^2024^529000000^Trang^A;Plus^2025^548000000^Xanh duong^A;City Edition^2025^569000000^Vang^P;Premium^2026^589000000^Den^A",
            "VinFast|VF 6|Crossover|SUV dien hang B|dung hoa giua tam van hanh, kich thuoc va cong nghe|Mo to dien, dan dong cau truoc, hop so don cap|5 cho|gia dinh tre can xe dien gam cao cho nhu cau hang ngay|Eco^2024^689000000^Trang^A;Plus^2025^749000000^Xanh^A;Urban Plus^2025^779000000^Xam^P;Premium^2026^809000000^Den^A",
            "VinFast|VF 7|Crossover|SUV dien hang C|phong cach the thao, cabin hien dai va hinh anh tre trung|Mo to dien, cau truoc hoac AWD tuy phien ban|5 cho|khach hang can SUV dien co hinh anh noi bat va nhieu cong nghe|Eco^2024^799000000^Trang^A;Plus^2025^899000000^Do^A;Plus Sunroof^2025^949000000^Xam^P;Premium AWD^2026^999000000^Den^A",
            "VinFast|VF 8|SUV|SUV dien hang D|rong rai, hien dai va phu hop nhom gia dinh can xe dien lon|Mo to dien, dan dong cau sau hoac AWD tuy phien ban|5 cho|gia dinh can xe dien de di xa va co khoang cabin thoai mai|Eco^2024^1099000000^Trang^A;Plus^2025^1199000000^Xanh^A;Lux Plus^2025^1269000000^Xam^P;Premium AWD^2026^1349000000^Den^A",
            "Ferrari|296 GTB|Coupe|sieu xe hybrid|khi dong hoc cao, ti le dep va tinh huu dung cho ca duong pho lan su kien|Dong co V6 hybrid hieu nang cao, hop so ly hop kep|2 cho|nguoi choi xe muon Ferrari moi de trung bay, trai nghiem va suu tam|Assetto Fiorano^2025^15900000000^Do Rosso Corsa^A",
            "Ferrari|SF90 Spider|Mui trần|sieu xe plug-in hybrid|suc manh rat lon, mui xep va phong cach rat hop cho su kien cao cap|Dong co V8 hybrid, hop so ly hop kep, dan dong 4 banh|2 cho|khach hang can Ferrari mui tran cao cap cho bo suu tap dac biet|Performance^2025^34900000000^Vang Giallo Modena^A",
            "Ferrari|12Cilindri|Coupe|grand tourer V12|giu tinh than Ferrari dong co V12 voi phom dang GT sang trong|Dong co V12 dung tich lon, hop so ly hop kep|2+2 cho|nha suu tam muon mot mau GT Ferrari co gia tri lau dai|V12 Coupe^2026^24500000000^Xanh Blu Pozzi^A",
            "Ferrari|Purosangue|SUV|SUV hieu nang cao|ket hop cabin rong hon voi ban sac van hanh rat rieng cua Ferrari|Dong co V12, hop so tu dong ly hop kep, AWD|4 cho|khach hang can Ferrari de dung linh hoat hon nhung van rat doc dao|Luxury Performance^2026^39500000000^Do Bordeaux^A",
            "Ferrari|F80|Coupe|hypercar gioi han|cuc ky hiem, thiet ke rat giong xe y tuong va mang tinh bieu tuong cao|He truyen dong hybrid hieu nang dinh cao, khung carbon|2 cho|nha suu tam can mot mau Ferrari bieu tuong de trung bay lau dai|Collector Edition^2026^95000000000^Do Carbon^A",
            "Lamborghini|Temerario|Coupe|sieu xe hybrid|sac canh, moi me va mang hinh anh rat hop trung bay|Dong co hybrid hieu nang cao, hop so ly hop kep|2 cho|khach hang can Lamborghini doi moi, noi bat va de xuat hien su kien|Launch Edition^2026^23500000000^Cam Arancio^A",
            "Lamborghini|Revuelto|Coupe|hypercar hybrid|la bieu tuong V12 hybrid moi cua hang, cua cat canh va phong cach rat manh|Dong co V12 hybrid, hop so ly hop kep, dan dong 4 banh|2 cho|nguoi choi xe can mot Lamborghini dau bang cho bo suu tap|V12 Hybrid^2025^43900000000^Xanh Verde^A",
            "Lamborghini|Urus SE|SUV|SUV hieu nang cao|ket hop tinh thuc dung cua SUV va ngoai hinh Lamborghini rat ro net|Dong co hybrid, hop so tu dong, AWD|5 cho|khach hang can sieu SUV de di lai hang ngay va giu su khac biet|Plug-in Hybrid^2026^22900000000^Vang^A",
            "Lamborghini|Huracan STO|Coupe|sieu xe dinh huong track|ham ho, muc do trung bay cao va rat hop su kien xe hieu nang|Dong co V10, hop so ly hop kep, dan dong cau sau|2 cho|nguoi thich Lamborghini thuan chat, de trung bay va lai su kien|Track Focused^2025^30900000000^Xanh duong^A",
            "Lamborghini|Huracan Sterrato|Coupe|sieu xe all-terrain|la la, hiem va khac biet nhat trong danh muc Lamborghini hien nay|Dong co V10, hop so ly hop kep, dan dong 4 banh|2 cho|khach hang muon mot Lamborghini hiem, la va de gay chu y|All-Terrain^2025^26900000000^Xanh reu^A",
        ];

        private static readonly IReadOnlyList<ModelSeed> ModelSeeds = ParseCatalogData();

        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ShowroomDbContext>();
            var environment = services.GetRequiredService<IWebHostEnvironment>();

            var seedCars = BuildSeedCars(environment.WebRootPath);
            var seedLookup = seedCars.ToDictionary(car => BuildKey(car.Brand, car.CarName, car.Year), StringComparer.OrdinalIgnoreCase);
            var existingCars = await context.Cars.ToListAsync();
            var existingLookup = existingCars
                .GroupBy(car => BuildKey(car.Brand, car.CarName, car.Year), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var seedCar in seedCars)
            {
                var key = BuildKey(seedCar.Brand, seedCar.CarName, seedCar.Year);

                if (existingLookup.TryGetValue(key, out var existingCar))
                {
                    ApplySeedValues(existingCar, seedCar);
                }
                else
                {
                    context.Cars.Add(seedCar);
                }
            }

            var legacyDemoCars = existingCars
                .Where(car => IsLegacyDemoCar(car) && !seedLookup.ContainsKey(BuildKey(car.Brand, car.CarName, car.Year)))
                .ToList();

            if (legacyDemoCars.Count > 0)
            {
                context.Cars.RemoveRange(legacyDemoCars);
            }

            await context.SaveChangesAsync();
        }

        private static List<Car> BuildSeedCars(string webRootPath)
        {
            var cars = ModelSeeds
                .SelectMany(model => model.Variants.Select(variant => CreateCar(model, variant, webRootPath)))
                .ToList();

            ValidateSeedCounts(cars);
            return cars;
        }

        private static void ValidateSeedCounts(IReadOnlyCollection<Car> cars)
        {
            if (cars.Count != 170)
            {
                throw new InvalidOperationException($"Seed catalog must contain 170 cars but currently has {cars.Count}.");
            }

            var brandCounts = cars
                .GroupBy(car => car.Brand, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (var brand in CarCatalogMetadata.MainstreamBrands)
            {
                if (!brandCounts.TryGetValue(brand, out var count) || count != 20)
                {
                    throw new InvalidOperationException($"Brand {brand} must contain 20 cars in the seed catalog.");
                }
            }

            foreach (var brand in CarCatalogMetadata.SupercarBrands)
            {
                if (!brandCounts.TryGetValue(brand, out var count) || count != 5)
                {
                    throw new InvalidOperationException($"Brand {brand} must contain 5 cars in the seed catalog.");
                }
            }
        }

        private static IReadOnlyList<ModelSeed> ParseCatalogData()
        {
            var models = new List<ModelSeed>();

            foreach (var line in CatalogLines)
            {
                var parts = line.Split('|');

                if (parts.Length != 9)
                {
                    throw new InvalidOperationException($"Invalid catalog line: {line}");
                }

                var variants = parts[8]
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(ParseVariant)
                    .ToArray();

                models.Add(new ModelSeed(
                    parts[0],
                    parts[1],
                    parts[2],
                    parts[3],
                    parts[4],
                    parts[5],
                    parts[6],
                    parts[7],
                    variants));
            }

            if (models.Count != 50)
            {
                throw new InvalidOperationException($"Seed catalog must contain 50 models but currently has {models.Count}.");
            }

            return models;
        }

        private static VariantSeed ParseVariant(string rawVariant)
        {
            var parts = rawVariant.Split('^');

            if (parts.Length != 5)
            {
                throw new InvalidOperationException($"Invalid variant definition: {rawVariant}");
            }

            return new VariantSeed(
                parts[0],
                int.Parse(parts[1]),
                decimal.Parse(parts[2]),
                parts[3],
                ParseStatus(parts[4]));
        }

        private static string ParseStatus(string statusCode)
        {
            return statusCode switch
            {
                "A" => OrderWorkflow.CarStatusAvailable,
                "P" => OrderWorkflow.CarStatusPromotion,
                "S" => OrderWorkflow.CarStatusSold,
                _ => throw new InvalidOperationException($"Unsupported status code: {statusCode}")
            };
        }

        private static Car CreateCar(ModelSeed model, VariantSeed variant, string webRootPath)
        {
            var fullName = $"{model.Brand} {model.ModelName} {variant.Name}".Trim();

            return new Car
            {
                Brand = model.Brand,
                ModelName = model.ModelName,
                CarName = fullName,
                Price = variant.Price,
                Year = variant.Year,
                Color = ToDisplayColor(variant.Color),
                BodyType = NormalizeBodyType(model.BodyType),
                Status = variant.Status,
                Image = ResolveSeedImage(webRootPath, model.Brand, model.ModelName),
                Specifications = BuildGeneralInformation(model, variant, fullName),
                Description = BuildDescription(model, variant, fullName),
                EngineAndChassis = BuildEngineAndChassis(model, variant),
                Exterior = BuildExterior(model, variant),
                Interior = BuildInterior(model, variant),
                Seats = BuildSeats(model, variant),
                Convenience = BuildConvenience(model, variant),
                SecurityAndAntiTheft = BuildSecurity(model, variant),
                ActiveSafety = BuildActiveSafety(model, variant),
                PassiveSafety = BuildPassiveSafety(model, variant)
            };
        }

        private static string BuildGeneralInformation(ModelSeed model, VariantSeed variant, string fullName)
        {
            return $"{fullName} là mẫu {GetBodyTypeDescription(model.BodyType)}, màu {ToDisplayColor(variant.Color)}, đời {variant.Year}. Phiên bản này hướng tới {GetTargetCustomer(model)}, có giá tham khảo {FormatCurrency(variant.Price)} và phù hợp để trưng bày trên website showroom với đầy đủ thông tin cơ bản.";
        }

        private static string BuildDescription(ModelSeed model, VariantSeed variant, string fullName)
        {
            var statusNote = variant.Status == OrderWorkflow.CarStatusPromotion
                ? "Phiên bản này đang được gắn nhãn khuyến mãi để người xem dễ nhận biết trên giao diện."
                : "Phiên bản này đang ở trạng thái còn hàng để phục vụ quy trình xem xe và đặt mua.";

            return $"{fullName} nổi bật với {GetShowcaseCharacter(model.BodyType, variant.Price)}. Cấu hình {variant.Name} vẫn giữ bản sắc của dòng xe, đồng thời phù hợp cho {GetTargetCustomer(model)}. {statusNote}";
        }

        private static string BuildEngineAndChassis(ModelSeed model, VariantSeed variant)
        {
            return $"{TranslatePowertrain(model.Powertrain)}. Khung gầm được cân chỉnh theo hướng {GetChassisTone(model.BodyType)}, giúp xe phù hợp hơn với điều kiện vận hành tại Việt Nam. Phiên bản {variant.Name} ưu tiên cảm giác lái {GetDrivingTone(model.BodyType)} và độ ổn định tốt trong quá trình sử dụng hằng ngày.";
        }

        private static string BuildExterior(ModelSeed model, VariantSeed variant)
        {
            return $"Ngoại thất theo ngôn ngữ thiết kế {GetExteriorTone(model.BodyType)}. Màu {ToDisplayColor(variant.Color)} giúp {model.ModelName} nổi bật hơn ở phần đầu xe, bộ mâm, cụm đèn và tổng thể thân xe khi trưng bày tại showroom.";
        }

        private static string BuildInterior(ModelSeed model, VariantSeed variant)
        {
            return $"Khoang cabin được bố trí theo hướng {GetInteriorTone(model.Segment)}, dễ quan sát và dễ thao tác trong quá trình sử dụng hằng ngày. Bảng tablo, màn hình trung tâm và các bề mặt tiếp xúc được mô tả theo phong cách phù hợp với phiên bản {variant.Name} và tầm giá {FormatCurrency(variant.Price)}.";
        }

        private static string BuildSeats(ModelSeed model, VariantSeed variant)
        {
            return $"Cấu hình ghế {NormalizeSeating(model.Seating)}. Cách bố trí hàng ghế ưu tiên {GetSeatTone(model.BodyType, model.Seating)}, phù hợp cho {GetTargetCustomer(model)}. Trên phiên bản {variant.Name}, khả năng gập/chỉnh ghế được định hướng theo tiêu chí dễ dùng và thoải mái khi đi xa.";
        }

        private static string BuildConvenience(ModelSeed model, VariantSeed variant)
        {
            return $"Trang bị tiện nghi tập trung vào {GetConvenienceTone(model.BodyType, variant.Price)}, bao gồm màn hình giải trí, kết nối điện thoại, điều hòa, cổng sạc, camera và các tính năng hỗ trợ sử dụng cơ bản. Với các phiên bản giá cao hơn, xe được định hướng thêm các tiện ích nâng tầm trải nghiệm như ghế chỉnh điện, cốp điện hoặc gói công nghệ riêng của từng dòng.";
        }

        private static string BuildSecurity(ModelSeed model, VariantSeed variant)
        {
            var securityTone = variant.Price >= 2500000000m
                ? "hệ thống khóa thông minh, immobilizer, báo động và giám sát xe theo hướng cao cấp hơn"
                : "khóa thông minh, immobilizer, báo động cơ bản và các lớp bảo vệ dễ dùng";

            return $"Xe được bố trí {securityTone}. Nhóm tính năng an ninh được xây dựng để phù hợp với vai trò của {model.ModelName}, giúp người dùng yên tâm hơn khi sử dụng xe hằng ngày, gửi xe lâu hơn hoặc trưng bày tại showroom.";
        }

        private static string BuildActiveSafety(ModelSeed model, VariantSeed variant)
        {
            return $"Gói an toàn chủ động gồm {GetActiveSafetyPackage(model.BodyType, variant.Price)}. Cách mô tả này giúp trang chi tiết xe có đủ thông tin như một website bán xe, đồng thời vẫn gọn và dễ hiểu cho đồ án.";
        }

        private static string BuildPassiveSafety(ModelSeed model, VariantSeed variant)
        {
            var airbags = GetAirbagCount(variant.Price, model.BodyType);
            var bodyShell = variant.Price >= 15000000000m
                ? "khung thân xe vật liệu hiệu năng cao và vùng bảo vệ người ngồi"
                : "khung thân xe cứng vững, vùng hấp thụ lực và dây đai 3 điểm";
            var familyFeature = model.BodyType is "SUV" or "MPV" or "Crossover"
                ? "có thêm móc ghế trẻ em ISOFIX và nhấn mạnh sự ổn định cho nhóm gia đình"
                : "duy trì các trang bị cơ bản hướng tới bảo vệ người ngồi";

            return $"An toàn bị động gồm {bodyShell}, {airbags} túi khí tùy tầm giá và cấu hình, cùng tựa đầu, dây đai an toàn và các điểm gia cố khoang hành khách. Mẫu xe này {familyFeature}.";
        }

        private static void ApplySeedValues(Car target, Car source)
        {
            target.Brand = source.Brand;
            target.ModelName = source.ModelName;
            target.CarName = source.CarName;
            target.Price = source.Price;
            target.Year = source.Year;
            target.Color = source.Color;
            target.BodyType = source.BodyType;
            target.Status = source.Status;
            target.Image = source.Image;
            target.Specifications = source.Specifications;
            target.Description = source.Description;
            target.EngineAndChassis = source.EngineAndChassis;
            target.Exterior = source.Exterior;
            target.Interior = source.Interior;
            target.Seats = source.Seats;
            target.Convenience = source.Convenience;
            target.SecurityAndAntiTheft = source.SecurityAndAntiTheft;
            target.ActiveSafety = source.ActiveSafety;
            target.PassiveSafety = source.PassiveSafety;
        }

        private static bool IsLegacyDemoCar(Car car)
        {
            return !string.IsNullOrWhiteSpace(car.Image) &&
                (car.Image.StartsWith("/images/demo-cars/", StringComparison.OrdinalIgnoreCase) ||
                 car.Image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildKey(string brand, string carName, int year)
        {
            return $"{brand}|{carName}|{year}";
        }

        private static string ResolveSeedImage(string webRootPath, string brand, string modelName)
        {
            return TryGetCatalogImagePath(webRootPath, brand, modelName)
                ?? BuildImageDataUri(brand, modelName);
        }

        private static string? TryGetCatalogImagePath(string webRootPath, string brand, string modelName)
        {
            var fileBaseName = Slugify($"{brand}-{modelName}");
            var imageFolders = new[]
            {
                (PhysicalPath: Path.Combine(webRootPath, "images", "catalog"), WebPath: "/images/catalog")
            };

            foreach (var (physicalPath, webPath) in imageFolders)
            {
                foreach (var extension in new[] { ".jpg", ".jpeg", ".png", ".webp" })
                {
                    var filePath = Path.Combine(physicalPath, $"{fileBaseName}{extension}");

                    if (File.Exists(filePath))
                    {
                        return $"{webPath}/{fileBaseName}{extension}";
                    }
                }
            }

            return null;
        }

        private static string BuildImageDataUri(string brand, string modelName)
        {
            var (primary, secondary) = GetBrandPalette(brand);
            var header = XmlEscape(brand);
            var title = XmlEscape(modelName);

            var svg = $"""
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 960 540'>
                  <defs>
                    <linearGradient id='bg' x1='0' y1='0' x2='1' y2='1'>
                      <stop offset='0%' stop-color='{primary}' />
                      <stop offset='100%' stop-color='{secondary}' />
                    </linearGradient>
                  </defs>
                  <rect width='960' height='540' fill='url(#bg)' />
                  <circle cx='180' cy='120' r='110' fill='rgba(255,255,255,0.16)' />
                  <circle cx='770' cy='400' r='132' fill='rgba(0,0,0,0.08)' />
                  <path d='M170 328h426l92 58H130l40-58z' fill='rgba(18,18,18,0.84)' />
                  <path d='M312 230h172c58 0 102 28 138 80H240c22-44 38-80 72-80z' fill='rgba(255,255,255,0.92)' />
                  <path d='M346 240h126c32 0 56 12 76 36H294c14-24 28-36 52-36z' fill='rgba(145,205,255,0.52)' />
                  <circle cx='270' cy='386' r='46' fill='#101010' />
                  <circle cx='558' cy='386' r='46' fill='#101010' />
                  <circle cx='270' cy='386' r='19' fill='#dadada' />
                  <circle cx='558' cy='386' r='19' fill='#dadada' />
                  <text x='62' y='86' font-family='Segoe UI, Arial' font-size='28' font-weight='700' letter-spacing='4' fill='rgba(255,255,255,0.86)'>{header}</text>
                  <text x='60' y='150' font-family='Georgia, Times New Roman, serif' font-size='54' font-weight='700' fill='white'>{title}</text>
                  <text x='60' y='458' font-family='Segoe UI, Arial' font-size='24' font-weight='600' fill='rgba(255,255,255,0.76)'>AutoCarShowroom Seed Fallback</text>
                </svg>
                """;

            return $"data:image/svg+xml,{Uri.EscapeDataString(svg)}";
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

        private static (string Primary, string Secondary) GetBrandPalette(string brand)
        {
            return brand switch
            {
                "Toyota" => ("#a63d40", "#f9dcc4"),
                "Hyundai" => ("#355070", "#dbe8ff"),
                "Kia" => ("#7c3e66", "#f5d9ea"),
                "Ford" => ("#1d3557", "#d8e5f3"),
                "Mazda" => ("#6d597a", "#efe3ff"),
                "Honda" => ("#bc4749", "#ffe0e0"),
                "Mitsubishi" => ("#386641", "#e3f0d8"),
                "VinFast" => ("#0077b6", "#d7f2ff"),
                "Ferrari" => ("#8b0000", "#ffd8d8"),
                "Lamborghini" => ("#ff7b00", "#ffe7cc"),
                _ => ("#33506b", "#d8e4ef")
            };
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

        private static string NormalizeBodyType(string bodyType)
        {
            return bodyType switch
            {
                "BÃ¡n táº£i" => "Bán tải",
                "Mui tráº§n" => "Mui trần",
                "KhÃ¡c" => "Khác",
                _ => bodyType
            };
        }

        private static string ToDisplayColor(string color)
        {
            return color switch
            {
                "Trang" => "Trắng",
                "Trang ngoc" => "Trắng ngọc",
                "Bac" => "Bạc",
                "Bac kim" => "Bạc kim",
                "Den" => "Đen",
                "Do" => "Đỏ",
                "Do do" => "Đỏ đô",
                "Do Rosso Corsa" => "Đỏ Rosso Corsa",
                "Do Bordeaux" => "Đỏ Bordeaux",
                "Do Carbon" => "Đỏ Carbon",
                "Xanh" => "Xanh",
                "Xanh duong" => "Xanh dương",
                "Xanh reu" => "Xanh rêu",
                "Xanh than" => "Xanh than",
                "Xanh Blu Pozzi" => "Xanh Blu Pozzi",
                "Xam" => "Xám",
                "Nau" => "Nâu",
                "Nau dong" => "Nâu đồng",
                "Nau cat" => "Nâu cát",
                "Cam" => "Cam",
                "Cam Arancio" => "Cam Arancio",
                "Vang" => "Vàng",
                "Vang cat" => "Vàng cát",
                "Vang Giallo Modena" => "Vàng Giallo Modena",
                _ => color
            };
        }

        private static string NormalizeSeating(string seating)
        {
            return seating
                .Replace(" cho", " chỗ", StringComparison.OrdinalIgnoreCase)
                .Replace(" hoac ", " hoặc ", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatCurrency(decimal price)
        {
            return $"{price:N0} VNĐ";
        }

        private static string GetTargetCustomer(ModelSeed model)
        {
            var bodyType = NormalizeBodyType(model.BodyType);
            var isPremiumLine = model.Variants.Max(variant => variant.Price) >= 1_000_000_000m;

            return bodyType switch
            {
                "SUV" => model.Seating.Contains("7", StringComparison.OrdinalIgnoreCase)
                    ? "gia đình cần xe gầm cao rộng rãi cho nhiều hành trình"
                    : "khách hàng cần xe gầm cao cân bằng giữa đi phố và đi xa",
                "Sedan" => isPremiumLine
                    ? "khách hàng ưu tiên sự lịch lãm, êm ái và hình ảnh chỉn chu"
                    : "người mua xe lần đầu hoặc gia đình nhỏ cần sedan dễ sử dụng",
                "Hatchback" => "người dùng đô thị cần xe nhỏ gọn, linh hoạt và cá tính",
                "MPV" => "gia đình đông thành viên hoặc người cần xe chở nhiều người",
                "Crossover" => "gia đình trẻ cần xe gầm cao gọn gàng để đi lại hằng ngày",
                "Bán tải" => "người cần xe vừa phục vụ công việc vừa đi lại cuối tuần",
                "Coupe" => isPremiumLine
                    ? "người chơi xe yêu thích cảm giác lái và giá trị trưng bày"
                    : "khách hàng muốn kiểu dáng thể thao và cảm xúc sau vô-lăng",
                "Mui trần" => "người yêu trải nghiệm mui mở và phong cách nổi bật",
                _ => "doanh nghiệp cần xe phục vụ vận chuyển hoặc đưa đón"
            };
        }

        private static string GetShowcaseCharacter(string bodyType, decimal price)
        {
            return NormalizeBodyType(bodyType) switch
            {
                "SUV" => "thân xe đầm chắc, khoảng sáng gầm hợp lý và dáng đứng nổi bật",
                "Sedan" => price >= 1_000_000_000m
                    ? "phong thái lịch lãm, vận hành êm và khoang lái gọn gàng"
                    : "sự cân bằng giữa êm ái, tiết kiệm và dễ điều khiển",
                "Hatchback" => "kích thước gọn, xoay trở linh hoạt và hình ảnh trẻ trung",
                "MPV" => "không gian rộng, cách bố trí linh hoạt và sự tiện dụng cho gia đình",
                "Crossover" => "thiết kế hiện đại, tầm quan sát tốt và khả năng đi lại đa dụng",
                "Bán tải" => "ngoại hình nam tính, khả năng chở hàng tốt và phong cách đồng hành",
                "Coupe" => price >= 8_000_000_000m
                    ? "dáng coupe thấp rộng, cảm giác tốc độ rõ và sức hút trưng bày rất cao"
                    : "phom coupe thể thao, thấp và tập trung nhiều vào người lái",
                "Mui trần" => "trải nghiệm mui mở giàu cảm xúc và hình ảnh trình diễn nổi bật",
                _ => "sự thực dụng, rõ ràng và hiệu quả trong quá trình sử dụng"
            };
        }

        private static string TranslatePowertrain(string powertrain)
        {
            return powertrain
                .Replace("Mo to dien", "Mô tơ điện", StringComparison.OrdinalIgnoreCase)
                .Replace("Dong co", "Động cơ", StringComparison.OrdinalIgnoreCase)
                .Replace("hop so", "hộp số", StringComparison.OrdinalIgnoreCase)
                .Replace("xang", "xăng", StringComparison.OrdinalIgnoreCase)
                .Replace("san", "sàn", StringComparison.OrdinalIgnoreCase)
                .Replace("hoac", "hoặc", StringComparison.OrdinalIgnoreCase)
                .Replace("tuy phien ban", "tùy phiên bản", StringComparison.OrdinalIgnoreCase)
                .Replace("dan dong", "dẫn động", StringComparison.OrdinalIgnoreCase)
                .Replace("cau truoc", "cầu trước", StringComparison.OrdinalIgnoreCase)
                .Replace("cau sau", "cầu sau", StringComparison.OrdinalIgnoreCase)
                .Replace("ly hop kep", "ly hợp kép", StringComparison.OrdinalIgnoreCase)
                .Replace("dung tich lon", "dung tích lớn", StringComparison.OrdinalIgnoreCase)
                .Replace("hieu nang cao", "hiệu năng cao", StringComparison.OrdinalIgnoreCase)
                .Replace("dinh cao", "đỉnh cao", StringComparison.OrdinalIgnoreCase)
                .Replace("don cap", "đơn cấp", StringComparison.OrdinalIgnoreCase)
                .Replace("co AWD", "có AWD", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetBodyTypeDescription(string bodyType)
        {
            return NormalizeBodyType(bodyType) switch
            {
                "SUV" => "dáng SUV gầm cao",
                "Sedan" => "dáng sedan 4 cửa",
                "Hatchback" => "dáng hatchback nhỏ gọn",
                "MPV" => "dáng MPV linh hoạt cho gia đình",
                "Crossover" => "dáng crossover đa dụng",
                "Bán tải" => "dáng bán tải phục vụ công việc và dã ngoại",
                "Coupe" => "dáng coupe hiệu năng cao",
                "Mui trần" => "dáng mui trần nhấn mạnh trải nghiệm mở",
                _ => "dáng xe đa mục đích"
            };
        }

        private static string GetDrivingTone(string bodyType)
        {
            return NormalizeBodyType(bodyType) switch
            {
                "SUV" => "đầm chắc và tầm nhìn cao",
                "Sedan" => "êm và ổn định",
                "Hatchback" => "linh hoạt trong phố",
                "MPV" => "nhẹ nhàng và dễ làm quen",
                "Crossover" => "cân bằng giữa dễ lái và sự đa dụng",
                "Bán tải" => "bền bỉ, có lực kéo và hợp nhiều mặt đường",
                "Coupe" => "nhanh, sát mặt đường và phản hồi trực diện",
                "Mui trần" => "cảm xúc và hướng tới trải nghiệm mở",
                _ => "dễ tiếp cận"
            };
        }

        private static string GetChassisTone(string bodyType)
        {
            return NormalizeBodyType(bodyType) switch
            {
                "SUV" => "đầm thân, ổn định thân xe và hợp đường trường",
                "Sedan" => "êm ái và cân bằng giữa thoải mái với khả năng ôm cua",
                "Hatchback" => "nhỏ gọn, phản hồi nhanh và dễ xoay sở",
                "MPV" => "dễ chịu cho nhiều hành khách và linh hoạt hàng ghế",
                "Crossover" => "vững chắc, tầm quan sát thoáng và hợp nhiều kiểu hành trình",
                "Bán tải" => "gân guốc, tải trọng tốt và bền bỉ cho công việc",
                "Coupe" => "thấp, cứng và ưu tiên khả năng bám đường",
                "Mui trần" => "thấp, thể thao và hướng tới trải nghiệm mở",
                _ => "thực dụng và dễ bảo trì"
            };
        }

        private static string GetExteriorTone(string bodyType)
        {
            return NormalizeBodyType(bodyType) switch
            {
                "SUV" => "mạnh khối và tư thế cao",
                "Sedan" => "thanh lịch và cân đối",
                "Hatchback" => "trẻ trung, gọn gàng",
                "MPV" => "thực dụng, dễ nhận biết",
                "Crossover" => "hiện đại và đa dụng",
                "Bán tải" => "nam tính và có chất đồng hành",
                "Coupe" => "thấp, rộng và nhấn mạnh tốc độ",
                "Mui trần" => "quyến rũ và hướng sự kiện",
                _ => "gọn và rõ ràng"
            };
        }

        private static string GetInteriorTone(string segment)
        {
            return segment switch
            {
                var value when value.Contains("hypercar", StringComparison.OrdinalIgnoreCase) => "tập trung mạnh vào người lái và tri ân giá trị sưu tầm",
                var value when value.Contains("sieu xe", StringComparison.OrdinalIgnoreCase) => "thấp, ôm người và hướng tới trải nghiệm",
                var value when value.Contains("cao cap", StringComparison.OrdinalIgnoreCase) => "sang, dễ ngồi lâu và hợp tiếp khách",
                var value when value.Contains("SUV", StringComparison.OrdinalIgnoreCase) => "rộng, dễ bước vào và dễ quan sát",
                var value when value.Contains("MPV", StringComparison.OrdinalIgnoreCase) => "linh hoạt, thoáng và dễ thao tác cho nhiều người",
                _ => "gọn gàng, hiện đại và dễ sử dụng"
            };
        }

        private static string GetSeatTone(string bodyType, string seating)
        {
            return NormalizeBodyType(bodyType) switch
            {
                "SUV" => seating.Contains("7", StringComparison.OrdinalIgnoreCase)
                    ? "hàng 2 và hàng 3 dễ chia người, gập hoặc gập điện linh hoạt"
                    : "ghế trước thoáng, hàng sau đủ để chân cho gia đình",
                "MPV" => "ba hàng ghế linh hoạt, dễ gập và dễ bước vào",
                "Bán tải" => "hàng ghế trước và sau cân bằng cho công việc lẫn đi xa",
                "Coupe" => "ghế trước ôm người, ưu tiên vị trí lái",
                "Mui trần" => "ghế trước ôm và nhấn mạnh trải nghiệm lái mở",
                _ => "khoang ngồi dễ tiếp cận, phù hợp nhiều đối tượng sử dụng"
            };
        }

        private static string GetConvenienceTone(string bodyType, decimal price)
        {
            if (price >= 15000000000m)
            {
                return "cụm điều khiển hiệu năng cao, giao diện xe sang và các tính năng phục vụ trưng bày hoặc sự kiện";
            }

            if (price >= 2000000000m)
            {
                return "màn hình lớn, camera 360, ghế chỉnh điện, âm thanh tốt và điều hòa đa vùng";
            }

            if (price >= 900000000m)
            {
                return NormalizeBodyType(bodyType) == "MPV"
                    ? "điều hòa nhiều vùng, ghế sau dễ chịu, cửa hoặc cốp điện và camera hỗ trợ"
                    : "màn hình trung tâm, kết nối thông minh, điều hòa tự động, camera và hỗ trợ đỗ xe";
            }

            return "các tính năng cơ bản như màn hình, Bluetooth, điều hòa, cổng sạc và camera hoặc cảm biến";
        }

        private static string GetActiveSafetyPackage(string bodyType, decimal price)
        {
            if (price >= 15000000000m)
            {
                return "phanh hiệu năng cao, kiểm soát lực kéo, cân bằng điện tử, camera hoặc cảm biến và các chế độ lái theo tình huống";
            }

            if (price >= 1200000000m)
            {
                return "ABS, EBD, BA, cân bằng điện tử, hỗ trợ khởi hành ngang dốc, camera 360, cảnh báo điểm mù, giữ làn và ga tự động thích ứng tùy phiên bản";
            }

            if (price >= 700000000m)
            {
                return "ABS, EBD, BA, cân bằng điện tử, hỗ trợ khởi hành ngang dốc, camera lùi, cảm biến và một số tính năng ADAS cơ bản";
            }

            return "ABS, EBD, BA, cân bằng điện tử, camera lùi và các hệ thống can thiệp phanh cơ bản";
        }

        private static int GetAirbagCount(decimal price, string bodyType)
        {
            if (price >= 15000000000m)
            {
                return 6;
            }

            if (price >= 1200000000m)
            {
                return bodyType is "SUV" or "MPV" ? 7 : 6;
            }

            if (price >= 700000000m)
            {
                return 6;
            }

            return 4;
        }

        private sealed record ModelSeed(
            string Brand,
            string ModelName,
            string BodyType,
            string Segment,
            string Highlight,
            string Powertrain,
            string Seating,
            string Usage,
            IReadOnlyList<VariantSeed> Variants);

        private sealed record VariantSeed(
            string Name,
            int Year,
            decimal Price,
            string Color,
            string Status);
    }
}
