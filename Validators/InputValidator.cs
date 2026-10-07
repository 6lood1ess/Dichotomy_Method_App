using DichotomyMethodApp.Infrastructure;

namespace DichotomyMethodApp.Validators {
  public class InputValidator : IInputValidator {
    public ValidationResult Validate(string functionExpr, string aText, string bText, string eText) {
      if (string.IsNullOrWhiteSpace(functionExpr)) {
        return ValidationResult.Fail("Введите функцию f(x)");
      }

      try {
        var parser = new ExpressionParser(functionExpr);
        parser.Evaluate(1.0);

      } catch (Exception ex) {
        return ValidationResult.Fail($"Некорректная формула: {ex.Message}");
      }

      if (!NumberParser.TryParse(aText, out double a)) {
        return ValidationResult.Fail("Некорректное значение a");
      }

      if (!NumberParser.TryParse(bText, out double b)) {
        return ValidationResult.Fail("Некорректное значение b");
      }

      if (!NumberParser.TryParse(eText, out double e)) {
        return ValidationResult.Fail("Некорректное значение e");
      }

      if (a >= b) {
        return ValidationResult.Fail("Значение a должно быть меньше b");
      }

      if (e <= 0) {
        return ValidationResult.Fail("Точность e должна быть положительной");
      }

      return ValidationResult.Ok();
    }
  }
}
