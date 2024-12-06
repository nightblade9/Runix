﻿using System.Threading;
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

            // Add the code input area
            _codeInput = panel.AddChild(new TextInput());
            _codeInput.Multiline = true;
            _codeInput.PlaceholderText = "Source\ncode\ngoes\nhere";
            _codeInput.Value = @"using System;
namespace Testing;

class CustomClass {
    public CustomClass() { }
    public string Run(string message) {
      return ($""Message is: {message}"");
   }
}";
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

        public override void Update()
        {
            base.Update();

            if (Input.KeyboardKey != Microsoft.Xna.Framework.Input.Keys.F5)
            {
                return;
            }
            
            // Doing this in the same thread unreasonably freezes the UI.
            // thread.Join() also freezes the UI ...
            var thread = new Thread(() => {
                // Compile
                var code = _codeInput.Value;
                _buildStatus.Text = "Building ...";
                _compilerErrors.Text = "";

                var isSuccess = Compiler.Compile(code);
                _buildStatus.Text = $"Build {(isSuccess ? "suceeded" : "failed!")}";
                if (!isSuccess)
                {
                    foreach (var message in Compiler.s_LastFailures)
                    {
                        _compilerErrors.Text += $"{message.Id} - {message.GetMessage()}";
                    }
                    return;
                }

                // Run code
                _buildStatus.Text = "Running ...";
                var response = Compiler.Run();
                _buildStatus.Text = $"Code returned: {response}";
            });

            thread.Start();
        }
    }
}
