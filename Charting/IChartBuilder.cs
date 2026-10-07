using System.Windows.Forms.DataVisualization.Charting;

namespace DichotomyMethodApp.Charting {
  public interface IChartBuilder {
    void Build(Chart chart, string expression, double a, double b, double? root);
  }
}
