public class PlayerData
{
    public static PlayerStats PlayerStats { get; private set; }

    public static AmmunitionHolder PlayerAmmo { get; private set; }

    public PlayerData()
    {
        Init();
    }

    public void Init()
    {
        PlayerStats = new PlayerStats();
        PlayerAmmo = new AmmunitionHolder();
    }
}
