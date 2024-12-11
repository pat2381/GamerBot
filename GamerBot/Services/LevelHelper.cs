using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GamerBot.Services
{
    public class LevelHelper
    {
        private readonly string _levelCurve;

        public LevelHelper(string levelCurve)
        {
            _levelCurve = levelCurve;
        }

        public int GetRequiredXPForLevel(int level)
        {
            // Beispielhafte Interpretation der LevelCurve:
            // Wenn Config z.B. "n*100" ist, dann required XP = level * 100
            // Man könnte auch einen Parser schreiben, aber hier halten wir es einfach.
            // Annahme: LevelCurve ist ein String im Format "n*100"
            // Dann machen wir einfach level * 100.

            // Für komplexere Formeln könntest du hier einen Parser einbauen.
            // Aktuell hartkodieren wir einfach einen Zusammenhang:

            if (_levelCurve.Contains("n*"))
            {
                var match = Regex.Match(_levelCurve, @"n\*(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int factor))
                {
                    return level * factor;
                }
            }

            // Fallback
            return level * 100;
        }
    }

}

