using System.Collections.Generic;
using UnityEngine;

// The guard's "brain": it decides which behaviour is active right now.
// Each behaviour is its own component (GuardWander, GuardChase, GuardSearch, GuardCatch), and
// switching state just turns the right ones on and the others off. Later states (Stunned,
// Blinded...) slot in the same way.
//
// Stealth rule: guards ignore you until one sees you steal. That makes you Wanted, and from
// then on any guard who sees you starts chasing. A guard who witnesses a theft also radios
// the other guards nearby, who come to search the spot.
//
// GuardAI also finds the Player and GameManager once and hands them to the other guard
// scripts, so a guard prefab can be dropped into the scene without wiring anything up.
//
//   Wander --(sees Wanted player)--> Chase --(loses sight for a while)--> Search
//     ^                                ^                                    |
//     |                                +-------(sees Wanted player)---------+
//     +-----------------------(search finished)-----------------------------+
//
// Any state --(banana / soccer ball)--> Stunned --(gets up)--> Search (if you're Wanted) or Wander
// Chase --(egg to the face, goes blind)--> Search where it last saw you
[RequireComponent(typeof(GuardWander), typeof(GuardChase), typeof(GuardCatch))]
[RequireComponent(typeof(GuardSight), typeof(GuardSearch), typeof(GuardStunned))]
public class GuardAI : MonoBehaviour
{
    public enum State
    {
        Wander,
        Chase,
        Search,
        Stunned
    }

    [Tooltip("What the guard does when the game starts.")]
    [SerializeField] private State startState = State.Wander;

    [Tooltip("Seconds the guard keeps chasing after losing sight of you before it gives up and searches. Lets it follow you around a corner.")]
    [SerializeField] private float loseSightTime = 2f;

    [Tooltip("When this guard witnesses a theft, other guards within this many meters are radioed to search the spot.")]
    [SerializeField] private float radioRange = 25f;

    public State CurrentState { get; private set; }

    // Every active guard in the scene. "static" means one shared list for the whole GuardAI class,
    // not one per guard, like a module-level variable in JS. Guards add and remove themselves.
    private static readonly List<GuardAI> allGuards = new List<GuardAI>();

    private GuardWander wander;
    private GuardChase chase;
    private GuardSearch search;
    private GuardCatch catcher;
    private GuardSight sight;
    private GuardStunned stunned;

    private Transform player;
    private PlayerLoot playerLoot;
    private PlayerWanted playerWanted;

    private Vector3 lastKnownPosition;
    private float timeSinceSeen;

    private void Awake()
    {
        wander = GetComponent<GuardWander>();
        chase = GetComponent<GuardChase>();
        search = GetComponent<GuardSearch>();
        catcher = GetComponent<GuardCatch>();
        sight = GetComponent<GuardSight>();
        stunned = GetComponent<GuardStunned>();

        // Look these up once here, never in Update. FindWithTag searches the scene for the object
        // tagged "Player"; FindAnyObjectByType finds the one GameManager.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("GuardAI: no object is tagged 'Player'. Select the Player and set its Tag to Player.");
            enabled = false;
            return;
        }
        player = playerObject.transform;
        playerLoot = playerObject.GetComponent<PlayerLoot>();
        playerWanted = playerObject.GetComponent<PlayerWanted>();
        GameManager gameManager = FindAnyObjectByType<GameManager>();

