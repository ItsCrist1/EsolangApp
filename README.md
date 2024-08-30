# EsolangApp
An application made with .NET MAUI for an custom esolang interpreter written in C# by me.

# Features
- Step By Step Debugging
- Lots of settings and configuration
- Comprehensive and customizable logging

# Interpreter
The interpreter is an esolang inspired by [Befunge](https://en.m.wikipedia.org/wiki/Befunge) with all sort of arithmetical and utility features. The interpreter works with a floating point number array and the pointer can move in all cardinal and intercardinal directions, meaning you can move diagonally.

# Instructions
∆ - Exit program immediately.

## Directions
W - Set movement direction to North.
S - Set movement direction to South.
D - Set movement direction to East.
A - Set movement direction to West.

E - Set movement direction to North East.
Q - Set movement direction to North West.
Z - Set movement direction to South West.
C - Set movement direction to South East.

## String Mode
" - You can use this to push a string onto the stack (e.g. "Hello world" => 72; 101; 108; 108; 111; 32; 87; 111;).

## Number Mode
' - You can use this to push any kind of number (double) onto the stack (e.g. '-232.4' => -232.4).
Tip: You can also use _ to delimiter decimals (no practical use but makes it easier to read).

## Operators
\+ - Gets the last two elements from the stack and pushes their sum (If only one element is present then the last element will get incremented by one).

\- - Gets the last two elements from the stack and pushes their substraction (If only one element is present then the last element will get decremented by one).

\* - Gets the last two elements from the stack and pushes their multiplication (If only one element is present then the last element will get doubled).

/ - Gets the last two elements from the stack and pushes their division (If only one element is present then the last element will get halves by one).

% - Gets the last two elements from the stack and pushes their modulo (If only one element is present then the last element will be moduloed by one).

^ - Gets the last two elements from the stack and pushes the first one at the power of the second one (If only one element is present then the last element will be squared).

## Arithmetics
π - Pushes pi to the stack.
∞ - Pushes infinity to the stack.
√ - Pushes the radical of the last number from the stack (original number gets removed).
& - Rounds the last number from the stack.

## If Statements
_ - Changes the direcrion to East if the stack is empty or if the last number from the stack is 0, the direction gets set to West otherwise.
| - Changes the direcrion to North if the stack is empty or if the last number from the stack is 0, the direction gets set to South otherwise.
; - If the last number isn't equal to the second to last one then the pointer moves an extra cell by the direction it already had.

## Input
{ - Prompts the user for a string that will get pushed to the stack.
} - Prompts the user for a number that will get pushed to the stack.

## Output
( - Outputs a character.
) - Outputs a number.
[ - Outputs the entire stack as a string.
] - Outputs the entire stack as numbers.

## Miscellaneous
@ - Gets the last number from the stack, rounds it, and does the next instruction that number amount of times (Basically a loop).
\# - Skips the next instruction.
= - Duplicates the last number from the stack.
? - Goes in a random cardinal direction.
! - Goes in a random cardinal or intercardinal direction.
$ - Removes the last number from the stack.
§ - Pushes the stack size as a number to the stack.

## File Operations
, - Gets the last two numbers, rounds them, and uses them as coordinates to look at the program board and pushes the character it founds at the x,y position to the stack. (First the program takes a number that will be **the x position** then a number that will be **the y position**).

. - Gets the last three numbers, rounds them, and uses them as coordinates to set the program's board x,y cell to the third number (First the program takes a number that will be **the character** then a number that will be **the x position** then a number that will be **the y position**).


# That's pretty much it!
