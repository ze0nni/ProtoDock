library ShellHook;

uses Windows, Messages, SysUtils;

var
  mtHook: THandle;
  uiWM_CapturedKeyboardLayout: UINT;

function HookProc (code: Integer; w: WParam; l: LParam): Longint; stdcall;
  var dwRecipients: DWORD;
begin
  writeln(code);

  if (code = HSHELL_LANGUAGE) then
  begin
    dwRecipients := BSM_APPLICATIONS;
    BroadcastSystemMessage(BSF_POSTMESSAGE, @dwRecipients, uiWM_CapturedKeyboardLayout, w, l);
  end;

  result := CallNextHookEx (mtHook, code, w, l);
end;

procedure RemoveHook; stdcall;
begin
  if mtHook=0 then exit;
  if UnHookWindowsHookEx(mtHook) then
    mtHook:=0;
end;

function SetHook(): THandle; stdcall;
begin
    AllocConsole();

    RemoveHook;
    mtHook := SetWindowsHookEx(WH_SHELL, @HookProc, hInstance, 0);
    result := mtHook;
end;

procedure DllEntryPoint(dwReason : DWORD);
begin
  case dwReason of
    Dll_Process_Attach:
      begin
        WriteLn('Attach');
        uiWM_CapturedKeyboardLayout := RegisterWindowMessage('KBHook_WM_CapturedKeyboardLayout');
      end;
    Dll_Process_Detach:
      begin
        WriteLn('Detach');
      end;
  end;
end;

exports SetHook, RemoveHook;

begin
    mtHook:=0;
    DLLProc := @DLLEntryPoint;
    DllEntryPoint(Dll_Process_Attach);
end.
