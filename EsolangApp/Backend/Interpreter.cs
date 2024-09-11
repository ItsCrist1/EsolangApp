using System;
using System.IO;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace EsolangApp;

public enum Dir { N, S, E, W, NE, NW, SE, SW }
public enum ErrorType { EmptyStack, InvalidRange, InvalidNumber, InvalidNumberInput, InvalidStringInput, InvalidPtrPos, DivisionByZero }
public enum InitErrorType { EmptySource, UnevenBrackets, LackOfEnd }

public struct Settings {
    [JsonInclude] public bool PerformInitialChecks;
    [JsonInclude] public bool CauseRuntimeErrors;
    [JsonInclude] public int Seed;
    [JsonInclude] public bool WrapAround;
    [JsonInclude] public string LogDir;
    [JsonInclude] public bool EnableLogging;
    [JsonInclude] public uint Precision;
    [JsonInclude] public bool Delimeter;

    [JsonConstructor]
    public Settings(bool PerformInitialChecks, bool CauseRuntimeErrors, int Seed, bool WrapAround, string LogDir, bool EnableLogging, uint Precision, bool Delimeter) {
        this.PerformInitialChecks = PerformInitialChecks;
        this.CauseRuntimeErrors = CauseRuntimeErrors;
        this.Seed = Seed;
        this.WrapAround = WrapAround;
        this.LogDir = LogDir;
        this.EnableLogging = EnableLogging;
        this.Precision = Precision;
        this.Delimeter = Delimeter;
    }

    public static Settings Default() {
        string logDir = Path.Combine(Globals.LOCAL_DATA,"EsolangLogs");
        if(!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
        return new(true,true,0,true,logDir,true,2,true);
    }
}

record struct Pos {
    public int x,y;
    public Pos(int x, int y) { this.x = x; this.y = y; }
    public Pos(int w) => x = y = w;

    public string TS => $"({x},{y})";
    public Tuple<int,int> TT => new(x,y);

    public bool IsWithin(char[,] b) {
        if(x < 0 || x > b.GetLength(0) - 1) return false;
        if(y < 0 || y > b.GetLength(1) - 1) return false;
        return true;
    }

    public static Pos operator +(Pos p, Dir d) => d switch {
        Dir.N => new Pos(p.x, p.y-1),
        Dir.E => new Pos(p.x+1, p.y),
        Dir.S => new Pos(p.x, p.y+1),
        Dir.W => new Pos(p.x-1, p.y),
        Dir.NE => new Pos(p.x+1, p.y-1),
        Dir.NW => new Pos(p.x-1, p.y-1),
        Dir.SE => new Pos(p.x+1, p.y+1),
        Dir.SW => new Pos(p.x-1, p.y +1),
        _ => p
    };
}

class Result {
    public readonly char[,] board;
    public readonly long execTime;
    public readonly string output;
    public readonly Stack<double> stack;
    public readonly Pos pos;
    public readonly Dir dir;
    public readonly uint steps;
    
    public readonly bool done;

    public Result(char[,] board, long execTime, string output, Stack<double> stack, Pos pos, Dir dir, uint steps, bool done) {
        this.board = board;
        this.execTime = execTime;
        this.output = output;
        this.stack = stack;
        this.pos = pos;
        this.dir = dir;
        this.steps = steps;
        this.done = done;
    }
    
    static public Result Null => new Result(null,0,null,null,new Pos(0),(Dir)0,0,false);

    public string GetStr(Settings settings) {
        StringBuilder str = new();
        
        string t = output == string.Empty ? "Nothing." : output;
        
        str.AppendLine($"\nOutput: {(t == string.Empty ? "Nothing" : t)}").Append("Stack:");
        
        if(stack.Count == 0) str.Append(" Empty.");
		else foreach(double i in stack) str.Append($" {i};");
        
        str.AppendLine()
           .AppendLine($"Pos: {pos.TS}")
           .AppendLine($"Dir: {dirTS(dir)}")
           .AppendLine($"Steps: {steps}")
           .Append($"Execution Time: {execTime}ms");
        return str.ToString();
    }
	
