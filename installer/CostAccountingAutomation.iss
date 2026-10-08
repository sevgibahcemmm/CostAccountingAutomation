; Cost Accounting Automation - Inno Setup kurulum betigi
;
; Bu betik framework-dependent (kucuk) bir setup.exe uretir. Paketi olusturmak
; icin Iss/compile adimlarini 'build-installer.ps1' yonetir; dogrudan cagirmak
; yerine o betigi kullanin.
;
; NOT: Bu dosya bilerek yalnizca ASCII karakter icerir. Inno Setup, BOM'suz
; UTF-8 .iss dosyalarini ANSI okuyabilir; Turkce karakterler bozulmasin diye
; metinler diakritiksiz yazilmistir.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef StagingDir
  #define StagingDir "staging"
#endif

#define AppName "Maliyet Muhasebesi Otomasyonu"
#define AppExe "Maliyet Muhasebesi Otomasyonu.exe"
#define ProvExe "caa-provision.exe"
; Masaustu kisa yolu icin kullanici simgesi: ayri bir dosya olarak kurulur
; ('{app}\{#AppIcon}'). exe'ye gomulmeyerek form/uygulama ikonlari ayni
; kalir; yalnizca kisa yollar bu simgeyi kullanir.
#define AppIcon "Maliyet Muhasebesi Otomasyonu.ico"
#define AppIconSource "..\src\Cost.Accounting.Automation.WinFormsApp\1 (246).ico"
#define AppIdGuid "{8D7D11EC-6C75-47EE-8CC7-9506E86D0618}"

