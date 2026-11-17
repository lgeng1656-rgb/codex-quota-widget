#define AppVersion "1.1.0"
[Setup]
AppId={{64818918-5E8C-4B95-995B-F54B52CBEEAC}
AppName=Codex Quota Widget
AppVersion={#AppVersion}
AppPublisher=GG
DefaultDirName={localappdata}\Programs\CodexQuotaWidget
DefaultGroupName=Codex Quota Widget
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=dist
OutputBaseFilename=CodexQuotaWidget-Setup-{#AppVersion}
SetupIconFile=assets\app.ico
UninstallDisplayIcon={app}\CodexQuotaWidget.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0
CloseApplications=yes
RestartApplications=no
[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked
Name: "startup"; Description: "Start with Windows (show desktop quota card)"
[Files]
Source: "dist\CodexQuotaWidget.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "DISTRIBUTION.md"; DestDir: "{app}"; DestName: "README.md"; Flags: ignoreversion
[Icons]
Name: "{group}\Codex Quota Widget"; Filename: "{app}\CodexQuotaWidget.exe"
Name: "{group}\Uninstall Codex Quota Widget"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Codex Quota Widget"; Filename: "{app}\CodexQuotaWidget.exe"; Tasks: desktopicon
[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "CodexQuotaWidget"; ValueData: """{app}\CodexQuotaWidget.exe"""; Flags: uninsdeletevalue; Tasks: startup
[Run]
Filename: "{app}\CodexQuotaWidget.exe"; Description: "Launch Codex Quota Widget"; Flags: nowait postinstall skipifsilent
