using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace EsolangApp;

partial class HomeTab : ContentPage {
    static readonly Color HIGHLIGHT_COL = Color.FromRgba(255,255,0,60);
    static readonly Color NORMAL_COL = Color.FromRgba(0,0,0,0);
    
    Interpreter itptr;
    bool atStart = true;
    
    public ObservableCollection<char> Characters;
    
    public HomeTab() {
        InitializeComponent();
        BindingContext = this;
        read();

        Globals.OnAppSleepEvent += write;
        Globals.OnExecPathChangeEvent += updateExecPath;
        
        Globals.OnExecPathChange();
        
        itptr = new Interpreter(Globals.Settings, this, onReset);
        Result res = itptr.Reset(CodeEditor.Text,false);
        
        populateDebugGrid(res.board,new(0,0));
    }
    
    void populateDebugGrid(char[,] v, Tuple<int,int> h) {
        Tuple<int,int> sz = new(v.GetLength(0),v.GetLength(1));

        DebugGrid.Children.Clear();
        DebugGrid.RowDefinitions.Clear();
        DebugGrid.ColumnDefinitions.Clear();
        
        for(int i=0; i < sz.Item1; i++) DebugGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for(int i=0; i < sz.Item2; i++) DebugGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        for(int y=0; y < sz.Item1; y++)
            for(int x=0; x < sz.Item2; x++)
                DebugGrid.Add(new Label {
                    Text = v[y,x].ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 14,
                    BackgroundColor = h.Item1==y && h.Item2==x ? HIGHLIGHT_COL : NORMAL_COL
                }, y,x);
    }
    
    void updateExecPath() => FileNameLabel.Text = Path.GetFileName(Globals.ExecFile);

    void read() => CodeEditor.Text = File.ReadAllText(Globals.ExecFile);
    void write() => File.WriteAllText(Globals.ExecFile, CodeEditor.Text);

    void onRefreshClick(object s, EventArgs e) => read();
    void onSaveClick(object s, EventArgs e) => write();

    async void onCopyOutput(object s, EventArgs e) {
        await Clipboard.SetTextAsync(OutputLabel.Text);
    }

    async void onRun(object s, EventArgs e) {
        if(Globals.SettingsChanged) {
             itptr.ChangeSettings(Globals.Settings);
             Globals.SettingsChanged = false;
        }
        Result res = await itptr.Interpret(CodeEditor.Text);
        OutputLabel.Text = res.GetStr();
        atStart =  false;
        
        stepButton.IsEnabled = false;
        resetButton.IsEnabled = true;
        
        populateDebugGrid(res.board,res.pos.TT);
    }
    
    async void onStep(object s, EventArgs e) {
        Result res;
        if(atStart) {
            if(Globals.SettingsChanged) {
                itptr.ChangeSettings(Globals.Settings);
                Globals.SettingsChanged = false;
            } itptr.Reset(CodeEditor.Text,true);
            res = await itptr.Step();
            atStart = false;
        } else res = await itptr.Step();
        
        stepButton.IsEnabled = !res.done;
        resetButton.IsEnabled = !atStart;
        
        OutputLabel.Text = res.GetStr();
        
        populateDebugGrid(res.board,res.pos.TT);
    }
    
    void onReset(object s, EventArgs e) {
        if(Globals.SettingsChanged) {
            itptr.ChangeSettings(Globals.Settings);
            Globals.SettingsChanged = false;
        } atStart = true;
        
        resetButton.IsEnabled = false;
        stepButton.IsEnabled = true;
        
        OutputLabel.Text = "Output will be displayed here";
        Result res = itptr.Reset(CodeEditor.Text,false);
        
        populateDebugGrid(res.board,new(0,0));
    }
}