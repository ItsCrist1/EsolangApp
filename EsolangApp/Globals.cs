using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace EsolangApp;

public static class Globals  {
	public readonly static string DEF_EXEC_FILE = "exec.el";
	public readonly static string EXEC_FILE_ADDRESS = "EXEC_FILE";
	public readonly static string LOCAL_DATA = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		
	public static string ExecFile;
		
    public static string SaveFile = "mem.json";
    public static Settings Settings;
	
	public static event Action OnAppSleepEvent;
    public static event Action OnExecPathChangeEvent;
	
	public static void OnAppSleep() {
		File.WriteAllText(SaveFile, JsonSerializer.Serialize(Settings));
		Preferences.Set(EXEC_FILE_ADDRESS, ExecFile);
		OnAppSleepEvent?.Invoke();
	}
    
    public static void OnExecPathChange() {
        OnExecPathChangeEvent?.Invoke();
    }
}
