using DichotomyMethodApp.Infrastructure;
using DichotomyMethodApp.Models;
using DichotomyMethodApp.Services;
using DichotomyMethodApp.Validators;
using DichotomyMethodApp.Views;

namespace DichotomyMethodApp.Presenters {
  public class MainPresenter {
    private readonly IMainView _view;
    private readonly IInputValidator _validator;
    private readonly IDichotomySolver _solver;

    public MainPresenter(
      IMainView view,
      IInputValidator validator,
      IDichotomySolver solver) {

      _view = view ?? throw new ArgumentNullException(nameof(view));
      _validator = validator ?? throw new ArgumentNullException(nameof(validator));
      _solver = solver ?? throw new ArgumentNullException(nameof(solver));

      _view.CalculateRequested += OnCalculateRequested;
      _view.ClearRequested += OnClearRequested;
      _view.BuildChartRequested += OnBuildChartRequested;
    }

    private void OnCalculateRequested(object sender, EventArgs e) {
      var validation = _validator.Validate(
        _view.FunctionExpression,
        _view.AText,
        _view.BText,
        _view.EpsilonText);

      if (!validation.IsValid) {
        _view.ShowError(validation.ErrorMessage);
        return;
      }

      NumberParser.TryParse(_view.AText, out double a);
      NumberParser.TryParse(_view.BText, out double b);
      NumberParser.TryParse(_view.EpsilonText, out double eps);

      try {
        var parameters = new DichotomyParameters(
          new FunctionDefinition(_view.FunctionExpression), a, b, eps);

        var result = _solver.Solve(parameters);

        _view.ClearSteps();
        _view.ShowSteps(result.Steps);

        string rootText =
          $"x = {result.Root:F6}    f(x) = {result.FunctionValueAtRoot:E3}{Environment.NewLine}" +
          $"Итераций: {result.IterationsCount}";

        _view.ShowRoot(rootText);
        _view.BuildChart(_view.FunctionExpression, a, b, result.Root);

      } catch (Exception ex) {
        _view.ShowError(ex.Message);
      }
    }

    private void OnBuildChartRequested(object sender, EventArgs e) {
      var validation = _validator.Validate(
        _view.FunctionExpression,
        _view.AText,
        _view.BText,
        _view.EpsilonText);

      if (!validation.IsValid) {
        _view.ShowError(validation.ErrorMessage);
        return;
      }

      NumberParser.TryParse(_view.AText, out double a);
      NumberParser.TryParse(_view.BText, out double b);

      try {
        _view.BuildChart(_view.FunctionExpression, a, b, null);

      } catch (Exception ex) {
        _view.ShowError($"Ошибка построения графика: {ex.Message}");
      }
    }

    private void OnClearRequested(object sender, EventArgs e) {
      _view.ClearSteps();
      _view.ClearRoot();
      _view.BuildChart(_view.FunctionExpression, 0, 1, null);
    }
  }
}
