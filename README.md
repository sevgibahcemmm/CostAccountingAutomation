# Cost Accounting Automation

Maliyet muhasebesi otomasyonu. WinForms istemcisi + EF Core 10 + SQL Server.

## Kurulum mimarisi

Veritabanı ikiye ayrılmıştır:

| Katalog | İçerik |
|---|---|
| `CostAccountingAutomationMaster` | Kurumlar, mali yıllar, kullanıcılar, roller, yetkiler, **mesajlar, çevrimiçi kayıtları** |
| `CAA_<Yıl>_<Kurum>` | O kurumun o mali yılın iş verileri (fatura, stok, maliyet) |

## Şemayı kim hazırlar
**Uygulama veritabanına hiçbir zaman dokunmaz.** Açılışta yalnızca şemanın
güncel olduğunu *salt okunur* kontrol eder (`DatabaseInitializer.CheckSchemaAsync`).

Şemayı yalnızca yönetici hazırlar:

```powershell
cd src
.\db-migrations.ps1 -Action Provision
```

Bu komut master veritabanını oluşturur, migration'ları uygular, rol ve kullanıcıları
tohumlar ve kurumlar için içinde bulunulan mali yılın iş veritabanlarını açar.

Kontrol (hiçbir şey yazmaz):

```powershell
.\db-migrations.ps1 -Action Status
```

### Neden bu ayrım gerekli

Uygulama eskiden açılışta veritabanını kendisi hazırlıyordu. Merkezi sunucuya
bağlanan on istemci aynı anda açıldığında her biri migration çalıştırıp
"şirket var mı?" kontrolü yapıyordu. Bu **kontrol et ve sonra yaz** biçimi
eşzamanlı açılışlarda şu hatalara yol açıyordu:

- `__EFMigrationsHistory` tablosuna eşzamanlı yazma
- Aynı kurum/rol/kullanıcının iki kez eklenmesi
- Aynı veritabanı için eşzamanlı `CREATE DATABASE`

Hazırlama işlemi artık iki korumayla çalışır:

1. **İstemci hiç hazırlamaz.** Yalnızca okur ve eksikse ekranda yöneticiye
   bildirir.
2. **Yönetici hazırlarken sunucu çapında kilitlenir.** `sp_getapplock` ile iki
   yönetici aynı anda çalıştırırsa ikincisi bekler, işlem bitince "yapılacak
   iş yok" diye çıkar. Kilit oturum kapsamlıdır; süreç çökse bile kilit
   kalmaz.

## Merkezi sunucu kurulum notları

**Veritabanı collation'ı önemli değildir.** Kullanıcı adı ve e-posta eşleşmesi
`COLLATE Latin1_General_100_CI_AS_KS` ile yapılır; sunucu `_CS` (harf duyarlı)
collation ile kurulmuş olsa bile `Admin` ile `admin` aynı kullanıcıya gider.
Aksi hâlde kullanıcı adındaki büyük/küçük harf farkı "geçersiz kullanıcı"
hatası üretirdi.

**Giriş ekranı önce kullanıcı adı ister.** "Kurum" ve "Mali Yıl" listeleri
kullanıcı adı yazılana kadar pasiftir ve *bulunamadı* diye bir bilgi vermez. Bu
bilinçli bir güvenlik kararıdır: şifre doğrulanmadan kimlerin sistemde olduğu,
hangi kurumların tanımlı olduğu ya da hangi mali yılların açık olduğu
söylenmez. Kullanıcı adınızı yazdığınızda listeler açılır.

> Bu davranış Release derlemesinde daha görünürdür, çünkü Debug derlemesinde
> `XtraLoginForm` kullanıcı adını önceden doldurur (`#if DEBUG` bloğu). **Listelerin
> boş görünmesi veritabanının boş olduğu anlamına gelmez** — ekran açılışında hiç
> sorgu yapılmamıştır.

### İstemci başına verilmesi gerekenler

| Ayar | Nerede |
|---|---|
| `ConnectionStrings__Master` | ortam değişkeni veya `appsettings.Local.json` |
| `ConnectionStrings__SqlServer` | aynı |
| `DatabaseProvisioning__Mode=VerifyOnly` | aynı |
| `Jwt__SecretKey` | aynı |

Kurulum klasörü `appsettings.Local.json` ile birlikte kopyalanırsa istemci
makinesinde hiçbir komut çalıştırmak gerekmez.

## Kurulum ayarı

