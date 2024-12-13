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
    }
}
