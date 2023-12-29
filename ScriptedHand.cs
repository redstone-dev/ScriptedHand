using System;
using System.IO;
using MoonSharp.Interpreter;
using StardewModdingAPI;
using System.Linq;

namespace ScriptedHand
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        Script library = new();

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.LoadConsoleCommands(helper);
        }


        /*********
        ** Private methods
        *********/

        private void RunScript(string command, string[] args)
        {
            string s = File.ReadAllText(Path.Combine(this.Helper.DirectoryPath, "Lua", args[0]));
            if (s.Split("\n")[0] != "function main()")
            {
                // requires main function so the script can't do anything funky
                Monitor.Log($"Lua Error: Script \"{args[0]}\" does not contain a main function!", LogLevel.Error);
                return;
            }
            Script script = new();
            this.InjectAPI(script);

            script.DoString(s);

            script.Call(script.Globals["main"]); // call main function
        }

        private void RunScripts(string command, string[] args)
        {
            foreach (string scriptName in args)
            {
                Script.RunString(File.ReadAllText(Path.Combine(this.Helper.DirectoryPath, "Lua", scriptName)));
            }
        }

        /// <summary>Method to load all the SMAPI console commands. For organization purposes.</summary>
        private void LoadConsoleCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add(
                "run_script",
                "Run the specified Lua script.\nExample: run_script helloworld.lua",
                RunScript
                );
            helper.ConsoleCommands.Add(
                "run_scripts",
                "Run a bunch of scripts in order from left to right.\nExample: run_scripts script1.lua script2.lua script3.lua",
                RunScripts
                );

        }

        /// <summary>
        /// Called to let Lua use C# methods.
        /// </summary>
        /// <param name="script">The Script object to allow access to.</param>

        internal void InjectAPI(Script script)
        {
            //script.Globals["movePlayer"] = (Func<int, int, int>)MovePlayer;
            //script.Globals["interact"] = (Func<int>)Interact;
            script.Globals["smapiPrint"] = (Func<string, string, int>)SMAPIPrint;
        }

        /*****************
         * Scripting API *
         *****************/

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
        private int MovePlayer(int x, int y)
        {
            return 0;
        }

        /// <summary>
        /// Emulates interacting with a tile in front 
        /// of the player.
        /// </summary>
        private int Interact()
        {
            return 0;
        }

        /// <summary>
        /// Emulates using a tool on a tile in front 
        /// of the player. Depends on the hotbar slot 
        /// currently selected.
        /// </summary>
        private int UseTool()
        {
            return 0;
        }

        /// <summary>
        /// Switches the currently active hotbar slot to the one 
        /// specified.
        /// </summary>
        /// <param name="slot">The hotbar slot to switch to, 
        /// from 0-9 as well as - and =.</param>
        private int SwitchItemTo(int slot)
        {
            // TODO: figure out how to emulate keyboard input or switch hotbar slots from C#
            return 0;
        }
		
		/// <summary>
		///	Prints to the SMAPI console.
		/// </summary>
		///	<param name="text">String to print to the console.</param>
		/// <param name="logLevel">Log level. Must be one of TRACE | DEBUG | INFO | ALERT | WARN | ERROR.</param>
        private int SMAPIPrint(string text, string logLevel)
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
                    this.Monitor.Log("[From Lua] Tried to log with invalid log level!", LogLevel.Error);
                    return 0;
            }
            this.Monitor.Log(text, ll);
            return 0;
        }
    }
}