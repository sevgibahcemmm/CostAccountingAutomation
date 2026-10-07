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

#define AppName "Cost Accounting Automation"
#define AppExe "Cost.Accounting.Automation.WinFormsApp.exe"
#define ProvExe "caa-provision.exe"
#define AppIdGuid "{8D7D11EC-6C75-47EE-8CC7-9506E86D0618}"

[Setup]
AppId={{8D7D11EC-6C75-47EE-8CC7-9506E86D0618}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Cost Accounting Automation
DefaultDirName={autopf}\Cost Accounting Automation
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
; WinForms istemcisi (appsettings.json, exe ve tum bagimliliklar dâhil).
Source: "{#StagingDir}\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Veritabani hazirlik araci ayri bir alt klasore kurulur.
Source: "{#StagingDir}\provision\*"; DestDir: "{app}\tools"; Flags: ignoreversion recursesubdirs createallsubdirs
; VC++ Redistributable gecici klasore acilir; kurulumdan sonra kod tarafindan silinir.
Source: "{#StagingDir}\runtime\vc_redist.x64.exe"; DestDir: "{tmp}"
; SQL Server 2022 Express LocalDB (yalnizca tek makine/deneme icin istege bagli).
Source: "{#StagingDir}\runtime\SqlLocalDB.msi"; DestDir: "{tmp}"

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\Veritabani Durumu (caa-provision status)"; Filename: "{app}\tools\{#ProvExe}"; Parameters: "status"; WorkingDir: "{app}\tools"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; Onkosullar (vc_redist, LocalDB, provisioning) [Code] icinde sirali calistirilir.
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\appsettings.Local.json"
Type: files; Name: "{app}\tools\appsettings.Local.json"
Type: filesandordirs; Name: "{app}\logs"

[Code]
var
  SettingsPage: TInputQueryWizardPage;
  RunProvisionCheck: TNewCheckBox;
  RunLocalDbCheck: TNewCheckBox;

const
  DefaultMaster = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CostAccountingAutomationMaster;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False';
  DefaultSqlServer = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False';

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

{ LocalDB secilirse uygulama ilk calismada semayi kendi kullanici profilinde
  olusturur; setup tarafinda provisioning gereksizdir. }
procedure LocalDbCheckClicked(Sender: TObject);
begin
  if RunLocalDbCheck.Checked then
  begin
    RunProvisionCheck.Checked := False;
    RunProvisionCheck.Enabled := False;
  end
  else
    RunProvisionCheck.Enabled := True;
end;

procedure InitializeWizard();
begin
  SettingsPage := CreateInputQueryPage(
    wpSelectTasks,
    'Veritabani Ayarlari',
    'Merkezi SQL Server baglanti bilgileri',
    'Asagidaki degerler kurulum klasorundeki appsettings.Local.json dosyasina yazilir.' + #13#10
    + 'Makinadaki ortam degiskenleri (ConnectionStrings__Master, Jwt__SecretKey) varsa' + #13#10
    + 'onlar bu degerlerin uzerine yazar.' + #13#10 + #13#10
    + 'JWT anahtari icin en az 64 karakter gerekir. Rastgele anahtar uretmek icin' + #13#10
    + 'PowerShell (yeni oturum) ile:' + #13#10
    + '$b=New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)');

  SettingsPage.Add('ConnectionStrings:Master (merkezi master veritabani)', False);
  SettingsPage.Add('ConnectionStrings:SqlServer (yil veritabani sablonu)', False);
  SettingsPage.Add('JWT imzalama anahtari (en az 64 karakter)', False);

  SettingsPage.Values[0] := DefaultMaster;
  SettingsPage.Values[1] := DefaultSqlServer;

  RunProvisionCheck := TNewCheckBox.Create(SettingsPage);
  RunProvisionCheck.Parent := SettingsPage.Surface;
  RunProvisionCheck.Left := 0;
  RunProvisionCheck.Top := SettingsPage.Edits[2].Top + SettingsPage.Edits[2].Height + ScaleY(16);
  RunProvisionCheck.Width := SettingsPage.SurfaceWidth;
  RunProvisionCheck.Height := ScaleY(17);
  RunProvisionCheck.Caption := 'Veritabanini simdi hazirla (caa-provision provision)';
  RunProvisionCheck.Checked := True;

  RunLocalDbCheck := TNewCheckBox.Create(SettingsPage);
  RunLocalDbCheck.Parent := SettingsPage.Surface;
  RunLocalDbCheck.Left := 0;
  RunLocalDbCheck.Top := RunProvisionCheck.Top + RunProvisionCheck.Height + ScaleY(6);
  RunLocalDbCheck.Width := SettingsPage.SurfaceWidth;
  RunLocalDbCheck.Height := ScaleY(17);
  RunLocalDbCheck.Caption := 'SQL Server Express LocalDB 2022 bu bilgisayara kurulsun (tek makine / deneme)';
  RunLocalDbCheck.Checked := False;
  RunLocalDbCheck.OnClick := @LocalDbCheckClicked;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Secret: String;
begin
  Result := True;

  if CurPageID = SettingsPage.ID then
  begin
    if Trim(SettingsPage.Values[0]) = '' then
    begin
      MsgBox('Master baglanti dizesi bos olamaz.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    if Trim(SettingsPage.Values[1]) = '' then
    begin
      MsgBox('Yil veritabani baglanti dizesi bos olamaz.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    Secret := Trim(SettingsPage.Values[2]);
    if Length(Secret) < 64 then
    begin
      MsgBox('JWT anahtari en az 64 karakter olmalidir. Sihirbaz ustundeki PowerShell' + #13#10
        + 'komutu ile rastgele bir anahtar uretebilirsiniz.', mbError, MB_OK);
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
    Lines.Add('    "Master": "' + JsonEscape(SettingsPage.Values[0]) + '",');
    Lines.Add('    "SqlServer": "' + JsonEscape(SettingsPage.Values[1]) + '"');
    Lines.Add('  },');
    Lines.Add('  "Jwt": {');
    Lines.Add('    "SecretKey": "' + JsonEscape(Trim(SettingsPage.Values[2])) + '"');
    Lines.Add('  },');
    Lines.Add('  "DatabaseProvisioning": {');
    if RunLocalDbCheck.Checked then
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

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  ProvDir, ProvExePath, VcRedist, LocalDbMsi: String;
begin
  if CurStep <> ssPostInstall then
    Exit;

  { Ayarlari hem istemciye hem yonetici aracina yaz; ikisi ayni sunucuya baglanir. }
  WriteSettingsFile(ExpandConstant('{app}\appsettings.Local.json'));
  WriteSettingsFile(ExpandConstant('{app}\tools\appsettings.Local.json'));

  VcRedist := ExpandConstant('{tmp}\vc_redist.x64.exe');
  LocalDbMsi := ExpandConstant('{tmp}\SqlLocalDB.msi');

  { 1) VC++ Redistributable (yoksa): self-contained .NET icin on kosul. }
  if VCRedistNeeded() then
  begin
    if not Exec(VcRedist, '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      MsgBox('Microsoft Visual C++ Redistributable kurulumu baslatilamadi: ' + SysErrorMessage(ResultCode), mbError, MB_OK)
    else if ResultCode <> 0 then
      MsgBox('Microsoft Visual C++ Redistributable kurulumu tamamlanamadi (kod ' + IntToStr(ResultCode) + ').', mbError, MB_OK);
  end;

  { 2) SQL Server Express LocalDB (tek makine icin istege bagli). }
  if RunLocalDbCheck.Checked then
  begin
    if not Exec('msiexec.exe',
        '/i "' + LocalDbMsi + '" /qn /norestart IACCEPTSQLLOCALDBLICENSETERMS=YES',
        '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      MsgBox('LocalDB kurulumu baslatilamadi: ' + SysErrorMessage(ResultCode), mbError, MB_OK)
    else if ResultCode <> 0 then
      MsgBox('LocalDB kurulumu tamamlanamadi (kod ' + IntToStr(ResultCode) + ').' + #13#10
        + 'Baglanti dizesini var olan bir SQL Server''a yonlendirip tekrar deneyebilirsiniz.', mbError, MB_OK);
  end;

  { Gecici onkosul dosyalarini temizle. }
  DeleteFile(VcRedist);
  DeleteFile(LocalDbMsi);

  { 3) Veritabani hazirligi. LocalDB secildiyse uygulama ilk calismada kendi
       kullanici profilinde semayi olusturur (Mode=Automatic); provisioning atlanir. }
  if (not RunProvisionCheck.Checked) or RunLocalDbCheck.Checked then
    Exit;

  ProvDir := ExpandConstant('{app}\tools');
  ProvExePath := ProvDir + '\{#ProvExe}';

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
    MsgBox('Veritabani hazirligi tamamlandi.', mbInformation, MB_OK);
end;