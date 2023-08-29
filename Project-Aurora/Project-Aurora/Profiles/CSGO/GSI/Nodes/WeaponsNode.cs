using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing information about player's weapons
/// </summary>
public class WeaponsNode : Node
{
    private const string WeaponTaser = "weapon_taser";
    private const string WeaponHegrenade = "weapon_hegrenade";
    private const string WeaponFlashbang = "weapon_flashbang";
    private const string WeaponSmokegrenade = "weapon_smokegrenade";
    private const string WeaponDecoy = "weapon_decoy";
    private const string WeaponMolotov = "weapon_molotov";

    private List<WeaponNode> _Weapons = new();

    /// <summary>
    /// The number of weapons a player has in their inventory
    /// </summary>
    public int Count => _Weapons.Count;

    private WeaponNode _activeWeaponDummy = new("");

    /// <summary>
    /// Player's currently active weapon
    /// </summary>
    public WeaponNode ActiveWeapon
    {
        get
        {
            foreach (var w in _Weapons.Where(w => w.State is WeaponState.Active or WeaponState.Reloading))
            {
                return w;
            }

            return _activeWeaponDummy;
        }
    }

    public bool HasPrimary => _Weapons.Exists(w => w.Type is WeaponType.Rifle or WeaponType.MachineGun or WeaponType.SniperRifle or WeaponType.SubmachineGun or WeaponType.Shotgun);
    public bool HasRifle => _Weapons.Exists(w => w.Type == WeaponType.Rifle);
    public bool HasMachineGun => _Weapons.Exists(w => w.Type == WeaponType.MachineGun);
    public bool HasShotgun => _Weapons.Exists(w => w.Type == WeaponType.Shotgun);
    public bool HasSniper => _Weapons.Exists(w => w.Type == WeaponType.SniperRifle);
    public bool HasKnife => _Weapons.Exists(w => w.Type == WeaponType.Knife);
    public bool HasTaser => _Weapons.Exists(w => w.Name == WeaponTaser);
    public bool HasSMG => _Weapons.Exists(w => w.Type == WeaponType.SubmachineGun);
    public bool HasPistol => _Weapons.Exists(w => w.Type == WeaponType.Pistol);
    public bool HasC4 => _Weapons.Exists(w => w.Type == WeaponType.C4);
    public bool HasGrenade => _Weapons.Exists(w => w.Type == WeaponType.Grenade);
    public bool HasHegrenade => _Weapons.Exists(w => w.Name == WeaponHegrenade);
    public bool HasFlashbang => _Weapons.Exists(w => w.Name == WeaponFlashbang);
    public bool HasSmoke => _Weapons.Exists(w => w.Name == WeaponSmokegrenade);
    public bool HasDecoy => _Weapons.Exists(w => w.Name == WeaponDecoy);
    public bool HasIncendiary => _Weapons.Exists(w => w.Name == WeaponMolotov);
    public int GrenadeCount => _Weapons.Sum(w => w.Type == WeaponType.Grenade ? 1 : 0);

    internal WeaponsNode(string json)
        : base(json)
    {
        foreach(var jt in _ParsedData.Children())
        {
            _Weapons.Add(new WeaponNode(jt.First.ToString()));
        }
    }

    /// <summary>
    /// Gets the weapon at a specific index
    /// </summary>
    /// <param name="index">The index to retrieve a weapon at</param>
    /// <returns>A weapon node at the specified index</returns>
    public WeaponNode this[int index]
    {
        get
        {
            if (index > _Weapons.Count - 1)
            {
                return new WeaponNode("");
            }

            return _Weapons[index];
        }
    }
}