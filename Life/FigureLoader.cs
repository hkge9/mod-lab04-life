using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Life
{
    public class FigureLoader
    {
        private string figuresDirectory = Path.Combine(Directory.GetCurrentDirectory(), "figures");

        
        public Dictionary<string, bool[,]> LoadFigures()
        {
            var figures = new Dictionary<string, bool[,]>();

            var files = Directory.GetFiles(figuresDirectory, "*.txt");
            foreach (var file in files)
            {
                string figureName = Path.GetFileNameWithoutExtension(file);
                var figure = LoadFigureFromFile(file);
                figures.Add(figureName, figure);
            }

            return figures;
        }

        private bool[,] LoadFigureFromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            int width = lines.Max(l => l.Length);
            int height = lines.Length;

            bool[,] figure = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    figure[x, y] = lines[y][x] == '*';
                }
            }

            return figure;
        }
    }

}
