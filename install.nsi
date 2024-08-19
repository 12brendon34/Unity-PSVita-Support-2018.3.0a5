!include "MUI2.nsh"
!define PRODUCT_VERSION "2018.3.0.48317"
!define VERSION "2018.3.0.48317"

Name "Unity 2018.3.0a5 Playstation Vita Support"
OutFile "UnitySetup-Playstation-Vita-Support-for-Editor-2018.3.0a5.exe"

VIFileVersion "${VERSION}"
VIProductVersion "${PRODUCT_VERSION}"
VIAddVersionKey "CompanyName" "Unity Technologies ApS"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "LegalCopyright" "(c) 2018 Unity Technologies ApS. All rights reserved."
VIAddVersionKey "FileDescription" "Unity 2018.3.0a5 Playstation Vita Support Installer 10796221-a4bcbd131364 for Editor 2018.3.0.48317"
VIAddVersionKey "ProductName" "Unity 2018.3.0a5 Playstation Vita Support"
VIAddVersionKey "Unity Version" "2018.3.0a5"

Unicode true
SetCompressor lzma
SetCompressorDictSize 8

;Get installation folder from registry if available
InstallDirRegKey HKCU "Software\Unity Technologies\Installer\Unity 2018.3.0a5" "Location x64"

;UAC
RequestExecutionLevel admin

;Interface Settings
!define MUI_ICON "Plugins\Icon.ico"
!define MUI_ABORTWARNING
!define MUI_LICENSEPAGE_CHECKBOX
!define MUI_WELCOMEFINISHPAGE_BITMAP "Plugins\modern-wizard.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "Plugins\modern-wizard.bmp"

;Pages
!insertmacro MUI_PAGE_WELCOME

!define MUI_PAGE_CUSTOMFUNCTION_SHOW LicensePageChanges
!insertmacro MUI_PAGE_LICENSE "Plugins\License.rtf"

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

;Languages
!insertmacro MUI_LANGUAGE "English"

;documentation path
Var _58_

;\Editor\Data\PlaybackEngines\PSP2Player
Var _59_

;\Editor\Data\il2cpp
Var _60_

Function LicensePageChanges
    ShowWindow $mui.LicensePage.TopText 0
    ShowWindow $mui.LicensePage.Text 0
    System::Call 'user32::MoveWindow(i$mui.LicensePage.LicenseText,i0,i0,i450,i200,i0)'
FunctionEnd

Section "Unity Playstation Vita Support" Description_Text
    ;SetOutPath "$INSTDIR"
    StrCpy $_59_ \Editor\Data\PlaybackEngines\PSP2Player
    StrCpy $_60_ \Editor\Data\il2cpp


${If} ${FileExists} "$INSTDIR$_59_\*"
    DetailPrint "Removing old Unity Playstation Vita Support installation..."
    RMDir /r $INSTDIR$_59_
${EndIf}

    DetailPrint "Installing Unity Playstation Vita Support..."
    CreateDirectory $INSTDIR$_59_
    SetOutPath $INSTDIR$_59_
    SetOverwrite try
    File /r "INSTDIR\*.*"
	

${If} ${FileExists} "$INSTDIR$_60_\*"
    DetailPrint "Removing unpatched il2cpp..."
    RMDir /r $INSTDIR$_60_
${EndIf}

    SetOutPath $INSTDIR$_60_
    File /r "IL2CPP\*.*"

    ;documentation
    DetailPrint "Installing documentation for Playstation Vita ..."
	
    ; 58 = reg editor path +  \Editor\Data\PlaybackEngines\PSP2Player + \Documentation
    StrCpy $_58_ $INSTDIR$_59_\Documentation
    SetOutPath $_58_
    File /r "DOCS\*.*"
	
    DetailPrint "Indexing documentation ..."
    ExecWait "$\"$_58_\DocCombiner.exe$\" -autopaths $\"$INSTDIR$\"" $0
	
SectionEnd

;Language strings
LangString DESC_SecDummy ${LANG_ENGLISH} "Fuck it we ball."

;Assign language strings to sections
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${Description_Text} $(DESC_SecDummy)
!insertmacro MUI_FUNCTION_DESCRIPTION_END
