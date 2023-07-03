public class EnterVirtualKeyboardButton : VirtualKeyboardButton
{
    public override void OnPress()
    {
        ((UsesVirtualKeyboardMiniGameManager)MiniGameManager._Instance).SimulateEnter();
    }
}
