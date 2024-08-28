using System;
using System.IO;
using System.Text.Json;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace EsolangApp;

partial class SettingsTab : ContentPage {
	public SettingsTab() {
		InitializeComponent();
		
		PICtoggle.IsToggled = Globals.Settings.PerformInitialChecks;
		CREtoggle.IsToggled = Globals.Settings.CauseRuntimeErrors;
		
		PICtoggle.Toggled += onTogglePIC;
		CREtoggle.Toggled += onToggleCRE;
		
		updateExecPath();
        
        SeedLabel.Text = $"Seed: {Globals.Settings.Seed}";
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
	
	void onExecPathResetClick(object s, EventArgs e) {
		Globals.ExecFile = Path.Combine(Globals.LOCAL_DATA, Globals.DEF_EXEC_FILE);
		if(!File.Exists(Globals.ExecFile)) File.WriteAllText(Globals.ExecFile, "∆");
		updateExecPath();
	}
	
	async void onExecPathClick(object s, EventArgs e) {
		string r = await DisplayPromptAsync("Select new Path", "Enter a new Execution file path:", initialValue: Globals.ExecFile, keyboard: Keyboard.Url);
		if(File.Exists(r)) {
			Globals.ExecFile = r;
			updateExecPath();
		} else if(r != null) await DisplayAlert("Error", $"{r} is not a valid file path", "Ok");
	}
    
    void onSeedResetClick(object s, EventArgs e) {
        Globals.Settings.Seed = 0;
        SeedLabel.Text = "Seed: 0";
    }
    
    async void onSeedChangeClick(object s, EventArgs e) {
        string r = await DisplayPromptAsync("Select new seed", "Enter a new seed for the random generation:", initialValue: Globals.Settings.Seed.ToString(), keyboard: Keyboard.Numeric);
        if(int.TryParse(r, out int n)) {
            Globals.Settings.Seed = n;
            SeedLabel.Text = $"Seed: {n}";
        } else await DisplayAlert("Error", $"{r} is not a valid seed (an integer)", "Ok");
    }
}