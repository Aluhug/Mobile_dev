using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace exam
{
    public class GameLogic
    {
        public List<Point> Pattern { get; private set; }
        private int gridSize;
        private HashSet<Point> userSelection;
        private Random rnd;

        public GameLogic(int size)
        {
            gridSize = size;
            Pattern = new List<Point>();
            userSelection = new HashSet<Point>();
            rnd = new Random();
        }

        public void GenerateNewPattern(int patternSize)
        {
            Pattern.Clear();
            userSelection.Clear();
            int count = Math.Min(patternSize, gridSize * gridSize);
            while (Pattern.Count < count)
            {
                Point p = new Point(rnd.Next(gridSize), rnd.Next(gridSize));
                if (!Pattern.Contains(p)) Pattern.Add(p);
            }
        }

        public bool CheckPosition(Point pos)
        {
            userSelection.Add(pos);
            return Pattern.Contains(pos);
        }

        public bool IsLevelCompleted()
        {
            return userSelection.SetEquals(Pattern);
        }
    }
}
