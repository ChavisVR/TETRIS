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

                // Mover la figura actual hacia abajo a intervalos regulares
                if (Frame % (FrameToMoveFigure - Level) == 0)
                {
                    CurrentFigureRow++;
                    Frame = 0;
                    Score++;
                }

                // Verificar colisiones y actualizar
                if (Colision(CurrentFigure))
                {
                    AgregarFiguraActual();
                    int lineas = ComprobarLIneasCompletas();

                    Score += ScorePerLines[lineas] * Level;

                    CurrentFigureCol = 0;
                    CurrentFigureRow = 0;

                    // Fin del juego
                    if (Colision(CurrentFigure))
                    {
                        File.AppendAllLines(ScoresFileName, new List<string>
                        {
                            $"[{DateTime.Now.ToString()}] {Environment.UserName} => {Score}"
                        });
                        var scoreAsString = Score.ToString();
                        scoreAsString += new string(' ', 7 - scoreAsString.Length);
                        Write("╔══════════════╗", 5, 5);
                        Write("║ Juego        ║", 6, 5);
                        Write("║    terminado!║", 7, 5);
                        Write("║              ║", 8, 5);
                        Write("╚══════════════╝", 9, 5);
                        Console.ReadKey();
                        PlayGame = false;

                        return;
                    }
                }

                DibujarBorde();
                DibInfo();
                DibujarTetrisCampo();
                DibujarFiguraActual();

                Thread.Sleep(40);
            }
        }

        // Obtener la siguiente figura aleatoria
        private static bool[,] ObtenerSiguienteFigura()
        {
            NextFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            return NextFigure;
        }

        // Actualizar el nivel según la puntuación alcanzada
        private static void UpdateLevel()
        {
            if (Score <= 0)
            {
                Level = 1;
            }

            if (Score >= 100 )
            {
                Level = 2;
            }
            else if (Score == 250)
            {
                Level = 3;
            }
            else if (Score == 400)
            {
                Level = 4;
            }
            else if (Score == 550)
            {
                Level = 5;
            }
            else if (Score == 700)
            {
                Level = 6;
            }
            else if (Score == 850)
            {
                Level = 9;
            }
            else if (Score == 1000)
            {
                Level = 8;
            }
            else if (Score == 1150)
            {
                Level = 9;
            }
            else if (Score == 1300)
            {
                Level = 10;
            }
        }

        // Rotar la figura actual en sentido horario
        private static void RotarFiguraActual()
        {
            var nuevaFigura = new bool[CurrentFigure.GetLength(1), CurrentFigure.GetLength(0)];
            for (int fila = 0; fila < CurrentFigure.GetLength(0); fila++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    nuevaFigura[col, CurrentFigure.GetLength(0) - fila - 1] = CurrentFigure[fila, col];
                }
            }
            if (!Colision(nuevaFigura))
            {
                CurrentFigure = nuevaFigura;
            }
        }

        // Comprobar si hay líneas completas en el campo de Tetris
        private static int ComprobarLIneasCompletas()
        {
            int lineas = 0;

            for (int fila = 0; fila < TetrisField.GetLength(0); fila++)
            {
                bool filaCompleta = true;
                for (int col = 0; col < TetrisField.GetLength(1); col++)
                {
                    if (!TetrisField[fila, col])
                    {
                        filaCompleta = false;
                        break;
                    }
                }

                if (filaCompleta)
                {
                    for (int filaAMover = fila; filaAMover >= 1; filaAMover--)
                    {
                        for (int col = 0; col < TetrisField.GetLength(1); col++)
                        {
                            TetrisField[filaAMover, col] = TetrisField[filaAMover - 1, col];
                        }
                    }

                    lineas++;
                }
            }
            return lineas;
        }

        // Agregar la figura actual al campo de Tetris
        private static void AgregarFiguraActual()
        {
            for (int fila = 0; fila < CurrentFigure.GetLength(0); fila++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[fila, col])
                    {
                        TetrisField[CurrentFigureRow + fila, CurrentFigureCol + col] = true;
                    }
                }
            }

            CurrentFigure = NextFigure;
            ObtenerSiguienteFigura();
        }

        // Comprobar colisiones para la figura actual
        static bool Colision(bool[,] figura)
        {
            if (CurrentFigureCol > TetrisCols - figura.GetLength(1))
            {
                return true;
            }

            if (CurrentFigureRow + figura.GetLength(0) == TetrisRows)
            {
                return true;
            }

            for (int fila = 0; fila < figura.GetLength(0); fila++)
            {
                for (int col = 0; col < figura.GetLength(1); col++)
                {
                    if (figura[fila, col] && TetrisField[CurrentFigureRow + fila + 1, CurrentFigureCol + col])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Dibujar la información del juego en la consola
        static void DibInfo()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
            }

            Write("Nivel:", 1, TetrisCols + 3);
            Write(Level.ToString(), 3, TetrisCols + 3);
            Write("Puntuacion:", 5, TetrisCols + 3);
            Write(Score.ToString(), 7, TetrisCols + 3);
            Write("Siguiente Figura:", 13, TetrisCols + 3);

            DibSigFigura();
        }

        // Dibujar el campo de Tetris en la consola
        static void DibujarTetrisCampo()
        {
            for (int fila = 0; fila < TetrisField.GetLength(0); fila++)
            {
                string linea = "";
                for (int col = 0; col < TetrisField.GetLength(1); col++)
                {
                    if (TetrisField[fila, col])
                    {
                        linea += $"{FigureSymbol}";
                    }
                    else
                    {
                        linea += " ";
                    }
                }
                Write(linea, fila + 1, 1);
            }
        }

        // Dibujar la figura actual en la consola
        static void DibujarFiguraActual()
        {
            for (int fila = 0; fila < CurrentFigure.GetLength(0); fila++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[fila, col])
                    {
                        Write($"{FigureSymbol}", fila + 1 + CurrentFigureRow, col + 1 + CurrentFigureCol);
                    }
                }
            }
        }

        // Dibujar la próxima figura en la consola
        static void DibSigFigura()
        {
            for (int fila = 0; fila < NextFigure.GetLength(0); fila++)
            {
                for (int col = 0; col < NextFigure.GetLength(1); col++)
                {
                    if (NextFigure[fila, col])
                    {
                        Write($"{FigureSymbol}", fila + 1 + NextFigureRow, col + 1 + NextFigureCol);
                    }
                }
            }
        }

        // Dibujar el borde del juego en la consola
        static void DibujarBorde()
        {
            Console.SetCursorPosition(0, 0);

            string primeraLinea = "╔";
            primeraLinea += new string('═', TetrisCols);
            primeraLinea += "╦";
            primeraLinea += new string('═', InfoCols);
            primeraLinea += "╗";

            string lineaMedia = "";
            for (int i = 0; i < TetrisRows; i++)
            {
                lineaMedia += "║";
                lineaMedia += new string(' ', TetrisCols) + "║" + new string(' ', InfoCols) + "║" + "\n";
            }

            string ultimaLinea = "╚";
            ultimaLinea += new string('═', TetrisCols);
            ultimaLinea += "╩";
            ultimaLinea += new string('═', InfoCols);
            ultimaLinea += "╝";

            string marcoBorde = primeraLinea + "\n" + lineaMedia + ultimaLinea;
            Console.Write(marcoBorde);
        }

    }
}
