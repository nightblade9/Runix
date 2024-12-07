using Microsoft.Xna.Framework;
using MonoGo.Engine;
using MonoGo.Engine.EC;
using MonoGo.Engine.SceneSystem;
using MonoGo.Engine.UI;
using MonoGo.Engine.UI.Controls;
using MonoGo.Engine.UI.Defs;
using Runix.Editor.Controllers;

namespace Runix.Editor.Screens
{
    public class VisualDesignScreen : Entity, IHaveGUI
    {
        private CameraController _cameraController;

        public VisualDesignScreen(CameraController cameraController) : base(SceneMgr.DefaultLayer)
        {
            _cameraController = cameraController;
        }

        public void CreateUI()
        {
            UISystem.LoadTheme("DefaultTheme");
            
            var sceneTreePanel = CreateAndAddPanel(Anchor.CenterLeft, 25);
            // Setting DraggableMode doesn't seem to make them draggable
            sceneTreePanel.AddChild(new Paragraph("Root"));
            sceneTreePanel.AddChild(new Paragraph("  Child1"));
            sceneTreePanel.AddChild(new Paragraph("    Subchild2"));
            sceneTreePanel.AddChild(new Paragraph("  Child2"));
            
            var currentScenePanel = CreateAndAddPanel(Anchor.CenterRight, 75);
        }

        private static Panel CreateAndAddPanel(Anchor anchor, int widthPercent)
        {
            var panel = new Panel(UISystem.DefaultStylesheets.Panels)
            {
                Anchor = anchor,
                OverflowMode = OverflowMode.HideOverflow,
            };
            panel.Size.X.SetPercents(widthPercent);
            panel.Size.Y.SetPercents(100);
            
            UISystem.Add(panel);
            
            return panel;
        }
    }
}
