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
                Color = variant.Color,
                BodyType = model.BodyType,
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
            return $"{fullName} la mau {model.Segment}, than xe {GetBodyTypeDescription(model.BodyType)}, mau {variant.Color}, doi {variant.Year}. Mau xe huong toi {model.Usage}, gia tham khao {variant.Price:N0} VNĐ va phu hop de dua len web showroom co du thong tin mo ta.";
        }

        private static string BuildDescription(ModelSeed model, VariantSeed variant, string fullName)
        {
            var statusNote = variant.Status == OrderWorkflow.CarStatusPromotion
                ? "Phien ban nay dang duoc gan nhan khuyen mai de de nhan biet tren web."
                : "Phien ban nay dang o trang thai con hang de phuc vu demo quy trinh mua xe.";

            return $"{fullName} noi bat voi {model.Highlight}. Cau hinh {variant.Name} giu duoc tinh cach cua dong xe, dong thoi de tiep can cho nhu cau {model.Usage}. {statusNote}";
        }

        private static string BuildEngineAndChassis(ModelSeed model, VariantSeed variant)
        {
            return $"{model.Powertrain}. Khung gam duoc can chinh theo huong {GetChassisTone(model.BodyType)}, giup xe phu hop hon voi dieu kien van hanh tai Viet Nam. Phien ban {variant.Name} uu tien cam giac lai {GetDrivingTone(model.BodyType)} va do on dinh phu hop voi nhom khach hang muc tieu.";
        }

        private static string BuildExterior(ModelSeed model, VariantSeed variant)
        {
            return $"Ngoai that theo ngon ngu thiet ke {GetExteriorTone(model.BodyType)}, nhan manh dac trung {model.Highlight}. Mau {variant.Color} giup xe len hinh dep hon tren giao dien showroom, de lam noi bat dau xe, bo mam, cum den va tong the than xe.";
        }

        private static string BuildInterior(ModelSeed model, VariantSeed variant)
        {
            return $"Khoang cabin duoc bo tri theo huong {GetInteriorTone(model.Segment)}, de quan sat va de thao tac trong qua trinh su dung hang ngay. Cac be mat noi that, bang tablo va man hinh trung tam duoc mo ta theo phong cach hop voi phien ban {variant.Name} va tam gia {variant.Price:N0} VNĐ.";
        }

        private static string BuildSeats(ModelSeed model, VariantSeed variant)
        {
            return $"Cau hinh ghe {model.Seating}. Cach bo tri hang ghe uu tien {GetSeatTone(model.BodyType, model.Seating)}, phu hop cho {model.Usage}. Tren phien ban {variant.Name}, chat lieu va kha nang gap/chinh ghe duoc mo ta theo huong de dung, de tiep can va phuc vu trai nghiem ngoi dai hon.";
        }

        private static string BuildConvenience(ModelSeed model, VariantSeed variant)
        {
            return $"Trang bi tien nghi tap trung vao {GetConvenienceTone(model.BodyType, variant.Price)}, bao gom man hinh giai tri, ket noi dien thoai, dieu hoa, cong sac, camera va cac tinh nang ho tro su dung co ban. Voi cac mau gia cao hon, xe duoc dinh huong them cop dien, ghe chinh dien, am thanh tot hon hoac goi cong nghe theo tinh cach cua dong xe.";
        }

        private static string BuildSecurity(ModelSeed model, VariantSeed variant)
        {
            var securityTone = variant.Price >= 2500000000m
                ? "he thong khoa thong minh, immobilizer, bao dong va giam sat xe theo huong cao cap hon"
                : "khoa thong minh, immobilizer, bao dong co ban va cac lop bao ve de dung";

            return $"Xe duoc bo tri {securityTone}. Nhom tinh nang an ninh duoc xay dung de phu hop voi vai tro cua {model.ModelName}, giup nguoi dung yen tam hon khi dung xe hang ngay, gui xe lau hon hoac trung bay tai showroom.";
        }

        private static string BuildActiveSafety(ModelSeed model, VariantSeed variant)
        {
            return $"Goi an toan chu dong gom {GetActiveSafetyPackage(model.BodyType, variant.Price)}. Cach mo ta nay giup trang chi tiet xe co du thong tin nhu mot web ban xe, dong thoi van giu o muc tong quan va de hieu cho do an.";
        }

        private static string BuildPassiveSafety(ModelSeed model, VariantSeed variant)
        {
            var airbags = GetAirbagCount(variant.Price, model.BodyType);
            var bodyShell = variant.Price >= 15000000000m
                ? "khung than xe vat lieu hieu nang cao va vung bao ve nguoi ngoi"
                : "khung than xe cung vung, vung hap thu luc va day dai 3 diem";
            var familyFeature = model.BodyType is "SUV" or "MPV" or "Crossover"
                ? "co them moc ghe tre em ISOFIX va nhan manh su on dinh cho nhom gia dinh"
                : "duy tri cac trang bi co ban huong toi bao ve nguoi ngoi";

            return $"An toan bi dong gom {bodyShell}, {airbags} tui khi tuy tam gia va cau hinh, cung voi tua dau, day dai an toan va cac diem gia co khoang hanh khach. Mau xe nay {familyFeature}.";
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

        private static string GetBodyTypeDescription(string bodyType)
        {
            return bodyType switch
            {
                "SUV" => "dang SUV gam cao",
                "Sedan" => "dang sedan 4 cua",
                "Hatchback" => "dang hatchback nho gon",
                "MPV" => "dang MPV linh hoat cho gia dinh",
                "Crossover" => "dang crossover da dung",
                "Bán tải" => "dang pickup phuc vu cong viec va da ngoai",
                "Coupe" => "dang coupe hieu nang cao",
                "Mui trần" => "dang mui tran nhan manh trai nghiem",
                _ => "dang xe da muc dich"
            };
        }

        private static string GetDrivingTone(string bodyType)
        {
            return bodyType switch
            {
                "SUV" => "dam chac va tam nhin cao",
                "Sedan" => "em va on dinh",
                "Hatchback" => "linh hoat trong pho",
                "MPV" => "nhe nhang va de lam quen",
                "Crossover" => "can bang giua de lai va su da dung",
                "Bán tải" => "ben bi, co luc keo va hop nhieu mat duong",
                "Coupe" => "nhanh, sat mat duong va phan hoi truc dien",
                "Mui trần" => "cam xuc va huong toi trai nghiem mo",
                _ => "de tiep can"
            };
        }

        private static string GetChassisTone(string bodyType)
        {
            return bodyType switch
            {
                "SUV" => "dam than, on dinh than xe va hop duong truong",
                "Sedan" => "em ai va can bang giua thoai mai voi kha nang om cua",
                "Hatchback" => "nho gon, phan hoi nhanh va de xoay so",
                "MPV" => "de chiu cho nhieu hanh khach va linh hoat hang ghe",
                "Crossover" => "vang chac, tam sat thoang va hop nhieu kieu hanh trinh",
                "Bán tải" => "gan bo, tai trong tot va ben bi cho cong viec",
                "Coupe" => "thap, cung va uu tien kha nang bam duong",
                "Mui trần" => "thap, the thao va huong toi trai nghiem mo",
                _ => "thuc dung va de bao tri"
            };
        }

        private static string GetExteriorTone(string bodyType)
        {
            return bodyType switch
            {
                "SUV" => "manh khoi va tam the cao",
                "Sedan" => "thanh lich va can doi",
                "Hatchback" => "tre trung, gon gang",
                "MPV" => "thuc dung, de nhan biet",
                "Crossover" => "hien dai va da dung",
                "Bán tải" => "nam tinh va co chat dong hanh",
                "Coupe" => "thap, rong, nhan manh toc do",
                "Mui trần" => "quyen ru va huong su kien",
                _ => "gon va ro rang"
            };
        }

        private static string GetInteriorTone(string segment)
        {
            return segment switch
            {
                var value when value.Contains("hypercar", StringComparison.OrdinalIgnoreCase) => "tap trung manh vao nguoi lai va tri an gia tri suu tam",
                var value when value.Contains("sieu xe", StringComparison.OrdinalIgnoreCase) => "thap, om nguoi va huong toi trai nghiem",
                var value when value.Contains("cao cap", StringComparison.OrdinalIgnoreCase) => "sang, de ngoi lau va hop tiep khach",
                var value when value.Contains("SUV", StringComparison.OrdinalIgnoreCase) => "rong, de buoc vao va de quan sat",
                var value when value.Contains("MPV", StringComparison.OrdinalIgnoreCase) => "linh hoat, thoang va de thao tac cho nhieu nguoi",
                _ => "gon gang, hien dai va de su dung"
            };
        }

        private static string GetSeatTone(string bodyType, string seating)
        {
            return bodyType switch
            {
                "SUV" => seating.Contains("7", StringComparison.OrdinalIgnoreCase)
                    ? "hang 2 va hang 3 de chia nguoi, gap/gap dien linh hoat"
                    : "ghe truoc thoang, hang sau de duoi chan cho gia dinh",
                "MPV" => "ba hang ghe linh hoat, de gap va de buoc vao",
                "Bán tải" => "hang ghe truoc va sau can bang cho cong viec va di xa",
                "Coupe" => "ghe truoc om nguoi, uu tien vi tri lai",
                "Mui trần" => "ghe truoc om va nhan manh trai nghiem lai mo",
                _ => "khoang ngoi de tiep can, phu hop nhieu doi tuong su dung"
            };
        }

        private static string GetConvenienceTone(string bodyType, decimal price)
        {
            if (price >= 15000000000m)
            {
                return "cum dieu khien hieu nang cao, giao dien xe sang va cac tinh nang su kien";
            }

            if (price >= 2000000000m)
            {
                return "man hinh lon, camera 360, ghe chinh dien, am thanh va dieu hoa da vung";
            }

            if (price >= 900000000m)
            {
                return bodyType == "MPV"
                    ? "dieu hoa nhieu vung, ghe sau de chiu, cua/cua cop dien va camera"
                    : "man hinh trung tam, ket noi thong minh, dieu hoa tu dong, camera va de xe";
            }

            return "cac tinh nang co ban nhu man hinh, Bluetooth, dieu hoa, cong sac va camera/cam bien";
        }

        private static string GetActiveSafetyPackage(string bodyType, decimal price)
        {
            if (price >= 15000000000m)
            {
                return "phanh hieu nang cao, kiem soat luc keo, can bang dien tu, camera/cam bien va cac chuong trinh lai theo tinh huong";
            }

            if (price >= 1200000000m)
            {
                return "ABS, EBD, BA, can bang dien tu, ho tro khoi hanh ngang doc, camera 360, canh bao diem mu, giu lan va ga tu dong thich ung tuy phien ban";
            }

            if (price >= 700000000m)
            {
                return "ABS, EBD, BA, can bang dien tu, ho tro khoi hanh ngang doc, camera lui, cam bien va mot so tinh nang ADAS co ban";
            }

            return "ABS, EBD, BA, can bang dien tu, camera lui va cac he thong can thiep phanh/co ban";
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