`appsettings.json` **izlenen (tracked) bir dosyadır ve gizli değer içermez.** Her iki
uygulamanın (WinForms istemcisi ve `caa-provision` aracı) kendi `appsettings.json`
dosyası vardır; bu doğrudur, çünkü iki uygulama iki ayrı giriş noktasıdır.

Makineler arasında değişen değerler **ortam değişkeni** ile verilir ve JSON'un
**üzerine** yazar:

| Ortam değişkeni | Açıklama |
|---|---|
| `ConnectionStrings__Master` | Merkezi sunucudaki master veritabanı |
| `ConnectionStrings__SqlServer` | Yıl veritabanı bağlantı şablonu |
| `Jwt__SecretKey` | Token imzalama anahtarı (en az 64 karakter) |
| `DatabaseProvisioning__Mode` | `VerifyOnly` (üretim) \| `Automatic` (tek makine) |

PowerShell:

```powershell
setx ConnectionStrings__Master  "Data Source=SERVER;Initial Catalog=CostAccountingAutomationMaster;..."
setx ConnectionStrings__SqlServer "Data Source=SERVER;Initial Catalog=master;..."
setx DatabaseProvisioning__Mode "VerifyOnly"
```

### Geliştirme anahtarı

Geliştirme makinesinde rastgele bir anahtar üretip kaydetmek için:

```powershell
cd src
.\Set-LocalSecrets.ps1
```

Betik 256 bitlik kriptografik rastgele bir anahtar üretir, kullanıcı düzeyinde
ortam değişkeni olarak kaydeder ve ekrana basmaz. `setx` etkisi yeni açılan
oturumlarda geçerlidir; Visual Studio'yu yeniden başlatın.

Anahtarı döndürmek için: `.\Set-LocalSecrets.ps1 -Rotate`

> Anahtar `appsettings.json` içine **yazılmamalıdır.** O dosya izlenir; anahtarı
> oraya yazmak onu tüm commit geçmişine yayar ve anahtarı bilen herkes istediği
> kullanıcı adına geçerli token üretebilir. Repo geçmişinde daha önce bulunan
> anahtar canlıda kullanılmamalıdır.

### Provisioning modu

| Mode | Nerede kullanılır |
|---|---|
| `VerifyOnly` | **Merkezi sunucu.** İstemci şemaya dokunmaz. Üretimde bu değer verilmelidir. |
| `Automatic` | Yalnızca tek geliştirici makinesi / LocalDB. İlk kurulumda sihirbazı gösterir. |

Ayar okunamaz veya yazım hatalıysa uygulama `VerifyOnly` davranır: şemaya
dokunmamak her zaman güvenli olan taraftır.

## Geliştirici kurulumu

```powershell
# Ilk kurulum (yalnizca bu makinede)
.\db-migrations.ps1 -Action Provision

# Token anahtari
.\Set-LocalSecrets.ps1

# Uygulamayi calistir
dotnet run --project Cost.Accounting.Automation.WinFormsApp
```

Tohumlanan kullanıcılar (tümü parola `1`):

| Kullanıcı | Rol |
|---|---|
| `admin` | `sys_admin` |
| `ahmet.yilmaz` | `muhasebe_muduru` |
| `ayse.kaya`, `mehmet.demir` | `muhasebe_elemani` |
| `fatma.celik` | `muhasebe_elemani` |

## Migration geliştirme

```powershell
# Her iki context için migration üretir (Master + Year)
.\db-migrations.ps1 -Action Add -Name Mig-1

# Yalnızca master'ı güncelle (yıl veritabanları uygulama tarafından açılır)
.\db-migrations.ps1 -Action UpdateMaster

.\db-migrations.ps1 -Action List
.\db-migrations.ps1 -Action Remove
```

Yıl veritabanlarının adı çalışma anında seçildiği için
`database update --context ApplicationDbContext` bilerek kullanılmaz; yanlışlıkla
master'a iş verisi yazılmasını engeller. Yıl veritabanları `Provision` ile açılır.

## Proje yapısı

```
src/
  Cost.Accounting.Automation.Domain           Aggregate'lar, value object'ler
  Cost.Accounting.Automation.Application      MediatR istekleri, yetki, validasyon
  Cost.Accounting.Automation.Infrastructure   EF Core, migration'lar, veri erişimi
  Cost.Accounting.Automation.Provisioning     Yönetici konsol aracı (caa-provision)
  Cost.Accounting.Automation.WinFormsApp      WinForms + DevExpress istemci
  db-migrations.ps1                           EF ve veritabanı hazırlık komutları
  Set-LocalSecrets.ps1                        Gizli anahtar üretir (ortam değişkenine yazar)
```

## Mesajlaşma

