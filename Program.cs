using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PoorMansTSqlFormatterRedux;
using PoorMansTSqlFormatterRedux.Formatters;
using Terminal.Gui;

namespace SqlUnpackerApp;

public enum SqlDialect
{
    Oracle,
    MsSql
}

class Program
{
    static void Main()
    {
        Application.Init();
        var top = Application.Top;

        var win = new Window("SqlUnpacker TUI v3.1")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        top.Add(win);

        var dialectFrame = new FrameView("Целевая СУБД")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 4
        };

        var dialectRadio = new RadioGroup()
        {
            X = 1,
            Y = 0,
            RadioLabels = new NStack.ustring[] { "Oracle (HEXTORAW)", "MS SQL Server (0xHEX)" },
            DisplayMode = DisplayModeLayout.Horizontal,
            SelectedItem = 0
        };
        dialectFrame.Add(dialectRadio);
        win.Add(dialectFrame);

        var inputFrame = new FrameView("Исходный SQL с параметрами NHibernate/EF")
        {
            X = 0,
            Y = Pos.Bottom(dialectFrame),
            Width = Dim.Fill(),
            Height = Dim.Percent(40)
        };

        var inputTextView = new TextView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Multiline = true,
            WordWrap = false
        };

        inputFrame.Add(inputTextView);
        win.Add(inputFrame);

        // Кнопки управления
        var pasteBtn = new Button(" Вставить (Ctrl+B) ")
        {
            X = 1,
            Y = Pos.Bottom(inputFrame) + 1
        };

        var processBtn = new Button(" Обработать (Ctrl+R) ")
        {
            X = Pos.Right(pasteBtn) + 1,
            Y = Pos.Bottom(inputFrame) + 1,
            IsDefault = true
        };

        var copyBtn = new Button(" Копировать результат (Ctrl+C) ")
        {
            X = Pos.Right(processBtn) + 1,
            Y = Pos.Bottom(inputFrame) + 1
        };

        var clearBtn = new Button(" Очистить ")
        {
            X = Pos.Right(copyBtn) + 1,
            Y = Pos.Bottom(inputFrame) + 1
        };

        var exitBtn = new Button(" Выход (Esc) ")
        {
            X = Pos.Right(clearBtn) + 1,
            Y = Pos.Bottom(inputFrame) + 1
        };

        win.Add(pasteBtn, processBtn, copyBtn, clearBtn, exitBtn);

        var outputFrame = new FrameView("Результат (Inlined & Formatted SQL)")
        {
            X = 0,
            Y = Pos.Bottom(pasteBtn) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var outputTextView = new TextView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            Multiline = true,
            WordWrap = false
        };
        outputFrame.Add(outputTextView);
        win.Add(outputFrame);

        // --- ДЕЙСТВИЯ ---

        Action pasteFromClipboardAction = () =>
        {
            var text = Clipboard.Contents?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                inputTextView.Text = text;
            }
            else
            {
                MessageBox.Query("Буфер пуст", "В системном буфере обмена нет текста.", "OK");
            }
        };

        Action copyToClipboardAction = () =>
        {
            string outputText = outputTextView.Text.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(outputText))
            {
                Clipboard.Contents = outputText;
                MessageBox.Query("Успешно", "Результат скопирован в буфер обмена!", "OK");
            }
            else
            {
                MessageBox.ErrorQuery("Ошибка", "Окно результата пустое!", "OK");
            }
        };

        Action processSqlAction = () =>
        {
            string rawInput = inputTextView.Text.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(rawInput))
            {
                MessageBox.ErrorQuery("Ошибка", "Поле ввода SQL пустое!", "OK");
                return;
            }

            try
            {
                SqlDialect dialect = dialectRadio.SelectedItem == 0 ? SqlDialect.Oracle : SqlDialect.MsSql;

                var (sql, parameters) = ExtractSqlAndParameters(rawInput, dialect);
                string sqlWithInlinedParams = InlineParameters(sql, parameters);
                
                // Используем библиотеку форматирования вместо самописного регулярного выражения
                string beautifiedSql = SqlFormatterEngine.FormatSql(sqlWithInlinedParams);

                outputTextView.Text = beautifiedSql;
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Ошибка обработки", ex.Message, "OK");
            }
        };

        pasteBtn.Clicked += pasteFromClipboardAction;
        processBtn.Clicked += processSqlAction;
        copyBtn.Clicked += copyToClipboardAction;

        clearBtn.Clicked += () =>
        {
            inputTextView.Text = string.Empty;
            outputTextView.Text = string.Empty;
            inputTextView.SetFocus();
        };

        exitBtn.Clicked += () => Application.RequestStop();

        top.KeyPress += (e) =>
        {
            if (e.KeyEvent.Key == (Key.B | Key.CtrlMask))
            {
                pasteFromClipboardAction();
                e.Handled = true;
            }
            else if (e.KeyEvent.Key == (Key.R | Key.CtrlMask))
            {
                processSqlAction();
                e.Handled = true;
            }
            else if (e.KeyEvent.Key == (Key.C | Key.CtrlMask))
            {
                copyToClipboardAction();
                e.Handled = true;
            }
            else if (e.KeyEvent.Key == Key.Esc)
            {
                Application.RequestStop();
                e.Handled = true;
            }
        };

        Application.Run();
        Application.Shutdown();
    }

    private static (string Sql, Dictionary<string, string> Parameters) ExtractSqlAndParameters(string input, SqlDialect dialect)
    {
        var paramsDict = new Dictionary<string, string>();

        var paramPattern = new Regex(@"(:\w+)\s*=\s*([^\s\[]+)\s*\[Type:\s*([^\]]+)\]");
        var matches = paramPattern.Matches(input);

        foreach (Match match in matches)
        {
            string paramName = match.Groups[1].Value;
            string value = match.Groups[2].Value;
            string type = match.Groups[3].Value;

            string formattedValue = FormatValueForDialect(value, type, dialect);
            paramsDict[paramName] = formattedValue;
        }

        int firstParamIndex = Regex.Match(input, @":p0\s*=").Index;
        string sqlText = firstParamIndex > 0 ? input[..firstParamIndex] : input;

        return (sqlText, paramsDict);
    }

    private static string FormatValueForDialect(string val, string type, SqlDialect dialect)
    {
        if (type.StartsWith("Binary", StringComparison.OrdinalIgnoreCase))
        {
            string hex = val.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? val[2..] : val;

            return dialect switch
            {
                SqlDialect.Oracle => $"HEXTORAW('{hex}')",
                SqlDialect.MsSql  => $"0x{hex}",
                _ => val
            };
        }

        if (val.Equals("True", StringComparison.OrdinalIgnoreCase)) return "1";
        if (val.Equals("False", StringComparison.OrdinalIgnoreCase)) return "0";

        return val;
    }

    private static string InlineParameters(string sql, Dictionary<string, string> parameters)
    {
        foreach (var key in parameters.Keys.OrderByDescending(k => k.Length))
        {
            sql = Regex.Replace(sql, $@"{key}\b", parameters[key]);
        }
        return sql;
    }
}

public static class SqlFormatterEngine
{
    public static string FormatSql(string rawSql)
    {
        if (string.IsNullOrWhiteSpace(rawSql)) return string.Empty;

        var options = new TSqlStandardFormatterOptions
        {
            IndentString = "    ",            // 4 пробела для отступа
            SpacesPerTab = 4,
            ExpandCommaLists = true,          // Каждая колонка в списке SELECT с новой строки
            TrailingCommas = true,            // Запятые в конце строк
            SpaceAfterExpandedComma = true,
            ExpandBooleanExpressions = true,  // Переносы для AND / OR
            ExpandCaseStatements = true,      // Разворачивание CASE WHEN
            ExpandBetweenConditions = true
        };

        var formatter = new TSqlStandardFormatter(options);
        var manager = new SqlFormattingManager(formatter);

        return manager.Format(rawSql);
    }
}