namespace DichotomyMethodApp.Models {
  // Данные одного шага метода половинного деления
  public class DichotomyStep {
    public int Number { get; set; }
    public double A { get; set; }
    public double B { get; set; }
    public double C { get; set; }
    public double IntervalLength => B - C;
  }
}