Kullanıcılar birbirine yazabilir, yönetici rolü herkese duyuru gönderebilir.
Veriler **master** veritabanında `Messages` tablosunda tutulur; yıl/şirket
seçimi yapılmadan da çalışır.

### Ekranlar

| Ekran | Açıklama |
| --- | --- |
| Mesajlar | Solda **tüm aktif kullanıcılar** (çevrimiçi/pasif), sağda konuşma listesi. |
| Konuşma | İki kullanıcı arasındaki geçmiş ve yanıt kutusu. Ctrl+Enter gönderir. |
| Yeni Mesaj / Duyuru Gönder | Alıcı arama, konu ve gövde. İki mod tek ekrandadır. |

Ribbon menüsündeki grup: **Mesajlaşma → Mesajlar**.

Kullanıcı listesinde **oturum açan kişinin kendisi görünmez.** Kendine mesaj
gönderilemediği için (bkz. `MessageSendCommand`) listede yer alması yalnızca
"Çevrimiçi" yazan bir satır olurdu; tıklanıp gönderildiğinde hata verirdi.
Filtre `MessageDirectoryQuery` içinde, sorgunun en başında yapılır — böylece
hiçbir ekran unutursa kendi satırını göstermeye çalışmaz. Durum çubuğundaki
`Çevrimiçi: N` sayacı da kendini saymaz, böylece sayı listede görünenlerle
tutarlıdır.

## Çevrimiçi kullanıcılar ve bildirimler

Uygulamanın sunucu katmanı **yoktur**; istemciler doğrudan master
veritabanına bağlanır. Bu yüzden çevrimiçi bilgisi bir anlık iletim kanalıyla
değil, **kalp atışı** sütunuyla tutulur: her istemci kendi satırını periyodik
olarak tazeler, diğer istemciler de aynı satırları okur. Yeni paket gerekmez.

### `UserPresences` tablosu

| Sütun | Anlamı |
| --- | --- |
| `UserId` | Başlık. Bir kullanıcının **tek** satırı vardır. |
| `CompanyId` | Oturumun açıldığı kurum. |
| `SessionId` | Satırı şu an tazeleyen oturumun kimliği. |
| `SessionStartedAt` | Oturumun başlangıcı. |
| `LastHeartbeatAt` | Son tazeleme. **Çevrimiçi olup olmamak bundan türetilir.** |
| `LoggedOutAt` | Düğmeye basarak çıkıldığında yazılır. |
| `MachineName` | İstemcinin çalıştığı makine. |

### Neden bayrak değil, türetilmiş durum

"Çevrimiçi" bir `bit` sütunu değildir: `LoggedOutAt` boş **ve** son kalp atışından
`UserPresence.OnlineWindow` (75 saniye) kadar süre geçmemiş olması gerekir.

Böylece istemci çöktüğünde, ağ koptuğunda ya da uygulama çöktüğünde veritabanında
"açık" kalmış bir kullanıcı oluşmaz; pencere aşılınca kullanıcı kendiliğinden
**Pasif** görünür. Düğmeye basıldığında ayrıca `LoggedOutAt` yazıldığı için diğer
istemciler kapanışı beklemez.

Pencere uzunluğu kalp atışı aralığının üç katıdır: tek bir kaybolan kalp atışı
(kısa bir ağ kesintisi) yanlışlıkla "Pasif" göstermemelidir.

### Çoklu oturum

Aynı kullanıcı iki makineden çalışabilir. Bunu `SessionId` ayırır: kalp atışı
yazan oturum kimliğini yazar, çıkış **yalnızca kendi oturumu** için geçerlidir.
Bir pencereden çıkmak diğer pencerenin oturumunu düşürmez.

### Bildirimler

`LiveMessagingService` (singleton) her 10 saniyede bir tur yapar ve iki tür
bilgiyi karşılaştırır:

| Turda görülen fark | Bildirim |
| --- | --- |
| Yeni oturum açan kullanıcı | **Ses** + "Ahmet Yılmaz oturum açtı." |
| Yeni okunmamış mesaj | **Ses** + "Ayşe Kaya size 2 mesaj gönderdi: ..." |

Metinler `MessagingNotifier` içinde üretilir, sesler `SoundHelper` ile çalınır.
Ses, mesajlar ekranındaki **Sesli Bildirim** düğmesiyle kapatılabilir.

İlk tur yalnızca **başlangıç noktasıdır** ve hiçbir şey duyurmaz. Aksi hâlde
uygulamayı açan kullanıcı, ekranda zaten çalışan bütün arkadaşları için
"oturum açtı" duyurusu alırdı.

