using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UgnayDesktop.Controls;

public enum DashboardThemeMode
{
    Light,
    Dark,
}

public sealed class DashboardThemePalette
{
    public Color Canvas { get; init; }
    public Color Surface { get; init; }
    public Color SurfaceMuted { get; init; }
    public Color Ink { get; init; }
    public Color MutedInk { get; init; }
    public Color Border { get; init; }
    public Color Accent { get; init; }
    public Color AccentHover { get; init; }
    public Color AccentSoft { get; init; }
    public Color Danger { get; init; }
    public Color DangerHover { get; init; }
    public Color Header { get; init; }
}

public static class DashboardTheme
{
    private static readonly DashboardThemePalette LightPalette = new()
    {
        Canvas = ColorTranslator.FromHtml("#F3F6FA"),
        Surface = Color.White,
        SurfaceMuted = ColorTranslator.FromHtml("#F8FAFD"),
        Ink = ColorTranslator.FromHtml("#17212B"),
        MutedInk = ColorTranslator.FromHtml("#334155"),
        Border = ColorTranslator.FromHtml("#D8E1EC"),
        Accent = ColorTranslator.FromHtml("#0E7490"),
        AccentHover = ColorTranslator.FromHtml("#0C657F"),
        AccentSoft = ColorTranslator.FromHtml("#D8EEF4"),
        Danger = ColorTranslator.FromHtml("#BE123C"),
        DangerHover = ColorTranslator.FromHtml("#9F1239"),
        Header = ColorTranslator.FromHtml("#1F3A56"),
    };

    private static readonly DashboardThemePalette DarkPalette = new()
    {
        Canvas = ColorTranslator.FromHtml("#0B1220"),
        Surface = ColorTranslator.FromHtml("#111B2D"),
        SurfaceMuted = ColorTranslator.FromHtml("#17263D"),
        Ink = ColorTranslator.FromHtml("#E5EDF5"),
        MutedInk = ColorTranslator.FromHtml("#A9BBCE"),
        Border = ColorTranslator.FromHtml("#24344C"),
        Accent = ColorTranslator.FromHtml("#0891B2"),
        AccentHover = ColorTranslator.FromHtml("#0EA5C0"),
        AccentSoft = ColorTranslator.FromHtml("#123044"),
        Danger = ColorTranslator.FromHtml("#BE123C"),
        DangerHover = ColorTranslator.FromHtml("#D61F4A"),
        Header = ColorTranslator.FromHtml("#0F172A"),
    };

    public static DashboardThemePalette GetPalette(DashboardThemeMode mode)
    {
        return mode == DashboardThemeMode.Dark ? DarkPalette : LightPalette;
    }

    public static void Apply(
        Form form,
        IEnumerable<Button> primaryButtons,
        IEnumerable<Button>? secondaryButtons = null,
        IEnumerable<Button>? dangerButtons = null,
        IEnumerable<DataGridView>? grids = null,
        DashboardThemeMode mode = DashboardThemeMode.Light)
    {
        var palette = GetPalette(mode);

        form.SuspendLayout();
        form.BackColor = palette.Canvas;
        form.ForeColor = palette.Ink;
        form.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

        foreach (var control in EnumerateControls(form))
        {
            if (control is Label label)
            {
                label.ForeColor = palette.Ink;
            }
            else if (control is TextBox textBox)
            {
                StyleTextBox(textBox, palette);
            }
            else if (control is ComboBox comboBox)
            {
                StyleComboBox(comboBox, palette);
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = palette.Ink;
                checkBox.BackColor = Color.Transparent;
            }
        }

        foreach (var button in primaryButtons)
        {
            StyleButton(button, palette.Accent, palette.AccentHover, Color.White, palette.Border, borderless: true);
        }

        if (secondaryButtons != null)
        {
            foreach (var button in secondaryButtons)
            {
                StyleButton(button, palette.Surface, palette.SurfaceMuted, palette.Ink, palette.Border, borderless: false);
            }
        }

        if (dangerButtons != null)
        {
            foreach (var button in dangerButtons)
            {
                StyleButton(button, palette.Danger, palette.DangerHover, Color.White, palette.Border, borderless: true);
            }
        }

        if (grids != null)
        {
            foreach (var grid in grids)
            {
                StyleGrid(grid, palette);
            }
        }

        form.ResumeLayout(true);
    }

    private static void StyleButton(Button button, Color backColor, Color hoverColor, Color foreColor, Color borderColor, bool borderless)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.BackColor = backColor;
        button.ForeColor = foreColor;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        button.FlatAppearance.BorderSize = borderless ? 0 : 1;
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.MouseOverBackColor = hoverColor;
        button.FlatAppearance.MouseDownBackColor = hoverColor;
    }

    private static void StyleTextBox(TextBox textBox, DashboardThemePalette palette)
    {
        textBox.BackColor = palette.Surface;
        textBox.ForeColor = palette.Ink;
        textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void StyleComboBox(ComboBox comboBox, DashboardThemePalette palette)
    {
        comboBox.BackColor = palette.Surface;
        comboBox.ForeColor = palette.Ink;
        comboBox.FlatStyle = FlatStyle.Flat;
    }

    private static void StyleGrid(DataGridView grid, DashboardThemePalette palette)
    {
        grid.BackgroundColor = palette.Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle.BackColor = palette.Header;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = palette.Header;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);

        grid.DefaultCellStyle.BackColor = palette.Surface;
        grid.DefaultCellStyle.ForeColor = palette.Ink;
        grid.DefaultCellStyle.SelectionBackColor = palette.AccentSoft;
        grid.DefaultCellStyle.SelectionForeColor = palette.Ink;
        grid.AlternatingRowsDefaultCellStyle.BackColor = palette.SurfaceMuted;

        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = palette.Border;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        grid.RowTemplate.Height = 28;
        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
    }

    private static IEnumerable<Control> EnumerateControls(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;

            foreach (var nested in EnumerateControls(child))
            {
                yield return nested;
            }
        }
    }
}


