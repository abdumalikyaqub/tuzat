#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

[Setup]
AppId={{8F3C2A71-5B4E-4D2A-9C1F-6E7A3B2D9F10}
AppName=Tuzat
AppVersion={#AppVersion}
AppVerName=Tuzat {#AppVersion}
AppPublisher=abdumalikyaqub
AppPublisherURL=https://github.com/abdumalikyaqub/tuzat
AppSupportURL=https://github.com/abdumalikyaqub/tuzat/issues
AppUpdatesURL=https://github.com/abdumalikyaqub/tuzat/releases
LicenseFile=..\LICENSE
DefaultDirName={localappdata}\Programs\Tuzat
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppMutex=Tuzat.SingleInstance
UninstallDisplayIcon={app}\Tuzat.exe
OutputDir=..\artifacts
OutputBaseFilename=Tuzat-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "autostart"; Description: "{cm:AutoStartProgram,Tuzat}"

[Files]
Source: "..\artifacts\publish\Tuzat.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Tuzat"; Filename: "{app}\Tuzat.exe"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Tuzat"; ValueData: """{app}\Tuzat.exe"""; Flags: uninsdeletevalue; Tasks: autostart
; Removes the value on uninstall even when autostart was turned on later from the tray menu.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "Tuzat"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\Tuzat.exe"; Description: "{cm:LaunchProgram,Tuzat}"; Flags: nowait postinstall skipifsilent
