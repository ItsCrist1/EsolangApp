using System;
using System.IO;
using System.Text.Json;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace EsolangApp;

partial class SettingsTab : ContentPage {
	public SettingsTab() {
		InitializeComponent();
		
		PICtoggle.IsToggled = Globals.Settings.PerformInitialChecks;
		CREtoggle.IsToggled = Globals.Settings.CauseRuntimeErrors;
        WAtoggle.IsToggled = Globals.Settings.WrapAround;
        LOGtoggle.IsToggled = Globals.Settings.EnableLogging;
		
		PICtoggle.Toggled += onTogglePIC;
		CREtoggle.Toggled += onToggleCRE;
        WAtoggle.Toggled += onToggleWA;
		LOGtoggle.Toggled += onToggleLOG;
        
		updateExecPath();
        
        SeedLabel.Text = $"Seed: {Globals.Settings.Seed}";
        LogPath.Text = $"Log Path: {Globals.Settings.LogDir}";
        PrecisionLabel.Text = $"Decimals Displayed: {Globals.Settings.Precision}";
	}
	
	void updateExecPath() {
        ExecPath.Text = $"Execution File Path: {Path.GetFileName(Globals.ExecFile)}";
        Globals.OnExecPathChange();
    }
	
	void onTogglePIC(object s, EventArgs e) {
		Globals.Settings.PerformInitialChecks = !Globals.Settings.PerformInitialChecks;
	    Globals.OnAppSleep();
	}
	
	void onToggleCRE(object s, EventArgs e) {
		Globals.Settings.CauseRuntimeErrors = !Globals.Settings.CauseRuntimeErrors;
		Globals.OnAppSleep();
	}
    
    void onToggleWA(object s, EventArgs e) {
        Globals.Settings.WrapAround = !Globals.Settings.WrapAround;
        Globals.OnAppSleep();
    }
    
    void onToggleLOG(object s, EventArgs e) {
        Globals.Settings.EnableLogging = !Globals.Settings.EnableLogging;
        Globals.OnAppSleep();
    }
	
	void onExecPathResetClick(object s, EventArgs e) {
		Globals.ExecFile = Path.Combine(Globals.LOCAL_DATA, Globals.DEF_EXEC_FILE);
		if(!File.Exists(Globals.ExecFile)) File.WriteAllText(Globals.ExecFile, "∆");
		updateExecPath();
	}
	
	async void onExecPathClick(object s, EventArgs e) {
		string r = await DisplayPromptAsync("Select new path", "Enter a new execution file path:", initialValue: Globals.ExecFile, keyboard: Keyboard.Url);
		if(File.Exists(r)) {
			Globals.ExecFile = r;
			updateExecPath();
		} else if(r != null) await DisplayAlert("Error", $"{r} is not a valid file path", "Ok");
	}
    
    void onSeedResetClick(object s, EventArgs e) {
        Globals.Settings.Seed = 0;
        SeedLabel.Text = "Seed: 0";
        Globals.OnAppSleep();
    }
    
    async void onSeedChangeClick(object s, EventArgs e) {
        string r = await DisplayPromptAsync("Select new seed", "Enter a new seed for the random generation:", initialValue: Globals.Settings.Seed.ToString(), keyboard: Keyboard.Numeric);
        if(int.TryParse(r, out int n)) {
            Globals.Settings.Seed = n;
            SeedLabel.Text = $"Seed: {n}";
            Globals.OnAppSleep();
        } else if(!string.IsNullOrEmpty(r)) await DisplayAlert("Error", $"{r} is not a valid seed (an integer)", "Ok");
    }
    
    void onLogcPathResetClick(object s, EventArgs e) {
        string logDir = Path.Combine(Globals.LOCAL_DATA,"EsolangLogs");
        if(!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
        Globals.Settings.LogDir = logDir;
        LogPath.Text = $"Log Path: {logDir}";
    }
    
    async void onLogPathClick(object s, EventArgs e) {
    	string r = await DisplayPromptAsync("Select new Log path:", "Enter a new logging file path:", initialValue: Globals.Settings.LogDir, keyboard: Keyboard.Url);
		if(Directory.Exists(r)) {
			Globals.Settings.LogDir = r;
			LogPath.Text = $"Log Path: {r}";
		} else if(r != null) await DisplayAlert("Error", $"{r} is not a valid directory", "Ok");
    }
    
    async void onDocsClick(object s, EventArgs e) {
        await Launcher.OpenAsync("https://github.com/ItsCrist1/EsolangApp/blob/main/README.md");
    }
    
    void onPrecisionResetClick(object s, EventArgs e) {
        Globals.Settings.Precision = 2;
        PrecisionLabel.Text = "Decimals Displayed: 2";
    }
    
    async void onPrecisionClick(object s, EventArgs e) {
        string r = await DisplayPromptAsync("Select new precision", "Enter a new precision to be displayed for floating point numbers:", initialValue: Globals.Settings.Precision.ToString(), keyboard: Keyboard.Numeric);
        if(uint.TryParse(r, out uint n)) {
            Globals.Settings.Precision = n;
            PrecisionLabel.Text = $"Decimals Displayed: {n}";
            Globals.OnAppSleep();
        } else if(!string.IsNullOrEmpty(r)) await DisplayAlert("Error", $"{r} is not a valid precision (a positive integer)", "Ok");
    }
}