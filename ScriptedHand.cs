using System;
using System.IO;
using MoonSharp.Interpreter;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;

namespace ScriptedHand
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.LoadConsoleCommands(helper);
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Script.DefaultOptions.DebugPrint = (str) =>
            {
                Monitor.Log(str, LogLevel.Info);
            };
        }
        
        void OnButtonPressed(object sender, ButtonPressedEventArgs ev)
        {
            
            //if (!Context.IsWorldReady) return;
            if (Game1.activeClickableMenu != null || (!Context.IsPlayerFree))
                return;
            // Display UI if user presses J
            if (ev.Button == SButton.J)
                Game1.activeClickableMenu = new ControlPanel();
        }

        /*********
        ** Private methods
        *********/

        private void RunScript(string command, string[] args)
        {
            string s = File.ReadAllText(Path.Combine(Helper.DirectoryPath,
                "Lua", args[0]));
            if (s.Split(Environment.NewLine)[0] != "function main()")
            {
                // requires main function so the script can't do anything funky
                Monitor.Log($"Lua Error: Script \"{args[0]}\" does not start " +
                    $"with a main function!", LogLevel.Error);
                return;
            }

            Script script = new();
            InjectAPI(script);

            script.DoString(s);

            script.Call(script.Globals["main"]); // call main function
        }

        private void RunScripts(string command, string[] args)
        {
            foreach (string scriptName in args)
            {
                Script.RunString(File.ReadAllText
                    (Path.Combine(this.Helper.DirectoryPath,
                    "Lua", scriptName)));
            }
        }

        /// <summary>Method to load all the SMAPI console commands.
        /// For organization purposes.</summary>
        private void LoadConsoleCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add(
                "run_script",
                @"Run the specified Lua script.
Example: run_script helloworld.lua",
                RunScript
                );
            helper.ConsoleCommands.Add(
                "run_scripts",
                @"Run a bunch of scripts in order from left to right.
Example: run_scripts script1.lua script2.lua script3.lua",
                RunScripts
                );
            LoadEntryScript(helper);
        }

        private void LoadEntryScript(IModHelper helper) {
            // Load console commands from Lua files
            string p = Path.Combine(Helper.DirectoryPath, "Lua", "_entry.lua");
            if (File.Exists(p))
            {
                Monitor.LogOnce("Detected _entry.lua, running script...");
                string s = File.ReadAllText(p);
                if (!s.Contains("function commands()"))
                {
                    Monitor.Log($"Lua Error: _entry.lua does not contain a " +
                        $"commands function, and was prevented from running.",
                        LogLevel.Error);
                    return;
                }
                Script script = new();
                
                InjectAPI(script);
                    
                script.DoString(s);

                DynValue commands = script.Globals.Get("commands");

                Table commandTable = commands.Table;

                // Add console commands from script
                foreach (TablePair pair in commandTable.Pairs)
                {
                    Table table = pair.Value.Table;
                    
                    helper.ConsoleCommands.Add(
                        table.Get(1).String,
                        table.Get(2).String, 
                        (command, args) =>
                        {
                            script.Call(script.Globals[table.Get(3).String],
                                command, args);
                        }
                    );
                }

                // check for code that could crash SDV
                if (CheckScript(s))
                    throw new Exception("WARNING: Your script could cause the game to hang and/or crash, so it was prevented from running. You can try removing any \"while true\" loops to let it run.");

                // Run the startup script's main() as well
                if (TableContains(script.Globals, "main"))
                    script.Call(script.Globals.Get("main"));

                // and finally, load the controls
                // (the most complicated part)
                Table controlsTable = script.Globals.Get("controls").Table;
                string layoutType = controlsTable.Get("layout").String;
                Table controls = controlsTable.Get("controls").Table;

                List<ControlData> controlsData = new List<ControlData>();

                foreach(TablePair pair in controls.Pairs)
                {
                    Table control = pair.Value.Table;
                    string controlName = control.Get(1).String;
                    string controlType = control.Get(2).String;
                    ControlPanel.ControlType eControlType;

                    Table controlProperties = control.Get(3).Table;
                    string controlValue = controlType != "BUTTON" ? control.Get(4).String : "";

                    switch (controlType)
                    {
                        case "BUTTON":
                            eControlType = ControlPanel.ControlType.Button; break;
                        case "CHECKBOX":
                            eControlType = ControlPanel.ControlType.Checkbox; break;
                        case "NUMBERINPUT":
                            eControlType = ControlPanel.ControlType.NumberInput; break;
                        case "TEXTINPUT":
                            eControlType = ControlPanel.ControlType.TextInput; break;
                        case "SLIDER":
                            eControlType = ControlPanel.ControlType.Slider; break;
                        default:
                            throw new Exception($"Lua Error: {controlType} is not a valid control type!");
                    }

                    if (controlType == "BUTTON")
                    {
                        controlsData.Add(new ControlData(controlName, controlProperties.Get("text").String, eControlType));
                        continue;
                    }


                }
            }
        }

        /// <summary>
        /// Checks if a Table contains a certain key.
        /// </summary>
        /// <param name="table">The Table to check.</param>
        /// <param name="key">The key to look for.</param>
        /// <returns>true if the key was found.</returns>
        internal bool TableContains(Table table, string key)
        {

            return !(table.Keys.All((DynValue d) => d.String != key));
        }

        /// <summary>
        /// Checks a script for malicious code.
        /// </summary>
        /// <param name="script">The script, as a string, to check.</param>
        /// <returns>true if malicious code was found.</returns>
        internal bool CheckScript(string script)
        {
            return script.Contains("while true do");
        }

        /// <summary>
        /// Called to let Lua use C# methods.
        /// </summary>
        /// <param name="script">The Script object to allow access to.</param>

        internal void InjectAPI(Script script)
        {
            //script.Globals["movePlayer"] = (Func<int, int, int>)MovePlayer;
            //script.Globals["interact"] = (Func<int>)Interact;
            script.Globals["printl"] = (Action<string, string>)SMAPIPrint;

            // controlPanel.* api
            Table controlPanel = new Table(script);

            // This was the only way I could think of to convert AddControl
            // to a DynValue, and I'm sorry.
            // (I could have made it a one-liner which would have been worse)
            //Action<string, string, ControlPanel.ControlType> acDelegate
            //  = ControlPanel.AddControl;
            //DynValue acdelToDynValue = DynValue.FromObject(script, acDelegate);
            //Monitor.Log($"{acdelToDynValue.Type}", LogLevel.Debug);
            //controlPanel["addControl"] = acdelToDynValue;



        }

        /*****************
         * Scripting API *
         *****************/
        // TODO: harmony hijacking the InputState.GetGamePadState and pretending to be a game controller
        /// <summary>
        /// A type representing either the Left or Right mouse button.
        /// </summary>
        private enum MouseButton { Left, Right };

        /// <summary>
        /// Moves the player by (x, y) tiles horizontally 
        /// and/or vertically.
        /// </summary>
        /// <param name="x">Amount of tiles to move 
        /// horizontally.</param>
        /// <param name="y">Amount of tiles to move 
        /// vertically.</param>
        private void MovePlayer(int x, int y)
        {
            //PathFindController pfc = new(Game1.player, );
            //pfc.;
            // TODO: figure out how to move player x & y
        }

        /// <summary>
        /// Emulates interacting with a tile in front 
        /// of the player.
        /// </summary>
        private void Interact()
        {
            // TODO: force player to interact
        }

        /// <summary>
        /// Emulates using a tool on a tile in front 
        /// of the player. Depends on the hotbar slot 
        /// currently selected.
        /// </summary>
        private void UseTool()
        {
            // TODO: force player to use tool
        }

        /// <summary>
        /// Switches the currently active hotbar slot to the one 
        /// specified.
        /// </summary>
        /// <param name="slot">The hotbar slot to switch to, 
        /// from 0-9 as well as - and =.</param>
        private void SwitchItemTo(int slot)
        {
            // TODO: figure out how to emulate keyboard input or switch hotbar slots from C#
            
        }
		
		/// <summary>
		///	Prints to the SMAPI console.
		/// </summary>
		///	<param name="text">String to print to the console.</param>
		/// <param name="logLevel">Log level. Must be one of TRACE | DEBUG | INFO | ALERT | WARN | ERROR.</param>
        private void SMAPIPrint(string text, string logLevel)
        {
            LogLevel ll;
            switch(logLevel)
            {
                case "TRACE":
                    ll = LogLevel.Trace;
                    break;
                case "DEBUG":
                    ll = LogLevel.Debug;
                    break;
                case "INFO":
                    ll = LogLevel.Info;
                    break;
                case "ALERT":
                    ll = LogLevel.Alert;
                    break;
                case "WARN":
                    ll = LogLevel.Warn;
                    break;
                case "ERROR":
                    ll = LogLevel.Error;
                    break;
                default:
                    this.Monitor.Log("Lua Error: Tried to log with invalid log level!", LogLevel.Error);
                    return;
            }
            this.Monitor.Log(text, ll);
            
        }
    }
}