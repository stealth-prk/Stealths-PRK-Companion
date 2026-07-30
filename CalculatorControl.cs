namespace PrkCompanion;

internal sealed class CalculatorControl : UserControl
{
    private readonly Label historyDisplay = new() { Dock = DockStyle.Top, Height = 62, Font = new Font("Segoe UI", 9, FontStyle.Regular), ForeColor = Color.FromArgb(143, 188, 195), TextAlign = ContentAlignment.BottomRight };
    private readonly Label expression = new() { Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 11, FontStyle.Regular), ForeColor = Color.FromArgb(165, 220, 225), TextAlign = ContentAlignment.MiddleRight };
    private readonly TextBox display = new() { Dock = DockStyle.Top, Height = 58, ReadOnly = true, TabStop = false, Text = "0", TextAlign = HorizontalAlignment.Right, Font = new Font("Segoe UI", 22, FontStyle.Bold), BackColor = Color.FromArgb(4, 20, 26), ForeColor = Color.FromArgb(222, 250, 252), BorderStyle = BorderStyle.FixedSingle };
    private readonly List<string> history = new();
    private decimal accumulator;
    private string pendingOperation = string.Empty;
    private string currentFormula = string.Empty;
    private bool startNewNumber = true;

    public CalculatorControl()
    {
        BackColor = Color.FromArgb(8, 43, 54);
        var body = new Panel { Size = new Size(460, 460), BackColor = Color.FromArgb(8, 43, 54) };
        body.Controls.Add(BuildKeys());
        body.Controls.Add(display);
        body.Controls.Add(expression);
        body.Controls.Add(historyDisplay);
        Controls.Add(body);
        Resize += (_, _) => body.Location = new Point(Math.Max(24, (ClientSize.Width - body.Width) / 2), Math.Max(20, (ClientSize.Height - body.Height) / 2));
    }

    private Control BuildKeys()
    {
        var keys = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 5, Padding = new Padding(0, 12, 0, 0) };
        for (var i = 0; i < 4; i++) keys.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (var i = 0; i < 5; i++) keys.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        string[,] labels = { { "C", "⌫", "±", "÷" }, { "7", "8", "9", "×" }, { "4", "5", "6", "−" }, { "1", "2", "3", "+" }, { "0", ".", "=", "" } };
        for (var row = 0; row < 5; row++)
        for (var col = 0; col < 4; col++)
        {
            var label = labels[row, col];
            if (string.IsNullOrEmpty(label)) continue;
            var button = new Button { Text = label, Dock = DockStyle.Fill, Margin = new Padding(4), TabStop = false, FlatStyle = FlatStyle.Flat, BackColor = label is "=" or "+" ? Color.FromArgb(16, 101, 116) : Color.FromArgb(11, 62, 75), ForeColor = label is "=" or "+" ? Color.FromArgb(255, 213, 130) : Color.FromArgb(202, 244, 247), Font = new Font("Segoe UI", 13, FontStyle.Bold), FlatAppearance = { BorderColor = Color.FromArgb(42, 123, 138) } };
            button.Click += (_, _) => Press(label);
            keys.Controls.Add(button, col, row);
        }
        return keys;
    }

    public bool HandleKeyboard(Keys key, bool shift)
    {
        if (shift && key == Keys.Oemplus) return PressAndHandled("+");
        if (shift && key == Keys.D8) return PressAndHandled("×");
        return key switch
        {
            Keys.D0 or Keys.NumPad0 => PressAndHandled("0"), Keys.D1 or Keys.NumPad1 => PressAndHandled("1"),
            Keys.D2 or Keys.NumPad2 => PressAndHandled("2"), Keys.D3 or Keys.NumPad3 => PressAndHandled("3"),
            Keys.D4 or Keys.NumPad4 => PressAndHandled("4"), Keys.D5 or Keys.NumPad5 => PressAndHandled("5"),
            Keys.D6 or Keys.NumPad6 => PressAndHandled("6"), Keys.D7 or Keys.NumPad7 => PressAndHandled("7"),
            Keys.D8 or Keys.NumPad8 => PressAndHandled("8"), Keys.D9 or Keys.NumPad9 => PressAndHandled("9"),
            Keys.Decimal or Keys.OemPeriod => PressAndHandled("."), Keys.Add => PressAndHandled("+"),
            Keys.Subtract or Keys.OemMinus => PressAndHandled("−"), Keys.Multiply => PressAndHandled("×"),
            Keys.Divide or Keys.OemQuestion => PressAndHandled("÷"), Keys.Enter or Keys.Oemplus => PressAndHandled("="),
            Keys.Back => PressAndHandled("⌫"), Keys.Delete or Keys.Escape => PressAndHandled("C"), _ => false
        };
    }

    private bool PressAndHandled(string key) { Press(key); return true; }

    private void Press(string key)
    {
        if (char.IsDigit(key[0]) || key == ".")
        {
            display.Text = startNewNumber || display.Text == "0" ? (key == "." ? "0." : key) : key != "." || !display.Text.Contains('.') ? display.Text + key : display.Text;
            startNewNumber = false;
            UpdateExpression();
            return;
        }
        if (key == "C") { accumulator = 0; pendingOperation = string.Empty; currentFormula = string.Empty; display.Text = "0"; expression.Text = string.Empty; startNewNumber = true; return; }
        if (key == "⌫") { display.Text = !startNewNumber && display.Text.Length > 1 ? display.Text[..^1] : "0"; UpdateExpression(); return; }
        if (key == "±") { if (decimal.TryParse(display.Text, out var number)) display.Text = (-number).ToString(); UpdateExpression(); return; }
        if (!decimal.TryParse(display.Text, out var value)) return;

        if (key == "=")
        {
            if (string.IsNullOrEmpty(pendingOperation)) return;
            var formula = $"{currentFormula} {value}";
            var result = Calculate(accumulator, value, pendingOperation);
            AddHistory($"{formula} = {result}");
            display.Text = result.ToString(); accumulator = result; pendingOperation = string.Empty; currentFormula = string.Empty; expression.Text = formula; startNewNumber = true;
            return;
        }

        if (string.IsNullOrEmpty(pendingOperation))
        {
            accumulator = value;
            currentFormula = $"{value} {key}";
            pendingOperation = key;
            startNewNumber = true;
            UpdateExpression();
            return;
        }
        if (startNewNumber)
        {
            pendingOperation = key;
            currentFormula = $"{accumulator} {key}";
            UpdateExpression();
            return;
        }
        accumulator = Calculate(accumulator, value, pendingOperation);
        display.Text = accumulator.ToString();
        currentFormula = $"{currentFormula} {value} {key}";
        pendingOperation = key;
        startNewNumber = true;
        UpdateExpression();
    }

    private void UpdateExpression() => expression.Text = string.IsNullOrEmpty(pendingOperation) ? string.Empty : startNewNumber ? currentFormula : $"{currentFormula} {display.Text}";

    private void AddHistory(string entry)
    {
        history.Add(entry);
        historyDisplay.Text = string.Join(Environment.NewLine, history.TakeLast(3));
    }

    private static decimal Calculate(decimal left, decimal right, string operation) => operation switch { "+" => left + right, "−" => left - right, "×" => left * right, "÷" when right != 0 => left / right, _ => left };
}