	string dirTS(Dir d) => d switch {
		Dir.N => "North",
		Dir.E => "East",
		Dir.S => "South",
		Dir.W => "West",
		
		Dir.NE => "North-East",
		Dir.NW => "North-West",
		Dir.SE => "South-East",
		Dir.SW => "South-West"
	};
}

class Logger {
    const bool CONSOLE_LOG = false;
    readonly string file;
    
    public Logger(string file, bool logInit=true) {
        this.file = file;
        if(!logInit) return;
        string s = $"[{DateTime.Now}] Debugging started\n";
        if(CONSOLE_LOG) File.WriteAllText(file, $"{s}\n");
    }
    
    public void Log(string str) {
        string s = $"[{DateTime.Now}] {str}\n";
        File.AppendAllText(file, $"{s}\n");
        if(CONSOLE_LOG) Console.WriteLine(s);
    }
}

class InitError {
    readonly InitErrorType initErrorType;
    readonly string additionalInfo;

    public InitError(InitErrorType initErrorType, string additionalInfo) {
        this.initErrorType = initErrorType;
        this.additionalInfo = additionalInfo;
    }

    public void Cause() => throw new Exception($"Initial esolang error\nType: {initErrorType}\nAdditional Info: {additionalInfo}");
}

class Error {
    readonly ErrorType errorType;
    readonly string additionalInfo;
    readonly Settings settings;
    readonly Logger logger;

    public Error(ErrorType errorType, string additionalInfo, Settings settings, Logger logger) {
        this.errorType = errorType;
        this.additionalInfo = additionalInfo;
        this.settings = settings;
        this.logger = logger;
    }

    public void Cause() {
        string str = $"Esolang error\nType: {errorType}\nAdditional Info: {additionalInfo}";
        if(settings.EnableLogging) logger.Log(str);
        throw new Exception(str);
    }
}

class CMD {
	readonly Action syncFunc;
	readonly Func<Task> asyncFunc;
	
	public CMD(Action syncFunc) => this.syncFunc = syncFunc;
	public CMD(Func<Task> asyncFunc) => this.asyncFunc = asyncFunc;
	
	public async Task ExecFunc() {
		if(asyncFunc == null) syncFunc();
		else await asyncFunc();
	}
}

class Interpreter : Utils {
    Stack<double> stack;
    string output;
    Pos pos;
    Dir dir;
    char c;
    bool strMode, numMode;
    string numBuf, strBuf;
    int toLoop;
    uint steps;

    Stopwatch sw = new();
    Settings settings;
    ContentPage contentPage;
    Random rand;
    Action<object,EventArgs> resetFunc;
    Logger logger;
    
    char[,] board;
	
	Dictionary<char,CMD> CMDS;
    
    /* TODO: Implement an unknown symbol count to
	register all of the unknown symbols if the dictionary
	doesn't find it. */

