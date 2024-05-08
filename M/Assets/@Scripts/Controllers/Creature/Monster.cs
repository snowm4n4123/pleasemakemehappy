using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Monster : Creature
{
    public override ECreatureState CreatureState
    {
        get { return base.CreatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;
                switch (value)
                {
                    case ECreatureState.Idle:
                        UpdateAITick = 0.5f;
                        break;
                    case ECreatureState.Move:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Skill:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 1.0f;
                        break;
                }
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        CreatureType = ECreatureType.Monster;
        CreatureState = ECreatureState.Idle;
        Speed = 3.0f;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    void Start()
    {
        _initPos = transform.position;
    }

    #region AI
    public float SearchDistance { get; private set; } = 8.0f;
    public float AttackDistance { get; private set; } = 4.0f;
    Creature _target;
    Vector3 _destPos;
    Vector3 _initPos;

    protected override void UpdateIdle()
    {
        Debug.Log("Idle");

        //patrol
        {
            int patrolPercent = 10;
            int rand = Random.Range(0, 100);
            if (rand <= patrolPercent)
            {
                _destPos = _initPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2));
                CreatureState = ECreatureState.Move;
                return;
            }
        }

        //searchplayer
        {
            Creature target = null;
            float bestDistanceSqr = float.MaxValue;
            float searchDistanceSqr = SearchDistance * SearchDistance;

            foreach (Hero hero in Managers.Object.Heroes)
            {
                Vector3 dir = hero.transform.position - transform.position;
                float distToTargetSqr = dir.sqrMagnitude;

                Debug.Log(distToTargetSqr);

                if (distToTargetSqr > searchDistanceSqr)
                    continue;

                if (distToTargetSqr > bestDistanceSqr)
                    continue;

                target = hero;
                bestDistanceSqr = distToTargetSqr;
            }

            _target = target;

            if (_target != null)
                CreatureState = ECreatureState.Move;
        }
    }

    protected override void UpdateMove()
    {
        Debug.Log("Move");

        if (_target == null)
        {
            // 패트롤 아님 돌아가기
            Vector3 dir = (_destPos - transform.position);
            float moveDist = Mathf.Min(dir.magnitude, Time.deltaTime * Speed);
            transform.TranslateEx(dir.normalized * moveDist);

            if (dir.sqrMagnitude <= 0.01f)
            {
                CreatureState = ECreatureState.Idle;
            }
        }
        else
        {
            //추적
            Vector3 dir = (_target.transform.position - transform.position);
            float distToTargetSqr = dir.sqrMagnitude;
            float attackDistanceSqr = AttackDistance * AttackDistance;
            
            if (distToTargetSqr < attackDistanceSqr)
            {
                //공격범위안
                CreatureState = ECreatureState.Skill;
                StartWait(2.0f);
            }
            else
            {
                //공격범위밖
                float moveDist = Mathf.Min(dir.magnitude, Time.deltaTime * Speed);
                transform.TranslateEx(dir.normalized * moveDist);

                //너무멀어지면포기
                float searchDistanceSqr = SearchDistance * SearchDistance;
                if (distToTargetSqr > searchDistanceSqr)
                {
                    _destPos = _initPos;
                    _target = null;
                    CreatureState = ECreatureState.Move;
                }

            }
        }
    }

    protected override void UpdateSkill()
    {
        if (_coWait != null)
            return;
        CreatureState = ECreatureState.Move;
    }
    protected override void UpdateDead()
    {

    }
    #endregion
}