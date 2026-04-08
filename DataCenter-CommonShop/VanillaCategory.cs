namespace DataCenter_CommonShop;

/// <summary>
/// The built-in shop categories from the base game.
/// Use <see cref="VanillaCategoryExtensions.ToShopString"/> to get the exact string the game uses,
/// or pass it directly to <see cref="CustomShopItem.SetCategory"/>.
/// </summary>
public enum VanillaCategory
{
    SystemXServers,
    RISCServers,
    MainframeServers,
    GPUServers,
    Switches,
    PassiveComponents,
    Cables,
    SFPs,
    HLMods
}

public static class VanillaCategoryExtensions
{
    /// <summary>Returns the exact category string the base game uses for this category.</summary>
    public static string ToShopString(this VanillaCategory category) => category switch
    {
        VanillaCategory.SystemXServers    => "System X Servers",
        VanillaCategory.RISCServers       => "RISC Servers",
        VanillaCategory.MainframeServers  => "Mainframe Servers",
        VanillaCategory.GPUServers        => "GPU Servers",
        VanillaCategory.Switches          => "Switches",
        VanillaCategory.PassiveComponents => "Passive components",
        VanillaCategory.Cables            => "Cables",
        VanillaCategory.SFPs              => "SFPs",
        VanillaCategory.HLMods            => "HL Mods",
        _                                 => category.ToString()
    };
}
