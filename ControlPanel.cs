// Based on this guide: https://stardewvalleywiki.com/User:Dem1se#Define_your_UI

// 1. Create your new UI class
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using StardewModdingAPI;

namespace ScriptedHand
{
    public class ControlPanel : IClickableMenu
    {
        static int UIWidth = 800;
        static int UIHeight = 800;
        static int XPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (UIWidth / 2);
        static int YPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (UIHeight / 2);

        private static System.Collections.Generic.List<ControlDrawData> ScriptControls = new();

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

            foreach (ControlDrawData control in ScriptControls)
            {
                if (control.data.controlType == ControlType.Button)
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

            // draw every script control (pain)
            if (ScriptControls.Count > 0)
                foreach (ControlDrawData control in ScriptControls)
                {
                    // Draw label
                    Utility.drawTextWithShadow(b, control.data.label, Game1.dialogueFont, new Vector2(control.x, control.y), Color.Black);
                    
                    // Draw control
                    switch (control.data.controlType)
                    {
                        case ControlType.Button:
                            
                            break;
                        default:
                            throw new Exception($"Lua Error: Invalid control type " +
                                $"{control.data.controlType}. Must be one of Button," +
                                $"Checkbox, NumberInput, TextInput, or Slider");
                    }
                }

            // draw cursor at last
            drawMouse(b);
        }

        // TODO: Implement Control Panel scripting API
        public enum ControlType
        {
            Button, Checkbox, NumberInput, TextInput, Slider
        }


        static public void LoadControls()
        {
            throw new NotImplementedException("Lua Error: loadControls() is not " +
                "implemented yet");
        }

        static public void ListLayout(params ControlData[] controls)
        {
            const int PAGE_MAX_CONTROLS = 5;

            int i = 0;
            if (controls.Length < PAGE_MAX_CONTROLS && controls.Length > 0)
            {
                // Non-paged layout
                foreach (ControlData control in controls)
                {
                    ControlDrawData drawData = new()
                    {
                        data = control,

                        // the actual layout part lmao
                        index = i,

                        x = XPos + 60,
                        y = (YPos + 116) + (30 * (i + 1)),

                        width = UIWidth - 60,
                        height = 29
                    };

                    ScriptControls.Add(drawData);
                }
            }
        }

        struct ControlDrawData
        {
            public ControlData data;
            public int index;

            // Dimensions
            public int x;
            public int y;

            public int width;
            public int height;


        }
    }
}