using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading; // Used for simple delay effects

/// <summary>
/// INTERFACE (Abstraction): Defines a contract for any combat ability.
/// </summary>
public interface IAbility
{
    /// <summary>
    /// Executes the ability's effect on a target.
    /// </summary>
    void Execute(Character attacker, Character target);
}

/// <summary>
/// ABSTRACT BASE CLASS (Inheritance & Abstraction): Contains fundamental properties and methods shared by all units.
/// </summary>
public abstract class Character
{
    private int _currentHealth;
    public int CurrentHealth
    {
        get => _currentHealth;
        protected set
        {
            if (value < 0) _currentHealth = 0;
            else if (value > MaxHealth) _currentHealth = MaxHealth;
            else _currentHealth = value;
        }
    }

    // Default Properties
    public string Name { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int Damage { get; protected set; }
    public bool IsAlive => CurrentHealth > 0;

    // Constructor
    protected Character(string name, int maxHealth, int damage)
    {
        Name = name;
        MaxHealth = maxHealth;
        Damage = damage;
        CurrentHealth = maxHealth; // Start at full health using property (clamped)
    }

    /// <summary>
    /// Universal method for taking damage.
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        int damageAmount = Math.Max(0, damage);
        CurrentHealth = CurrentHealth - damageAmount;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n{Name} takes {damageAmount} damage! Remaining Health: {CurrentHealth}/{MaxHealth}");
        Console.ResetColor();

        if (!IsAlive)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n*** {Name} has been defeated! ***");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Universal method for healing.
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;

        int healAmount = Math.Max(0, amount);
        int oldHealth = CurrentHealth;
        CurrentHealth = CurrentHealth + healAmount;

        int actualHealed = CurrentHealth - oldHealth;
        if (actualHealed > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{Name} heals for {actualHealed} HP.");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"{Name} is already at full health.");
        }
    }

    /// <summary>
    /// Base method for a standard attack.
    /// </summary>
    public virtual void Attack(Character target)
    {
        Console.WriteLine($"{Name} attacks {target.Name} with a standard strike.");
        target.TakeDamage(Damage);
    }

    /// <summary>
    /// ABSTRACT METHOD (Polymorphism): Must be implemented by all derived classes
    /// to define a unique, high-damage or special ability.
    /// </summary>
    public abstract void UseSpecialAbility(Character target);
}

// -----------------------------------------------------------------------------
// STUDENT IMPLEMENTATION AREA
// -----------------------------------------------------------------------------
public class PlayerCharacter : Character
{
    private readonly IAbility _specialAbility;

    private const int PLAYER_MAX_HEALTH = 120;
    private const int PLAYER_BASE_DAMAGE = 18;
    private const int PLAYER_POWERSTRIKE_BONUS = 12;
    public const int PLAYER_REST_HEAL = 15;

    public PlayerCharacter(string name) : base(name, PLAYER_MAX_HEALTH, PLAYER_BASE_DAMAGE)
    {
        _specialAbility = new PowerStrikeAbility(PLAYER_POWERSTRIKE_BONUS);
    }

    public override void UseSpecialAbility(Character target)
    {
        _specialAbility.Execute(this, target);
    }
}

public class GoblinEnemy : Character
{
    private readonly IAbility _specialAbility;
    private static readonly Random _rng = new Random();

    private const int GOBLIN_MAX_HEALTH = 60;
    private const int GOBLIN_BASE_DAMAGE = 10;
    private const int GOBLIN_STAB_BONUS = 8;
    private const double GOBLIN_SPECIAL_CHANCE = 0.5;

    public GoblinEnemy(string name) : base(name, GOBLIN_MAX_HEALTH, GOBLIN_BASE_DAMAGE)
    {
        _specialAbility = new GoblinStabAbility(GOBLIN_STAB_BONUS);
    }

    public override void UseSpecialAbility(Character target)
    {
        if (_rng.NextDouble() <= GOBLIN_SPECIAL_CHANCE)
        {
            _specialAbility.Execute(this, target);
        }
        else
        {
            Console.WriteLine($"{Name} failed the sneaky stab!");
            Attack(target);
        }
    }
}

public class TrollEnemy : Character
{
    private readonly IAbility _specialAbility;

    private const int TROLL_MAX_HEALTH = 110;
    private const int TROLL_BASE_DAMAGE = 16;
    private const double TROLL_CRIT_CHANCE = 0.25;
    private const int TROLL_CRIT_MULTIPLIER = 2;

    public TrollEnemy(string name) : base(name, TROLL_MAX_HEALTH, TROLL_BASE_DAMAGE)
    {
        _specialAbility = new TrollSmashAbility(TROLL_CRIT_CHANCE, TROLL_CRIT_MULTIPLIER);
    }

    public override void UseSpecialAbility(Character target)
    {
        _specialAbility.Execute(this, target);
    }
}
public class PowerStrikeAbility : IAbility
{
    private readonly int _bonusDamage;

    public PowerStrikeAbility(int bonusDamage)
    {
        _bonusDamage = bonusDamage;
    }

    public void Execute(Character attacker, Character target)
    {
        int totalDamage = attacker.Damage + _bonusDamage;

        Console.WriteLine($"{attacker.Name} unleashes a POWER STRIKE!");
        target.TakeDamage(totalDamage);
    }
}
public class GoblinStabAbility : IAbility
{
    private readonly int _bonusDamage;

    public GoblinStabAbility(int bonusDamage)
    {
        _bonusDamage = bonusDamage;
    }

    public void Execute(Character attacker, Character target)
    {
        int totalDamage = attacker.Damage + _bonusDamage;

        Console.WriteLine($"{attacker.Name} performs a Quick Stab!");
        target.TakeDamage(totalDamage);
    }
}
public class TrollSmashAbility : IAbility
{
    private readonly double _critChance;
    private readonly int _critMultiplier;
    private static readonly Random _rng = new Random();

