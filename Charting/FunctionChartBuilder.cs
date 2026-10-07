using System.Windows.Forms.DataVisualization.Charting;
using DichotomyMethodApp.Services;
using DichotomyMethodApp.Theme;

namespace DichotomyMethodApp.Charting {
  public class FunctionChartBuilder : IChartBuilder {
    private const int PointCount = 600;
    private const double YRangeFactor = 1.4;

    private readonly IFunctionEvaluator _evaluator;

    public FunctionChartBuilder(IFunctionEvaluator evaluator) {
      _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public void Build(Chart chart, string expression, double a, double b, double? root) {
      if (chart == null) {
        throw new ArgumentNullException(nameof(chart));
      }

      if (a >= b) {
        throw new ArgumentException("a должно быть меньше b");
      }

      chart.Series.Clear();
      chart.Titles.Clear();

      if (chart.ChartAreas.Count == 0) {
        chart.ChartAreas.Add(new ChartArea("MainArea"));
      }

      var area = chart.ChartAreas[0];

      area.AxisX.Minimum = a;
      area.AxisX.Maximum = b;
      area.AxisY.Minimum = double.NaN;
      area.AxisY.Maximum = double.NaN;
      area.RecalculateAxesScale();

      double step = (b - a) / PointCount;
      var points = new List<PointF>(PointCount + 1);

      double yMin = double.PositiveInfinity;
      double yMax = double.NegativeInfinity;

      for (int i = 0; i <= PointCount; i++) {
        double x = a + i * step;

        try {
          double y = _evaluator.Evaluate(expression, x);

          if (double.IsNaN(y) || double.IsInfinity(y)) {
            continue;
          }

          points.Add(new PointF((float)x, (float)y));

          if (y < yMin) yMin = y;
          if (y > yMax) yMax = y;

        } catch {
          // Пропуск точек, где функция не определена
        }
      }

      if (points.Count == 0) {
        return;
      }

      double absMax = Math.Max(Math.Abs(yMin), Math.Abs(yMax));

      if (absMax < 1e-9) {
        absMax = 1.0;
      }

      double halfRange = absMax * YRangeFactor;
      double yLow = -halfRange;
      double yHigh = halfRange;

      if (root.HasValue) {
        double localSpan = Math.Max(1.0, Math.Abs(b - a));
        yLow = Math.Min(yLow, -localSpan * 0.15);
        yHigh = Math.Max(yHigh, localSpan * 0.15);
      }

      area.BackColor = AppTheme.Card;
      area.AxisX.Title = "x";
      area.AxisY.Title = "f(x)";
      area.AxisX.LineColor = AppTheme.Border;
      area.AxisY.LineColor = AppTheme.Border;
      area.AxisX.LabelStyle.ForeColor = AppTheme.TextSecondary;
      area.AxisY.LabelStyle.ForeColor = AppTheme.TextSecondary;
      area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8f);
      area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8f);
      area.AxisX.TitleForeColor = AppTheme.TextSecondary;
      area.AxisY.TitleForeColor = AppTheme.TextSecondary;
      area.AxisX.TitleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
      area.AxisY.TitleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
      area.AxisX.MajorGrid.LineColor = AppTheme.Border;
      area.AxisY.MajorGrid.LineColor = AppTheme.Border;
      area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
      area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
      area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
      area.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
      area.AxisX.Minimum = a;
      area.AxisX.Maximum = b;
      area.AxisY.Minimum = yLow;
      area.AxisY.Maximum = yHigh;

      area.Position.Auto = false;
      area.Position = new ElementPosition(8, 8, 88, 84);
      area.InnerPlotPosition.Auto = false;
      area.InnerPlotPosition = new ElementPosition(14, 8, 84, 84);

      var functionSeries = new Series("Function") {
        ChartType = SeriesChartType.Line,
        BorderWidth = 2,
        Color = AppTheme.Accent,
        ChartArea = "MainArea",
        LegendText = "f(x)"
      };

      foreach (var p in points) {
        functionSeries.Points.AddXY(p.X, p.Y);
      }

      chart.Series.Add(functionSeries);

      var zeroSeries = new Series("Zero") {
        ChartType = SeriesChartType.Line,
        BorderWidth = 1,
        Color = Color.FromArgb(160, 168, 180),
        ChartArea = "MainArea",
        LegendText = "y = 0"
      };

      zeroSeries.Points.AddXY(a, 0);
      zeroSeries.Points.AddXY(b, 0);
      chart.Series.Add(zeroSeries);

      if (root.HasValue && root.Value >= a && root.Value <= b) {
        double rx = root.Value;

        var guideSeries = new Series("RootGuide") {
          ChartType = SeriesChartType.Line,
          BorderWidth = 1,
          Color = AppTheme.Danger,
          BorderDashStyle = ChartDashStyle.Dash,
          ChartArea = "MainArea",
          LegendText = "x*"
        };

        guideSeries.Points.AddXY(rx, yLow);
        guideSeries.Points.AddXY(rx, 0);
        chart.Series.Add(guideSeries);

        var rootSeries = new Series("Root") {
          ChartType = SeriesChartType.Point,
          MarkerStyle = MarkerStyle.Circle,
          MarkerSize = 12,
          MarkerColor = AppTheme.Danger,
          MarkerBorderColor = Color.White,
          MarkerBorderWidth = 2,
          ChartArea = "MainArea",
          LegendText = $"x* = {rx:F4}"
        };

        rootSeries.Points.AddXY(rx, 0);
        chart.Series.Add(rootSeries);

        rootSeries.Points[0].Label = $"x* = {rx:F4}";
        rootSeries.Points[0].Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        rootSeries.Points[0].LabelForeColor = AppTheme.Danger;
        rootSeries.Points[0].LabelBackColor = Color.White;
        rootSeries.Points[0].LabelBorderColor = AppTheme.Danger;
        rootSeries.Points[0].LabelBorderDashStyle = ChartDashStyle.Solid;
        rootSeries.Points[0].LabelBorderWidth = 1;
      }
    }
  }
}