    public Interpreter(Settings settings, ContentPage contentPage, Action<object,EventArgs> resetFunc, string logDir) {
        this.settings = settings;
        this.contentPage = contentPage;
        this.rand = new(settings.Seed);
        this.resetFunc = resetFunc;
        this.logger = new(Path.Combine(logDir, $"Log_{Directory.GetFiles(logDir).Count()+1}.log"), settings.EnableLogging);
		
		bool b = settings.CauseRuntimeErrors;
		
		this.CMDS = new() {
			{ _NORTH, new(() => { dir = Dir.N; }) },
			{ _SOUTH, new(() => { dir = Dir.S; }) },
            { _EAST,  new(() => { dir = Dir.E; }) },
			{ _WEST,  new(() => { dir = Dir.W; }) },
			
			{ _NORTH_EAST, new(() => { dir = Dir.NE; }) },
			{ _NORTH_WEST, new(() => { dir = Dir.NW; }) },
            { _SOUTH_EAST, new(() => { dir = Dir.SE; }) },
			{ _SOUTH_WEST, new(() => { dir = Dir.SW; }) },
			
			{ _ADD, new(() => OP_add(b)) },
			{ _SUBTRACT, new(() => OP_sub(b)) },
            { _MULTIPLY, new(() => OP_mlt(b)) },
			{ _DIVIDE, new(() => OP_div(b)) },
			{ _MODULO, new(() => OP_mod(b)) },
			{ _POW, new(() => OP_pow(b)) },
			
			{ _GET, new(() => MC_get(b)) },
			{ _SET, new(() => MC_set(b)) },
			
			{ _STR_OUTPUT, new(() => {
				if(stack.Count > 0) output += (char)stack.Pop();
                else if(b) new Error(ErrorType.EmptyStack, "Cannot output from an empty stack",settings,logger).Cause();
			})},
			
			{ _NUM_OUTPUT, new(() => {
				if(stack.Count > 0) output += stack.Pop().ToString($"{(settings.Delimeter?"N":"F")}{settings.Precision}");
                else if(b) new Error(ErrorType.EmptyStack, "Cannot output from an empty stack",settings,logger).Cause();
			})},
			
			{ _STR_DUMP, new(() => {
				if(stack.Count > 0) while(stack.Count > 0) output += (char)stack.Pop();
                else if(b) new Error(ErrorType.EmptyStack, "Cannot dump an empty stack as string",settings,logger).Cause();
			})},
			
			{ _NUM_DUMP, new(() => {
				if(stack.Count > 0) while (stack.Count > 0) output += stack.Pop().ToString($"{(settings.Delimeter?"N":"F")}{settings.Precision}");
                else if(b) new Error(ErrorType.EmptyStack, "Cannot dump an empty stack as number",settings,logger).Cause();
			})},
			
			{ _STR_INPUT, new(async () => await IP_str(b)) },
			{ _NUM_INPUT, new(async () => await IP_num(b)) },
			
			{ _HORIZONTAL_IF, new(() => {
			    if(stack.Count > 0) dir = stack.Peek() == 0 ? Dir.E : Dir.W;
                else dir = Dir.E;
			})},
			
			{ _VERTICAL_IF, new(() => {
				if(stack.Count > 0) dir = stack.Peek() == 0 ? Dir.N : Dir.S;
                else dir = Dir.N;
			})},
			
			{ _EQUAL_IF, new(() => {if (stack.Pop() != stack.Peek()) pos += dir; }) },
			
			{ _LOOP, new(() => MC_loop(b)) },
			{ _SKIP, new(() => { pos += dir; }) },
			{ _DUPLICATE, new(() => {
			    if(stack.Count > 0) stack.Push(stack.Peek());
                else stack.Push(1);
			})},
			{ _GET_STACK_SIZE, new(() => { stack.Push(stack.Count); }) },
			{ _POP, new(() => { stack.Pop(); }) },
			
			{ _STR_MODE, new(() => { strMode = true; strBuf = string.Empty; }) },
			{ _NUM_MODE, new(() => { numMode = true; }) },
			
			{ _RAND_DIR_8, new(() => { dir = (Dir)rand.Next(0,8); }) },
			{ _RAND_DIR_4, new(() => { dir = (Dir)rand.Next(0,4); }) },
			
			{ _INFINITY, new(() => { stack.Push(double.PositiveInfinity); }) },
			{ _PI, new(() => { stack.Push(Math.PI); }) },
			{ _ROUND, new(() => {
				if(stack.Count > 0) stack.Push(Math.Round(stack.Pop()));
                else if(b) new Error(ErrorType.EmptyStack, "Cannot round from an empty stack",settings,logger).Cause();
			})},
			{ _RADICAL, new(() => MATH_radical(b)) },
			
			{ _SIN, new(() => {
			    if(stack.Count > 0) stack.Push(Math.Sin(stack.Pop()));
                else if(b) new Error(ErrorType.EmptyStack, "Cannot get the sinus from an empty stack",settings,logger).Cause();
			})},
			{ _COS, new(() => {
				if(stack.Count > 0) stack.Push(Math.Cos(stack.Pop()));
                else if(b) new Error(ErrorType.EmptyStack, "Cannot get the cosinus from an empty stack",settings,logger).Cause();
			})},
			{ _TAN, new(() => {
				if(stack.Count > 0) stack.Push(Math.Tan(stack.Pop()));
                else if(b) new Error(ErrorType.EmptyStack, "Cannot get the tangent from an empty stack",settings,logger).Cause();
			})},
			{ _CTG, new(() => MATH_ctg(b)) }
		};
    }
    
