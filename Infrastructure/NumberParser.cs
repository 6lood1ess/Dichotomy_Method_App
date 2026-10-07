using System.Globalization;

namespace DichotomyMethodApp.Infrastructure {
  // Парсинг чисел с поддержкой точки и запятой как разделителя
  public static class NumberParser {
    public static bool TryParse(string input, out double value) {
      value = 0;

      if (string.IsNullOrWhiteSpace(input)) return false;

      string normalized = input.Trim().Replace(',', '.');

      return double.TryParse(
        normalized,
        NumberStyles.Float | NumberStyles.AllowThousands,
        CultureInfo.InvariantCulture,
        out value);
    }
  }
}
