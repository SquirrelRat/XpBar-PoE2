using System;
using ExileCore2;
using System.Drawing;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.Shared.Enums;
using System.Numerics;
using RectangleF = ExileCore2.Shared.RectangleF;

namespace XPBar
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        // Constants and Experience Table
        private const string DEFAULT_TIME_DISPLAY = "00:00:00";
        private const string MAX_TIME_DISPLAY = ">99:59:59";
        private const int MIN_TIME_FOR_CALCULATION = 10;
        private const int MAX_LEVEL = 100;

        private static readonly uint[] ExpTable = 
        {
            0, 525, 1760, 3781, 7184, 12186, 19324, 29377, 43181, 61693, 85990,
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
            4250334444 
        };

        // Tracking variables
        private DateTime sessionStart;
        private uint sessionStartXp, lastXpAmount, areaStartXp;
        private double xpPerSecond, areaXpGain, areasToLevelUp;
        private DateTime lastXpGainTime;
        private int lastLevel;

        // Initialisation
        public override bool Initialise()
        {
            ResetTracking();
            return true;
        }

        // Area Change Handling
        public override void AreaChange(AreaInstance area)
        {
            base.AreaChange(area);
            UpdateAreaXpGain();
            ResetTracking();
        }

        // Reset Tracking
        private void ResetTracking()
        {
            sessionStart = DateTime.Now;
            xpPerSecond = lastXpAmount = sessionStartXp = 0;
            lastXpGainTime = sessionStart;
            
            var player = GameController.Player?.GetComponent<Player>();
            if (player != null) areaStartXp = player.XP;
            lastLevel = player?.Level ?? 0;
        }

        // Update Area XP Gain
        private void UpdateAreaXpGain()
        {
            var player = GameController.Player?.GetComponent<Player>();
            if (player != null)
            {
                areaXpGain = player.XP - areaStartXp;
                var remainingXp = ExpTable[player.Level] - player.XP;
                if (areaXpGain > 0)
                {
                    areasToLevelUp = remainingXp / areaXpGain;
                    areaStartXp = player.XP;
                }
            }
        }

        // Calculate XP Percentage
        private double GetExpPct(int level, uint exp)
        {
            if (level >= MAX_LEVEL) return 0.0;

            if (level > lastLevel)
            {
                ResetTracking();
                lastLevel = level;
            }

            var levelStart = ExpTable[level - 1];
            return (exp - levelStart) / (double)(ExpTable[level] - levelStart) * 100;
        }

        // Calculate Time to Level Up (TTL)
        private string GetTTL(uint currentXp, int level)
        {
            var now = DateTime.Now;

            if (currentXp > lastXpAmount)
            {
                lastXpGainTime = now;
                lastXpAmount = currentXp;
            }
            else if ((now - lastXpGainTime).TotalMinutes > Settings.ResetTimerMinutes)
            {
                ResetTracking();
                return DEFAULT_TIME_DISPLAY;
            }

            if (sessionStart == default || sessionStartXp == 0)
            {
                sessionStart = now;
                sessionStartXp = currentXp;
                return DEFAULT_TIME_DISPLAY;
            }

            var totalTime = (now - sessionStart).TotalSeconds;
            if (totalTime < MIN_TIME_FOR_CALCULATION) return DEFAULT_TIME_DISPLAY;

            var gained = (long)currentXp - sessionStartXp;
            if (gained > 0) xpPerSecond = gained / totalTime;
            if (xpPerSecond <= 0) return DEFAULT_TIME_DISPLAY;

            var remaining = ExpTable[level] - currentXp;
            var seconds = remaining / xpPerSecond;

            return double.IsInfinity(seconds) || double.IsNaN(seconds) 
                ? DEFAULT_TIME_DISPLAY 
                : TimeSpan.FromSeconds(seconds).Hours > 99 
                    ? MAX_TIME_DISPLAY 
                    : $"{TimeSpan.FromSeconds(seconds).Hours:00}:{TimeSpan.FromSeconds(seconds).Minutes:00}:{TimeSpan.FromSeconds(seconds).Seconds:00}";
        }

        // Render XP Bar and TTL Display
        public override void Render()
        {
            var player = GameController.Player?.GetComponent<Player>();
            if (player == null) return;

            var pct = GetExpPct(player.Level, player.XP);
            var xpText = $"{player.Level} ({Math.Round(pct, Settings.DecimalPlaces.Value)}%)";

            string additionalText = "";
            if (Settings.ShowTTL)
            {
                var ttlText = $"TTL: {GetTTL(player.XP, player.Level)}";
                var areaTTLText = $" - Area TTL: {Math.Ceiling(areasToLevelUp)}";
                additionalText = ttlText + areaTTLText;
            }

            if (GameController.Game.IngameState.IngameUi.GameUI?.GetChildAtIndex(0) is not Element expBar) return;

            using (Graphics.SetTextScale(Settings.TextScaleSize.Value))
            {
                var frameWidth = expBar.GetClientRect().Width - 25;
                var frameHeight = 12;
                var frameX = expBar.GetClientRect().X + (expBar.GetClientRect().Width - frameWidth) / 2;
                var frameY = expBar.GetClientRect().Center.Y - frameHeight / 2;

                var framePosition = new Vector2(frameX, frameY);
                var adjustedWidth = frameWidth - 2;
                var barHeight = frameHeight - 2;
                var barPosition = new Vector2(framePosition.X + 1, framePosition.Y + 1);

                if (Settings.ShowXPBar)
                {
                    Graphics.DrawFrame(new RectangleF(framePosition.X, framePosition.Y, frameWidth, frameHeight), Settings.OuterBarColor, 1);

                    var initialXPWidth = adjustedWidth * ((sessionStartXp - ExpTable[player.Level - 1]) / (float)(ExpTable[player.Level] - ExpTable[player.Level - 1]));
                    Graphics.DrawBox(new RectangleF(barPosition.X, barPosition.Y, initialXPWidth, barHeight), Settings.BarColor);

                    var newXPWidth = adjustedWidth * ((player.XP - sessionStartXp) / (float)(ExpTable[player.Level] - ExpTable[player.Level - 1]));
                    var newXPPosition = new Vector2(barPosition.X + initialXPWidth, barPosition.Y);
                    Graphics.DrawBox(new RectangleF(newXPPosition.X, newXPPosition.Y, newXPWidth, barHeight), Settings.NewXPBarColor);

                    var remainingWidth = adjustedWidth - initialXPWidth - newXPWidth;
                    var remainingPosition = new Vector2(newXPPosition.X + newXPWidth, barPosition.Y);
                    Graphics.DrawBox(new RectangleF(remainingPosition.X, remainingPosition.Y, remainingWidth, barHeight), Settings.BackgroundColor);
                }

                var xpTextSize = Graphics.MeasureText(xpText);
                var additionalTextSize = Graphics.MeasureText(additionalText);

                Graphics.DrawText(xpText, new Vector2(barPosition.X + 3, barPosition.Y + (barHeight / 2) - (xpTextSize.Y / 2)), Settings.TextColor);
                Graphics.DrawText(additionalText, new Vector2(framePosition.X + frameWidth - Graphics.MeasureText(additionalText).X - 3, barPosition.Y + (barHeight / 2) - (additionalTextSize.Y / 2)), Settings.TextColor);
            }
        }
    }
}