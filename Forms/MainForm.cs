using System.Windows.Forms.DataVisualization.Charting;
using DichotomyMethodApp.Charting;
using DichotomyMethodApp.Models;
using DichotomyMethodApp.Presenters;
using DichotomyMethodApp.Services;
using DichotomyMethodApp.Theme;
using DichotomyMethodApp.Validators;
using DichotomyMethodApp.Views;
using GdiFont = System.Drawing.Font;

namespace DichotomyMethodApp.Forms {
  public class MainForm : Form, IMainView {
    private Panel _sidebar;
    private Panel _workspace;
    private Panel _chartCard;
    private Panel _tableCard;

    private TextBox _txtFunction;
    private TextBox _txtA;
    private TextBox _txtB;
    private TextBox _txtE;

    private DataGridView _gridSteps;
    private Label _lblRootValue;
    private Chart _chart;

    private Label _lblResultHeader;
    private Label _lblTableHeader;

    private Button _btnCalculate;
    private Button _btnClear;
    private Button _btnBuild;

    private readonly IChartBuilder _chartBuilder;
    private readonly MainPresenter _presenter;

    public MainForm() {
      BuildUi();

      var evaluator = new FunctionEvaluator();
      var validator = new InputValidator();
      var solver = new DichotomySolver(evaluator);
      _chartBuilder = new FunctionChartBuilder(evaluator);

      _presenter = new MainPresenter(this, validator, solver);
    }

    public string FunctionExpression => _txtFunction.Text;
    public string AText => _txtA.Text;
    public string BText => _txtB.Text;
    public string EpsilonText => _txtE.Text;

    public event EventHandler CalculateRequested;
    public event EventHandler ClearRequested;
    public event EventHandler BuildChartRequested;

    public void ShowSteps(IEnumerable<DichotomyStep> steps) {
      _gridSteps.Rows.Clear();

      foreach (var s in steps) {
        _gridSteps.Rows.Add(
          s.Number,
          s.A.ToString("F6"),
          s.B.ToString("F6"),
          s.C.ToString("F6"),
          s.IntervalLength.ToString("F6"));
      }
    }

    public void ShowRoot(string rootText) {
      string[] lines = rootText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

      _lblRootValue.Text = lines.Length > 0 ? lines[0] : string.Empty;

      string iterationsHeader = "Итерации";

      if (lines.Length > 1) {
        string[] parts = lines[1].Split(':');

        if (parts.Length > 1) {
          string number = parts[1].Trim();
          iterationsHeader = "Итерации: " + number;
        }
      }

      _lblTableHeader.Text = iterationsHeader;
    }

    public void ClearSteps() {
      _gridSteps.Rows.Clear();
    }

    public void ClearRoot() {
      _lblRootValue.Text = "—";
      _lblTableHeader.Text = "Итерации";
    }

    public void BuildChart(string expression, double a, double b, double? root) {
      _chartBuilder.Build(_chart, expression, a, b, root);
    }