    public TrollSmashAbility(double critChance, int critMultiplier)
    {
        _critChance = critChance;
        _critMultiplier = critMultiplier;
    }

    public void Execute(Character attacker, Character target)
    {
        int baseDamage = attacker.Damage;
        bool isCrit = _rng.NextDouble() <= _critChance;

        if (isCrit)
        {
            int critDamage = baseDamage * _critMultiplier;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{attacker.Name} lands a CRITICAL SMASH!");
            Console.ResetColor();
            target.TakeDamage(critDamage);
        }
        else
        {
            Console.WriteLine($"{attacker.Name} smashes the target!");
            target.TakeDamage(baseDamage);
        }
    }
}

// -----------------------------------------------------------------------------
// GAME MANAGEMENT
// -----------------------------------------------------------------------------
public class GameManager
{
    private PlayerCharacter _player;
    private List<Character> _enemies;
    private static readonly Random _rng = new Random();

    private const int START_DELAY_MS = 500;
    private const int BETWEEN_TURNS_DELAY_MS = 650;

    public GameManager()
    {
        _player = new PlayerCharacter("Student Hero");

        _enemies = new List<Character>
        {
            new GoblinEnemy("Goblin Raider"),
            new TrollEnemy("Mountain Troll")
        };
    }

    public void StartGame()
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine("          C# CONSOLE RPG COMBAT SIM           ");
        Console.WriteLine("==============================================");
        Console.WriteLine($"\nA challenging encounter awaits, {_player.Name}!");
        Thread.Sleep(START_DELAY_MS);

        while (_player.IsAlive && _enemies.Any(e => e.IsAlive))
        {
            DisplayStatus();
            PlayerTurn();
            CleanUpEnemies();
            if (!_enemies.Any(e => e.IsAlive)) break;

            EnemyTurn();
            Thread.Sleep(BETWEEN_TURNS_DELAY_MS);
        }

        EndGame();
    }

    private void DisplayStatus()
    {
        Console.WriteLine("\n--- STATUS ---");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"{_player.Name}: {GetHealthBar(_player)} ({_player.CurrentHealth}/{_player.MaxHealth})");
        Console.ResetColor();

        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            if (enemy.IsAlive)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"[{i + 1}] ");
                Console.WriteLine($"{enemy.Name}: {GetHealthBar(enemy)} ({enemy.CurrentHealth}/{enemy.MaxHealth})");
                Console.ResetColor();
            }
        }
        Console.WriteLine("--------------");
    }

    private string GetHealthBar(Character unit)
    {
        const int barLength = 20;
        double ratio = unit.MaxHealth > 0 ? (double)unit.CurrentHealth / unit.MaxHealth : 0;
        int filled = (int)Math.Round(ratio * barLength);
        filled = Math.Max(0, Math.Min(barLength, filled));
        return $"[{new string('#', filled)}{new string('-', barLength - filled)}]";
    }

    private void PlayerTurn()
    {
        Console.WriteLine("\n--- YOUR TURN ---");
        Console.WriteLine("Choose action:");
        Console.WriteLine("1: Standard Attack");
        Console.WriteLine("2: Special Ability (Polymorphism test)");
        Console.WriteLine($"3: Rest (Heal {PlayerCharacter.PLAYER_REST_HEAL} HP)");

        string choice = Console.ReadLine();

        if (choice == "1" || choice == "2")
        {
            Character target = SelectTarget();
            if (target == null) return;

            switch (choice)
            {
                case "1":
                    _player.Attack(target);
                    break;
                case "2":
                    _player.UseSpecialAbility(target);
                    break;
            }
        }
        else if (choice == "3")
        {
            _player.Heal(PlayerCharacter.PLAYER_REST_HEAL);
        }
        else
        {
            Console.WriteLine("Invalid choice. Turn skipped.");
        }
    }

    private Character SelectTarget()
    {
        var livingEnemies = _enemies.Where(e => e.IsAlive).ToList();
        if (!livingEnemies.Any()) return null;

        Console.WriteLine("Select Target (Enter number):");
        for (int i = 0; i < livingEnemies.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {livingEnemies[i].Name} ({livingEnemies[i].CurrentHealth}/{livingEnemies[i].MaxHealth})");
        }

        if (int.TryParse(Console.ReadLine(), out int targetIndex) && targetIndex >= 1 && targetIndex <= livingEnemies.Count)
        {
            return livingEnemies[targetIndex - 1];
        }
        else
        {
            Console.WriteLine("Invalid target selection.");
            return null;
        }
    }

    private void EnemyTurn()
    {
        Console.WriteLine("\n--- ENEMY TURN ---");
        Thread.Sleep(500);

        foreach (var enemy in _enemies.Where(e => e.IsAlive).ToList())
        {
            if (!_player.IsAlive) return;

            // Simple AI Logic: 1/3 chance to use special ability, otherwise standard attack
            if (_rng.Next(0, 3) == 0)
            {
                enemy.UseSpecialAbility(_player);
            }
            else
            {
                enemy.Attack(_player);
            }
            Thread.Sleep(750);
        }
    }

    private void CleanUpEnemies()
    {
        _enemies.RemoveAll(e => !e.IsAlive);
    }

    private void EndGame()
    {
        Console.WriteLine("\n==============================================");
        if (_player.IsAlive)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("               🎉 VICTORY ACHIEVED! 🎉               ");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("                DEFEAT! GAME OVER.                ");
        }
        Console.ResetColor();
        Console.WriteLine("==============================================");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        GameManager game = new GameManager();
        game.StartGame();
    }
}
