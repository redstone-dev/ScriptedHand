// Based on this guide: https://stardewvalleywiki.com/User:Dem1se#Define_your_UI

// 1. Create your new UI class
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;


namespace ScriptedHand
{
    public class ControlPanel : IClickableMenu
    {
        static int UIWidth = 800;
        static int UIHeight = 800;
        int XPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (UIWidth / 2);
        int YPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (UIHeight / 2);

        private static System.Collections.Generic.List<CPControl> ScriptControls;

        // 3. Declare all the UI elements
        ClickableComponent TitleLabel;

        public ControlPanel()
        {
            initialize(XPos, YPos, UIWidth, UIHeight);

            // 4. initialize and lay out all the declared UI components
            TitleLabel = new ClickableComponent(new Rectangle(XPos + 60, YPos + 116, 0, 64), "ScriptedHand Control Panel");

        }

        // 5. The method invoked when the player left-clicks on the menu.
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (TitleLabel.containsPoint(x, y))
            {
                // handle user clicking on the title. More practical use-case would be with buttons

            }

            foreach (CPControl control in ScriptControls)
            {
                if (control.controlType == ControlType.Button)
                {
                    // TODO: trigger button action
                }
            }
        }

        public static explicit operator bool(ControlPanel v)
        {
            return true;
        }

        // 6. Render the UI that has been set up to the screen. 
        // Gets called automatically every render tick when this UI is active
        public override void draw(SpriteBatch b)
        {
            //draw screen fade
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // draw menu dialogue box
            Game1.drawDialogueBox(XPos, YPos, UIWidth, UIHeight, false, true);

            // draw the TitleLabel
            Utility.drawTextWithShadow(b, TitleLabel.name, Game1.dialogueFont, new Vector2(TitleLabel.bounds.X, TitleLabel.bounds.Y), Color.Black);

            // draw every script control (pain :'3)
            foreach (CPControl control in ScriptControls)
            {
                switch (control.controlType)
                {
                    case ControlType.NumberInput:
                        
                        break;
                    default:
                        throw new Exception($"Lua Error: Invalid control type " +
                            $"{control.controlType}. Must be one of");
                }
            }

            // draw cursor at last
            drawMouse(b);
        }

        // TODO: Implement Control Panel scripting API
        public enum ControlType
        {
            Button, ToggleSwitch, NumberInput, TextInput, HSlider
        }

        static public void AddControl(string name, string label,
            ControlType controlType)
        {
            ScriptControls.Add(new CPControl(name, label, controlType));
        }

        static public void LoadControls()
        {
            throw new NotImplementedException("Lua Error: loadControls() is not " +
                "implemented yet");
        }
    }
}