public class EnterVirtualKeyboardButton : VirtualKeyboardButton
{
    public override void OnPress()
    {
        ((WordoGameManager)MiniGameManager._Instance).SimulateEnter();
    }
}
