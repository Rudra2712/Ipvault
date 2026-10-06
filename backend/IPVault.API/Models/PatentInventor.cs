namespace IPVault.API.Models;

public class PatentInventor
{
    public int PatentId { get; set; }
    public Patent? Patent { get; set; }

    public int InventorId { get; set; }
    public Inventor? Inventor { get; set; }

    public bool IsPrimaryInventor { get; set; } = false;
}
