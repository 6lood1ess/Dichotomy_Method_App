using DichotomyMethodApp.Models;

namespace DichotomyMethodApp.Views {
  // Абстракция главного окна. Presenter работает только с этим интерфейсом
  public interface IMainView {
    string FunctionExpression { get; }
    string AText { get; }
    string BText { get; }
    string EpsilonText { get; }

    void ShowSteps(IEnumerable<DichotomyStep> steps);
    void ShowRoot(string rootText);
    void ClearSteps();
    void ClearRoot();

    void BuildChart(string expression, double a, double b, double? root);
 
    void ShowError(string message);
    void ShowInfo(string message);

    event EventHandler CalculateRequested;
    event EventHandler ClearRequested;
    event EventHandler BuildChartRequested;
  }
}
