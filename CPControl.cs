using System;
namespace ScriptedHand
{
    public class CPControl
    {
        public string name;
        public string label;
        public ControlPanel.ControlType controlType;
        public Action onChange;

        public CPControl(string name, string label, ControlPanel.ControlType controlType)
        {
            this.name = name;
            this.label = label;
            this.controlType = controlType;
        }

    }
}
