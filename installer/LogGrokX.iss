#define MyAppName "LogGrokX"
#define MyAppPublisher "LogGrokX"
#define MyAppURL "https://github.com/zhenyatnk/LogGrokX"
#define MyAppExeName "LogGrokX.exe"

#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif
#ifndef MyAppIdLine
  #define MyAppIdLine "AppId={{E9F2AF62-B22A-458C-88CF-1078F8A6CB65}"
#endif
#ifndef MyArchSuffix
  #define MyArchSuffix "x64"
#endif
#ifndef MyArchitecturesAllowed
  #define MyArchitecturesAllowed "x64compatible"
#endif
#ifndef MyArchitecturesInstallIn64BitModeLine
  #define MyArchitecturesInstallIn64BitModeLine "ArchitecturesInstallIn64BitMode=x64compatible"
#endif
#ifndef MySourceDir
  #define MySourceDir "publish-x64"
#endif

[Setup]
{#MyAppIdLine}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=LogGrokX-{#MyAppVersion}-{#MyArchSuffix}-setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed={#MyArchitecturesAllowed}
{#MyArchitecturesInstallIn64BitModeLine}
UninstallDisplayIcon={app}\{#MyAppExeName}
AppMutex=2A7759B1-AA14-4ABA-A05C-CFFEF9CE1D5A
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "fileassoc_log"; Description: "{cm:AssocLog}"; GroupDescription: "{cm:FileAssoc}"

[Dirs]
; Logs and crash dumps go under %ProgramData%\LogGrokX\Users\<user>. Grant the Users group
; modify access so each user can create their own subfolder without admin rights.
; (Per-user settings and caches live under %LOCALAPPDATA% and need no special permissions.)
Name: "{commonappdata}\LogGrokX"; Permissions: users-modify
Name: "{commonappdata}\LogGrokX\Users"; Permissions: users-modify

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Excludes: "appsettings.yaml"; Flags: ignoreversion recursesubdirs createallsubdirs
; Never overwrite user settings on upgrade, and keep them on uninstall.
Source: "{#MySourceDir}\appsettings.yaml"; DestDir: "{app}"; Flags: onlyifdoesntexist uninsneveruninstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKA; Subkey: "Software\Classes\.log\OpenWithProgids"; ValueType: string; ValueName: "LogGrokX.log"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc_log
Root: HKA; Subkey: "Software\Classes\LogGrokX.log"; ValueType: string; ValueName: ""; ValueData: "Log File"; Flags: uninsdeletekey; Tasks: fileassoc_log
Root: HKA; Subkey: "Software\Classes\LogGrokX.log\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc_log
Root: HKA; Subkey: "Software\Classes\LogGrokX.log\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc_log

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[CustomMessages]
english.FileAssoc=File associations:
english.AssocLog=Associate with .log files
russian.FileAssoc=Ассоциации файлов:
russian.AssocLog=Связать с файлами .log
german.FileAssoc=Dateizuordnungen:
german.AssocLog=Mit .log-Dateien verknüpfen
french.FileAssoc=Associations de fichiers :
french.AssocLog=Associer aux fichiers .log
spanish.FileAssoc=Asociaciones de archivos:
spanish.AssocLog=Asociar con archivos .log
japanese.FileAssoc=ファイルの関連付け:
japanese.AssocLog=.log ファイルに関連付ける
polish.FileAssoc=Skojarzenia plików:
polish.AssocLog=Skojarz z plikami .log
brazilianportuguese.FileAssoc=Associações de arquivos:
brazilianportuguese.AssocLog=Associar a arquivos .log
