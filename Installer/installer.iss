#define public Version "0.6.1"

#ifdef EXTERNAL_VERSION
 #if len(EXTERNAL_VERSION)>0
     #define public Version EXTERNAL_VERSION
 #endif
#endif 

[Setup]
AppId={{9444602B-C5D8-4EF5-9D5B-E76D06B53C71}
AppName=Aurora
AppVersion=v{#Version}
AppVerName=Aurora v{#Version}
AppPublisher=Anton Pupkov
AppPublisherURL=http://www.project-aurora.com/
AppSupportURL=https://github.com/antonpup/Aurora/issues/
AppUpdatesURL=https://github.com/antonpup/Aurora/releases
DefaultDirName={pf64}\Aurora
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir=..\
OutputBaseFilename=Aurora-setup-v{#Version}
Compression=lzma
SolidCompression=yes
UninstallDisplayIcon={app}\Aurora.exe
SetupIconFile=Aurora_updater.ico
WizardImageFile=Aurora-wizard.bmp

//#include <idp.iss>

[Messages]
WelcomeLabel2=This will install Aurora on your computer.%n%nAurora is a utility that unifies RGB lighting devices across different brands and enables them to work alongside each other, all while adding and improving RGB lighting support for various games that previous had none or little RGB lighting support.

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
//Source: "unzipper.dll"; Flags: dontcopy
Source: "..\Build\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
//AfterInstall: ExtractMe('{app}\Aurora-v{#Version}.zip', '{app}')
Source: "vcredist_x86.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "vcredist_x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{commonprograms}\Aurora"; Filename: "{app}\Aurora.exe"
Name: "{commondesktop}\Aurora"; Filename: "{app}\Aurora.exe"; Tasks: desktopicon
  
[Code]
//procedure unzip(src, target: AnsiString);
//external 'unzip@files:unzipper.dll stdcall delayload';

//procedure ExtractMe(src, target : String);
//begin
//  unzip(ExpandConstant(src), ExpandConstant(target));
//end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  case CurUninstallStep of
    usUninstall:
      begin
        if MsgBox(ExpandConstant('Do you want to remove all the settings?'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
          begin
             DelTree(ExpandConstant('{userappdata}\Aurora'), True, True, True);
          end
      end;
  end;
end; 


#IFDEF UNICODE
  #DEFINE AW "W"
#ELSE
  #DEFINE AW "A"
#ENDIF
function VCRedistX64NeedsInstall: Boolean;
begin
  Result := not RegKeyExists(HKEY_CLASSES_ROOT,'Installer\Dependencies\,,amd64,14.0,bundle\Dependents\{5b295ba9-ef89-4aeb-8acc-b61adb0b9b5f}');
end;

function VCRedistX86NeedsInstall: Boolean;
begin
  Result := not RegKeyExists(HKEY_CLASSES_ROOT,'Installer\Dependencies\,,x86,14.0,bundle\Dependents\{ec9c2282-a836-48a6-9e41-c2f0bf8d678b}');
end;

[Run]
Filename: "{app}\Aurora.exe"; Flags: nowait postinstall skipifsilent runascurrentuser; Description: "{cm:LaunchProgram,Aurora}"
Filename: "{tmp}\vcredist_x86.exe"; Check: VCRedistX86NeedsInstall
Filename: "{tmp}\vcredist_x64.exe"; Check: VCRedistX64NeedsInstall and IsWin64

[UninstallDelete]
;This works only if it is installed in default location
Type: filesandordirs; Name: "{pf}\Aurora"


;This works if it is installed in custom location
Type: files; Name: "{app}\*"; 
Type: filesandordirs; Name: "{app}"

