using DichotomyMethodApp.Infrastructure;

namespace DichotomyMethodApp.Services {
  public class FunctionEvaluator : IFunctionEvaluator {
    public double Evaluate(string expression, double x) {
      if (string.IsNullOrWhiteSpace(expression)) {
        throw new ArgumentException("Пустое выражение функции", nameof(expression));
      }

      try {
        var parser = new ExpressionParser(expression);
        return parser.Evaluate(x);

      } catch (Exception ex) {
        throw new FormatException($"Не удалось вычислить '{expression}': {ex.Message}", ex);
      }
    }
  }
}
