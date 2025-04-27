using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.IO;

namespace cli_life
{

    public class Settings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int CellSize { get; set; }
        public double LiveDensity { get; set; }

        public static Settings Load(string path)
        {
            if (!File.Exists(path))
            {
                // Создадим файл с настройками по умолчанию
                var defaultSettings = new Settings
                {
                    Width = 50,
                    Height = 20,
                    CellSize = 1,
                    LiveDensity = 0.5
                };
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(path, JsonSerializer.Serialize(defaultSettings, options));
                return defaultSettings;
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Settings>(json);
        }
    }
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }

        public void Save(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        if (Cells[x, y].IsAlive)
                        {
                            writer.Write('1');
                        }
                        else
                        {
                            writer.Write('0');
                        }


                    }

                    writer.WriteLine();
                }
            }
        }


        class Program
        {
            static Board board;
            static Settings settings;
            static private void Reset()
            {
                settings = Settings.Load("setting.json");

                board = new Board(
                    width: settings.Width,
                    height: settings.Height,
                    cellSize: settings.CellSize,
                    liveDensity: settings.LiveDensity);
            }
            static void Render()
            {
                for (int row = 0; row < board.Rows; row++)
                {
                    for (int col = 0; col < board.Columns; col++)
                    {
                        var cell = board.Cells[col, row];
                        if (cell.IsAlive)
                        {
                            Console.Write('*');
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                    Console.Write('\n');
                }
            }

            static private void LoadPattern()
            {
                Console.Write("Путь до файла: ");
                string filename = Console.ReadLine();
                string path = filename;

                if (!File.Exists(path))
                {
                    Console.WriteLine("Файл не найден.");
                    return;
                }

                settings = Settings.Load("settings.json");
                board = new Board(
                    width: settings.Width,
                    height: settings.Height,
                    cellSize: settings.CellSize,
                    liveDensity: 0
                );

                var lines = File.ReadAllLines(path);

                int offsetX = (board.Columns - lines.Max(l => l.Length)) / 2;
                int offsetY = (board.Rows - lines.Length) / 2;

                for (int y = 0; y < lines.Length; y++)
                {
                    string line = lines[y];
                    for (int x = 0; x < line.Length; x++)
                    {
                        if (line[x] == '*')
                        {
                            int posX = offsetX + x;
                            int posY = offsetY + y;

                            if (posX >= 0 && posX < board.Columns && posY >= 0 && posY < board.Rows)
                            {
                                board.Cells[posX, posY].IsAlive = true;
                            }
                        }
                    }
                }
            }


            static void Main(string[] args)
            {
                Reset();
                while (true)
                {
                    Console.Clear();
                    Render();
                    board.Advance();
                    Thread.Sleep(1000);

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.S)
                        {
                            Console.Write("Введите имя файла для сохранения: ");
                            string filename = Console.ReadLine();
                            board.Save(filename);
                            Console.WriteLine("Игра сохранена!");
                        }
                        if (key.Key == ConsoleKey.L)
                        {
                            LoadPattern();
                        }
                    }
                }
            }
        }
    }
}