    public void ShowError(string message) {
      MessageBox.Show(message, "Ошибка",
        MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowInfo(string message) {
      MessageBox.Show(message, "Информация",
        MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BuildUi() {
      this.Text = "Метод половинного деления";
      this.ClientSize = new Size(1240, 760);
      this.MinimumSize = new Size(1100, 680);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.BackColor = AppTheme.Background;
      this.Font = AppTheme.FontRegular;

      _workspace = new Panel {
        Dock = DockStyle.Fill,
        Padding = new Padding(0),
        BackColor = AppTheme.Background,
        AutoScroll = true
      };
      this.Controls.Add(_workspace);

      _sidebar = new Panel {
        Dock = DockStyle.Left,
        Width = 360,
        BackColor = AppTheme.Sidebar,
        Padding = new Padding(0)
      };
      this.Controls.Add(_sidebar);

      BuildSidebar();
      BuildWorkspace();
    }

    private void BuildSidebar() {
      int top = 10;

      var lblTitle = new Label {
        Text = "Метод дихотомии",
        Font = AppTheme.FontTitle,
        ForeColor = Color.White,
        Location = new Point(88, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(lblTitle);

      top += 30;

      var lblSub = new Label {
        Text = "Поиск корня f(x) = 0 на [a, b]",
        Font = AppTheme.FontRegular,
        ForeColor = AppTheme.SidebarText,
        Location = new Point(90, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(lblSub);

      top += 48;

      _txtFunction = CreateSidebarField("Функция f(x)", ref top);
      _txtFunction.Text = "x^3 - 2*x - 5";

      _txtA = CreateSidebarField("Левая граница a", ref top);
      _txtA.Text = "1";

      _txtB = CreateSidebarField("Правая граница b", ref top);
      _txtB.Text = "3";

      _txtE = CreateSidebarField("Точность e", ref top);
      _txtE.Text = "0,0001";

      top += 12;

      _btnCalculate = CreateSidebarButton("Рассчитать", AppTheme.Accent, ref top);
      _btnCalculate.Click += (s, e) => CalculateRequested?.Invoke(this, EventArgs.Empty);

      _btnBuild = CreateSidebarButton("Построить график", AppTheme.AccentHover, ref top);
      _btnBuild.Click += (s, e) => BuildChartRequested?.Invoke(this, EventArgs.Empty);

      _btnClear = CreateSidebarButton("Очистить", Color.FromArgb(58, 68, 90), ref top);
      _btnClear.Click += (s, e) => {
        ClearRequested?.Invoke(this, EventArgs.Empty);
        _txtFunction.Clear();
        _txtA.Clear();
        _txtB.Clear();
        _txtE.Clear();
      };
    }

    private TextBox CreateSidebarField(string caption, ref int top) {
      var label = new Label {
        Text = caption,
        Font = AppTheme.FontBold,
        ForeColor = AppTheme.SidebarText,
        Location = new Point(22, top),
        AutoSize = true
      };
      _sidebar.Controls.Add(label);

      top += 24;

      var textBox = new TextBox {
        Location = new Point(24, top),
        Width = _sidebar.Width - 48,
        Font = AppTheme.FontRegular,
        BorderStyle = BorderStyle.FixedSingle
      };
      _sidebar.Controls.Add(textBox);

      top += 42;

      return textBox;
    }

    private Button CreateSidebarButton(string text, Color backColor, ref int top) {
      var button = new Button {
        Text = text,
        Location = new Point(24, top),
        Width = _sidebar.Width - 48,
        Height = 40,
        FlatStyle = FlatStyle.Flat,
        BackColor = backColor,
        ForeColor = Color.White,
        Font = AppTheme.FontBold,
        Cursor = Cursors.Hand
      };
      button.FlatAppearance.BorderSize = 0;
      _sidebar.Controls.Add(button);

      top += 50;

      return button;
    }

    private void BuildWorkspace() {
      _tableCard = CreateCard();
      _tableCard.Dock = DockStyle.Left;
      _tableCard.Width = 440;
      _tableCard.Padding = new Padding(16, 12, 16, 16);

      _lblTableHeader = new Label {
        Text = "Итерации",
        Font = AppTheme.FontHeader,
        ForeColor = AppTheme.TextPrimary,
        Dock = DockStyle.Top,
        Height = 28,
        TextAlign = ContentAlignment.MiddleLeft
      };

      _gridSteps = new DataGridView {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        BackgroundColor = AppTheme.Card,
        BorderStyle = BorderStyle.None,
        GridColor = AppTheme.Border,
        Font = new GdiFont("Consolas", 9f, FontStyle.Regular),
        ScrollBars = ScrollBars.Vertical,
        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle {
          BackColor = Color.FromArgb(240, 242, 246),
          ForeColor = AppTheme.TextPrimary,
          Font = AppTheme.FontBold,
          SelectionBackColor = Color.FromArgb(240, 242, 246),
          SelectionForeColor = AppTheme.TextPrimary,
          Alignment = DataGridViewContentAlignment.MiddleCenter
        },
        DefaultCellStyle = new DataGridViewCellStyle {
          SelectionBackColor = Color.FromArgb(220, 235, 250),
          SelectionForeColor = AppTheme.TextPrimary,
          Alignment = DataGridViewContentAlignment.MiddleCenter,
          Padding = new Padding(2)
        },
        EnableHeadersVisualStyles = false
      };

      _gridSteps.Columns.Add("n", "n");
      _gridSteps.Columns.Add("a_n", "a_n");
      _gridSteps.Columns.Add("b_n", "b_n");
      _gridSteps.Columns.Add("c_n", "c_n");
      _gridSteps.Columns.Add("diff", "b_n - c_n");
      _gridSteps.ColumnHeadersHeight = 32;
      _gridSteps.RowTemplate.Height = 28;

      var resultPanel = new Panel {
        Dock = DockStyle.Bottom,
        Height = 76,
        BackColor = AppTheme.Card,
        Padding = new Padding(0, 8, 0, 0)
      };

      _lblResultHeader = new Label {
        Text = "Найденный корень",
        Font = AppTheme.FontBold,
        ForeColor = AppTheme.TextSecondary,
        Dock = DockStyle.Top,
        Height = 22,
        TextAlign = ContentAlignment.MiddleLeft
      };

      _lblRootValue = new Label {
        Text = "—",
        Font = new GdiFont("Consolas", 14f, FontStyle.Bold),
        ForeColor = AppTheme.Accent,
        Dock = DockStyle.Top,
        Height = 34,
        TextAlign = ContentAlignment.MiddleLeft
      };

      resultPanel.Controls.Add(_lblRootValue);
      resultPanel.Controls.Add(_lblResultHeader);

      _tableCard.Controls.Add(_gridSteps);
      _tableCard.Controls.Add(_lblTableHeader);
      _tableCard.Controls.Add(resultPanel);

      var midSpacer = new Panel {
        Dock = DockStyle.Left,
        Width = 16,
        BackColor = AppTheme.Background
      };

      _chartCard = CreateCard();
      _chartCard.Dock = DockStyle.Left;
      _chartCard.Width = 440;
      _chartCard.Padding = new Padding(16, 12, 16, 16);

      var lblChartHeader = new Label {
        Text = "График функции",
        Font = AppTheme.FontHeader,
        ForeColor = AppTheme.TextPrimary,
        Dock = DockStyle.Top,
        Height = 28,
        TextAlign = ContentAlignment.MiddleLeft
      };

      _chart = new Chart {
        Dock = DockStyle.Fill,
        BackColor = AppTheme.Card
      };

      var area = new ChartArea("MainArea");
      area.BackColor = AppTheme.Card;
      _chart.ChartAreas.Add(area);

      var legend = new Legend("MainLegend") {
        Docking = Docking.Bottom,
        Alignment = StringAlignment.Center,
        BackColor = Color.Transparent,
        ForeColor = AppTheme.TextSecondary,
        Font = AppTheme.FontRegular
      };
      _chart.Legends.Add(legend);

      _chartCard.Controls.Add(_chart);
      _chartCard.Controls.Add(lblChartHeader);

      _workspace.Controls.Add(_tableCard);
      _workspace.Controls.Add(midSpacer);
      _workspace.Controls.Add(_chartCard);
    }

    private Panel CreateCard() {
      var panel = new Panel {
        BackColor = AppTheme.Card,
        Padding = new Padding(16)
      };
      panel.Paint += (s, e) => {
        var p = (Panel)s;
        using (var pen = new Pen(AppTheme.Border, 1)) {
          e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }
      };
      return panel;
    }
  }
}