using System.Net.Http;
using System.Text;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();
var form = new Form { Text = "SelfAssistant", Width = 800, Height = 600 };

var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
var input = new TextBox { Dock = DockStyle.Bottom, Height = 30 };
var send = new Button { Text = "Send", Dock = DockStyle.Bottom, Height = 30 };

// Layout: controls added top-down; use a container to place input+send at bottom
var container = new Panel { Dock = DockStyle.Fill };
container.Controls.Add(panel);
form.Controls.Add(container);
form.Controls.Add(send);
form.Controls.Add(input);

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
