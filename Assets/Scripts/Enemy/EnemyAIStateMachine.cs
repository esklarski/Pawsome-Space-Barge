﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyTargetingSystem), typeof(EnemyEngineSystem), typeof(EnemyNavigationSystem))]
public class EnemyAIStateMachine : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType;
    public EnemyType Type => enemyType;

    [Header("Events")] 
    [SerializeField] private UnityEvent onIdleEnter;
    [SerializeField] private UnityEvent onIdleExit;
    [SerializeField] private UnityEvent onSeekEnter;
    [SerializeField] private UnityEvent onSeekExit;
    [SerializeField] private UnityEvent onContactEnter;
    [SerializeField] private UnityEvent onContactExit;

    public EnemyAIState currentState = EnemyAIState.Idle;
    private Animator animator;

    private EnemyEngineSystem engines;
    public EnemyEngineSystem Engines => engines;
    private EnemyTargetingSystem targeting;
    public EnemyTargetingSystem Targeting => targeting;
    private EnemyNavigationSystem navigation;
    public EnemyNavigationSystem Navigation => navigation;
    private AlertEventListener eventNetwork;
    public AlertEventListener EventNetwork => eventNetwork;


    private static readonly int EnemyTypeParameter = Animator.StringToHash("EnemyType");
    private static readonly int BargeDetectedParameter = Animator.StringToHash("BargeDetected");
    private static readonly int BargeContactParameter = Animator.StringToHash("BargeContact");


    private void Awake()
    {
        animator = GetComponent<Animator>();
        engines = GetComponent<EnemyEngineSystem>();
        targeting = GetComponent<EnemyTargetingSystem>();
        navigation = GetComponent<EnemyNavigationSystem>();
        eventNetwork = GetComponent<AlertEventListener>();
    }


    private void Start()
    {
        animator.SetInteger(EnemyTypeParameter, (int)enemyType);
        animator.SetBool(BargeDetectedParameter, false);
        animator.SetBool(BargeContactParameter, false);
    }

    private void Update()
    {
        animator.SetBool(BargeDetectedParameter, targeting.TargetLocked);
        animator.SetBool(BargeContactParameter, targeting.IsBargeContact());
    }


    public void OnStateEnter(EnemyAIState state)
    {
        switch (state)
        {
            case EnemyAIState.Idle:
                onIdleEnter.Invoke();
                break;
            case EnemyAIState.Seek:
                onSeekEnter.Invoke();
                break;
            case EnemyAIState.Contact:
                onContactEnter.Invoke();
                break;
            default:
                Debug.LogError($"EnemyAIStateMachine: Not implemented: {state}");
                return;
        }

        currentState = state;
    }


    public void OnStateExit(EnemyAIState state)
    {
        switch (state)
        {
            case EnemyAIState.Idle:
                onIdleExit.Invoke();
                break;
            case EnemyAIState.Seek:
                onSeekExit.Invoke();
                break;
            case EnemyAIState.Contact:
                onContactExit.Invoke();
                break;
            default:
                Debug.LogError($"EnemyAIStateMachine: Not implemented: {state}");
                break;
        }
    }
}