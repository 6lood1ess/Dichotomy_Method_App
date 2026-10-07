using DichotomyMethodApp.Models;

namespace DichotomyMethodApp.Services {
  // Интерфейс решателя методом половинного деления
  public interface IDichotomySolver {
    DichotomyResult Solve(DichotomyParameters parameters);
  }
}
