using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;

namespace PrkCompanion;

internal sealed class CompanionForm : Form
{
    private const int HotkeyId = 0x50524B; // PRK
    private const int WmHotkey = 0x0312;
    private const uint VkOem3 = 0xC0; // ` / ~ key on a US keyboard
    private readonly Panel browserHost = new() { Dock = DockStyle.Fill };
    private readonly Panel webToolbar = new() { Dock = DockStyle.Fill, Padding = new Padding(24, 6, 20, 4), BackColor = Color.FromArgb(8, 43, 54) };
    private readonly TextBox webAddress = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(4, 20, 26), ForeColor = Color.FromArgb(222, 250, 252), BorderStyle = BorderStyle.FixedSingle };
    private readonly CalculatorControl calculator = new() { Dock = DockStyle.Fill, Visible = false };
    private readonly Dictionary<string, WebView2> browserTabs = new();
    private readonly Dictionary<string, Button> navigationButtons = new();
    private static readonly Image RefreshIcon = LoadRefreshIcon();
    private readonly AppSettings settings;
    private Panel? outerPanel;
    private Label? hotkeyHint;
    private WebView2? activeBrowserTab;

    private static readonly Bookmark[] Bookmarks =
    {
        new("Auno", "https://auno.org/"),
        new("TinkerTools", "https://ao.tinkeringidiot.com/"),
        new("PRKTools", "https://anarchy.at/"),
        new("PRK DB", "https://prk.gg/"),
        new("Faffy's PRK Guide", "https://docs.google.com/document/d/13HJVAcIUAGzLPYk_eLI0ZHxTyReCi3hS1ZMyINpPkhg/edit?pli=1&tab=t.0#heading=h.c6skc9wn6w10"),
        new("PRK Player Portal", "https://portal.project-rk.com/"),
        new("PRK Bug Report", "https://git.project-rk.com/prk/issues/issues"),
        new("AO#", "https://www.youtube.com/watch?v=QDia3e12czc"),
        new("Calculator", "calculator://"),
        new("Web Browser", "about:blank")
    };

    private static readonly Bookmark[] AoUniverseBookmarks =
    {
        new("AO-Universe Home", "https://www.ao-universe.com/main/news"),
        new("Pre-Built Implants", "https://www.ao-universe.com/guides/classic-ao/gameplay-guides-6/pre-built-implants"),
        new("Buffing Guide", "https://www.ao-universe.com/guides/classic-ao/gameplay-guides-6/buffing-guide"),
        new("Pocket Boss Guide", "https://www.ao-universe.com/guides/shadowlands/tradeskill-guides-5/general-crafting-4/pocket-boss-guide"),
        new("Dyna-Camps", "https://www.ao-universe.com/guides/classic-ao/gameplay-guides-6/dyna-camps"),
        new("Master Blitz List", "https://www.ao-universe.com/guides/classic-ao/gameplay-guides-6/master-blitz-list-v20")
    };

    public CompanionForm()
    {
        settings = AppSettings.Load();
        Text = "PRK Companion";
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        TopMost = true;
        KeyPreview = true;
        Opacity = settings.Opacity;
        BackColor = Color.FromArgb(5, 21, 27);

        Controls.Add(BuildOverlay());
        Shown += async (_, _) => await InitializeBrowserAsync();
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) Hide();
            else if (calculator.Visible && calculator.HandleKeyboard(e.KeyCode, e.Shift)) e.SuppressKeyPress = true;
        };
        FormClosing += (_, _) => UnregisterHotKey(Handle, HotkeyId);
    }

    private Control BuildOverlay()
    {
        var outer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(5, 21, 27),
            Padding = new Padding(110, 64, 110, 64)
        };
        outerPanel = outer;
        ApplyDeckScale();

        var deck = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(8, 35, 44),
            Padding = new Padding(2)
        };
        outer.Controls.Add(deck);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(10, 43, 54),
            ColumnCount = 1,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        deck.Controls.Add(layout);

        layout.Controls.Add(BuildMasthead(), 0, 0);
        BuildWebToolbar();
        layout.Controls.Add(webToolbar, 0, 1);
        layout.Controls.Add(BuildNavigation(), 0, 2);

        var browserFrame = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(4, 20, 26),
            Padding = new Padding(1),
            Margin = new Padding(18, 0, 18, 18)
        };
        browserFrame.Controls.Add(browserHost);
        browserFrame.Controls.Add(calculator);
        layout.Controls.Add(browserFrame, 0, 3);
        return outer;
    }

    private Control BuildMasthead()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 14, 30, 12),
            ColumnCount = 2,
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        var accent = new Panel
        {
            BackColor = Color.FromArgb(62, 202, 213),
            Dock = DockStyle.Left,
            Width = 4
        };
        var brand = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(16, 0, 0, 0) };
        brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        brand.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var wordmark = new Label
        {
            Text = "PRK  //  COMPANION",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(205, 250, 252),
            TextAlign = ContentAlignment.MiddleLeft
        };
        var descriptor = new Label
        {
            Text = "PROJECT RUBI-KA  /  KNOWLEDGE OVERLAY",
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(143, 188, 195)
        };
        hotkeyHint = new Label
        {
            Text = $"ASSIGNED HOTKEY: {settings.HotkeyLabel}  OR  ESC  —  RETURN TO AO",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(174, 231, 234),
            TextAlign = ContentAlignment.MiddleRight
        };
        brand.Controls.Add(wordmark, 0, 0);
        brand.Controls.Add(descriptor, 0, 1);
        var brandFrame = new Panel { Dock = DockStyle.Fill };
        brandFrame.Controls.Add(brand);
        brandFrame.Controls.Add(accent);
        header.Controls.Add(brandFrame, 0, 0);
        header.Controls.Add(hotkeyHint, 1, 0);
        return header;
    }

    private Control BuildNavigation()
    {
        var navigationArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            BackColor = Color.FromArgb(9, 43, 54)
        };
        navigationArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        navigationArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 246));

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 12, 0, 10),
            BackColor = Color.Transparent,
            WrapContents = false,
            AutoScroll = true
        };

        foreach (var bookmark in Bookmarks)
        {
            var button = CreateNavigationButton(bookmark.Label.ToUpperInvariant());
            button.Click += async (_, _) => await ActivateTabAsync(bookmark);
            navigationButtons.Add(bookmark.Label, button);
            nav.Controls.Add(button);

            if (bookmark.Label == "PRK DB") nav.Controls.Add(BuildAoUniverseMenu());
        }
        var settingsButton = CreateNavigationButton("SETTINGS");
        settingsButton.AutoSize = true;
        settingsButton.MinimumSize = new Size(0, 34);
        settingsButton.Margin = new Padding(0, 0, 8, 0);
        settingsButton.Padding = new Padding(6, 0, 6, 0);
        settingsButton.FlatAppearance.BorderSize = 1;
        settingsButton.FlatAppearance.BorderColor = Color.FromArgb(42, 123, 138);
        settingsButton.Click += (_, _) => OpenSettings();
        var quitButton = CreateNavigationButton("QUIT");
        quitButton.AutoSize = true;
        quitButton.MinimumSize = new Size(0, 34);
        quitButton.Margin = new Padding(0);
        quitButton.Padding = new Padding(6, 0, 6, 0);
        quitButton.FlatAppearance.BorderSize = 1;
        quitButton.FlatAppearance.BorderColor = Color.FromArgb(42, 123, 138);
        quitButton.Click += (_, _) => Close();
        var settingsHolder = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 12, 16, 10),
            BackColor = Color.Transparent
        };
        actions.Controls.Add(settingsButton);
        actions.Controls.Add(quitButton);
        settingsHolder.Controls.Add(actions);
        navigationArea.Controls.Add(nav, 0, 0);
        navigationArea.Controls.Add(settingsHolder, 1, 0);
        return navigationArea;
    }

    private Button CreateNavigationButton(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Height = 34,
        Margin = new Padding(0, 0, 8, 0),
        Padding = new Padding(6, 0, 6, 0),
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.FromArgb(11, 62, 75),
        ForeColor = Color.FromArgb(165, 220, 225),
        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        FlatAppearance = { BorderColor = Color.FromArgb(42, 123, 138) }
    };

    private Control BuildAoUniverseMenu()
    {
        var button = CreateNavigationButton("AO-UNIVERSE  ▾");
        var menu = new ContextMenuStrip
        {
            BackColor = Color.FromArgb(8, 43, 54),
            ForeColor = Color.FromArgb(202, 244, 247),
            ShowImageMargin = false,
            ShowCheckMargin = false,
            Padding = new Padding(4, 4, 4, 4),
            MinimumSize = new Size(238, 0),
            Renderer = new CompanionMenuRenderer()
        };

        foreach (var bookmark in AoUniverseBookmarks)
        {
            var item = new ToolStripMenuItem(bookmark.Label.ToUpperInvariant())
            {
                ForeColor = Color.FromArgb(165, 220, 225),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(14, 5, 22, 5)
            };
            item.Click += async (_, _) => await ActivateTabAsync(bookmark);
            menu.Items.Add(item);
        }

        button.Click += (_, _) => menu.Show(button, new Point(0, button.Height + 6));
        navigationButtons.Add("AO-Universe", button);
        return button;
    }

    private async Task InitializeBrowserAsync()
    {
        try
        {
            RegisterHotKey(Handle, HotkeyId, 0, settings.HotkeyVirtualKey);
            await ActivateTabAsync(Bookmarks[0]);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PRK Companion needs Microsoft Edge WebView2 Runtime to browse sites.\n\n{ex.Message}",
                "Browser unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task ActivateTabAsync(Bookmark bookmark)
    {
        if (bookmark.Label == "Calculator")
        {
            browserHost.Visible = false;
            webToolbar.Enabled = false;
            calculator.Visible = true;
            calculator.BringToFront();
            SetActiveNavigation(bookmark.Label);
            return;
        }

        calculator.Visible = false;
        browserHost.Visible = true;
        webToolbar.Enabled = true;
        if (!browserTabs.TryGetValue(bookmark.Label, out var tab))
        {
            tab = new WebView2 { Dock = DockStyle.Fill, Visible = false };
            browserTabs.Add(bookmark.Label, tab);
            browserHost.Controls.Add(tab);

            await tab.EnsureCoreWebView2Async();
            tab.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            tab.CoreWebView2.Settings.AreDevToolsEnabled = false;
            tab.CoreWebView2.DocumentTitleChanged += (_, _) =>
            {
                if (tab.Visible) Text = $"PRK Companion — {tab.CoreWebView2.DocumentTitle}";
            };
            tab.CoreWebView2.SourceChanged += (_, _) =>
            {
                if (tab.Visible) webAddress.Text = tab.Source?.ToString() ?? string.Empty;
            };
            tab.CoreWebView2.Navigate(bookmark.Url);
        }

        foreach (var openTab in browserTabs.Values) openTab.Visible = false;
        tab.Visible = true;
        tab.BringToFront();
        activeBrowserTab = tab;
        webAddress.Text = tab.Source?.ToString() ?? bookmark.Url;
        SetActiveNavigation(bookmark.Label);
    }

    private void BuildWebToolbar()
    {
        var back = CreateBrowserButton("back");
        back.Click += (_, _) =>
        {
            if (activeBrowserTab?.CoreWebView2 is { CanGoBack: true } view) view.GoBack();
        };

        var forward = CreateBrowserButton("forward");
        forward.Click += (_, _) =>
        {
            if (activeBrowserTab?.CoreWebView2 is { CanGoForward: true } view) view.GoForward();
        };

        var refresh = CreateBrowserButton("refresh");
        refresh.Click += (_, _) =>
        {
            if (activeBrowserTab?.CoreWebView2 is { } view) view.Reload();
        };

        var go = CreateNavigationButton("GO");
        go.Dock = DockStyle.Right;
        go.AutoSize = false;
        go.Width = 78;
        go.Margin = new Padding(8, 0, 0, 0);
        go.Padding = new Padding(0);
        go.Text = string.Empty;
        var goNormalColor = Color.FromArgb(11, 62, 75);
        var goHoverColor = Color.FromArgb(16, 101, 116);
        var goPressedColor = Color.FromArgb(23, 126, 143);
        go.FlatAppearance.MouseOverBackColor = goHoverColor;
        go.FlatAppearance.MouseDownBackColor = goPressedColor;
        var goCaption = new Label
        {
            Text = "GO",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(165, 220, 225),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        goCaption.Click += (_, _) => NavigateWeb();
        goCaption.MouseEnter += (_, _) => go.BackColor = goHoverColor;
        goCaption.MouseLeave += (_, _) => go.BackColor = goNormalColor;
        goCaption.MouseDown += (_, _) => go.BackColor = goPressedColor;
        goCaption.MouseUp += (_, _) => go.BackColor = goHoverColor;
        go.Controls.Add(goCaption);
        go.MouseEnter += (_, _) => go.BackColor = goHoverColor;
        go.MouseLeave += (_, _) => go.BackColor = goNormalColor;
        go.Click += (_, _) => NavigateWeb();
        webAddress.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { NavigateWeb(); e.SuppressKeyPress = true; } };
        webToolbar.Controls.Add(webAddress);
        webToolbar.Controls.Add(go);
        webToolbar.Controls.Add(refresh);
        webToolbar.Controls.Add(forward);
        webToolbar.Controls.Add(back);
    }

    private Button CreateBrowserButton(string icon)
    {
        var button = new Button
        {
            Dock = DockStyle.Left,
            AutoSize = false,
            Width = 54,
            Margin = new Padding(0, 0, 5, 0),
            Padding = new Padding(0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(11, 62, 75),
            ForeColor = Color.FromArgb(165, 220, 225),
            FlatAppearance = { BorderColor = Color.FromArgb(42, 123, 138) }
        };
        if (icon == "refresh")
        {
            button.Paint += (_, e) => DrawRefreshIcon(e.Graphics, button.ClientRectangle);
        }
        else
        {
            button.Paint += (_, e) => DrawBrowserIcon(e.Graphics, button.ClientRectangle, icon, button.ForeColor);
        }
        return button;
    }

    private static Image LoadRefreshIcon()
    {
        using var stream = typeof(CompanionForm).Assembly.GetManifestResourceStream("PrkCompanion.Assets.refresh.png")
            ?? throw new InvalidOperationException("The embedded refresh icon could not be loaded.");
        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    }

    private static void DrawRefreshIcon(Graphics graphics, Rectangle bounds)
    {
        var iconSize = Math.Min(bounds.Height - 8, (int)Math.Round(18f * graphics.DpiX / 96f));
        var target = new Rectangle(
            bounds.Left + (bounds.Width - iconSize) / 2,
            bounds.Top + (bounds.Height - iconSize) / 2,
            iconSize,
            iconSize);

        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        graphics.DrawImage(RefreshIcon, target, 0, 0, RefreshIcon.Width, RefreshIcon.Height, GraphicsUnit.Pixel);
    }

    private static void DrawBrowserIcon(Graphics graphics, Rectangle bounds, string icon, Color color)
    {
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var pen = new Pen(color, 2.2f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
        var centerX = bounds.Left + bounds.Width / 2f;
        var centerY = bounds.Top + bounds.Height / 2f;

        var direction = icon == "back" ? -1 : 1;
        graphics.DrawLine(pen, centerX - direction * 8, centerY, centerX + direction * 8, centerY);
        graphics.DrawLine(pen, centerX + direction * 8, centerY, centerX + direction * 2, centerY - 6);
        graphics.DrawLine(pen, centerX + direction * 8, centerY, centerX + direction * 2, centerY + 6);
    }

    private void NavigateWeb()
    {
        var input = webAddress.Text.Trim();
        if (string.IsNullOrWhiteSpace(input) || activeBrowserTab?.CoreWebView2 is null) return;
        var target = input.Contains("://") || input.Contains('.') ? (input.Contains("://") ? input : "https://" + input) : "https://duckduckgo.com/?q=" + Uri.EscapeDataString(input);
        activeBrowserTab.CoreWebView2.Navigate(target);
    }

    private void OpenSettings()
    {
        using var dialog = new SettingsDialog(settings);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        UnregisterHotKey(Handle, HotkeyId);
        settings.Opacity = dialog.UpdatedSettings.Opacity;
        settings.HotkeyVirtualKey = dialog.UpdatedSettings.HotkeyVirtualKey;
        settings.HotkeyLabel = dialog.UpdatedSettings.HotkeyLabel;
        settings.DeckScale = dialog.UpdatedSettings.DeckScale;
        settings.Save();
        Opacity = settings.Opacity;
        ApplyDeckScale();
        hotkeyHint!.Text = $"ASSIGNED HOTKEY: {settings.HotkeyLabel}  OR  ESC  —  RETURN TO AO";
        if (!RegisterHotKey(Handle, HotkeyId, 0, settings.HotkeyVirtualKey)) MessageBox.Show("That hotkey is already in use. Choose another option in Settings.", "PRK Companion");
    }

    private void ApplyDeckScale()
    {
        if (outerPanel is null) return;
        outerPanel.Padding = new Padding(110 * 100 / settings.DeckScale, 64 * 100 / settings.DeckScale, 110 * 100 / settings.DeckScale, 64 * 100 / settings.DeckScale);
    }

    private void SetActiveNavigation(string activeLabel)
    {
        foreach (var (label, button) in navigationButtons)
        {
            var active = label == activeLabel || (label == "AO-Universe" && AoUniverseBookmarks.Any(bookmark => bookmark.Label == activeLabel));
            button.BackColor = active ? Color.FromArgb(16, 101, 116) : Color.FromArgb(11, 62, 75);
            button.ForeColor = active ? Color.FromArgb(255, 213, 130) : Color.FromArgb(165, 220, 225);
            button.FlatAppearance.BorderColor = active ? Color.FromArgb(246, 169, 72) : Color.FromArgb(42, 123, 138);
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId)
        {
            if (Visible) Hide();
            else
            {
                Show();
                WindowState = FormWindowState.Maximized;
                Activate();
            }
        }
        base.WndProc(ref m);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        if (calculator.Visible && key == Keys.Enter)
        {
            calculator.HandleKeyboard(Keys.Enter, (keyData & Keys.Shift) == Keys.Shift);
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

internal sealed class CompanionMenuRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        using var brush = new SolidBrush(Color.FromArgb(8, 43, 54));
        e.Graphics.FillRectangle(brush, e.Item.Bounds);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Selected ? Color.FromArgb(255, 213, 130) : Color.FromArgb(165, 220, 225);
        base.OnRenderItemText(e);
    }
}
