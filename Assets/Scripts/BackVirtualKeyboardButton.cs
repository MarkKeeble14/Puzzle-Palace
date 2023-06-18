public class BackVirtualKeyboardButton : VirtualKeyboardButton
{
    public override void OnPress()
    {
        ((UsesVirtualKeyboardMiniGameManager)MiniGameManager._Instance).SimulateBack();
    }
}
