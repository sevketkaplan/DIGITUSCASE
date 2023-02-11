# DIGITUSCASE

Proje indirildikten sonra Properties penceresinden Single Startup Project seçeneğinden WM seçilecektir.
Solution->Properties->Starup Project -> Select WM
Daha sonra PostgreSql DB kullanılarak DB connection ayarları DataAccessLayer katmanında bulunan WMDbContext.cs dosyasında local db ayarlarıı da yapabilirsiniz.(Tercihen)
Projede Remote Db kullanıldığı için Database oluşturulmasına gerek yoktur.
Database Script Github Dosyaları arasında bulunmaktadır.

Visual Studio 2022 kullanılarak geliştirilmiştir.
Gerekli RunTime & SDK -> .net Core 2.2 yüklenmesi gereklidir.

Gerekli ayarlar yapıldıktan sonra proje başlatılabilir.

Proje https://localhost:5001 adresi ile başlayacaktır.
