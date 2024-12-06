using MonoGo.Engine;
using MonoGo.Engine.EC;
using MonoGo.Engine.SceneSystem;
using MonoGo.Engine.UI;
using MonoGo.Engine.UI.Controls;
using MonoGo.Engine.UI.Defs;

namespace Runix.Editor
{
    public class IdeScreen : Entity, IHaveGUI
    {
        private TextInput _codeInput;
        private Paragraph _compilerErrors;
        private Paragraph _buildStatus;

        private CameraController _cameraController;

        public IdeScreen(CameraController cameraController) : base(SceneMgr.DefaultLayer)
        {
            _cameraController = cameraController;
        }

        public void CreateUI()
        {
            UISystem.LoadTheme("DefaultTheme");

            // Add the code input area
            var panel = CreateAndAddPanel();
            _codeInput = panel.AddChild(new TextInput());
            _codeInput.PlaceholderText = "Source\ncode\ngoes\nhere";
            _codeInput.Size.Y.SetPixels(450);
            _codeInput.Multiline = true;
            _codeInput.CreateVerticalScrollbar();

            // Add the compiler results label: compiler errors
            panel.AddChild(new HorizontalLine());
            _compilerErrors = panel.AddChild(new Paragraph("Compiler errors go here\n(Multiple if applicable)"));
            _compilerErrors.Size.Y.SetPixels(200);

            // Add the build-messages thing
            panel.AddChild(new HorizontalLine());
            _buildStatus = panel.AddChild(new Paragraph("Build status: didn't build yet"));
            _buildStatus.Size.Y.SetPixels(720 - 450 - 200);
        }

        private Panel CreateAndAddPanel()
        {
            // Non-default stylesheet, so that it's visible.
            // One panel per scene, I think. Not 100% sure.
            var panel = new Panel(UISystem.DefaultStylesheets.Panels)
            {
                Anchor = Anchor.Center,
                OverflowMode = OverflowMode.HideOverflow,
            };
            panel.Size.X.SetPercents(100);
            panel.Size.Y.SetPercents(100);
            UISystem.Add(panel);
            return panel;
        }
    }
}
