using HeatConductivity.Core.Models;

namespace HeatConductivity.Core.Tests;

/// <summary>
/// Материалы и пресеты стен, скопированные из исходной Form1.
/// Временно: когда появится проект Services, данные уйдут туда и тесты будут брать их оттуда.
/// </summary>
internal static class TestData
{
    public static readonly Material Concrete = new("Железобетон", 1.70, 2500, 840, 200);
    public static readonly Material Brick = new("Кирпич керамический", 0.70, 1800, 880, 250);
    public static readonly Material AeratedBlock = new("Блоки газосиликатные (D500)", 0.13, 500, 840, 400);
    public static readonly Material MineralWool = new("Минеральная вата (Фасад)", 0.040, 110, 840, 100);
    public static readonly Material Xps = new("Пенополистирол (XPS)", 0.032, 35, 1450, 100);
    public static readonly Material Wood = new("Дерево (брус сосна/ель)", 0.15, 500, 2300, 150);
    public static readonly Material Plaster = new("Штукатурка защитно-отделочная", 0.87, 1700, 1050, 15);

    /// <summary>Климат «Минск»: -24 / +20 °C, влажность 55 %.</summary>
    public static readonly ClimateConditions Minsk = new(-24, 20, 55);

    public static WallConstruction AeratedMineralPlaster() => new(new[]
    {
        new Layer(AeratedBlock, 400), new Layer(MineralWool, 100), new Layer(Plaster, 15)
    });

    public static WallConstruction BrickXps() => new(new[]
    {
        new Layer(Brick, 380), new Layer(Xps, 120), new Layer(Plaster, 15)
    });

    public static WallConstruction Panel() => new(new[]
    {
        new Layer(Concrete, 160), new Layer(MineralWool, 140), new Layer(Concrete, 80)
    });

    public static WallConstruction WoodMineralPlaster() => new(new[]
    {
        new Layer(Wood, 150), new Layer(MineralWool, 100), new Layer(Plaster, 15)
    });
}
