using System.Globalization;
using System.Text;

namespace HeatConductivity.Services.Reporting;

/// <summary>Общие помощники форматирования: все тексты форматируются в одной заданной культуре.</summary>
internal static class TextFormat
{
    /// <summary>Знак «норма выполнена» (U+2714).</summary>
    public const string Ok = "✔";

    /// <summary>Знак «предупреждение» (U+26A0 + U+FE0F), как в исходной программе.</summary>
    public const string Warning = "⚠️";

    public static IFormatProvider Culture(IFormatProvider? culture) => culture ?? CultureInfo.CurrentCulture;

    public static string Format(IFormatProvider? culture, FormattableString text) => text.ToString(Culture(culture));
}

/// <summary>Накапливает строки текста, форматируя числа в заданной культуре.</summary>
internal sealed class LineBuilder
{
    private readonly StringBuilder _sb = new();
    private readonly IFormatProvider _culture;

    public LineBuilder(IFormatProvider? culture) => _culture = TextFormat.Culture(culture);

    /// <summary>Добавляет строку; интерполяция форматируется в культуре билдера.</summary>
    public void LineF(FormattableString text) => _sb.AppendLine(text.ToString(_culture));

    public void Line(string text = "") => _sb.AppendLine(text);

    public override string ToString() => _sb.ToString();
}
