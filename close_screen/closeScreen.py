# encoding = utf-8
from ctypes import windll

from wox import Wox,WoxAPI

HWND_BROADCAST = 0xffff
WM_SYSCOMMAND = 0x0112
SC_MONITORPOWER = 0xF170
MonitorPowerOff = 2
SW_SHOW = 5

class CloseScreen(Wox):
    def query(self, key):
        results = []
        results.append({
            "Title":"Close Screen",
            "SubTitle":"close your laptop's screen",
            "IcoPath":"Images/screen.png",
            "JsonRPCAction":{
                "method":"close",
                "parameters":[],
                "dontHideAfterAction":False
            }
        })
        return results

    def close(self):
        windll.user32.PostMessageW(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, MonitorPowerOff)

        shell32 = windll.LoadLibrary("shell32.dll");
        shell32.ShellExecuteW(None, 'open', 'rundll32.exe', 'USER32,LockWorkStation', '', SW_SHOW)

if __name__ == "__main__":
      CloseScreen()
