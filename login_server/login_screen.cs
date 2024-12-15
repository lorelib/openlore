using Godot;

namespace EQGodot.login_server;

public partial class login_screen : Control
{
    [Signal]
    public delegate void DoLoginEventHandler(string username, string password);

    private void OnLoginButtonPressed()
    {
        var username = GetNode<LineEdit>("Background/Margins/VBox/UsernameLineEdit").Text;
        var password = GetNode<LineEdit>("Background/Margins/VBox/PasswordLineEdit").Text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;
        EmitSignal(SignalName.DoLogin, username, password);
    }

    public void OnMessageUpdate(string message)
    {
        GetNode<Label>("Background/Margins/VBox/StatusLabel").Text = message;
    }
}