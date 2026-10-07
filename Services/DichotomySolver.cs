using DichotomyMethodApp.Models;

namespace DichotomyMethodApp.Services {
  // Реализация метода половинного деления (дихотомии)
  public class DichotomySolver : IDichotomySolver {
    private const int MaxIterations = 10_000;
    private readonly IFunctionEvaluator _evaluator;

    public DichotomySolver(IFunctionEvaluator evaluator) {
      _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public DichotomyResult Solve(DichotomyParameters parameters) {
      if (parameters == null) {
        throw new ArgumentNullException(nameof(parameters));
      }

      if (parameters.A >= parameters.B) {
        throw new ArgumentException("Левая граница должна быть меньше правой");
      }

      if (parameters.Epsilon <= 0) {
        throw new ArgumentException("Точность должна быть положительной");
      }

      CheckContinuity(parameters.Function.Expression, parameters.A, parameters.B);

      string expr = parameters.Function.Expression;

      double left = parameters.A;
      double right = parameters.B;

      double fLeft = _evaluator.Evaluate(expr, left);
      double fRight = _evaluator.Evaluate(expr, right);

      if (Math.Abs(fLeft) < parameters.Epsilon) {
        return new DichotomyResult { Root = left, };
      }

      if (Math.Abs(fRight) < parameters.Epsilon) {
        return new DichotomyResult { Root = right, };
      }

      if (fLeft * fRight > 0) {
        throw DiagnoseNoRoot(expr, parameters.A, parameters.B);
      }

      var result = new DichotomyResult();
      int n = 1;
      double c = left;

      while ((right - left) > parameters.Epsilon && n <= MaxIterations) {
        c = (left + right) / 2.0;
        double fC = _evaluator.Evaluate(expr, c);

        result.Steps.Add(new DichotomyStep {
          Number = n,
          A = left,
          B = right,
          C = c
        });

        if (Math.Abs(fC) < double.Epsilon) {
          left = right = c;
          break;
        }

        if (fLeft * fC < 0) {
          right = c;
          fRight = fC;

        } else {
          left = c;
          fLeft = fC;
        }

        n++;
      }

      if (n > MaxIterations) {
        throw new InvalidOperationException("Превышено максимальное число итераций");
      }

      result.Root = (left + right) / 2.0;
      result.FunctionValueAtRoot = _evaluator.Evaluate(expr, result.Root);
      result.IterationsCount = result.Steps.Count;

      return result;
    }

    private void CheckContinuity(string expression, double a, double b) {
      const int SampleCount = 200;
      double step = (b - a) / SampleCount;

      for (int sampleIndex = 0; sampleIndex <= SampleCount; ++sampleIndex) {
        double x = a + sampleIndex * step;
        double y;

        try {
          y = _evaluator.Evaluate(expression, x);
        } catch (Exception ex) {
          throw new InvalidOperationException(
            $"Функция не определена в точке x = {x:F6}: {ex.Message}", ex);
        }

        if (double.IsNaN(y) || double.IsInfinity(y)) {
          throw new InvalidOperationException(
            $"Функция не определена в точке x = {x:F6} (значение = {y}). " +
            "На отрезке [a, b], скорее всего, есть точка разрыва");
        }
      }
    }

    private InvalidOperationException DiagnoseNoRoot(string expression, double a, double b) {
      const int ScanCount = 1000;
      double step = (b - a) / ScanCount;

      double prevX = a;
      double prevY = _evaluator.Evaluate(expression, a);

      var signChanges = new List<(double From, double To)>();
      var zeros = new List<double>();

      for (int scanIndex = 1; scanIndex <= ScanCount; ++scanIndex) {
        double x = a + scanIndex * step;
        double y = _evaluator.Evaluate(expression, x);

        bool prevIsFinite = !double.IsNaN(prevY) && !double.IsInfinity(prevY);
        bool currIsFinite = !double.IsNaN(y) && !double.IsInfinity(y);

        if (prevIsFinite && currIsFinite) {
          bool prevIsZero = Math.Abs(prevY) < 1e-12;
          bool currIsZero = Math.Abs(y) < 1e-12;

          // Случай 1: точное попадание в ноль
          if (currIsZero) {
            zeros.Add(x);
          }

          // Случай 2: чистый интервал смены знака - обе точки ненулевые и разных знаков
          if (!prevIsZero && !currIsZero && prevY * y < 0) {
            signChanges.Add((prevX, x));
          }
        }

        prevX = x;
        prevY = y;
      }

      int trueRootsCount = signChanges.Count + zeros.Count;

      // Случай A: ни смен знака, ни нулей - корней нет
      if (trueRootsCount == 0) {
        return new InvalidOperationException(
          $"Корень не отделён: f(a)·f(b) = {_evaluator.Evaluate(expression, a) * _evaluator.Evaluate(expression, b):F6} > 0. " +
          $"За {ScanCount} проверок не найдено ни одной смены знака и ни одного нуля - " +
          $"корней на отрезке [{a:F4}, {b:F4}] нет");
      }

      // Случай B: ровно один корень
      if (trueRootsCount == 1) {
        if (zeros.Count == 1) {
          return new InvalidOperationException(
            $"Корень не отделён: f(a)·f(b) = {_evaluator.Evaluate(expression, a) * _evaluator.Evaluate(expression, b):F6} > 0, " +
            $"но функция обращается в ноль в точке x ≈ {zeros[0]:F4}. " +
            "Метод дихотомии не находит кратные корни (точки касания). " +
            "Попробуйте сузить отрезок так, чтобы знак функции менялся");
        }

        var (from, to) = signChanges[0];
        double approxRoot = (from + to) / 2.0;

        return new InvalidOperationException(
          $"Корень не отделён: f(a)·f(b) = {_evaluator.Evaluate(expression, a) * _evaluator.Evaluate(expression, b):F6} > 0, " +
          $"но внутри отрезка обнаружена смена знака в окрестности x ≈ {approxRoot:F4}. " +
          $"Сузьте отрезок до [{from:F4}, {to:F4}] и повторите");
      }

      // Случай C: несколько корней - собираем описание
      var parts = new List<string>();

      foreach (var (from, to) in signChanges) {
        parts.Add($"[{from:F4}, {to:F4}]");
      }

      foreach (double z in zeros) {
        parts.Add($"x ≈ {z:F4}");
      }

      return new InvalidOperationException(
        $"Корень не отделён: f(a)·f(b) = {_evaluator.Evaluate(expression, a) * _evaluator.Evaluate(expression, b):F6} > 0. " +
        $"На отрезке обнаружено {trueRootsCount} корней: {string.Join(", ", parts)}. " +
        "Сузьте отрезок до окрестности одного из них и повторите.");
    }
  }
}
