[Setup]
AppName=SimNite
AppVersion=0.2.0
AppPublisher=SimNite
AppPublisherURL=https://github.com/erwang64/SimNite
DefaultDirName={autopf}\SimNite
DisableProgramGroupPage=yes
; Standalone means everything is in the EXE, no extra runtime downloaded setup needed
PrivilegesRequired=lowest
OutputDir=Installer
OutputBaseFilename=SimNite_Setup_v0.2.0
SetupIconFile=Logo\SimNite_icon.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Assurez-vous d'avoir bien build l'app avec la commande :
; dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
Source: "bin\Release\net10.0-windows\win-x64\publish\SimNite.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\SimNite"; Filename: "{app}\SimNite.exe"
Name: "{autodesktop}\SimNite"; Filename: "{app}\SimNite.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\SimNite.exe"; Description: "{cm:LaunchProgram,SimNite}"; Flags: nowait postinstall skipifsilent