[Setup]
AppId={{8D7D11EC-6C75-47EE-8CC7-9506E86D0618}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Emrullah AKPINAR-Muhasebe Yetkilisi
DefaultDirName={autopf}\Maliyet Muhasebesi Otomasyonu
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
OutputDir=dist
OutputBaseFilename=CostAccountingAutomation-Setup-{#AppVersion}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
WizardImageStretch=no

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; WinForms istemcisi (appsettings.json, exe ve tum bagimliliklar dahil).
Source: "{#StagingDir}\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Veritabani hazirlik araci ayri bir alt klasore kurulur.
Source: "{#StagingDir}\provision\*"; DestDir: "{app}\tools"; Flags: ignoreversion recursesubdirs createallsubdirs
; VC++ Redistributable gecici klasore acilir; kurulumdan sonra kod tarafindan silinir.
Source: "{#StagingDir}\runtime\vc_redist.x64.exe"; DestDir: "{tmp}"
; Masaustu kisa yolu simgesi: uygulama dosyalarinin yanina kurulur.
Source: "{#AppIconSource}"; DestDir: "{app}"; DestName: "{#AppIcon}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{app}\{#AppIcon}"
Name: "{group}\Veritabani Durumu (caa-provision status)"; Filename: "{app}\tools\{#ProvExe}"; Parameters: "status"; WorkingDir: "{app}\tools"; IconFilename: "{app}\{#AppIcon}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{app}\{#AppIcon}"; Tasks: desktopicon

[Run]
; Onkosullar (vc_redist, provisioning) [Code] icinde sirali calistirilir.
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[InstallDelete]
; Eski surumlerde exe eski adla kuruluyordu; yeni adla kurulturken artigi temizle.
Type: files; Name: "{app}\Cost.Accounting.Automation.WinFormsApp.exe"

[UninstallDelete]
Type: files; Name: "{app}\appsettings.Local.json"
Type: files; Name: "{app}\tools\appsettings.Local.json"
Type: filesandordirs; Name: "{app}\logs"
; Dikkat: veritabani klasoru ({app}\Data) bilincli olarak silinmez; veri kaybi olmasin.

[Code]
var
  SettingsPage: TWizardPage;
  MasterEdit: TNewEdit;
  SqlServerEdit: TNewEdit;
  SecretEdit: TNewEdit;
  DataDirEdit: TNewEdit;
  RunProvisionCheck: TNewCheckBox;
  RunAutomaticCheck: TNewCheckBox;
  GenerateKeyButton: TNewButton;

const
  DefaultMaster = 'Data Source=.\SQLEXPRESS;Initial Catalog=CostAccountingAutomationMaster;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False';
  DefaultSqlServer = 'Data Source=.\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False';
  HEX_CHARS = '0123456789ABCDEF';
  SQL2022_SSEI_URL = 'https://download.microsoft.com/download/5/1/4/5145fe04-4d30-4b85-b0d1-39533663a2f1/SQL2022-SSEI-Expr.exe';
  SQL_MEDIA_EXE = 'SQLEXPR_x64_ENU.exe';
  SQL_EXPRESS_INSTANCE = 'SQLEXPRESS';

{ Windows URL indirme. NOT: urlmon.dll icinde 'URLDownloadToFile' diye bir
  export yoktur; gercek adlar A/W son eklidir (C makrosu). Inno (Unicode)
  icin W varyanti kullanilir; aksi halde "Cannot Import dll:<utf8>urlmon.dll"
  hatasiyla kurulum baslamaz. 0 (S_OK) degeri basarili demektir. }
function URLDownloadToFileW(pCaller: Integer; szURL: String; szFileName: String; dwReserved: Integer; lpfnCB: Integer): Integer;
  external 'URLDownloadToFileW@urlmon.dll stdcall';

{ Windows CSP uzerinden kriptografik rastgele anahtar uretimi.
  crypt32.dll Inno Setup tarafindan import edilemediginden ole32.dll
  uzerinden CoCreateGuid kullanilir (v4 GUID'ler CSPRNG tabanlidir);
  5 GUID = 160 hex karakter, en az 64 karakter kosulunu asar. }
function CoCreateGuid(var Guid: TGUID): Integer;
  external 'CoCreateGuid@ole32.dll stdcall';

function ByteHex(B: Integer): String;
begin
  Result := Copy(HEX_CHARS, (B shr 4) + 1, 1) + Copy(HEX_CHARS, (B and $0F) + 1, 1);
end;

function Int32Hex(V: Cardinal): String;
var
  K: Integer;
begin
  Result := '';
  for K := 3 downto 0 do
    Result := Result + ByteHex((V shr (K * 8)) and $FF);
end;

function Int16Hex(V: Word): String;
begin
  Result := ByteHex(V shr 8) + ByteHex(V and $FF);
end;

function GuidHex(const G: TGUID): String;
var
  I: Integer;
begin
  Result := Int32Hex(G.D1) + Int16Hex(G.D2) + Int16Hex(G.D3);
  for I := 0 to 7 do
    Result := Result + ByteHex(G.D4[I]);
end;

procedure GenerateSecretClick(Sender: TObject);
var
  G: TGUID;
begin
  SecretEdit.Text := '';
  { 5 GUID x 32 hex karakter = 160 karakter; 64 kosulunu rahatca asar. }
  while Length(SecretEdit.Text) < 64 do
  begin
    if CoCreateGuid(G) <> 0 then
    begin
      MsgBox('Rastgele anahtar uretilemedi (ole32).', mbError, MB_OK);
      Exit;
    end;
    SecretEdit.Text := SecretEdit.Text + GuidHex(G);
  end;
end;

{ Yardimci: etiket + metin kutusu satiri olusturur. }
procedure AddField(RowTop: Integer; const Caption, Value: String; out FieldEdit: TNewEdit);
var
  Lbl: TNewStaticText;
begin
  Lbl := TNewStaticText.Create(SettingsPage);
  Lbl.Parent := SettingsPage.Surface;
  Lbl.Left := 0;
  Lbl.Top := RowTop;
  Lbl.Width := SettingsPage.SurfaceWidth;
  Lbl.Height := ScaleY(15);
  Lbl.Caption := Caption;

  FieldEdit := TNewEdit.Create(SettingsPage);
  FieldEdit.Parent := SettingsPage.Surface;
  FieldEdit.Left := 0;
  FieldEdit.Top := RowTop + ScaleY(17);
  FieldEdit.Width := SettingsPage.SurfaceWidth;
  FieldEdit.Height := ScaleY(23);
  FieldEdit.Text := Value;
end;

{ VC++ 2015-2022 Redistributable (x64) kurulu mu? Degilse sessiz kurulum tetiklenir.
  .NET calisma zamanlari self-contained pakete gomulu oldugu icin tek dis bagimlilik budur. }
function VCRedistNeeded(): Boolean;
var
  Installed: Cardinal;
begin
  Result := True;
  if RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Installed', Installed) then
    Result := Installed <> 1;
end;

{ JSON dizesi icin ters bolu ve tirnagi kacir. }
function JsonEscape(const S: String): String;
begin
  Result := S;
  StringChangeEx(Result, '\', '\\', True);
  StringChangeEx(Result, '"', '\"', True);
end;

{ Baglanti dizesindeki Data Source degerini dondurur (ornek: .\SQLEXPRESS). }
function ReadDataSource(const ConnectionString: String): String;
var
  Lower: String;
  Rest: String;
  P, Semi: Integer;
begin
  Result := '';
  Lower := LowerCase(ConnectionString);
  P := Pos('data source=', Lower);
  if P = 0 then
    Exit;

  { 'data source=' 12 karakterdir. }
  Rest := Trim(Copy(ConnectionString, P + 12, Length(ConnectionString)));
  Semi := Pos(';', Rest);
  if Semi > 0 then
    Rest := Copy(Rest, 1, Semi - 1);
  Rest := Trim(Rest);

  if (Length(Rest) >= 2) and (Rest[1] = '"') and (Rest[Length(Rest)] = '"') then
    Rest := Copy(Rest, 2, Length(Rest) - 2);

  Result := Rest;
end;

{ Baglanti dizesine gore SQL Server hizmet hesabini uretir:
  NT SERVICE\MSSQLSERVER (varsayilan instance) veya
  NT SERVICE\MSSQL$SQLEXPRESS (ornekli instance).
  Kurulum, veritabani klasorune bu hesap icin yazma yetkisi verir; dosyalari
  uygulama degil SQL Server hizmeti olusturur ve yazar. }
function SqlServiceAccount(const ConnectionString: String): String;
var
  Server: String;
  Instance: String;
  Slash: Integer;
begin
  Server := ReadDataSource(ConnectionString);
  Slash := Pos('\', Server);
  Instance := '';
  if Slash > 0 then
  begin
    Instance := Trim(Copy(Server, Slash + 1, Length(Server)));
    Server := Trim(Copy(Server, 1, Slash - 1));
  end;

  if (Instance = '') or (UpperCase(Instance) = 'MSSQLSERVER') then
    Result := 'NT SERVICE\MSSQLSERVER'
  else
    Result := 'NT SERVICE\MSSQL$' + Instance;
end;

{ Veritabani klasorunu olusturur ve SQL Server hizmetine yazma yetkisi verir. }
procedure GrantSqlServiceAccess(const Dir: String);
var
  Account: String;
  ResultCode: Integer;
begin
  Account := SqlServiceAccount(SqlServerEdit.Text);

  if not Exec(ExpandConstant('{sys}\icacls.exe'),
      '"' + Dir + '" /grant "' + Account + ':(OI)(CI)F" /T /C /Q',
      '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('icacls calistirilamadi: ' + SysErrorMessage(ResultCode), mbError, MB_OK);
    Exit;
  end;

  if ResultCode <> 0 then
    MsgBox('Veritabani klasorune SQL Server hizmeti (' + Account + ') yazma' + #13#10
      + 'yetkisi verilemedi. SQL Server kurulu degil ya da hizmet adi farkli olabilir.' + #13#10 + #13#10
      + 'Klasore elle yetki vermek icin (yonetici komut istemi):' + #13#10
      + 'icacls "' + Dir + '" /grant "' + Account + ':(OI)(CI)F"' + #13#10 + #13#10
      + 'Yetki verilmezse veritabani dosyalari bu klasore acilamaz; uygulama ilk' + #13#10
      + 'calistirmada hazirlik adiminda hatayi gosterir.', mbError, MB_OK);
end;

procedure InitializeWizard();
var
  DescLabel: TNewStaticText;
  ButtonGap: Integer;
  ButtonW: Integer;
begin
  { Sayfanin yuksekligine gore kutulari gorebilmek icin dort alan + iki onay
    kutusu yoGun bir yerlesimle elle dizilir; uzun aciklama boslugu
    yetisizligine yol aciyordu. }
  SettingsPage := CreateCustomPage(
    wpSelectTasks,
    'Veritabani Ayarlari',
    'SQL Server baglantisi ve veritabani klasoru');

  DescLabel := TNewStaticText.Create(SettingsPage);
  DescLabel.Parent := SettingsPage.Surface;
  DescLabel.Left := 0;
  DescLabel.Top := 0;
  DescLabel.Width := SettingsPage.SurfaceWidth;
  DescLabel.Height := ScaleY(46);
  DescLabel.WordWrap := True;
  DescLabel.Caption := 'Asagidaki degerler appsettings.Local.json dosyasina yazilir.' + #13#10
    + 'Veritabani klasoru, mdf/ldf dosyalarinin acilacagi klasordur. Kurulum' + #13#10
    + 'klasorundeki ortam degiskenleri (ConnectionStrings__*, Jwt__SecretKey) varsa' + #13#10
    + 'bu degerlerin uzerine yazar.';

  AddField(ScaleY(52), 'ConnectionStrings:Master (merkezi master veritabani)', DefaultMaster, MasterEdit);
  AddField(ScaleY(92), 'ConnectionStrings:SqlServer (yil veritabani sablonu)', DefaultSqlServer, SqlServerEdit);
  AddField(ScaleY(132), 'JWT imzalama anahtari (en az 64 karakter)', '', SecretEdit);
  AddField(ScaleY(172), 'Veritabani klasoru (mdf/ldf dosyalari)', '', DataDirEdit);

  { JWT alaninin sagina "rastgele uret" dugmesi. }
  ButtonGap := ScaleX(8);
  ButtonW := ScaleX(104);
  SecretEdit.Width := SettingsPage.SurfaceWidth - ButtonW - ButtonGap;

  GenerateKeyButton := TNewButton.Create(SettingsPage);
  GenerateKeyButton.Parent := SettingsPage.Surface;
  GenerateKeyButton.Left := SecretEdit.Left + SecretEdit.Width + ButtonGap;
  GenerateKeyButton.Top := SecretEdit.Top;
  GenerateKeyButton.Width := ButtonW;
  GenerateKeyButton.Height := SecretEdit.Height;
  GenerateKeyButton.Caption := 'Rastgele uret';
  GenerateKeyButton.OnClick := @GenerateSecretClick;

  RunProvisionCheck := TNewCheckBox.Create(SettingsPage);
  RunProvisionCheck.Parent := SettingsPage.Surface;
  RunProvisionCheck.Left := 0;
  RunProvisionCheck.Top := ScaleY(222);
  RunProvisionCheck.Width := SettingsPage.SurfaceWidth;
  RunProvisionCheck.Height := ScaleY(22);
  RunProvisionCheck.Caption := 'Veritabanini simdi hazirla (caa-provision provision)';
  RunProvisionCheck.Checked := True;

  RunAutomaticCheck := TNewCheckBox.Create(SettingsPage);
  RunAutomaticCheck.Parent := SettingsPage.Surface;
  RunAutomaticCheck.Left := 0;
  RunAutomaticCheck.Top := ScaleY(246);
  RunAutomaticCheck.Width := SettingsPage.SurfaceWidth;
  RunAutomaticCheck.Height := ScaleY(22);
  RunAutomaticCheck.Caption := 'Uygulama veritabanini ilk calistirmada kendisi hazirlayabilsin (yalnizca tek makinede isaretleyin)';
  RunAutomaticCheck.Checked := False;
end;

{ Veritabani klasoru alani ilk kez goruldugunde varsayilan degeri yaz.
  Uygulama klasoru sabiti o an icin gecerli kurulum klasorune gore genisletilir;
  boylece kullanici kurulum klasorunu degistirdikten sonraki deger dogru kalir. }
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = SettingsPage.ID then
  begin
    if Trim(DataDirEdit.Text) = '' then
      DataDirEdit.Text := ExpandConstant('{app}\Data');
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Secret: String;
begin
  Result := True;

  if CurPageID = SettingsPage.ID then
  begin
    if Trim(MasterEdit.Text) = '' then
    begin
      MsgBox('Master baglanti dizesi bos olamaz.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    if Trim(SqlServerEdit.Text) = '' then
    begin
      MsgBox('Yil veritabani baglanti dizesi bos olamaz.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    Secret := Trim(SecretEdit.Text);
    if Length(Secret) < 64 then
    begin
      MsgBox('JWT anahtari en az 64 karakter olmalidir. Alanin sagindaki' + #13#10
        + '''Rastgele uret'' dugmesiyle guclu bir anahtar olusturabilirsiniz.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    if Trim(DataDirEdit.Text) = '' then
    begin
      MsgBox('Veritabani klasoru bos olamaz.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
  end;
end;

procedure WriteSettingsFile(const FileName: String);
var
  Lines: TStringList;
begin
  Lines := TStringList.Create;
  try
    Lines.Add('{');
    Lines.Add('  "ConnectionStrings": {');
    Lines.Add('    "Master": "' + JsonEscape(Trim(MasterEdit.Text)) + '",');
    Lines.Add('    "SqlServer": "' + JsonEscape(Trim(SqlServerEdit.Text)) + '"');
    Lines.Add('  },');
    Lines.Add('  "DatabaseFiles": {');
    Lines.Add('    "DataDirectory": "' + JsonEscape(ExpandConstant(Trim(DataDirEdit.Text))) + '"');
    Lines.Add('  },');
    Lines.Add('  "Jwt": {');
    Lines.Add('    "SecretKey": "' + JsonEscape(Trim(SecretEdit.Text)) + '"');
    Lines.Add('  },');
    Lines.Add('  "DatabaseProvisioning": {');
    if RunAutomaticCheck.Checked then
      Lines.Add('    "Mode": "Automatic"')
    else
      Lines.Add('    "Mode": "VerifyOnly"');
    Lines.Add('  }');
    Lines.Add('}');
    Lines.SaveToFile(FileName);
  finally
    Lines.Free;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
end;

{ Baglanti dizesinden 'Data Source=...' (ya da 'Server=...') degerini ayirir. }
function ExtractDataSource(const ConnectionString: String): String;
var
  P, Q, R: Integer;
begin
  Result := '';
  P := Pos('Data Source', ConnectionString);
  if P = 0 then
    P := Pos('Server', ConnectionString);
  if P = 0 then
    Exit;
  Q := P;
  while (Q <= Length(ConnectionString)) and (ConnectionString[Q] <> '=') do
    Q := Q + 1;
  Q := Q + 1;
  while (Q <= Length(ConnectionString)) and (ConnectionString[Q] = ' ') do
    Q := Q + 1;
  R := Q;
  while (R <= Length(ConnectionString)) and (ConnectionString[R] <> ';') do
    R := R + 1;
  Result := Trim(Copy(ConnectionString, Q, R - Q));
end;

{ Hedef YEREL makinede SQLEXPRESS ornegine isaret ediyor mu? (uzak sunucuya
  SQL Express otomatik kurulamaz; yalnizca yerel makineye kurulabilir.) }
function IsLocalSqlExpress(const ConnectionString: String): Boolean;
var
  DS, Server, Instance: String;
  P: Integer;
begin
  Result := False;
  DS := ExtractDataSource(ConnectionString);
  if DS = '' then
    Exit;

  P := Pos('\', DS);
  if P > 0 then
  begin
    Instance := Trim(Copy(DS, P + 1, Length(DS) - P));
    if LowerCase(Instance) <> 'sqlexpress' then
      Exit;
    Server := Trim(Copy(DS, 1, P - 1));
  end
  else if LowerCase(Trim(DS)) = 'sqlexpress' then
    Server := ''   { cimaplak 'SQLEXPRESS': yerel makinenin ornegi sayilir. }
  else
    Exit;          { '.', 'localhost' gibi orneksiz hedeflere kurma. }

  if (Server = '') or (LowerCase(Server) = '.') or (LowerCase(Server) = 'localhost')
    or (LowerCase(Server) = '127.0.0.1') or (LowerCase(Server) = '::1') then
  begin
    Result := True;
    Exit;
  end;

  Result := CompareText(Server, GetComputerNameString) = 0;
end;

{ caa-provision can-connect: SQL Server erisilebiliyorsa True. }
function CanConnectToSql(const ProvDir: String): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec(ProvDir + '\{#ProvExe}', 'can-connect', ProvDir, SW_HIDE,
    ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function DownloadSqlExpress(const DestFile: String): Boolean;
begin
  Result := URLDownloadToFileW(0, SQL2022_SSEI_URL, DestFile, 0, 0) = 0;
end;

{ SQL Server Express'i (SQLEXPRESS) sessizce kurar; sonunda sunucu erisilebilir
  hale geldiyse True dondurur. }
function InstallSqlExpress(const ProvDir: String): Boolean;
var
  ResultCode: Integer;
  Bootstrapper, MediaDir, MediaExe: String;
begin
  Result := False;
  Bootstrapper := ExpandConstant('{tmp}\SQL2022-SSEI-Expr.exe');

  if not DownloadSqlExpress(Bootstrapper) then
  begin
    MsgBox('SQL Server Express yukleyicisi indirilemedi. Kurulumu elle yapin:' + #13#10
      + 'https://aka.ms/sqlexpress  (ornek adi: SQLEXPRESS)', mbError, MB_OK);
    Exit;
  end;

  MediaDir := ExpandConstant('{tmp}\sql2022media');
  if not ForceDirectories(MediaDir) then
  begin
    MsgBox('Gecici klasor olusturulamadi: ' + MediaDir, mbError, MB_OK);
    Exit;
  end;

  { SSEI yukleyicisi kurulum medyasini MEDIAPATH icine indirir. }
  if not Exec(Bootstrapper, '/ACTION=Download MEDIAPATH="' + MediaDir + '" MEDIATYPE=Core /QUIET',
    '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('SQL Server Express medyasi indirilemedi: ' + SysErrorMessage(ResultCode), mbError, MB_OK);
    Exit;
  end;

  if ResultCode <> 0 then
  begin
    MsgBox('SQL Server Express medyasi indirilemedi (kod ' + IntToStr(ResultCode) + ').' + #13#10
      + 'Kurulumu elle yapin: https://aka.ms/sqlexpress', mbError, MB_OK);
    Exit;
  end;

  MediaExe := MediaDir + '\' + SQL_MEDIA_EXE;
  if not FileExists(MediaExe) then
  begin
    MsgBox('SQL Server Express medyasi bulunamadi: ' + SQL_MEDIA_EXE + #13#10
      + 'Kurulumu elle yapin: https://aka.ms/sqlexpress', mbError, MB_OK);
    Exit;
  end;

  MsgBox('SQL Server (SQLEXPRESS) bulunamadi. Express surumu sessizce kuruluyor.' + #13#10
    + 'Bu islem medya indirmesiyle birlikte birkac dakika surebilir.', mbInformation, MB_OK);

  if not Exec(MediaExe, '/q /ACTION=Install /FEATURES=SQLENGINE /INSTANCENAME=' + SQL_EXPRESS_INSTANCE
    + ' /SQLSYSADMINACCOUNTS="BUILTIN\Administrators" /TCPENABLED=1 /IACCEPTSQLSERVERLICENSETERMS',
    '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('SQL Server Express kurulumu baslatilamadi: ' + SysErrorMessage(ResultCode), mbError, MB_OK);
    Exit;
  end;

  if ResultCode <> 0 then
  begin
    MsgBox('SQL Server Express kurulumu tamamlanamadi (kod ' + IntToStr(ResultCode) + ').' + #13#10
      + 'Kodu 3010 ise yeniden baslatma gerekir. Kurulumu elle yapin: https://aka.ms/sqlexpress', mbError, MB_OK);
    Exit;
  end;

  DeleteFile(Bootstrapper);
  Result := CanConnectToSql(ProvDir);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  ProvDir, ProvExePath, VcRedist, DataDir: String;
begin
  if CurStep <> ssPostInstall then
    Exit;

  { Ayarlari hem istemciye hem yonetici aracina yaz; ikisi ayni sunucuya baglanir. }
  WriteSettingsFile(ExpandConstant('{app}\appsettings.Local.json'));
  WriteSettingsFile(ExpandConstant('{app}\tools\appsettings.Local.json'));

  ProvDir := ExpandConstant('{app}\tools');
  ProvExePath := ProvDir + '\{#ProvExe}';

  VcRedist := ExpandConstant('{tmp}\vc_redist.x64.exe');

  { 1) VC++ Redistributable (yoksa): self-contained .NET icin on kosul. }
  if VCRedistNeeded() then
  begin
    if not Exec(VcRedist, '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      MsgBox('Microsoft Visual C++ Redistributable kurulumu baslatilamadi: ' + SysErrorMessage(ResultCode), mbError, MB_OK)
    else if ResultCode <> 0 then
      MsgBox('Microsoft Visual C++ Redistributable kurulumu tamamlanamadi (kod ' + IntToStr(ResultCode) + ').', mbError, MB_OK);
  end;

  { Gecici onkosul dosyasini temizle. }
  DeleteFile(VcRedist);

  { 2) Sunucu kurulumunda ("simdi hazirla" isaretli) SQL Server'i denetle.
     Erisilemiyorsa ve hedef yerel SQLEXPRESS ise sondan onceki surum kurulsun. }
  if RunProvisionCheck.Checked and not CanConnectToSql(ProvDir) then
  begin
    if IsLocalSqlExpress(SqlServerEdit.Text) then
      InstallSqlExpress(ProvDir)
    else
      MsgBox('Hedef SQL Server erisilemiyor: ' + ExtractDataSource(SqlServerEdit.Text) + #13#10
        + 'Baglanti dizesini ve ag erisimini kontrol edin.', mbError, MB_OK);
  end;

  { 3) Veritabani klasoru ve SQL hizmet yetkisi yalnizca sunucu kurulumunda
        yapilir. Istemci makinesinde yerel SQL Server yoktur; kurulum yalnizca
        ayar dosyalarini yazar ve uygulamayi kopyalar. }
  if RunProvisionCheck.Checked then
  begin
    DataDir := ExpandConstant(Trim(DataDirEdit.Text));
    if not ForceDirectories(DataDir) then
      MsgBox('Veritabani klasoru olusturulamadi: ' + DataDir, mbError, MB_OK)
    else
      GrantSqlServiceAccess(DataDir);
  end;

  { 4) Veritabani hazirligi. }
  if not RunProvisionCheck.Checked then
    Exit;

  if not Exec(ProvExePath, 'provision', ProvDir, SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('caa-provision calistirilamadi: ' + SysErrorMessage(ResultCode), mbError, MB_OK);
    Exit;
  end;

  if ResultCode <> 0 then
    MsgBox('Veritabani hazirligi basarisiz oldu (cikis kodu ' + IntToStr(ResultCode) + ').' + #13#10
      + 'Kurulum tamamlandi; hazirligi ''caa-provision provision'' komutu ile' + #13#10
      + 'elle tekrarlayabilirsiniz.', mbError, MB_OK)
  else
    MsgBox('Veritabani hazirligi tamamlandi. Veritabani dosyalari:' + #13#10
      + DataDir, mbInformation, MB_OK);
end;
