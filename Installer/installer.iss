#define public Version "0.8.0"

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
CloseApplications=no 
// ^ This line is important because when it is set to yes it would ask user to exit Aurora. But uninstaller/installer already kills Aurora processes.

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

function ExecuteHidden(const command: string; const parameters: string): Integer;
var
  ResultCode: Integer;
begin
  Exec(command, parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := ResultCode;
end;

function CmdLineParamExists(const Value: string): Boolean; // stackoverflow.com/a/48349992
var
  I: Integer;  
begin
  Result := False;
  for I := 1 to ParamCount do
    if CompareText(ParamStr(I), Value) = 0 then
    begin
      Result := True;
      Exit;
    end;
end;

function GetUninstallString(): String; // stackoverflow.com/a/2099805 but slightly modified to work with 64bit machines
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    begin
      if not RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString) then
        begin
          sUnInstPath := ExpandConstant('SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
          if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
            RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
        end;
    end;

  Result := sUnInstallString;
end;

function KeepSettings: Boolean;
begin
  Result := CmdLineParamExists('/keepsettings');
end;

function KeepStartupTask: Boolean;
begin
  Result := CmdLineParamExists('/keepstartuptask');
end;

procedure TaskKill(FileName: String);
begin
  ExecuteHidden('taskkill.exe','/f /im ' + '"' + FileName + '"')
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
   sUnInstallString: String;
begin
  case CurStep of
    ssInstall:
      begin
        if (not WizardSilent()) then
          begin
            MsgBox(ExpandConstant('The installer will now try to close running instances of {#SetupSetting("AppName")} and uninstall them. Please save your work.'), mbConfirmation, MB_OK or MB_DEFBUTTON2);
          end;
        TaskKill('Aurora.exe');
        TaskKill('Aurora-SkypeIntegration.exe');
        TaskKill('Aurora-Updater.exe');
        
        sUnInstallString := GetUninstallString();
        if sUnInstallString <> '' then 
          begin
            sUnInstallString := RemoveQuotes(sUnInstallString);
            ExecuteHidden(sUnInstallString,'/verysilent /keepsettings /keepstartuptask');
          end;

      end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  case CurUninstallStep of
    usUninstall:
      begin
        if (not UninstallSilent()) then
          begin
            MsgBox(ExpandConstant('The uninstaller will now try to close running instances of {#SetupSetting("AppName")} if there are any. Please save your work.'), mbConfirmation, MB_OK or MB_DEFBUTTON2);
            TaskKill('Aurora.exe');
            TaskKill('Aurora-SkypeIntegration.exe');
            TaskKill('Aurora-Updater.exe');

            if ((not KeepSettings()) and (MsgBox(ExpandConstant('Do you want to remove all the settings and user data?'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES)) then
              begin
                if (MsgBox(ExpandConstant('Do you really confirm removing all settings and user data?'), mbError, MB_YESNO or MB_DEFBUTTON2) = IDYES) then
                  begin
                    DelTree(ExpandConstant('{userappdata}\Aurora'), True, True, True);
                  end;

              end;
          end
        else
          begin
            TaskKill('Aurora.exe');
            TaskKill('Aurora-SkypeIntegration.exe');
            TaskKill('Aurora-Updater.exe');

            if (not KeepSettings()) then
              begin
                DelTree(ExpandConstant('{userappdata}\Aurora'), True, True, True);
              end;
          end;
        if (not KeepStartupTask()) then
          begin
            ExecuteHidden('schtasks.exe', '/delete /tn "AuroraStartup" /f');
          end;
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
  Result := not RegKeyExists(HKEY_LOCAL_MACHINE,'SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64');
end;
                                             
function VCRedistX86NeedsInstall: Boolean;
begin
  Result := not RegKeyExists(HKEY_LOCAL_MACHINE,'SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x86');
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

