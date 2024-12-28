using System;
using System.Collections.Generic;
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory;
using ExileCore2.Shared.Enums;
using System.Drawing;
using Vector2 = System.Numerics.Vector2;
using RectangleF = ExileCore2.Shared.RectangleF;

namespace XPBar
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        private readonly uint[] ExpTable = { 0, 525, 1760, 3781, 7184, 12186, 19324, 29377, 43181, 61693, 85990,
            117506, 157384, 207736, 269997, 346462, 439268, 551295, 685171,
            843709, 1030734, 1249629, 1504995, 1800847, 2142652, 2535122,
            2984677, 3496798, 4080655, 4742836, 5490247, 6334393, 7283446,
            8384398, 9541110, 10874351, 12361842, 14018289, 15859432, 17905634,
            20171471, 22679999, 25456123, 28517857, 31897771, 35621447, 39721017,
            44225461, 49176560, 54607467, 60565335, 67094245, 74247659, 82075627,
            90631041, 99984974, 110197515, 121340161, 133497202, 146749362,
            161191120, 176922628, 194049893, 212684946, 232956711, 255001620,
            278952403, 304972236, 333233648, 363906163, 397194041, 433312945,
            472476370, 514937180, 560961898, 610815862, 664824416, 723298169,
            786612664, 855129128, 929261318, 1009443795, 1096169525, 1189918242,
            1291270350, 1400795257, 1519130326, 1646943474, 1784977296,
            1934009687, 2094900291, 2268549086, 2455921256, 2658074992,
            2876116901, 3111280300, 3364828162, 3638186694, 3932818530,
            4250334444 };

        private DateTime sessionStart;
        private uint sessionStartXp;
        private bool isFirstRun = true;
        private double xpPerSecond;

		public override bool Initialise() 
		{
			ResetTTL();
			return true;
		}


        public override void AreaChange(AreaInstance area)
        {
            base.AreaChange(area);
            ResetTTL();
        }

        private void ResetTTL()
        {
            isFirstRun = true;
            xpPerSecond = 0;
        }

        private double GetExpPct(int level, uint exp)
        {
            if (level >= 100) return 0.0;

            var levelStart = ExpTable[level - 1];
            var nextLevel = ExpTable[level];
            var currExp = exp - levelStart;
            var neededExp = nextLevel - levelStart;

            return (double)currExp / neededExp * 100;
        }

        private string GetTTL(uint currentXp, int level)
        {
            if (!Settings.ShowTTL) return string.Empty;

            var now = DateTime.Now;
            if (isFirstRun)
            {
                sessionStart = now;
                sessionStartXp = currentXp;
                isFirstRun = false;
                return "00:00:00";
            }

            var totalTime = (now - sessionStart).TotalSeconds;
            if (totalTime < 10) return "00:00:00";

            var gained = (long)currentXp - sessionStartXp;
            if (gained > 0) xpPerSecond = gained / totalTime;
            if (xpPerSecond <= 0) return "00:00:00";

            var remaining = ExpTable[level] - currentXp;
            var seconds = remaining / xpPerSecond;

            if (double.IsInfinity(seconds) || double.IsNaN(seconds)) return "00:00:00";

            var time = TimeSpan.FromSeconds(seconds);
            return time.Hours > 99 ? ">99:59:59" : $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        public override void Render()
        {
            if (GameController.Game.IngameState.IngameUi.GameUI?.GetChildAtIndex(0) is not Element expBar) return;

            var player = GameController.Player.GetComponent<Player>();
            var pct = GetExpPct(player.Level, player.XP);
            var text = $"{player.Level}: {Math.Round(pct, Settings.DecimalPlaces.Value)}%";

            if (Settings.ShowTTL) text += $" - TTL: {GetTTL(player.XP, player.Level)}";

            using (Graphics.SetTextScale(Settings.TextScaleSize.Value))
            {
                var pos = Settings.XPos.Value != 0 || Settings.YPos.Value != 0
                    ? new Vector2(Settings.XPos, Settings.YPos)
                    : CalculateCenteredPosition(expBar.GetClientRect(), Graphics.MeasureText(text));

                var align = Settings.XPos.Value != 0 || Settings.YPos.Value != 0 ? FontAlign.Center : FontAlign.Left;

                Graphics.DrawTextWithBackground(text, pos, Settings.TextColor, align, Settings.BackgroundColor);
            }
        }

        private static Vector2 CalculateCenteredPosition(RectangleF container, Vector2 textSize) =>
            container.Location + (container.Size - textSize) / 2;
    }
}
