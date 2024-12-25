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
        
        [Menu("Text Color")]
        public ColorNode TextColor { get; set; } = new ColorNode(Color.White);
        
        [Menu("X Position")]
        public RangeNode<int> XPos { get; set; } = new RangeNode<int>(950, 0, 2160);
        
        [Menu("Y Position")]
        public RangeNode<int> YPos { get; set; } = new RangeNode<int>(1065, 0, 3840);
        
        [Menu("Text Scale")]
        public RangeNode<float> TextSize { get; set; } = new RangeNode<float>(1, 0.5f, 5);
    }
}
