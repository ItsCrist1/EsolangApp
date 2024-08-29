using System;
using System.IO;
using System.Text.Json;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace EsolangApp;	

partial class App : Application {
    const string DEF_CONTENT = "}0;SD=1-S\n∆)1AW   _$§@*)∆";
    
	public App() {
		InitializeComponent();
		InitializeGlobals();
	
		MainPage = new TabRoot();
	}
	
	void InitializeGlobals() {
		Globals.SaveFile = Path.Combine(Globals.LOCAL_DATA, Globals.SaveFile);
			
		if(!File.Exists(Globals.SaveFile)) {
			Globals.Settings = Settings.Default;
			File.WriteAllText(Globals.SaveFile, JsonSerializer.Serialize(Settings.Default));
		} else Globals.Settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Globals.SaveFile));
		
		string execFile = Preferences.Get(Globals.EXEC_FILE_ADDRESS, null);
        
		if(execFile == null) {
			Globals.ExecFile = Path.Combine(Globals.LOCAL_DATA, Globals.DEF_EXEC_FILE);
			File.WriteAllText(Globals.ExecFile, DEF_CONTENT);
		} else Globals.ExecFile = execFile;
	}
	
	protected override void OnSleep() => Globals.OnAppSleep();
}