    public void ChangeSettings(Settings settings) {
        this.settings = settings;
        this.rand = new(settings.Seed);
    }

    void performInitChecks(string s) {
        if(!settings.PerformInitialChecks) return;
        if(s == string.Empty) new InitError(InitErrorType.EmptySource, "The provided source is empty hence no code execution can be performed").Cause();
        if(!s.Contains(_EXIT)) new InitError(InitErrorType.LackOfEnd, $"The provided source contains no exit command {_EXIT} therefore it will result into an {(settings.WrapAround ? "infinite loop" : "out of bounds error")}.").Cause();
        if(s.Count(x => x == _STR_MODE) % 2 != 0) new InitError(InitErrorType.UnevenBrackets, $"The provided source contains an uneven amount of string brackets [{_STR_MODE}]").Cause();
        if(s.Count(x => x == _NUM_MODE) % 2 != 0) new InitError(InitErrorType.UnevenBrackets, $"The provided source contains an uneven amount of number brackets [{_NUM_MODE}]").Cause();
    }

    char[,] strToBoard(string code) {
        string[] split = code.Split('\n');
        char[,] board = new char[split.Max(x=>x.Length), split.Length];
        
        for(int y=0; y < board.GetLength(1); y++)
            for(int x=0; x < split[y].Length; x++) 
                board[x,y] = split[y][x];
            
        return board;
    }
    
    void resetVars() {
        stack = new();
        output = string.Empty;
        pos = new(0);
        dir = Dir.E;
        c = board[pos.x,pos.y];
        strMode = numMode = false;
        numBuf = strBuf = string.Empty;
        toLoop = 0;
        steps = 0;

        sw.Restart();
    }
    
    public Result Reset(string code, bool PIC) {
        if(PIC && settings.PerformInitialChecks) performInitChecks(code);
        board = strToBoard(code);
        resetVars();
        
        Result res = new(board,0,output,new(),new(),dir,steps,true);
        if(settings.EnableLogging) logger.Log($"Resetted to {code} {(PIC?"while":"without")} performing initial checks\nResult: {res.GetStr(settings)}");
        return res;
    }

    async public Task<Result> Interpret(string code) {
        if(settings.PerformInitialChecks) performInitChecks(code);
        board = strToBoard(code);
        resetVars();

        while(c != _EXIT) await Step(false);

        Result res = new(board, sw.ElapsedMilliseconds,output,stack,pos,dir,steps,true);
        if(settings.EnableLogging) logger.Log($"Interpretted {code}\nResult: {res.GetStr(settings)}");
        return res;
    }
    
