using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;
using System.Drawing;

namespace XPBar
{
    public class Settings : ISettings
    {
        [Menu("Enabled")]
        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("Show Time To Level")]
        public ToggleNode ShowTTL { get; set; } = new ToggleNode(true);
        
        [Menu("Show XP Penalty")]
        public ToggleNode ShowXPPenalty { get; set; } = new ToggleNode(true);
        
        [Menu("Show Debug Info")]
        public ToggleNode ShowDebugInfo { get; set; } = new ToggleNode(false);
        
        [Menu("Reset Timer On Idle (Minutes)")]
        public RangeNode<int> ResetTimerMinutes { get; set; } = new RangeNode<int>(1, 1, 10);
        
        [Menu("Text Color")]
        public ColorNode TextColor { get; set; } = new ColorNode(Color.White);

        [Menu("Background Color")]
        public ColorNode BackgroundColor { get; set; } = new ColorNode(Color.FromArgb(120, 0, 0, 0));
        
        [Menu("X Position")]
        public RangeNode<int> XPos { get; set; } = new RangeNode<int>(0, 0, 2160);
        
        [Menu("Y Position")]
        public RangeNode<int> YPos { get; set; } = new RangeNode<int>(0, 0, 3840);
        
        [Menu("Text Scale")]
        public RangeNode<float> TextScaleSize { get; set; } = new RangeNode<float>(1, 0.5f, 5);
        
        [Menu("Decimal Places")]
        public RangeNode<int> DecimalPlaces { get; set; } = new RangeNode<int>(2, 1, 4);
    }
}