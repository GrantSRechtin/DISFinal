using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RewindController : MonoBehaviour
{
    // ---------- Singleton setup ----------
    private static RewindController _instance;

    public static RewindController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RewindController>();

                if (_instance == null)
                {
                    var go = new GameObject("RewindController");
                    _instance = go.AddComponent<RewindController>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void OnEnable()
    {
        PlayerInputHandler.Instance.RewindPressed  += OnRewindPressed;
        PlayerInputHandler.Instance.RewindReleased += OnRewindReleased;
    }

    private void OnDisable()
    {
        PlayerInputHandler.Instance.RewindPressed  -= OnRewindPressed;
        PlayerInputHandler.Instance.RewindReleased -= OnRewindReleased;
    }

    private bool timeStopped = false;

    // Public
    public event Action TimeStarted;
    public event Action TimeStopped;
    public event Action RewindTimestep;
    public event Action StoreTimestep;

    void OnRewindPressed()
    {
        timeStopped = true;
        TimeStopped?.Invoke();
    }
    
    void OnRewindReleased()
    {
        timeStopped = false;
        TimeStarted?.Invoke();
    }

    void Start()
    {
        StartCoroutine(StoreTimestepLoop());
        StartCoroutine(RewindTimestepLoop());
    }

    IEnumerator StoreTimestepLoop()
    {
        yield return new WaitForSeconds(.05f);
        yield return new WaitForEndOfFrame();

        if (!timeStopped)
        {
            StoreTimestep?.Invoke();
        }

        StartCoroutine(StoreTimestepLoop());
    }

    IEnumerator RewindTimestepLoop()
    {
        yield return new WaitForSeconds(.025f);
        yield return new WaitForEndOfFrame();

        if (timeStopped)
        {
            RewindTimestep?.Invoke();
        }

        StartCoroutine(RewindTimestepLoop());
    }
}