    async public Task<Result> Step(bool log) {
        bool b = false;
        c = board[pos.x,pos.y];
        sw.Start();

        if(numMode) {
            if(c == _NUM_MODE) {
                if(numBuf != string.Empty) {
                    stack.Push(float.Parse(numBuf));
                    numBuf = string.Empty;
                }
                numMode = false;
            } else if(char.IsDigit(c)) numBuf += c;
            else if(!DECIMALS.Contains(c)) {
                if(c == '-' || c == '.') numBuf += c;
                else new Error(ErrorType.InvalidNumber, "Cannot read such number",settings,logger);
            }
        } else if(strMode) {
            if(c == _STR_MODE) {
                strMode = false;
                for(int i=strBuf.Length-1; i >= 0; i--) stack.Push((int)strBuf[i]);
            } else strBuf += c;
        } else if(char.IsDigit(c)) stack.Push(c - '0');
        else {
            int t = 1;
            if(toLoop > 0) { 
                t = toLoop;
                toLoop = 0;
            } for(int i=0; i < t; i++) {
				if(c == _EXIT) b = true;
				else if(CMDS.Keys.Contains(c)) await CMDS[c].ExecFunc();
			}
        } if(c != _EXIT) pos += dir;

        if(settings.WrapAround) {
            if(pos.x > board.GetLength(0)-1) pos.x = 0;
            else if(pos.x < 0) pos.x = board.GetLength(0) - 1;

            if(pos.y > board.GetLength(1)-1) pos.y = 0;
            else if(pos.y < 0) pos.y = board.GetLength(1) - 1;
        } else if(!pos.IsWithin(board)) {
            resetFunc(null,null);
            new Error(ErrorType.InvalidPtrPos, $"The pointer left it's bounds\nPointer: {pos.TS}\nBounds: ({board.GetLength(0)},{board.GetLength(1)})",settings,logger).Cause();
        }

        steps++;
        sw.Stop();
        
        Result res = new(board, sw.ElapsedMilliseconds,output,stack,pos,dir,steps,b);
        if(log && settings.EnableLogging) logger.Log($"Stepped\nResult: {res.GetStr(settings)}");
        return res;
    }
	
	void OP_add(bool b) {
		if(stack.Count == 1) stack.Push(stack.Pop() + 1);
        else if(stack.Count > 1) stack.Push(stack.Pop() + stack.Pop());
        else if(b) new Error(ErrorType.EmptyStack, "Cannot perform addition on an empty stack",settings,logger).Cause();
	}
	
	void OP_sub(bool b) {
        if(stack.Count == 1) stack.Push(stack.Pop() - 1);
        else if(stack.Count > 1) {
            double x = stack.Pop(), y = stack.Pop();
                stack.Push(y - x);
        } else if(b) new Error(ErrorType.EmptyStack, "Cannot perform substraction on an empty stack",settings,logger).Cause();
	}
	
	void OP_mlt(bool b) {
		if(stack.Count == 1) stack.Push(stack.Pop() * 2);
        else if(stack.Count > 1) stack.Push(stack.Pop() * stack.Pop());
        else if(b) new Error(ErrorType.EmptyStack, "Cannot perform multiplication on an empty stack",settings,logger).Cause();
	}
	
	void OP_div(bool b) {
        if(stack.Count == 1) stack.Push(stack.Pop() / 2);
        else if(stack.Count > 1){
            double x = stack.Pop(), y = stack.Pop();
            if(x != 0) stack.Push(y / x);
            else new Error(ErrorType.DivisionByZero, $"Cannot divide {y} by zero",settings,logger).Cause();
        } else if(b) new Error(ErrorType.EmptyStack, "Cannot perform division on an empty stack",settings,logger).Cause();
	}
	
	void OP_mod(bool b) {
        if(stack.Count == 1) stack.Push(stack.Pop() % 1);
        else if(stack.Count > 1) {
            double x = stack.Pop(), y = stack.Pop();
            if(x != 0) stack.Push(y % x);
            else if(b) new Error(ErrorType.DivisionByZero, $"Cannot modulo {y} by zero",settings,logger).Cause();
        } else if(b) new Error(ErrorType.EmptyStack, "Cannot perform modulo on an empty stack",settings,logger).Cause();
	}
	
