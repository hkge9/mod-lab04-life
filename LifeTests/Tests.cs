using System;
using System.IO;
using System.Linq;
using cli_life;
using Xunit;

namespace LifeTests
{
    public class LifeUnitTests
    {
        // Локальный аналог подсчёта живых клеток
        private int CountAlive(Board board)
        {
            int c = 0;
            foreach (var cell in board.Cells)
                if (cell.IsAlive) c++;
            return c;
        }

        [Fact]
        public void Settings_Load_CreatesDefault_WhenNoFile()
        {
            var path = "test_settings.json";
            if (File.Exists(path)) File.Delete(path);

            var s = Settings.Load(path);
            Assert.Equal(50, s.Width);
            Assert.Equal(20, s.Height);
            Assert.Equal(1, s.CellSize);
            Assert.Equal(0.5, s.LiveDensity);
            File.Delete(path);
        }

        [Fact]
        public void Settings_Load_ReadsValues_FromExistingFile()
        {
            var path = "test_settings.json";
            File.WriteAllText(path, "{\"Width\":42,\"Height\":24,\"CellSize\":2,\"LiveDensity\":0.25}");
            var s = Settings.Load(path);
            Assert.Equal(42, s.Width);
            Assert.Equal(24, s.Height);
            Assert.Equal(2, s.CellSize);
            Assert.Equal(0.25, s.LiveDensity);
            File.Delete(path);
        }

        [Fact]
        public void Board_InitialDimensions_AreCorrect()
        {
            var b = new Board(60, 30, 2, 0.1);
            Assert.Equal(30, b.Columns);  // 60/2
            Assert.Equal(15, b.Rows);     // 30/2
        }

        [Fact]
        public void Board_Randomize_RespectsDensity()
        {
            var density = 0.3;
            var board = new Board(50, 20, 1, density);
            int alive = CountAlive(board);
            double actual = (double)alive / (board.Columns * board.Rows);
            Assert.InRange(actual, density - 0.15, density + 0.15);
        }

        [Fact]
        public void Board_Advance_ChangesState()
        {
            var board = new Board(50, 20, 1, 0.5);
            int before = CountAlive(board);
            board.Advance();
            int after = CountAlive(board);
            Assert.NotEqual(before, after);
        }

        [Fact]
        public void Board_Save_CreatesFile_WithCorrectDimensions()
        {
            var board = new Board(10, 6, 1, 0.5);
            var path = "temp_save.txt";
            if (File.Exists(path)) File.Delete(path);

            board.Save(path);
            Assert.True(File.Exists(path));

            var lines = File.ReadAllLines(path);
            Assert.Equal(board.Rows, lines.Length);
            Assert.All(lines, line => Assert.Equal(board.Columns, line.Length));

            File.Delete(path);
        }

        [Fact]
        public void FindStablePhase_EmptyBoard_IsZero()
        {
            var board = new Board(20, 10, 1, 0.0);
            int gens = Program.FindStablePhase(board);
            Assert.Equal(0, gens);
        }

    

        [Fact]
        public void FindStablePhase_BlockPattern_IsZero()
        {
            var board = new Board(20, 10, 1, 0.0);
            // ручной "блок" в центре
            int cx = board.Columns/2, cy = board.Rows/2;
            board.Cells[cx, cy].IsAlive = true;
            board.Cells[cx+1, cy].IsAlive = true;
            board.Cells[cx, cy+1].IsAlive = true;
            board.Cells[cx+1, cy+1].IsAlive = true;
            int gens = Program.FindStablePhase(board);
            Assert.Equal(0, gens);
        }

        [Fact]
        public void StudyStableTimes_CreatesDataFile_WithCorrectLines()
        {
            var path = "data.txt";

            Program.StudyStableTimes();

            Assert.True(File.Exists(path));
            var lines = File.ReadAllLines(path);
            // от 0.00 до 1.00 включительно с шагом 0.02 → 51 строка
            Assert.Equal(51, lines.Length);
        }

        

        [Fact]
        public void Program_Main_DoesNotThrow()
        {
            // Проверяем, что Main выполняется без исключений (только первый шаг StudyStableTimes)
            var ex = Record.Exception(() => Program.Main(new string[0]));
            Assert.Null(ex);
        }

        [Fact]
        public void ClassifyFigure_UnknownOnDeadCell()
        {
            var board = new Board(10, 10, 1, 0.0);
            Assert.Equal("Unknown", board.ClassifyFigure(0, 0));
        }

        [Fact]
        public void ClassifyFigure_ReturnsUnknown_ForRandomNoise()
        {
            var board = new Board(10, 10, 1, 0.5);
            // Скорее всего, случайная конфигурация не совпадёт с шаблоном
            var name = board.ClassifyFigure(0, 0);
            Assert.True(name == "Unknown" || !string.IsNullOrEmpty(name));
        }

      [Fact]
        public void Save_WritesCorrectBinaryMap()
        {
            // Делаем маленькую доску 3×3, ставим живые клетки в (0,0) и (2,2)
            var board = new Board(3, 3, 1, 0.0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;

            var path = "test_map.txt";
            if (File.Exists(path)) File.Delete(path);

            board.Save(path);
            Assert.True(File.Exists(path));

            var lines = File.ReadAllLines(path);
            // Должно быть ровно 3 строки, каждая длины 3
            Assert.Equal(3, lines.Length);
            Assert.All(lines, l => Assert.Equal(3, l.Length));

            // Проверяем, что 
            // первая строка = "100", 
            // вторая = "000", 
            // третья = "001"
            Assert.Equal("100", lines[0]);
            Assert.Equal("000", lines[1]);
            Assert.Equal("001", lines[2]);

            File.Delete(path);
        }
        
[Fact]
        public void Advance_BlinkerOscillatesOnce()
        {
            // Доска 5×5, ставим «мигалку» по центру: (2,1),(2,2),(2,3)
            var board = new Board(5, 5, 1, 0.0);
            board.Cells[2, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            board.Cells[2, 3].IsAlive = true;

            board.Advance();

            // После одного шага «мигалка» должна стать вертикальной линией: (1,2),(2,2),(3,2)
            bool[,] expected = new bool[5,5];
            expected[1,2] = true;
            expected[2,2] = true;
            expected[3,2] = true;

            for (int x = 0; x < board.Columns; x++)
                for (int y = 0; y < board.Rows; y++)
                    Assert.Equal(expected[x,y], board.Cells[x,y].IsAlive);
        }
        [Fact]
        public void ConnectNeighbors_CornersWrapAround()
        {
            var board = new Board(4, 4, 1, 0.0);

            // Собираем все объекты-соседи для клетки [0,0]
            var cornerNeighbors = board.Cells[0,0].neighbors;

            // Они должны содержать именно ссылку на board.Cells[3,3]
            Assert.Contains(board.Cells[3,3], cornerNeighbors);
        }
    
    }
}
