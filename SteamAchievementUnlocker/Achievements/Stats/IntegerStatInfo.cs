namespace SteamAchievementUnlocker.Stats
{
    internal class IntegerStatInfo : StatInfo
    {
        public int OriginalValue;
        public int IntValue;

        public override object Value
        {
            get => this.IntValue;
            set
            {
                var i = int.Parse((string)value, System.Globalization.CultureInfo.CurrentCulture);
                if ((this.Permission & 2) != 0 &&
                    this.IntValue != i)
                {
                    throw new StatIsProtectedException();
                }
                this.IntValue = i;
            }
        }

        public override bool IsModified => this.IntValue != this.OriginalValue;
    }
}