	void OP_pow(bool b) {
	    if(stack.Count == 1) { }
        else if(stack.Count > 1) {
            double x = stack.Pop(), y = stack.Pop();
            stack.Push(Math.Pow(y, x));
        } else if(b) new Error(ErrorType.EmptyStack, "Cannot use powers on an empty stack",settings,logger).Cause();
	}
	
	void MC_get(bool b) {
	    if(stack.Count > 1) {
            int _ = (int)Math.Round(stack.Pop());
            Pos p = new Pos(_,(int)Math.Round(stack.Pop()));
            if(p.IsWithin(board)) stack.Push(board[p.x, p.y]);
            else if(b) new Error(ErrorType.InvalidRange, $"{p.TS} is not within the boundaries of the board: ({board.GetLength(0)}, {board.GetLength(1)})",settings,logger).Cause();
        } else if(b) new Error(ErrorType.EmptyStack, "The stack must have at least two items (x,y) to use get",settings,logger).Cause();
	}
	
	void MC_set(bool b) {
        if(stack.Count > 2) {
            int chr = (int)Math.Round(stack.Pop());
            int _ = (int)Math.Round(stack.Pop());
            Pos p = new Pos(_,(int)Math.Round(stack.Pop()));
            if(p.IsWithin(board)) board[p.x,p.y] = (char)chr;
            else if(b) new Error(ErrorType.InvalidRange, $"{p.TS} is not within the boundaries of the board: ({board.GetLength(0)}, {board.GetLength(1)})",settings,logger).Cause();
        } else if(b) new Error(ErrorType.EmptyStack, "The stack must have at least three items (x,y) and a character to use set",settings,logger).Cause();
	}
	
	void MC_loop(bool b) {
        if(stack.Count > 0) {
            double x = stack.Pop();
            if(x > 0) toLoop = (int)Math.Round(x);
            else if(b) new Error(ErrorType.InvalidRange, "Cannot loop from a negative number",settings,logger).Cause();
        } else if(b) new Error(ErrorType.EmptyStack, "Cannot loop from an empty stack",settings,logger).Cause();
	}
	
	async Task IP_str(bool b) {
		sw.Stop();
        string s = await contentPage.DisplayPromptAsync("String Input", "Enter a string:", "OK", null, placeholder: "Some string...", keyboard: Keyboard.Plain);
        sw.Start();
        if(string.IsNullOrEmpty(s) && b) new Error(ErrorType.InvalidStringInput, "You cannot not input anything (Expected a string)",settings,logger).Cause();
        for(int j=s.Length-1; j >= 0; j--) stack.Push(s[j]);
	}
	
	async Task IP_num(bool b) {
		sw.Stop();
        string d1 = await contentPage.DisplayPromptAsync("Number Input", "Enter a number:", "OK", null, placeholder: "-134.67", keyboard: Keyboard.Numeric);
        sw.Start();
        if(string.IsNullOrEmpty(d1) && b) new Error(ErrorType.InvalidNumberInput, "You cannot not input anything (Expected a number)",settings,logger).Cause();

        if(double.TryParse(d1, out double d2)) stack.Push(d2);
        else stack.Push(0);
	}
	
	void MATH_radical(bool b) {
        if(stack.Count > 0) {
            double n = stack.Pop();
            if(n >= 0) stack.Push(Math.Sqrt(n));
            else if(b) new Error(ErrorType.InvalidRange, "Cannot get the radial of a negative number",settings,logger).Cause();
        } else if(b) new Error(ErrorType.EmptyStack, "Cannot get the radical in an empty stack",settings,logger).Cause();
	}
	
	void MATH_ctg(bool b) {
        if(stack.Count > 0) {
            double x = stack.Pop(), y = Math.Tan(x);
            if(x != 0) stack.Push(1 / y);
            else new Error(ErrorType.DivisionByZero, $"The tangent of {x} is zero.",settings,logger).Cause();
        } else if(b) new Error(ErrorType.EmptyStack, "Cannot get the cotanget from an empty stack",settings,logger).Cause();
	}
}