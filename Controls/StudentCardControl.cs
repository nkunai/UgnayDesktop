using System.ComponentModel;
using System.Drawing;

namespace UgnayDesktop.Controls;

public partial class StudentCardControl : UserControl
{
    public event EventHandler? EditRequested;
    public event EventHandler? DetailsRequested;
    public event EventHandler? AlertRequested;

    public StudentCardControl()
    {
        InitializeComponent();
        btnEdit.Click += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);
        btnDetails.Click += (_, _) => DetailsRequested?.Invoke(this, EventArgs.Empty);
        btnAlert.Click += (_, _) => AlertRequested?.Invoke(this, EventArgs.Empty);
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string StudentName
    {
        get => lblHeading.Text;
        set => lblHeading.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string DeviceIdValue
    {
        get => lblDeviceIdValue.Text;
        set => lblDeviceIdValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string GestureValue
    {
        get => lblGestureValue.Text;
        set => lblGestureValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string HeartRateValue
    {
        get => lblHeartRateValue.Text;
        set => lblHeartRateValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SweatnessValue
    {
        get => lblSweatnessValue.Text;
        set => lblSweatnessValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TemperatureValue
    {
        get => lblTemperatureValue.Text;
        set => lblTemperatureValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ConnectionValue
    {
        get => lblConnectionValue.Text;
        set => lblConnectionValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ConnectionValueColor
    {
        get => lblConnectionValue.ForeColor;
        set => lblConnectionValue.ForeColor = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string LastSeenValue
    {
        get => lblLastSeenValue.Text;
        set => lblLastSeenValue.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SeverityText
    {
        get => lblSeverity.Text;
        set => lblSeverity.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color SeverityBackColor
    {
        get => lblSeverity.BackColor;
        set => lblSeverity.BackColor = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color SeverityForeColor
    {
        get => lblSeverity.ForeColor;
        set => lblSeverity.ForeColor = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor
    {
        get => panelAccent.BackColor;
        set => panelAccent.BackColor = value;
    }
}
