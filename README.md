# Cost Accounting Automation

Maliyet muhasebesi otomasyonu. WinForms istemcisi + EF Core 10 + SQL Server.

## Kurulum mimarisi

Veritabanı ikiye ayrılmıştır:

| Katalog | İçerik |
|---|---|
| `CostAccountingAutomationMaster` | Kurumlar, mali yıllar, kullanıcılar, roller, yetkiler |
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
| Mesajlar | Gelen kutusu. Kişisel konuşmalar ve duyuru kanalı ayrı grupta listelenir. |
| Konuşma | İki kullanıcı arasındaki geçmiş ve yanıt kutusu. Ctrl+Enter gönderir. |
| Yeni Mesaj / Duyuru Gönder | Alıcı arama, konu ve gövde. İki mod tek ekrandadır. |

Ribbon menüsündeki grup: **Mesajlaşma → Mesajlar**.

### Yetkiler

| Yetki | Karşılığı |
| --- | --- |
| `message:view` | Mesajları ve konuşma geçmişini görmek |
| `message:send` | Bir kullanıcıya mesaj göndermek |
| `message:announce` | Birden çok kullanıcıya duyuru göndermek |

Standart rollerde `muhasebe_muduru` üç yetkiye de sahiptir; `muhasebe_elemani`
yalnızca görüntüleme ve gönderme yetkisine.

`sys_admin` rolü tüm yetkilere sahiptir; yetki listesi bu rolde boş döner.

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
