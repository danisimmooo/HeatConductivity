namespace HeatConductivity.Core.Simulation;

/// <summary>
/// Снимок состояния расчёта. <see cref="Temperature"/> — независимая копия поля,
/// её можно безопасно читать из другого потока, пока расчёт продолжается.
/// </summary>
public sealed record SimulationProgress(int Step, int TotalSteps, double TimeSeconds, double[,] Temperature);
