namespace DichotomyMethodApp.Models {
  // Входные параметры задачи поиска корня
  public class DichotomyParameters {
    public FunctionDefinition Function { get; }
    public double A { get; }
    public double B { get; }
    public double Epsilon { get; }

    public DichotomyParameters(FunctionDefinition function, double a, double b, double epsilon) {
      Function = function;
      A = a;
      B = b;
      Epsilon = epsilon;
    }
  }
}
