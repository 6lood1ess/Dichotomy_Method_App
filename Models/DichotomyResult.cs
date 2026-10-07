namespace DichotomyMethodApp.Models {
  // Итог работы метода дихотомии
  public class DichotomyResult {
    public double Root { get; set; }
    public double FunctionValueAtRoot { get; set; }
    public int IterationsCount { get; set; }
    public List<DichotomyStep> Steps { get; set; } = new List<DichotomyStep>();
  }
}
