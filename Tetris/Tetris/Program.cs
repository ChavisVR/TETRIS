using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Media;

namespace Tetris
{
    class Program
    {
        // Definición de variables estáticas
        static int TetrisRows = 21;
        static int TetrisCols = 10;
        static int InfoCols = 20;
        static int ConsoleRows = 1 + TetrisRows + 1;
        static int ConsoleCols = 1 + TetrisCols + 1 + InfoCols + 1;
        static List<bool[,]> TetrisFigures = new List<bool[,]>()
        {
            // Definición de las formas de las piezas de Tetris
            new bool [,] { {true, true, true, true } },
            new bool [,] { {true, true  }, {true, true  } },
            new bool [,] { {false, true, false}, {true, true ,true } },
            new bool [,] { {false, true, true}, {true, true, false} },
            new bool[,] { {true, true, false}, {false, true, true} },
            new bool[,] { {false, false, true}, {true, true, true} },
            new bool[,] { {true, false, false}, {true, true, true} }
        };
        static string ScoresFileName = "score";
        static int[] ScorePerLines = { 0, 40, 100, 300, 1200 };
        static int HighScore = 0;
        static int Score = 0;
        static int Frame = 0;
        static int Level = 1;
        static string FigureSymbol = "■";
        static int FrameToMoveFigure = 16;
        static bool[,] CurrentFigure = null;
        static int CurrentFigureRow = 0;
        static int CurrentFigureCol = 0;
        static bool[,] NextFigure = null;
        static int NextFigureRow = 14;
        static int NextFigureCol = TetrisCols + 3;
        static bool[,] TetrisField = new bool[TetrisRows, TetrisCols];
        static Random Random = new Random();
        static bool PauseMode = false;
        static bool PlayGame = true;
        static int SongLevel = 2;

        // Método principal
        static void Main(string[] args)
        {
            // Configuraciones de la consola
            Console.OutputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows;
            Console.BufferWidth = ConsoleCols;

            // Inicializar las figuras actual y próxima
            CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];

            while (true)
            {
                Frame++;
                UpdateLevel();

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();

                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }

                    // Manejar la entrada del teclado para movimiento y rotación
                    if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
                    {
                        if (CurrentFigureCol >= 1)
                        {
                            CurrentFigureCol--;
                        }
                    }

                    if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    {
                        if ((CurrentFigureCol < TetrisCols - CurrentFigure.GetLength(1)))
                        {
                            CurrentFigureCol++;
                        }
                    }

                    if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
                    {
                        RotarFiguraActual();
                    }

                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        Frame = 1;
                        Score += Level;
                        CurrentFigureRow++;
                    }
                }
