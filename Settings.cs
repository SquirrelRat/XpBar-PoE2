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

        [Menu("Show XP Bar")]
        public ToggleNode ShowXPBar { get; set; } = new ToggleNode(true);

        [Menu("Show Time To Level")]
        public ToggleNode ShowTTL { get; set; } = new ToggleNode(true);

        [Menu("Reset Timer On Idle (Minutes)")]
        public RangeNode<int> ResetTimerMinutes { get; set; } = new RangeNode<int>(1, 1, 10);

        [Menu("Text Color")]
        public ColorNode TextColor { get; set; } = new ColorNode(Color.White);

        [Menu("Background Color")]
        public ColorNode BackgroundColor { get; set; } = new ColorNode(Color.FromArgb(25, 0, 0, 0));

        [Menu("Bar Color")]
        public ColorNode BarColor { get; set; } = new ColorNode(Color.FromArgb(200, 0, 100, 130));

        [Menu("Text Scale")]
        public RangeNode<float> TextScaleSize { get; set; } = new RangeNode<float>(1, 0.5f, 5);

        [Menu("Decimal Places")]
        public RangeNode<int> DecimalPlaces { get; set; } = new RangeNode<int>(2, 1, 4);

        [Menu("Outer Bar Color")]
        public ColorNode OuterBarColor { get; set; } = new ColorNode(Color.White);

        [Menu("New XP Bar Color")]
        public ColorNode NewXPBarColor { get; set; } = new ColorNode(Color.FromArgb(200, 255, 128, 0));
    }
}