Simple implementation of John Horton Conway's Game of Life. 

It is made in Godot with C# as the scripting language. 
It uses NewtonSoft JSON for saving to and loading from json. 

The menus aren't lookers at the moment, but at least they are functional. 

To use the program:
- Press space or p to pause/unpause
- Press F1 to generate a random initial situation using OpenSimplexNoise
- Press F2 to clear the screen
- Press F5 to quicksave
- Press F6 to quickload
- Press F9 to save to specific file
- Press F10 to load from specific file

- Click anywhere with left button to make that cell 'alive'
- Click anywhere with right button make that cell 'dead'


See below for some of the examples of what the program can generate:

Blinker puffer initial state:

![Blinker Puffer](https://github.com/kelgrim/CSharpGameOfLife/blob/master/example_output/blinker_puffer_1.png)

Blinker Puffer in action:

![Blinker Puffer 2](https://github.com/kelgrim/CSharpGameOfLife/blob/master/example_output/blinker_puffer_2.png)

glider guns initial:

![Glider gun 1](https://github.com/kelgrim/CSharpGameOfLife/blob/master/example_output/doublge_gliders1.png)

glider guns in action:

![Glider gun 2](https://github.com/kelgrim/CSharpGameOfLife/blob/master/example_output/double_gliders2.png)
