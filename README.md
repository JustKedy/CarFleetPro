# CarFleetPro

CarFleetPro, araç filosu yönetimini kolaylaştırmak için vizyoner bir bakış açısıyla tasarlanmış kapsamlı bir çözümdür. Proje, esnek ve güçlü bir arka uç (API) ile kullanıcı dostu bir mobil uygulamadan oluşmaktadır.

## 🚀 Proje Yapısı

Proje temel olarak iki ana bileşenden oluşmaktadır:

- **CarFleetPro.API**: Uygulamanın iş mantığını, veritabanı işlemlerini, veri güvenliğini ve servis entegrasyonlarını yöneten arka uç (backend) projesidir. Modern .NET mimarisi üzerine inşa edilmiştir.
- **CarFleetPro.Mobile**: Kullanıcıların filodaki araçların durumlarına, lokasyon veya teknik detaylarına anlık olarak erişebilmesini sağlayan çapraz platform (cross-platform) mobil uygulamadır.

## 🛠 Kullanılan Teknolojiler

- **Backend:** .NET, ASP.NET Core Web API
- **Mobil:** .NET MAUI / Xamarin (mobil çözüm platformu)
- **Geliştirme Ortamı:** Visual Studio 2022, Visual Studio Code

## ⚙️ Kurulum ve Çalıştırma

### Ön Koşullar
- [.NET SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (MAUI ve ASP.NET geliştirme iş yükleri yüklenmiş olmalıdır)

### Kurulum Adımları
1. Proje dizinine gidin veya repoyu bilgisayarınıza klonlayın.
2. Ana dizindeki `CarFleetPro.slnx` (Solution) dosyasını Visual Studio ile açın.
3. Bağımlılıkları yüklemek için Solution Explorer üzerinde sağ tıklayıp **Restore NuGet Packages** seçeneğini kullanın.

### Çalıştırma
- **API için:** `CarFleetPro.API` projesine sağ tıklayıp *Set as Startup Project* yapın ve projeyi ayağa kaldırın. (Varsayılan olarak Swagger arayüzü ile test edilebilir).
- **Mobil için:** `CarFleetPro.Mobile` projesini seçin, uygun bir Android veya iOS emülatörü (ya da fiziksel cihaz) belirleyip çalıştırın.

## 🤝 Katkıda Bulunma

Projeye katkıda bulunmak isterseniz, lütfen büyük değişiklikler yapmadan önce bir tartışma veya *issue* başlatın. Kod standartlarına uyduğunuzdan emin olun.

## 📝 Lisans

Tüm hakları saklıdır. Bu projenin kaynak kodları lisans sahibinin izni olmadan kopyalanamaz ve dağıtılamaz.
