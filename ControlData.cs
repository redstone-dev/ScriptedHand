using System;
namespace ScriptedHand
{
    public class ControlData
    {
        public string name;
        public string label;
        public ControlPanel.ControlType controlType;
        public Action onChange;

        public ControlData(string name, string label, ControlPanel.ControlType controlType)
        {
            this.name = name;
            this.label = label;
            this.controlType = controlType;
        }

    }
}
