using System.Net.Http;
using System.Text;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();
var form = new Form { Text = "SelfAssistant", Width = 800, Height = 600 };

var output = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Top, Height = 400 };
var input = new TextBox { Dock = DockStyle.Top, Height = 30 };
var send = new Button { Text = "Send", Dock = DockStyle.Top, Height = 30 };

form.Controls.Add(send);
form.Controls.Add(input);
form.Controls.Add(output);

var http = new HttpClient { BaseAddress = new Uri("http://localhost:5005") };

async void DoSend()
{
    var text = input.Text.Trim();
    if (string.IsNullOrEmpty(text)) return;
    try
    {
        await http.PostAsync("/chat", new StringContent(text, Encoding.UTF8, "text/plain"));
        output.AppendText($"You: {text}{Environment.NewLine}");
        input.Clear();
    }
    catch (Exception ex)
    {
        output.AppendText($"[Error sending] {ex.Message}{Environment.NewLine}");
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
            if (!string.IsNullOrEmpty(msg)) output.AppendText($"Assistant: {msg}{Environment.NewLine}");
        }
    }
    catch { }
};
timer.Start();

Application.Run(form);
