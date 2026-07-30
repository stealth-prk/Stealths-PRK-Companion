namespace PrkCompanion;

internal sealed class SettingsDialog : Form
{
    private readonly TrackBar opacity = new() { Minimum = 85, Maximum = 100, TickFrequency = 5, Width = 260 };
    private readonly Label opacityValue = new() { AutoSize = true };
    private readonly ComboBox hotkey = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly ComboBox deckScale = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };

    private sealed record HotkeyOption(string Label, uint VirtualKey)
    {
        public override string ToString() => Label;
    }

    public AppSettings UpdatedSettings { get; private set; }

    public SettingsDialog(AppSettings current)
    {
        UpdatedSettings = current.Copy();
        Text = "PRK Companion Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(390, 300);
        BackColor = Color.FromArgb(8, 43, 54);
        ForeColor = Color.FromArgb(202, 244, 247);

        hotkey.Items.AddRange(new object[]
        {
            new HotkeyOption("`  Backtick / Tilde key", 0xC0),
            new HotkeyOption("F1", (uint)Keys.F1),
            new HotkeyOption("F8", (uint)Keys.F8),
            new HotkeyOption("F10", (uint)Keys.F10)
        });
        hotkey.SelectedIndex = hotkey.Items.Cast<HotkeyOption>().ToList().FindIndex(option => option.VirtualKey == current.HotkeyVirtualKey);
        if (hotkey.SelectedIndex < 0) hotkey.SelectedIndex = 0;

        deckScale.Items.AddRange(new object[] { "Ultracompact  (80%)", "Compact  (90%)", "Standard  (100%)", "Wide  (110%)", "Ultrawide  (120%)" });
        deckScale.SelectedIndex = current.DeckScale switch { 80 => 0, 90 => 1, 110 => 3, 120 => 4, _ => 2 };
        opacity.Value = Math.Clamp((int)Math.Round(current.Opacity * 100), opacity.Minimum, opacity.Maximum);
        opacityValue.Text = $"{opacity.Value}%";
        opacity.Scroll += (_, _) => opacityValue.Text = $"{opacity.Value}%";

        var form = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), RowCount = 7, ColumnCount = 1 };
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        form.Controls.Add(MakeLabel("OVERLAY OPACITY"), 0, 0);
        var opacityRow = new FlowLayoutPanel { Dock = DockStyle.Fill }; opacityRow.Controls.Add(opacity); opacityRow.Controls.Add(opacityValue); form.Controls.Add(opacityRow, 0, 1);
        form.Controls.Add(MakeLabel("GLOBAL HOTKEY"), 0, 2); form.Controls.Add(hotkey, 0, 3);
        form.Controls.Add(MakeLabel("OVERLAY SCALE"), 0, 4); form.Controls.Add(deckScale, 0, 5);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 18, 0, 0) };
        var save = MakeButton("SAVE"); save.Click += SaveClicked;
        var cancel = MakeButton("CANCEL"); cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(cancel); buttons.Controls.Add(save); form.Controls.Add(buttons, 0, 6);
        Controls.Add(form);
    }

    private static Label MakeLabel(string text) => new() { Text = text, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(165, 220, 225) };
    private static Button MakeButton(string text) => new() { Text = text, AutoSize = true, Height = 30, Margin = new Padding(8, 0, 0, 0), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(11, 62, 75), ForeColor = Color.FromArgb(202, 244, 247), FlatAppearance = { BorderColor = Color.FromArgb(42, 123, 138) } };

    private void SaveClicked(object? sender, EventArgs e)
    {
        var selectedHotkey = (HotkeyOption)hotkey.SelectedItem!;
        UpdatedSettings.Opacity = opacity.Value / 100d;
        UpdatedSettings.HotkeyVirtualKey = selectedHotkey.VirtualKey;
        UpdatedSettings.HotkeyLabel = selectedHotkey.Label.StartsWith('`') ? "`" : selectedHotkey.Label;
        UpdatedSettings.DeckScale = deckScale.SelectedIndex switch { 0 => 80, 1 => 90, 3 => 110, 4 => 120, _ => 100 };
        DialogResult = DialogResult.OK;
    }
}
