// Based on this guide: https://stardewvalleywiki.com/User:Dem1se#Define_your_UI

// 1. Create your new UI class
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

public class ControlPanel : IClickableMenu
{
    // 2. Some of the constants you'll use to relatively lay out all the UI elements
    static int UIWidth = 800;
    static int UIHeight = 800;
    int XPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (UIWidth / 2);
    int YPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - (UIHeight / 2);

    object[] ControlsToAdd;

    // 3. Declare all the UI elements
    ClickableComponent TitleLabel;

    public ControlPanel()
    {
        base.initialize(XPos, YPos, UIWidth, UIHeight);

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


        // draw cursor at last
        drawMouse(b);
    }

    // TODO: Implement Control Panel scripting API
    //public enum ControlType
    //{
    //    Button, NumberInput, TextInput, HSlider
    //}

    //public void AddControl(string label, ControlType controlType, Action<object> onChange)
    //{

    //}
}