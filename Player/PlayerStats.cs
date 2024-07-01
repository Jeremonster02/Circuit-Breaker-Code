using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.XR;
//using NUnit.Framework.Constraints;

public enum PlayerType { Balance, LR, Tank, None }
public enum Conditions { None, Vulnerable}
public class PlayerStats : MonoBehaviour
{
    private GameManager gm;

    public float health = 100f;
    public float maxHealth = 100f;
    public float downedHealthModifier = 1f;
    public float charge = 25f;
    public float maxCharge = 100f;
    public float chargeRate;
    public float downedMeter;
    public float downedMax = 100f;
    public float downedDecreaseRate;
    public string playerName;
    public PlayerType playerType;
    public Conditions curCondition;
    public float knockback;


    public StateManager stateManager;
    public WeaponsManager weaponsManager;
    public PlayerController pController;
    public MeleeBasic meleeScr;
    private RoundWinUI roundWinUI;
    public bool playerDead = false;
    private int conditionCounter;

    [Header("UI Settings")]
    public Slider healthSlider;
    public Slider chargeSlider;
    public Slider overclockSlider;
    public Camera gameCamera;
    public Transform target;

    private void Awake()
    {
        gm = GameManager.instance;

        if (!gameCamera)
        {
            gameCamera = Camera.main;
        }
        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
        }
        if (gameObject.tag != "Dummy")
        {
            stateManager = GetComponent<StateManager>();
            curCondition = Conditions.None;
            weaponsManager = GetComponentInChildren<WeaponsManager>();
            roundWinUI = GetComponentInChildren<RoundWinUI>();
            meleeScr = GetComponent<MeleeBasic>();
        }

    }

    public void InitUI(PlayerInputHandler _pih)
    {
        GameObject obj = Instantiate(Resources.Load("Widgets/" + "PlayerUI") as GameObject, HUD.instance.playerUILayout);
        PlayerUI scr = obj.GetComponent<PlayerUI>();
        scr.Init(this, GetComponent<WeaponsManager>(), _pih);
    }

    private void Update()
    {
        //healthSlider.value = health;
        //healthSlider.transform.rotation = gameCamera.transform.rotation;
        //chargeSlider.value = GetComponent<WeaponsManager>().sCurTime;
        //overclockSlider.value = charge;
    }

    private void FixedUpdate()
    {
        if (gameObject.tag != "Dummy")
        {
            if (charge < maxCharge && !stateManager.isOverclockActive)
            {
                charge = charge + (chargeRate / 60);
            }
            else if (charge >= maxCharge)
            {
                charge = maxCharge;
            }

            if (stateManager.isDowned != true)
            {
                if (!justHit)
                {
                    downedMeter = downedMeter + (downedDecreaseRate / 60);
                }
            }

            if (downedMeter <= 0)
            {
                downedMeter = 0;
            }
        }

    }

    public void ChangeCondition(Conditions _con)
    {
        curCondition = _con;
    }

    public void TakeDamage(float _damage, float _downer, bool _knockback, Vector2 _knockbackVector)
    {
        if (meleeScr.isMelee == true) return;
        if (playerDead) return;
        if (stateManager.isPostDowned == true) return;

        weaponsManager.StopAttackCoroutine();
        pController.animC.HitstunStart();
        StopCoroutine("JustHitCoroutine");
        StartCoroutine("JustHitCoroutine");

        float _trueDamage;
        float _trueDowned;
        
        if (weaponsManager.firedPrimaryProjectiles != null)
        {
            foreach (GameObject p in weaponsManager.firedPrimaryProjectiles)
            {
                Destroy(p);
            }
        }

        _trueDamage = CalculateDamage(_damage);
        _trueDowned = CalculateDowned(_downer);

        health = health - _trueDamage;
        downedMeter = downedMeter + _trueDowned * downedHealthModifier;
        GetComponent<StateManager>().downedDamage += _trueDamage;

        if (downedMeter >= downedMax)
        {
            stateManager.startDownedState();
        }

        Debug.Log("Player " + playerName + " damaged: " + (_trueDamage * downedHealthModifier));
        if (_knockback)
        {
            Knockback(_knockbackVector);
        }

        if (health <= 0)
        {
            playerDead = true;
            pController.animC.DeathStart();

            if (GetComponent<PlayerController>().playerIdx == 0)
            {
                GameManager.instance.roundMgr.player2Wins++;
            }
            if (GetComponent<PlayerController>().playerIdx == 1)
            {
                GameManager.instance.roundMgr.player1Wins++;
            }
            //Destroy(this.gameObject); //Kill the player; make a proper method for this later
            roundWinUI.ShowRoundWinUI(pController.playerIdx);
            StartCoroutine(LetWinPlayDelay());
            //GameManager.instance.roundMgr.RoundEnd();
        }
    }

    private float CalculateDamage(float _damage)
    {
        float _trueDamage;

        _trueDamage = _damage;

        if (curCondition == Conditions.Vulnerable)
        {
            _trueDamage = _trueDamage * 2;
        }
        if (stateManager.defencePU == true)
        {
            _trueDamage = _trueDamage / 2;
        }
        if (stateManager.isDowned)
        {
            _trueDamage = _trueDamage * downedHealthModifier;
        }
        return _trueDamage;
    }

    private float CalculateDowned(float _down)
    {
        float _trueDowned;

        _trueDowned = _down;

        if (curCondition == Conditions.Vulnerable)
        {
            _trueDowned = _trueDowned * 2;
        }
        if (stateManager.defencePU == true)
        {
            _trueDowned = _trueDowned / 2;
        }
        if (stateManager.isDowned)
        {
            _trueDowned = _trueDowned * downedHealthModifier;
        }
        return _trueDowned;
    }

    public void ChangeCharge(float _charge)
    {
        charge = charge + _charge;
        if (charge > maxCharge) { charge = maxCharge; }
    }

    private void Knockback(Vector2 _knockbackVector)
    {
        IEnumerator coroutine = KnockbackCoroutine(_knockbackVector);
        StartCoroutine(coroutine);
    }

    IEnumerator KnockbackCoroutine(Vector2 _knockbackVector)
    {
        float starttime = Time.time;
        Vector3 moveDerection = _knockbackVector * knockback;
        while (starttime + .1f > Time.time)
        {
            GetComponent<CharacterController>().Move(moveDerection * Time.deltaTime * (knockback / 2));
            yield return null;
        }
    }

    public void ConditionExitStarter(float _conditionLength, Conditions _condition)
    {
        IEnumerator coroutine = ConditionExit(_conditionLength, _condition);
        StartCoroutine(coroutine);
    }


    public IEnumerator ConditionExit(float _conditionLength, Conditions _condition)
    {
        conditionCounter = conditionCounter + 1;
        yield return new WaitForSeconds(_conditionLength);
        conditionCounter = conditionCounter - 1;
        if (curCondition == _condition && conditionCounter == 0)
        {
            ChangeCondition(Conditions.None);
            Debug.Log("Player is no longer " + _condition);
        }
    }

    private IEnumerator LetWinPlayDelay()
    {
        yield return new WaitForSeconds(4f);
        GameManager.instance.roundMgr.RoundEnd();

    }

    private bool justHit;
    private IEnumerator JustHitCoroutine()
    {
        justHit = true;
        yield return new WaitForSeconds(1.5f);
        justHit = false;
    }
}