Çıkışta ses çalınmaz: kullanıcı bunu kendi yaptığı için yeni bilgi değildir.

### Nerede çalışır

Bildirimler mesajlar ekranına değil, **uygulama geneline** aittir. Dashboard'da
çalışırken de biri oturum açtığında haber verilir. Motor uygulama ömrü boyunca
yaşar; ekran açılıp kapanırken durmaz.

Durum çubuğunda sürekli `Çevrimiçi: N` ve gerekiyorsa `Yeni mesaj: N` yazar.

### Okundu işaretleme

Konuşma penceresi açıldığında ve her yoklamada o kanaldaki mesajlar okundu
sayılır (`MessageMarkConversationAsReadCommand`). Kullanıcı zaten okuduğu şey
için bildirim almaz ve rozet kendiliğinden düşer.

### Yetkiler

| Yetki | Karşılığı |
| --- | --- |
| `message:view` | Mesajları, konuşma geçmişini **ve çevrimiçi listesini** görmek |
| `message:send` | Bir kullanıcıya mesaj göndermek |
| `message:announce` | Birden çok kullanıcıya duyuru göndermek |

Standart rollerde `muhasebe_muduru` üç yetkiye de sahiptir; `muhasebe_elemani`
yalnızca görüntüleme ve gönderme yetkisine.

`sys_admin` rolü tüm yetkilere sahiptir; yetki listesi bu rolde boş döner.

`message:view` yetkisi olmayan kullanıcı için canlı motor **başlatılmaz**: ne
çevrimiçi bilgisi yayınlanır ne de bildirim gösterilir.

### Sicil numarası

Alıcı araması sicil numarası, TC kimlik numarası, kullanıcı adı, ad ve soyad
alanlarında çalışır. Sicil numarası **kullanıcı** kaydında tutulur ve
`UserEditForm` içindeki **Sicil Numarası** alanından girilir. Aynı sicil
numarası iki kullanıcıya verilemez.

Sicil numarası opsiyoneldir; boş bırakılabilir. Boş değilse rakam ve tire
dışında karakter kabul edilmez.

### Alıcı arama kuralı

Aynı `MessageBody` / `MessageSubject` örneği birden çok mesajda paylaşılırsa
EF yalnızca bir satırı gövde alanıyla yazar ve geri kalanları `Body_Value`
NULL olduğu için reddeder. Bu nedenle her mesaj için **yeni** değer nesnesi
oluşturulur; bu kural `MessageBroadcastCommand` içinde yorumla da belgelenmiştir.

## Canlı güncelleme nasıl çalışır

| Parça | Nerede | Görevi |
| --- | --- | --- |
| `LiveMessagingService` | `WinFormsApp/Tools` | 10 saniyelik döngü: kalp atışı + yoklama + karşılaştırma |
| `MessagingNotifier` | `WinFormsApp/Tools` | Olayları arayüz iş parçacığına taşır, ses ve metni üretir |
| `SoundHelper` | `WinFormsApp/Tools` | Sistem sesleri; proje medya dosyası taşımaz |
| `DirectoryRow` | `WinFormsApp/Forms/MessageForms` | Listede gösterilen satır; ekrana özgü alanlar burada |

Durum noktası bir görsel olarak değil, `CustomDrawCell` içinde **elle çizilir**.
DevExpress'in görsel düzenleyicisi bağlı sütunda "görsel yok" yer tutucusu
çizebiliyordu; renkleri doğrudan `SkinTheme`'ten alan hücre boyama hem her
temada doğru sonucu verir hem de nokta hücresi odaklanamaz olduğu için ilk
satırda kaybolmaz.

Servis `Program.Main` içinde `AddSingleton` olarak kaydedilir,
`RibbonMainForm_Load` içinde başlatılır ve çıkışta `session.Clear()` **öncesinde**
durdurulur. Sıra önemlidir: çıkış bildirimi oturum kimliğini okur, kimlik
temizlendikten sonra gönderilemez.

Yoklama başarısız olursa bekleme 45 saniyeye çıkar. Sunucuya ulaşılamadığında
her 10 saniyede bir hata yazmak günlük dosyayı şişirip gerçek hatayı gizler.

### Liste sıçraması neden sorun değil

Kullanıcı listesi ve konuşma listesi on saniyede bir yenilenir. `DataSource`
her turda yeniden atansaydı kaydırma konumu sıfırlanır ve kullanıcı imleci listede
okurken satır kaybolurdu. Bu yüzden `ApplyWhenChanged` yalnızca satır kümesi
gerçekten değiştiğinde listeyi yeniden kurar.