        // Hand the references to the scripts that need them.
        chase.Init(player);
        sight.Init(player);
        catcher.Init(player, gameManager);
    }

    private void OnEnable()
    {
        playerLoot.ItemStolen += OnItemStolen;
        sight.Blinded += OnBlinded;
        allGuards.Add(this);
    }

    private void OnDisable()
    {
        playerLoot.ItemStolen -= OnItemStolen;
        sight.Blinded -= OnBlinded;
        allGuards.Remove(this);
    }

    // Start (not Awake) so every component has finished its own setup before we switch them.
    private void Start()
    {
        SetState(startState);
    }

    private void Update()
    {
        // A switch on the current state, like a reducer: each state only checks the
        // events that can move it to another state.
        switch (CurrentState)
        {
            case State.Wander:
                if (playerWanted.IsWanted && sight.CanSeePlayer())
                {
                    StartChase();
                }
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Search:
                if (playerWanted.IsWanted && sight.CanSeePlayer())
                {
                    StartChase();
                }
                else if (search.IsFinished)
                {
                    SetState(State.Wander);
                }
                break;

            case State.Stunned:
                if (stunned.IsFinished)
                {
                    GetBackUp();
                }
                break;
        }
    }

    // Called by things that knock the guard over (banana peel, soccer ball).
    // "knockback" is how far, and which way, to shove the guard, in meters.
    public void KnockDown(float duration, Vector3 knockback)
    {
        stunned.Setup(duration, knockback);

        // Turning it off first means OnEnable runs again, so a second hit restarts the stun.
        stunned.enabled = false;
        SetState(State.Stunned);
    }

    private void GetBackUp()
    {
        if (playerWanted.IsWanted && sight.CanSeePlayer())
        {
            StartChase();
        }
        else if (playerWanted.IsWanted)
        {
            // Dazed: look around right here for whoever knocked it over.
            search.SetSearchPoint(transform.position);
            SetState(State.Search);
        }
        else
        {
            SetState(State.Wander);
        }
    }

    // Egg to the face. A chasing guard can't follow what it can't see, so it gives up right away
    // and stumbles over to where it last saw you (it still can't see anything until the blindness wears off).
    private void OnBlinded()
    {
        if (CurrentState == State.Chase)
        {
            search.SetSearchPoint(lastKnownPosition);
            SetState(State.Search);
        }
    }

    private void UpdateChase()
    {
        if (sight.CanSeePlayer())
        {
            // Still in view: remember where they are and reset the "lost them" timer.
            lastKnownPosition = player.position;
            timeSinceSeen = 0f;
            return;
        }

        timeSinceSeen += Time.deltaTime;
        if (timeSinceSeen >= loseSightTime)
        {
            search.SetSearchPoint(lastKnownPosition);
            SetState(State.Search);
        }
    }

    // Runs at the exact moment the player steals something, for every guard.
    private void OnItemStolen()
    {
        if (sight.CanSeePlayer())
        {
            // This guard is a witness: the player is now Wanted, and this guard reacts right away.
            playerWanted.MakeWanted();
            StartChase();
            RadioGuards(transform.position, radioRange, player.position, this);
        }
    }

    // Sends every guard within range of "from" to search "searchPoint". Static so anything can
    // call in an alert, not just guards: security cameras use GuardAI.RadioGuards(...) too.
    // "sender" is skipped so a guard doesn't radio itself (cameras pass null).
    public static void RadioGuards(Vector3 from, float range, Vector3 searchPoint, GuardAI sender)
    {
        foreach (GuardAI guard in allGuards)
        {
            if (guard != sender && Vector3.Distance(from, guard.transform.position) <= range)
            {
                guard.HearRadio(searchPoint);
            }
        }
    }

    // Another guard called in a theft. Head over and search, unless already chasing someone.
    // Once there, the normal rules apply: seeing the Wanted player starts a chase.
    public void HearRadio(Vector3 position)
    {
        // Busy chasing, or lying on the floor: ignore the radio.
        if (CurrentState == State.Chase || CurrentState == State.Stunned)
        {
            return;
        }

        search.SetSearchPoint(position);

        // Re-enable search so it restarts with the new point, even if this guard was already searching.
        search.enabled = false;
        SetState(State.Search);
    }

    private void StartChase()
    {
        lastKnownPosition = player.position;
        timeSinceSeen = 0f;
        SetState(State.Chase);
    }

    public void SetState(State newState)
    {
        CurrentState = newState;

        // Stunned goes first: switching it off un-freezes the NavMeshAgent before the next behaviour starts walking.
        stunned.enabled = newState == State.Stunned;
        wander.enabled = newState == State.Wander;
        chase.enabled = newState == State.Chase;
        search.enabled = newState == State.Search;

        // Only a chasing guard can catch you, so bumping into a wandering or searching guard doesn't end the game.
        catcher.enabled = newState == State.Chase;
    }

    // [ContextMenu] adds these to the component's ⋮ menu in the Inspector, so you can
    // test states by hand during Play mode before anything triggers them for real.
    [ContextMenu("Debug: Start Chasing")]
    private void DebugStartChasing()
    {
        StartChase();
    }

    [ContextMenu("Debug: Knock Down (3s)")]
    private void DebugKnockDown()
    {
        KnockDown(3f, -transform.forward * 1.5f);
    }

    [ContextMenu("Debug: Start Wandering")]
    private void DebugStartWandering()
    {
        SetState(State.Wander);
    }
}
