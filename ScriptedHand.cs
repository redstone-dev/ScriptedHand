using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Linq;
using MoonSharp.Interpreter;
using System.Runtime.CompilerServices;

namespace ScriptedHand
{
    /*
     * TODO:
     * - let Lua use the SMAPIPrint() function
     */
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private IModHelper modHelper;

        string scriptName = "";
        string scriptPath = "";
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.modHelper = helper;
            this.LoadConsoleCommands(helper);
        }


        /*********
        ** Private methods
        *********/

        /// <summary>Reloads the current Lua script.</summary>
        private void ReloadCurrentScript(string command, string[] args)
        {
            string[] _args = { scriptPath };
            this.LoadLuaScript(command, _args);
            this.Monitor.Log($"Reloaded script {scriptName}", LogLevel.Info);
        }

        /// <summary>SMAPI console command to load the specified Lua script.</summary>
        private void LoadLuaScript(string command, string[] args)
        {
            scriptName = args[0];
            scriptPath = Path.Combine(modHelper.DirectoryPath, "Scripts", args[0]);

            this.Monitor.Log($"Loaded script {args[0]}", LogLevel.Info);
        }

        /// <summary>SMAPI console command to run the currently loaded Lua script.</summary>
        private void RunCurrentScript(string command, string[] args)
        {
            this.Monitor.Log($"Running script '{scriptName}'...", LogLevel.Info);
            Script.RunFile(scriptPath);
        }

        /// <summary>SMAPI console command to unload the currently loaded Lua script.</summary>
        private void UnloadCurrentScript(string command, string[] args)
        {
            try
            {
                this.Monitor.Log($"Unloading script {args[0]}!", LogLevel.Info);

                this.scriptName = "";
                this.scriptPath = "";
                Script.RunString("");
            } catch
            {
                this.Monitor.Log($"Could not unload script {args[0]}!", LogLevel.Error);
            }
        }

        /// <summary>Method to load all the SMAPI console commands. For organizaiton purposes.</summary>
        private void LoadConsoleCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add(
                "reload_script", 
                "Updates the currently loaded Lua script. Run this after every change you make.", 
                ReloadCurrentScript);
            helper.ConsoleCommands.Add(
                "load_script",
                "Loads the specified script.\nTakes the script's filename as an argument.\nNOTE: Scripts MUST be placed in a subdirectory of the mod's directory called Lua.",
                LoadLuaScript);
            helper.ConsoleCommands.Add(
                "unload_script",
                "Unloads the currently loaded Lua script.",
                UnloadCurrentScript);

        }

        /*****************
         * Scripting API *
         *****************/

        /// <summary>
        /// A type representing either the Left or Right mouse button.
        /// </summary>
        public enum MouseButton { Left, Right }

        /// <summary>
        /// Moves the player by (x, y) tiles horizontally 
        /// and/or vertically.
        /// </summary>
        /// <param name="x">Amount of tiles to move 
        /// horizontally.</param>
        /// <param name="y">Amount of tiles to move 
        /// vertically.</param>
        public void MovePlayerBy(int x, int y)
        {

        }

        /// <summary>
        /// Emulates interacting with a tile in front 
        /// of the player.
        /// </summary>
        public void Interact()
        {

        }

        /// <summary>
        /// Emulates using a tool on a tile in front 
        /// of the player. Depends on the hotbar slot 
        /// currently selected.
        /// </summary>
        public void UseTool()
        {

        }

        /// <summary>
        /// Switches the currently active hotbar slot to the one 
        /// specified.
        /// </summary>
        /// <param name="slot">The hotbar slot to switch to, 
        /// from 0-9 as well as - and =.</param>
        public void SwitchItemTo(int slot)
        {
            // TODO: figure out how to emulate keyboard input or switch hotbar slots from C#
        }
		
		/// <summary>
		///	Prints to the SMAPI console.
		/// </summary>
		///	<param name="text">String to print to the console.</param>
		/// <param name="logLevel">Log level. Must be one of TRACE | DEBUG | INFO | ALERT | WARN | ERROR.</param>
        public void SMAPIPrint(string text, string logLevel)
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
                    return;
            }
            this.Monitor.Log(text, ll);
        }
    }
}