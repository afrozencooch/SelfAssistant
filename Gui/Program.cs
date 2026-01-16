using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

ApplicationConfiguration.Initialize();
var form = new Form { Text = "SelfAssistant", Width = 800, Height = 600 };

var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
var input = new TextBox { Height = 44, Width = 600, Anchor = AnchorStyles.Left | AnchorStyles.Right, Font = new Font("Segoe UI", 10F) };
var send = new Button { Text = "Send", Width = 100, Height = 44, Font = new Font("Segoe UI", 10F) };

// Use a TableLayoutPanel to ensure the input area stays visible at the bottom
var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

var bottomFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8), AutoSize = false };
bottomFlow.Controls.Add(input);
bottomFlow.Controls.Add(send);

tbl.Controls.Add(panel, 0, 0);
tbl.Controls.Add(bottomFlow, 0, 1);

form.Controls.Add(tbl);

var http = new HttpClient { BaseAddress = new Uri("http://localhost:5005") };

// Add API key header if environment variable is set
var apiKey = Environment.GetEnvironmentVariable("SELFASSISTANT_API_KEY");
if (!string.IsNullOrEmpty(apiKey)) http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

void AddBubble(string text, bool sentByUser)
{
    var lbl = new Label();
    lbl.AutoSize = false;
    lbl.MaximumSize = new System.Drawing.Size(panel.ClientSize.Width - 40, 0);
    lbl.Text = text;
    lbl.Padding = new Padding(8);
    lbl.Margin = new Padding(6);
    lbl.BorderStyle = BorderStyle.None;
    lbl.BackColor = sentByUser ? System.Drawing.Color.LightBlue : System.Drawing.Color.LightGray;
    lbl.TextAlign = sentByUser ? System.Drawing.ContentAlignment.MiddleRight : System.Drawing.ContentAlignment.MiddleLeft;
    lbl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
    lbl.AutoSize = true;

    panel.Controls.Add(lbl);
    // Auto-scroll to bottom
    panel.ScrollControlIntoView(lbl);
}

async void DoSend()
{
    var text = input.Text.Trim();
    if (string.IsNullOrEmpty(text)) return;
    try
    {
        var resp = await http.PostAsync("/chat", new StringContent(text, Encoding.UTF8, "text/plain"));
        if (!resp.IsSuccessStatusCode)
        {
            AddBubble($"[Error sending] {resp.StatusCode}", true);
            return;
        }
        AddBubble(text, true);
        input.Clear();
    }
    catch (Exception ex)
    {
        AddBubble($"[Error sending] {ex.Message}", true);
    }
}

send.Click += (s, e) => DoSend();
input.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DoSend(); } };

// Poll incoming messages from the service
var timer = new System.Windows.Forms.Timer { Interval = 1000 };
timer.Tick += async (s, e) =>
{
    try
    {
        var resp = await http.GetAsync("/chat");
        if (resp.IsSuccessStatusCode && resp.Content != null)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(msg)) AddBubble(msg, false);
        }
    }
    catch (Exception ex)
    {
        AddBubble($"[Error polling] {ex.Message}", false);
    }
};
timer.Start();

Application.Run(form);
