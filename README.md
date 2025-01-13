# EsolangApp
An application made with [.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui) for a custom esolang interpreter written in C#.

# Compatibility
This application only works on **Android** for now.

# Features
- Step By Step Debugging
- Lots of settings and configuration
- Comprehensive and customizable logging

# Installing
Install the APK from the latest release.

# Building From Source
Works best on **Windows** but should work on **Linux** with proper .NET MAUI compatibility.

## Requirements
-Git (or the GitHub CLI)
-.NET SDK with MAUI support

## Building

To build this from source you can run this in either **PowerShell** or **Bash**:

```
git clone https://www.github.com/ItsCrist1/EsolangApp.git
cd EsolangApp
dotnet build
```

# Interpreter
The interpreter is an esolang inspired by [Befunge](https://en.m.wikipedia.org/wiki/Befunge) with all sorts of arithmetical and utility features. The interpreter uses a floating-point stack.

# Instructions

## Control Flow
∆ - Exit program immediately.

## Directions
W - Set movement direction to North. <br/>
S - Set movement direction to South. <br/>
D - Set movement direction to East. <br/>
A - Set movement direction to West. <br/>

E - Set movement direction to North East.<br/>
Q - Set movement direction to North West.<br/>
Z - Set movement direction to South West.<br/>
C - Set movement direction to South East.<br/>

## String Mode
" - You can use this to push a string onto the stack (e.g. "Hello world" => 72; 101; 108; 108; 111; 32; 87; 111;).

## Number Mode
' - You can use this to push any kind of number (double) onto the stack (e.g. '-232.4' => -232.4).
Tip: You can also use _ to delimiter decimals (no practical use but makes it easier to read).

## Operators
\+ - Gets the last two elements from the stack and pushes their sum (If only one element is present then the last element will get incremented by one).

\- - Gets the last two elements from the stack and pushes their substraction (If only one element is present then the last element will get decremented by one).

\* - Gets the last two elements from the stack and pushes their multiplication (If only one element is present then the last element will get doubled).

/ - Gets the last two elements from the stack and pushes their division (If only one element is present then the last element is halved).

% - Gets the last two elements from the stack and pushes their modulo (If only one element is present then the last element will be moduloed by one).

^ - Gets the last two elements from the stack and pushes the first one at the power of the second one (If only one element is present then the last element will be squared).

## Arithmetics
π - Pushes pi to the stack. <br/>
∞ - Pushes infinity to the stack. <br/>
√ - Pushes the radical of the last number from the stack (original number gets removed). <br/>
& - Rounds the last number from the stack. <br/>

## If Statements
_ - Changes the direction to East if the stack is empty or if the last number from the stack is 0, the direction gets set to West otherwise. <br/>
| - Changes the direcrion to North if the stack is empty or if the last number from the stack is 0, the direction gets set to South otherwise. <br/>
; - If the last number isn't equal to the second to last one then the pointer moves an extra cell by the direction it already had. <br/>

## Input
{ - Prompts the user for a string that will get pushed to the stack. <br/>
} - Prompts the user for a number that will get pushed to the stack. <br/>

## Output
( - Outputs a character. <br/>
) - Outputs a number. <br/>
[ - Outputs the entire stack as a string. <br/>
] - Outputs the entire stack as numbers. <br/>

## Miscellaneous
@ - Gets the last number from the stack, rounds it, and does the next instruction that number amount of times (Basically a loop). <br/>
\# - Skips the next instruction. <br/>
= - Duplicates the last number from the stack. <br/>
? - Goes in a random cardinal direction. <br/>
! - Goes in a random cardinal or intercardinal direction. <br/>
$ - Removes the last number from the stack. <br/>
§ - Pushes the stack size as a number to the stack. <br/>
¥ - Swaps the last two items of the stack <br/>

## Clipboard
c - Copies the last stack item <br/>
x - Pops the last stack item and copies it <br/>
v - Pushes the copied value to stack (zero by default) <br/>

## Code Operations
, - Gets the last two numbers, rounds them, and uses them as coordinates to look at the program board and pushes the character it founds at the x,y position to the stack. (First the program takes a number that will be **the x position** then a number that will be **the y position**).

. - Gets the last three numbers, rounds them, and uses them as coordinates to set the program's board x,y cell to the third number (First the program takes a number that will be **the character** then a number that will be **the x position** then a number that will be **the y position**).

# Examples

## Hello World
```"Hello World!" [ ∆```

## Calculate circle circumference
```} 2π ** )∆```

## Calculate circle area
```} 2^ π* )∆```

## Convert degrees to radians
```} π '180' /* ) ∆```

## Radical
```} √ ) ∆```

## Factorial
```
}0;SD=1-S
∆)1AW   _$§1-@*)∆
```

## Fibonacci Sequence
```
}11Dx=vc+v¥€1-S
   W€         _ ] ∆
```

# Attributions
This project uses [The JetBrains Mono](https://github.com/JetBrains/JetBrainsMono/) font that is under [The SIL Open Font License 1.1](https://github.com/JetBrains/JetBrainsMono/blob/master/OFL.txt)

# Contact
You can contact me at cristi9270@gmail.com or cristi123612 on discord.

# License
This project is licensed under [The MIT License](https://github.com/ItsCrist1/EsolangApp/blob/main/LICENSE.txt).
