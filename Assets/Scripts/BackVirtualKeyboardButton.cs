public class BackVirtualKeyboardButton : VirtualKeyboardButton
{
    public override void OnPress()
    {
        ((WordoGameManager)MiniGameManager._Instance).SimulateBack();
    }
}
