library ShellHook;

uses
  Windows;

{$R *.res}

var
  HookHandle: HHOOK;

function ShellProc(nCode: Integer; WParam: WPARAM; LParam: LPARAM): LRESULT;  stdcall;
begin
  Result := CallNextHookEx(HookHandle, nCode, wParam,lParam);
end;

procedure SetListener() export; stdcall;
begin
    //HookHandle := SetWindowsHookEx(WH_SHELL, @ShellProc, HInstance, 0);
end;

procedure RemoveListener() export; stdcall;
begin

end;


exports SetListener name 'setListener';
exports RemoveListener name 'removeListener';

begin

end.
